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

if (ROOT / "TASKS").is_dir():
    fail("TASKS/ must not exist; task tracking lives in GitHub Issues")

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
if not re.search(r"^  bundleVersion:\s*1\.1\s*$", project_settings, re.M):
    fail("ProjectSettings/ProjectSettings.asset: bundleVersion must be 1.1")

secret_pattern = re.compile(
    r"(?i)(api[_-]?key|secret|password|token|bearer)\s*[:=]\s*['\"][^'\"]{8,}['\"]"
)
scanned_suffixes = {".cs", ".json", ".yml", ".yaml", ".md", ".ps1", ".bat", ".asset", ".unity", ".txt"}
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
