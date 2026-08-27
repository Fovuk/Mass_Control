#!/usr/bin/env python3
"""Build levels 4-6 via Unity batch mode or preview layouts offline."""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
UNITY = Path(r"C:\Program Files\Unity\Hub\Editor\6000.4.2f1\Editor\Unity.exe")

LEVELS = {
    4: ("Spike Alley", 130, "4 moving + 3 static spikes, pushable, finish ~122"),
    5: ("Heavy Lifting", 165, "5 moving + 4 static spikes, high pushable star, finish ~156"),
    6: ("Grand Finale", 205, "6 moving + 6 static spikes, pushable, finish ~196"),
}


def preview() -> None:
    print("Levels 4-6 (see Assets/Editor/LevelDefinitions.cs):\n")
    for num, (name, width, notes) in LEVELS.items():
        print(f"  Level {num:02d}: {name} ({width} tiles wide) — {notes}")


def build() -> int:
    if not UNITY.exists():
        print(f"Unity not found at {UNITY}", file=sys.stderr)
        return 1

    marker = ROOT / "Library" / ".level_builder_v2_done"
    if marker.exists():
        marker.unlink()

    log = ROOT / "Logs" / "level_build_456.log"
    log.parent.mkdir(parents=True, exist_ok=True)

    cmd = [
        str(UNITY),
        "-batchmode",
        "-nographics",
        "-projectPath",
        str(ROOT),
        "-executeMethod",
        "LevelBuilder.BuildLevels456",
        "-logFile",
        str(log),
        "-quit",
    ]

    print("Running Unity batch build for levels 4-6...")
    print("Close the Unity Editor first if batch mode reports a project lock.")
    result = subprocess.run(cmd)
    if result.returncode == 0:
        print("Levels 4-6 built successfully.")
    else:
        print(f"Build failed (exit {result.returncode}). See {log}")
    return result.returncode


def main() -> int:
    if len(sys.argv) > 1 and sys.argv[1] == "build":
        return build()
    preview()
    print("\nBuild options:")
    print("  python Tools/build_levels_offline.py       (no Unity required)")
    print("  python Tools/generate_levels.py build      (Unity batch, editor closed)")
    print("  Unity menu: Tools > Build Levels 4-6       (Unity Editor open)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
