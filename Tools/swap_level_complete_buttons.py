#!/usr/bin/env python3
"""Swap Play Again and Next Level button positions in level complete panels."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENES_DIR = ROOT / "Assets" / "Scenes"

BUTTON_NAMES = ("PlayAgain", "Nextlevel")
POSITIONS = {
    "PlayAgain": -120.0,
    "Nextlevel": -20.0,
}


def swap_positions(content: str) -> tuple[str, int]:
    changes = 0

    pattern = re.compile(
        r"(--- !u!1 &(\d+)\nGameObject:.*?m_Name: (PlayAgain|Nextlevel)\n"
        r".*?"
        r"--- !u!224 &(\d+)\nRectTransform:.*?m_AnchoredPosition: \{x: 0, y: )"
        r"(-?\d+(?:\.\d+)?)(\})",
        re.DOTALL,
    )

    def collect(match: re.Match[str]) -> str:
        name = match.group(3)
        target_y = POSITIONS[name]
        current_y = float(match.group(5))
        if abs(current_y - target_y) > 0.001:
            nonlocal changes
            changes += 1
            y_text = str(int(target_y) if target_y.is_integer() else target_y)
            return match.group(1) + y_text + match.group(6)
        return match.group(0)

    updated = pattern.sub(collect, content)
    return updated, changes


def swap_complete_card_children(content: str) -> tuple[str, int]:
    """Put Next Level above Play Again in hierarchy for consistent draw order."""
    pattern = re.compile(
        r"(m_Name: CompleteCard\n.*?m_Children:\n(?:  - \{fileID: \d+\}\n)*?)"
        r"  - \{fileID: (\d+)\}\n"
        r"  - \{fileID: (\d+)\}\n"
        r"(  - \{fileID: \d+\})",
        re.DOTALL,
    )

    button_ids = {
        "1993812057": "PlayAgain",
        "960246295": "Nextlevel",
        "105240444": "PlayAgain",
        "1403782554": "Nextlevel",
    }

    changes = 0

    def repl(match: re.Match[str]) -> str:
        nonlocal changes
        first_id = match.group(2)
        second_id = match.group(3)
        first_name = button_ids.get(first_id)
        second_name = button_ids.get(second_id)

        if first_name == "PlayAgain" and second_name == "Nextlevel":
            changes += 1
            return (
                f"{match.group(1)}"
                f"  - {{fileID: {second_id}}}\n"
                f"  - {{fileID: {first_id}}}\n"
                f"{match.group(4)}"
            )

        return match.group(0)

    return pattern.sub(repl, content), changes


def process_scene(path: Path) -> None:
    original = path.read_text(encoding="utf-8")
    updated, position_changes = swap_positions(original)
    updated, hierarchy_changes = swap_complete_card_children(updated)

    if updated != original:
        path.write_text(updated, encoding="utf-8")
        print(
            f"{path.name}: swapped {position_changes} button position(s), "
            f"{hierarchy_changes} hierarchy order(s)"
        )
    else:
        print(f"{path.name}: no changes needed")


def main() -> None:
    for scene_path in sorted(SCENES_DIR.glob("Level_*.unity")):
        process_scene(scene_path)


if __name__ == "__main__":
    main()
