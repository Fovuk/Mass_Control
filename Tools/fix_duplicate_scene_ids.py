#!/usr/bin/env python3
"""Fix duplicate Unity YAML file identifiers in scene files."""

from __future__ import annotations

import re
from collections import Counter
from pathlib import Path

SCENES_DIR = Path(__file__).resolve().parents[1] / "Assets" / "Scenes"


def fix_scene(path: Path) -> list[str]:
    content = path.read_text(encoding="utf-8")
    changes: list[str] = []

    while True:
        ids = re.findall(r"^--- !u!\d+ &(\d+)$", content, re.MULTILINE)
        counts = Counter(ids)
        duplicate_id = next((file_id for file_id, count in counts.items() if count > 1), None)
        if duplicate_id is None:
            break

        pattern = rf"--- !u!114 &{duplicate_id}\nMonoBehaviour:[\s\S]*?(?=--- !u!|\Z)"
        matches = list(re.finditer(pattern, content))
        if len(matches) < 2:
            break

        second = matches[1]
        block = second.group(0)
        go_match = re.search(r"m_GameObject: \{fileID: (\d+)\}", block)
        if not go_match:
            break

        go_id = go_match.group(1)
        used_ids = set(ids)
        new_id = max(int(value) for value in used_ids) + 1

        new_block = block.replace(f"--- !u!114 &{duplicate_id}", f"--- !u!114 &{new_id}", 1)
        content = content[: second.start()] + new_block + content[second.end() :]

        go_pattern = (
            rf"(--- !u!1 &{go_id}\nGameObject:[\s\S]*?m_Component:\n"
            rf"(?:  - component: {{fileID: \d+}}\n)+)"
        )
        go_match_obj = re.search(go_pattern, content)
        if go_match_obj:
            section = go_match_obj.group(1)
            updated = section.replace(f"{{fileID: {duplicate_id}}}", f"{{fileID: {new_id}}}", 1)
            content = content.replace(section, updated, 1)

        changes.append(f"{duplicate_id} -> {new_id} (GameObject {go_id})")

    if changes:
        path.write_text(content, encoding="utf-8")
    return changes


def main() -> None:
    for scene in sorted(SCENES_DIR.glob("*.unity")):
        changes = fix_scene(scene)
        if changes:
            print(f"{scene.name}: " + "; ".join(changes))
        else:
            dup_check = Counter(re.findall(r"^--- !u!\d+ &(\d+)$", scene.read_text(encoding="utf-8"), re.M))
            bad = [k for k, v in dup_check.items() if v > 1]
            print(f"{scene.name}: {'STILL HAS DUPS ' + str(bad) if bad else 'ok'}")


if __name__ == "__main__":
    main()
