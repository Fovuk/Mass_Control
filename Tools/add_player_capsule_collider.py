#!/usr/bin/env python3
"""Add CapsuleCollider2D to Player and disable BoxCollider2D in level scenes."""

from __future__ import annotations

import re
from pathlib import Path

SCENES_DIR = Path(__file__).resolve().parents[1] / "Assets" / "Scenes"
WIDTH_SCALE = 0.82
HEIGHT_SCALE = 0.96


def capsule_block(component_id: int, game_object_id: str, offset_x: str, offset_y: str, size_x: float, size_y: float) -> str:
    return f"""--- !u!70 &{component_id}
CapsuleCollider2D:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {game_object_id}}}
  m_Enabled: 1
  serializedVersion: 2
  m_Density: 1
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_ForceSendLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ForceReceiveLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_ContactCaptureLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_CallbackLayers:
    serializedVersion: 2
    m_Bits: 4294967295
  m_IsTrigger: 0
  m_UsedByEffector: 0
  m_CompositeOperation: 0
  m_CompositeOrder: 0
  m_Offset: {{x: {offset_x}, y: {offset_y}}}
  m_Size: {{x: {size_x}, y: {size_y}}}
  m_Direction: 0
"""


def next_component_id(content: str) -> int:
    ids = [int(value) for value in re.findall(r"^--- !u!\d+ &(\d+)$", content, re.MULTILINE)]
    sane_ids = [value for value in ids if value < 2_000_000_000]
    return max(sane_ids, default=1_000_000) + 1


def find_box_collider_block(content: str, game_object_id: str) -> re.Match[str] | None:
    line = r"(?:\n(?!--- !u!)[^\n]*)"
    pattern = (
        rf"--- !u!61 &(\d+)\nBoxCollider2D:"
        rf"{line}*?"
        rf"m_GameObject: {{fileID: {game_object_id}}}"
        rf"{line}*"
    )
    return re.search(pattern, content)


def find_player_object_ids(content: str) -> list[str]:
    ids: list[str] = []
    for tag_match in re.finditer(r"m_Name: Player\n  m_TagString: Player", content):
        before = content[: tag_match.start()]
        go_ids = re.findall(r"--- !u!1 &(\d+)\nGameObject:", before)
        if go_ids:
            ids.append(go_ids[-1])
    return ids


def patch_scene(path: Path) -> list[str]:
    content = path.read_text(encoding="utf-8")
    changes: list[str] = []

    for go_id in find_player_object_ids(content):

        existing_capsule = re.search(
            rf"--- !u!70 &\d+\nCapsuleCollider2D:[\s\S]*?m_GameObject: {{fileID: {go_id}}}",
            content,
        )
        if existing_capsule:
            changes.append(f"Player {go_id}: capsule already present")
            continue

        box_match = find_box_collider_block(content, go_id)
        if not box_match:
            changes.append(f"Player {go_id}: BoxCollider2D not found")
            continue

        box_id = box_match.group(1)
        box_block = box_match.group(0)
        size_match = re.search(r"m_Size: \{x: ([^,]+), y: ([^}]+)\}", box_block)
        offset_match = re.search(r"m_Offset: \{x: ([^,]+), y: ([^}]+)\}", box_block)
        if not size_match or not offset_match:
            changes.append(f"Player {go_id}: could not parse BoxCollider2D")
            continue

        box_size_x = float(size_match.group(1))
        box_size_y = float(size_match.group(2))
        offset_x = offset_match.group(1)
        offset_y = offset_match.group(2)

        disabled_box = re.sub(r"(--- !u!61 &" + box_id + r"\nBoxCollider2D:[\s\S]*?m_Enabled: )1", r"\g<1>0", box_block, count=1)
        content = content.replace(box_block, disabled_box, 1)

        new_id = next_component_id(content)
        block = capsule_block(
            new_id,
            go_id,
            offset_x,
            offset_y,
            round(box_size_x * WIDTH_SCALE, 6),
            round(box_size_y * HEIGHT_SCALE, 6),
        )

        insert_at = content.find(disabled_box) + len(disabled_box)
        if insert_at < len(content) and content[insert_at] != "\n":
            block = "\n" + block
        content = content[:insert_at] + block + content[insert_at:]

        go_header_pattern = (
            rf"(--- !u!1 &{go_id}\nGameObject:[\s\S]*?m_Component:\n"
            rf"(?:  - component: {{fileID: \d+}}\n)+)"
        )
        go_header_match = re.search(go_header_pattern, content)
        if not go_header_match:
            changes.append(f"Player {go_id}: failed to update component list")
            continue

        section = go_header_match.group(1)
        updated_section = section.rstrip("\n") + f"\n  - component: {{fileID: {new_id}}}\n"
        content = content.replace(section, updated_section, 1)
        changes.append(f"Player {go_id}: added CapsuleCollider2D {new_id}, disabled BoxCollider2D {box_id}")

    if changes:
        path.write_text(content, encoding="utf-8")
    return changes


def main() -> None:
    for scene in sorted(SCENES_DIR.glob("Level_*.unity")):
        changes = patch_scene(scene)
        if changes:
            print(f"{scene.name}:")
            for change in changes:
                print(f"  - {change}")
        else:
            print(f"{scene.name}: no Player object found")


if __name__ == "__main__":
    main()
