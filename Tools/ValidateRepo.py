#!/usr/bin/env python3
"""Static validation for Citybuilder. No Unity installation required.

Run from anywhere: python Tools/ValidateRepo.py
Exit code 0 = OK, 1 = validation errors.
"""
from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors: list[str] = []
warnings: list[str] = []


def fail(message: str) -> None:
    errors.append(message)


def warn(message: str) -> None:
    warnings.append(message)


def read_text(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig", errors="replace")


def cs_clean_lines(text: str) -> list[str]:
    """C# lines with comments and literals blanked out for structural scans."""
    result: list[str] = []
    in_block = False
    for raw in text.splitlines():
        line = raw
        if in_block:
            end = line.find("*/")
            if end < 0:
                result.append("")
                continue
            line = line[end + 2:]
            in_block = False
        while "/*" in line:
            start = line.find("/*")
            end = line.find("*/", start + 2)
            if end < 0:
                line = line[:start]
                in_block = True
                break
            line = line[:start] + line[end + 2:]
        line = re.sub(r"//.*$", "", line)
        line = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
        line = re.sub(r"'(?:\\.|[^'\\])*'", "''", line)
        result.append(line)
    return result


CS_MODIFIERS = r"(?:public|private|protected|internal|static|sealed|override|virtual|partial|extern|new|async|unsafe|abstract|readonly)\s+"
CS_START = re.compile(
    r"^\s*(?:\[[^\]]+\]\s*)*(?:" + CS_MODIFIERS + r")*[\w<>,\[\]\.\?]+\s+(\w+)\s*\("
)
CS_NOT_METHODS = {
    "if", "for", "foreach", "while", "switch", "catch", "using", "lock",
    "return", "new", "else", "do", "fixed", "checked", "unchecked",
    "when", "where", "select", "get", "set", "init", "add", "remove",
    "delegate", "operator", "base", "this",
}


def csharp_methods(lines: list[str]) -> list[tuple[int, int]]:
    """Return (start line, length) for methods, signature through closing brace."""
    found: list[tuple[int, int]] = []
    index = 0
    while index < len(lines):
        match = CS_START.match(lines[index])
        if not match or match.group(1) in CS_NOT_METHODS:
            index += 1
            continue
        signature = lines[index]
        balance = signature.count("(") - signature.count(")")
        last = index
        while balance > 0 and last + 1 < len(lines):
            last += 1
            balance += lines[last].count("(") - lines[last].count(")")
            signature += " " + lines[last]
        if balance != 0 or ";" in signature[signature.rfind(")") + 1:]:
            index = last + 1
            continue
        opener = last
        while opener < len(lines) and not any(
            token in lines[opener] for token in ("{", ";", "=>")
        ):
            opener += 1
        if opener >= len(lines) or ";" in lines[opener]:
            index = opener + 1
            continue
        if "=>" in lines[opener]:
            found.append((index + 1, opener + 1 - index))
            index = opener + 1
            continue
        depth = 0
        end = opener
        while end < len(lines):
            depth += lines[end].count("{") - lines[end].count("}")
            if depth <= 0:
                break
            end += 1
        found.append((index + 1, end + 1 - index))
        index = end + 1
    return found


CS_TYPE = re.compile(r"\b(class|interface|struct|enum|record|delegate)\s+\w+")


def top_level_types(lines: list[str]) -> list[tuple[int, str]]:
    """Type declarations directly in the namespace (or file scope)."""
    found: list[tuple[int, str]] = []
    depth = 0
    for number, line in enumerate(lines, 1):
        if depth <= 1:
            match = CS_TYPE.search(line)
            if match:
                found.append((number, match.group(1)))
        depth += line.count("{") - line.count("}")
    return found


LINQ_CALLS = (
    "Select", "SelectMany", "Where", "OrderBy", "OrderByDescending",
    "ThenBy", "GroupBy", "Any", "All", "First", "FirstOrDefault",
    "Last", "LastOrDefault", "Single", "SingleOrDefault", "Sum",
    "Average", "Min", "Max", "Distinct", "Aggregate", "ToList",
    "ToArray", "Count", "LongCount", "Skip", "Take", "Join", "OfType",
)
LINQ_CALL = re.compile(r"\.(" + "|".join(LINQ_CALLS) + r")\s*\(")
FORBIDDEN_CS = {
    "regex": re.compile(r"\bRegex\b|System\.Text\.RegularExpressions"),
    "async/await": re.compile(r"\basync\b|\bawait\b|\bTask\s*<|Task\.Run"),
    "reflection": re.compile(r"\bBindingFlags\b|\bMethodInfo\b|\bActivator\b|\bAssembly\b|GetType\(\)"),
    "DI container": re.compile(r"\bIServiceCollection\b|\bServiceCollection\b|\bDependencyInjection\b|\bInject\b"),
}


def linq_chain_lengths(text: str) -> list[int]:
    """Lengths of adjacent LINQ call chains; needs `using System.Linq` (gated by caller)."""
    spans: list[tuple[int, int]] = []
    for match in LINQ_CALL.finditer(text):
        depth = 1
        pos = match.end()
        while pos < len(text) and depth:
            if text[pos] == "(":
                depth += 1
            elif text[pos] == ")":
                depth -= 1
            pos += 1
        spans.append((match.start(), pos))
    chains: list[list[tuple[int, int]]] = []
    for span in spans:
        if chains and not text[chains[-1][-1][1]:span[0]].strip():
            chains[-1].append(span)
        else:
            chains.append([span])
    return [len(chain) for chain in chains]


def check_csharp_source(rel: str, text: str) -> None:
    clean = cs_clean_lines(text)
    for line, length in csharp_methods(clean):
        if length > 30:
            warn(f"{rel}:{line}: method exceeds 30 lines ({length})")
    types = top_level_types(clean)
    if len(types) > 1:
        classes = [item for item in types if item[1] in ("class", "interface", "record")]
        if len(classes) > 1:
            fail(f"{rel}: {len(types)} top-level types — one file must hold one class")
        else:
            warn(f"{rel}: {len(types)} top-level types — AGENTS.md prefers one class per file")
    if "using System.Linq;" in text or "System.Linq.Enumerable." in text:
        for size in linq_chain_lengths(text):
            if size > 2:
                fail(f"{rel}: LINQ chain of {size} calls exceeds 2 (AGENTS.md:77)")
    if rel.startswith("Assets/Citybuilder/Scripts/"):
        for label, pattern in FORBIDDEN_CS.items():
            match = pattern.search(text)
            if match:
                fail(f"{rel}: forbidden {label} API ({match.group(0)})")


def tracked_files() -> list[Path]:
    try:
        output = subprocess.check_output(
            ["git", "ls-files"], cwd=ROOT, text=True, encoding="utf-8"
        )
        return [
            ROOT / line
            for line in output.splitlines()
            if line and (ROOT / line).is_file()
        ]
    except Exception:
        return [
            path
            for path in ROOT.rglob("*")
            if path.is_file() and ".git" not in path.parts
        ]


files = tracked_files()
files_by_rel = {path.relative_to(ROOT).as_posix(): path for path in files}

if (ROOT / "HANDOFF.md").exists():
    fail("HANDOFF.md must not exist in v1.1")

if not (ROOT / "TASKS" / "README.md").is_file():
    fail("TASKS/README.md (epic queue) is required; see ONBOARDING.md")

credits_text = read_text(ROOT / "CREDITS.md") if (ROOT / "CREDITS.md").is_file() else ""
for path in files:
    rel = path.relative_to(ROOT).as_posix()
    if rel.startswith("Assets/Citybuilder/Models/") and path.suffix.lower() in (".fbx", ".png"):
        if f"`{rel}`" not in credits_text and rel not in credits_text:
            fail(f"{rel}: asset not listed in CREDITS.md")

for path in files:
    rel = path.relative_to(ROOT).as_posix()
    if path.suffix != ".cs":
        continue
    text = read_text(path)
    line_count = len(text.splitlines())
    if line_count > 200:
        fail(f"{rel}: {line_count} lines exceeds hard C# limit 200")
    elif line_count > 150:
        warn(f"{rel}: {line_count} lines exceeds soft C# limit 150")
    if rel.startswith("Assets/Citybuilder/Scripts/") and "namespace Citybuilder" not in text:
        fail(f"{rel}: missing namespace Citybuilder")
    check_csharp_source(rel, text)

guids: dict[str, str] = {}
for path in files:
    rel = path.relative_to(ROOT).as_posix()
    if rel.startswith("Assets/") and not rel.endswith(".meta"):
        meta = Path(str(path) + ".meta")
        if not meta.exists():
            fail(f"{rel}: missing .meta file")
    if rel.endswith(".meta"):
        target_rel = rel[:-5]
        target_path = ROOT / target_rel
        if not target_path.exists():
            fail(f"{rel}: orphan .meta file (target does not exist: {target_rel})")
        elif target_path.is_dir():
            if not any(f.startswith(target_rel + "/") for f in files_by_rel):
                fail(f"{rel}: orphan .meta file (target directory has no tracked files: {target_rel})")
            elif not any(target_path.iterdir()):
                fail(f"{rel}: orphan .meta file (target directory is empty: {target_rel})")
        match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", read_text(path), re.M)
        if match:
            guid = match.group(1).lower()
            if guid in guids:
                fail(f"duplicate GUID {guid}: {guids[guid]} and {rel}")
            else:
                guids[guid] = rel

assets_root = ROOT / "Assets"
if assets_root.is_dir():
    for disk_meta in sorted(assets_root.rglob("*.meta")):
        rel_meta = disk_meta.relative_to(ROOT).as_posix()
        disk_target = disk_meta.with_name(disk_meta.name[:-5])
        target_rel = disk_target.relative_to(ROOT).as_posix()
        if not disk_target.exists():
            if rel_meta not in files_by_rel:
                fail(f"{rel_meta}: orphan .meta file on disk (target does not exist: {target_rel})")
        elif disk_target.is_dir() and not any(disk_target.iterdir()):
            if rel_meta not in files_by_rel:
                fail(f"{rel_meta}: orphan .meta file on disk (target directory is empty: {target_rel})")

for rel in (
    "Packages/manifest.json",
    "Packages/packages-lock.json",
    "Tools/mcp.example.json",
    "ProjectSettings/SceneTemplateSettings.json",
):
    path = ROOT / rel
    if not path.exists():
        fail(f"missing required JSON file: {rel}")
        continue
    try:
        json.loads(read_text(path))
    except json.JSONDecodeError as exc:
        fail(f"{rel}: invalid JSON: {exc}")

manifest = json.loads(read_text(ROOT / "Packages/manifest.json"))
if "com.unity.multiplayer.center" in manifest.get("dependencies", {}):
    fail("Packages/manifest.json still contains unused com.unity.multiplayer.center")

project_settings = read_text(ROOT / "ProjectSettings/ProjectSettings.asset")
bundle = re.search(r"^  bundleVersion:\s*(\d+\.\d+(?:\.\d+)?)\s*$", project_settings, re.M)
if not bundle:
    fail("ProjectSettings/ProjectSettings.asset: missing bundleVersion")
else:
    version = bundle.group(1)
    readme_release = re.search(
        r"Current release:\s*\*\*v?(\d+\.\d+(?:\.\d+)?)\*\*", read_text(ROOT / "README.md")
    )
    latest_release = re.search(
        r"^## \[v?(\d+\.\d+(?:\.\d+)?)\]", read_text(ROOT / "CHANGELOG.md"), re.M
    )
    if not readme_release or not latest_release:
        fail("version mismatch: README.md / CHANGELOG.md release not found")
    elif readme_release.group(1) != latest_release.group(1):
        fail(f"version drift: README {readme_release.group(1)} vs CHANGELOG {latest_release.group(1)}")
    elif latest_release.group(1) != version and not latest_release.group(1).startswith(version + "."):
        fail(f"bundleVersion {version} does not match release {latest_release.group(1)}")

secret_pattern = re.compile(
    r"(?i)(api[_-]?key|secret|password|token|bearer)\s*[:=]\s*['\"][^'\"]{8,}['\"]"
)
scanned_suffixes = {".cs", ".json", ".yml", ".yaml", ".md", ".ps1", ".bat", ".asset", ".unity", ".txt", ".py"}
for path in files:
    rel = path.relative_to(ROOT).as_posix()
    if path.suffix not in scanned_suffixes:
        continue
    for number, line in enumerate(read_text(path).splitlines(), 1):
        if secret_pattern.search(line):
            fail(f"{rel}:{number}: possible hardcoded credential")

# Historical docs (CHANGELOG/AGENTS/DECISIONS) may mention the removal; operational docs must not.
operational_docs = (
    "README.md",
    "ARCHITECTURE.md",
    "GAME_DESIGN.md",
    "UNITY_MCP_SETUP.md",
    "CONTRIBUTING.md",
    "SECURITY.md",
)
for rel in operational_docs:
    path = ROOT / rel
    if path.exists() and "HANDOFF.md" in read_text(path):
        fail(f"{rel}: references removed HANDOFF.md")

for rel in (
    "README.md",
    "UNITY_MCP_SETUP.md",
    "Tools/Start-WorkBuddyStack.ps1",
    "Tools/Check-UnityMcp.ps1",
):
    path = ROOT / rel
    if path.exists() and r"C:\Users\admin\Citybuilder" in read_text(path):
        fail(f"{rel}: hardcoded personal project path")

required_files = (
    "CHANGELOG.md",
    "CONTRIBUTING.md",
    "SECURITY.md",
    "DECISIONS/ADR-005-v1.1-scope.md",
    "Tools/mcp.example.json",
    "Tools/ValidateRepo.py",
    ".github/workflows/ci.yml",
    ".github/CODEOWNERS",
)
for rel in required_files:
    if not (ROOT / rel).exists():
        fail(f"missing required file: {rel}")

for message in warnings:
    print(f"WARN: {message}")
for message in errors:
    print(f"ERROR: {message}")

if errors:
    print(f"FAILED: {len(errors)} error(s), {len(warnings)} warning(s)")
    sys.exit(1)

print(f"OK: {len(files)} tracked files, {len(guids)} unique GUIDs, {len(warnings)} warning(s)")
