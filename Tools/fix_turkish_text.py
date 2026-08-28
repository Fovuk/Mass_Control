#!/usr/bin/env python3
"""Fix Turkish text rendering in Unity TMP scene files."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENES_DIR = ROOT / "Assets" / "Scenes"

HOGFISH_GUID = "0bfb1f34031e05b4b99aceac64b99e36"
LIBERATION_GUID = "8f586378b4e144a9851e7b34d9b748ee"
UPPERCASE_BIT = 16


def fix_font_style(content: str) -> tuple[str, int]:
    changes = 0

    def repl(match: re.Match[str]) -> str:
        nonlocal changes
        value = int(match.group(1))
        new_value = value & ~UPPERCASE_BIT
        if new_value != value:
            changes += 1
        return f"m_fontStyle: {new_value}"

    return re.sub(r"m_fontStyle: (\d+)", repl, content), changes


def replace_hogfish_with_liberation(content: str) -> tuple[str, int]:
    count = content.count(HOGFISH_GUID)
    if count:
        content = content.replace(HOGFISH_GUID, LIBERATION_GUID)
    return content, count


def process_scene(path: Path) -> None:
    original = path.read_text(encoding="utf-8")
    updated, style_changes = fix_font_style(original)
    updated, font_changes = replace_hogfish_with_liberation(updated)

    if updated != original:
        path.write_text(updated, encoding="utf-8")
        print(
            f"{path.name}: removed Uppercase from {style_changes} text(s), "
            f"replaced Hogfish in {font_changes // 2} component(s)"
        )


def main() -> None:
    for scene_path in sorted(SCENES_DIR.glob("*.unity")):
        process_scene(scene_path)


if __name__ == "__main__":
    main()
