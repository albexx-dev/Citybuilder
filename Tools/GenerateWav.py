#!/usr/bin/env python3
"""Детерминированный генератор звуков Citybuilder. Только Python stdlib.

    python Tools/GenerateWav.py --out <папка>          # записать WAV-файлы
    python Tools/GenerateWav.py --verify               # сверить хеши с репо
    python Tools/GenerateWav.py --verify-exact         # только бито-точные
    python Tools/GenerateWav.py --verify --verbose     # таблица по файлам

Исходные WAV в Assets/Citybuilder/Audio/ никогда не перезаписываются.
"""
from __future__ import annotations

import argparse
import hashlib
import math
import random
import struct
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
AUDIO = ROOT / "Assets" / "Citybuilder" / "Audio"
RATE = 22050
SEED = 125
EXACT = ("click.wav", "coin.wav", "pop.wav")


def wav_bytes(samples: list[int]) -> bytes:
    """RIFF/WAVE, 16-bit mono, без дополнительных чанков."""
    pcm = struct.pack("<" + "h" * len(samples), *samples)
    header = b"RIFF" + struct.pack("<I", 36 + len(pcm)) + b"WAVE"
    header += b"fmt " + struct.pack("<IHHIIHH", 16, 1, 1, RATE, RATE * 2, 2, 16)
    return header + b"data" + struct.pack("<I", len(pcm)) + pcm


def tone(i: int, freq: float, amp: float, decay: float) -> int:
    t = i / RATE
    return int(32767 * amp * math.sin(2 * math.pi * freq * t) * math.exp(-decay * t))


def click() -> list[int]:
    return [tone(i, 1100, 0.35, 45) for i in range(992)]


def coin() -> list[int]:
    samples = []
    for i in range(3087):
        freq = 987.77 if i / RATE < 0.055 else 1318.51
        samples.append(tone(i, freq, 0.45, 14))
    return samples


def pop() -> list[int]:
    samples = []
    for i in range(1543):
        t = i / RATE
        phase = 2 * math.pi * (420 * t + 4000 * t * t)
        samples.append(int(32767 * 0.4 * math.exp(-28 * t) * math.sin(phase)))
    return samples


def swoosh() -> list[int]:
    n, samples = 2425, []
    rng = random.Random(SEED)
    drift = rng.uniform(-20, 20)
    for i in range(n):
        p = i / n
        freq = 290 + 430 * math.sin(math.pi * p) + drift
        samples.append(int(32767 * 0.3 * math.sin(math.pi * p)
                           * math.sin(2 * math.pi * freq * i / RATE)))
    return samples


def fire() -> list[int]:
    rng = random.Random(SEED + 1)
    first = []
    for j in range(6081):
        t = j / RATE
        freq = 660 + 220 * t / (6081 / RATE)
        env = math.sin(math.pi * j / 6081) * math.exp(-1.5 * t)
        phase = 2 * math.pi * freq * t
        first.append(int(32767 * 0.38 * env
                         * (math.sin(phase) + rng.uniform(-0.02, 0.02))))
    second = []
    for j in range(11576):
        t = j / RATE
        env = math.sin(math.pi * j / 11576)
        phase = 2 * math.pi * (880 * t + 220 * t * t / 0.525)
        second.append(int(32767 * 0.38 * env
                          * (math.sin(phase) + 0.2 * math.sin(2 * phase))))
    return first + [0] * 2188 + second


def crime() -> list[int]:
    samples, rng = [], random.Random(SEED + 2)
    for i in range(12127):
        t = i / RATE
        env = math.sin(math.pi * i / 12127) * math.exp(-3 * t)
        beats = math.sin(2 * math.pi * 392 * t) + math.sin(2 * math.pi * 415 * t)
        chirp = math.sin(2 * math.pi * (900 * t - 600 * t * t))
        samples.append(int(32767 * 0.34 * env
                           * (beats + 0.25 * chirp + rng.uniform(-0.01, 0.01))))
    return samples


def ambient(night: bool) -> list[int]:
    rng = random.Random(SEED + (4 if night else 3))
    samples, chirps = [], []
    for _ in range(24 if not night else 32):
        at = rng.randrange(22050, RATE * 23)
        length = rng.randrange(2205, 4410)
        chirps.append((at, length, rng.uniform(700, 1800), rng.uniform(0.04, 0.12)))
    base = 0.18 if night else 0.32
    for i in range(RATE * 24):
        t = i / RATE
        drone = (math.sin(2 * math.pi * 55 * t) + 0.5 * math.sin(2 * math.pi * 82.5 * t)
                 + 0.25 * math.sin(2 * math.pi * 110 * t)) * base
        event = 0.0
        for at, length, freq, amp in chirps:
            if at <= i < at + length:
                u = (i - at) / length
                event += amp * math.sin(math.pi * u) * math.sin(2 * math.pi * freq * u * length / RATE)
        samples.append(max(-32768, min(32767, int(32767 * (drone + event)))))
    return samples


SPEC = {
    "click.wav": click, "coin.wav": coin, "pop.wav": pop, "swoosh.wav": swoosh,
    "event-fire.wav": fire, "event-crime.wav": crime,
    "ambient-day.wav": lambda: ambient(False), "ambient-night.wav": lambda: ambient(True),
}


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def verify(names: list[str], verbose: bool) -> int:
    bad = 0
    print(f"seed={SEED} rate={RATE} exact={','.join(EXACT)}")
    for name in names:
        data = wav_bytes(SPEC[name]())
        path = AUDIO / name
        committed = path.read_bytes() if path.is_file() else b""
        same = data == committed
        if not same:
            bad += 1
        mark = "MATCH" if same else ("DIFF (approximation)" if name not in EXACT else "DIFF")
        if verbose or not same:
            print(f"{name:20} gen={digest(data)[:16]} repo={digest(committed)[:16]} {mark}")
    print(f"verified {len(names)} files, {bad} difference(s)")
    return 1 if bad else 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, help="папка для сгенерированных WAV")
    parser.add_argument("--verify", action="store_true", help="сравнить хеши с закоммиченными")
    parser.add_argument("--verify-exact", action="store_true", help="проверить только бито-точные")
    parser.add_argument("--verbose", action="store_true")
    parser.add_argument("--force", action="store_true", help="перезаписать файлы в --out")
    args = parser.parse_args()
    if args.verify or args.verify_exact:
        names = list(EXACT) if args.verify_exact else list(SPEC)
        return verify(names, args.verbose)
    if not args.out:
        parser.error("нужно --out или --verify")
    if args.out.resolve().is_relative_to(AUDIO.resolve()):
        print(f"ERROR: --out не может быть внутри {AUDIO.as_posix()}: "
              "исходные WAV перезаписывать запрещено", file=sys.stderr)
        return 2
    args.out.mkdir(parents=True, exist_ok=True)
    for name, build in SPEC.items():
        target = args.out / name
        if target.exists() and not args.force:
            print(f"skip {name} (есть; --force чтобы перезаписать)")
            continue
        target.write_bytes(wav_bytes(build()))
        print(f"write {target} sha256={digest(target.read_bytes())}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
