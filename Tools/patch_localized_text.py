#!/usr/bin/env python3
"""Add LocalizedText components to UI TextMeshPro objects in Unity scenes."""

from __future__ import annotations

import re
import sys
from pathlib import Path

LOCALIZED_TEXT_GUID = "d9e5b3c2f4a6478901bcdef234567890"
TMP_IDENTIFIER = "Unity.TextMeshPro::TMPro.TextMeshProUGUI"
LOCALIZED_IDENTIFIER = "Assembly-CSharp::LocalizedText"

LEVEL_TEXT_TO_KEY = {
    "Bölüm 1": "level_1",
    "Bölüm 2": "level_2",
    "Bölüm 3": "level_3",
    "Bölüm 4": "level_4",
    "Bölüm 5": "level_5",
    "Bölüm 6": "level_6",
    "Bölüm 6\n": "level_6",
}

SKIP_TEXTS = {"TR", "EN", "X", "YILDIZ: 0"}

NAME_TO_KEY = {
    "Header": "game_title",
    "Title": None,  # resolved by parent
    "LanguageLabel": "language",
    "MusicLabel": "music",
    "SfxLabel": "sfx",
    "MorphButton": "morph",
    "JumpButton": "jump",
    "Nextlevel": "next_level",
    "PlayAgain": "play_again",
}

PARENT_CONTEXT_KEY = {
    ("SettingsPanel", "Title"): "settings",
    ("SettingsButton", "Text (TMP)"): "settings",
    ("CompleteCard", "Title"): "level_complete",
    ("CompleteCard", "MainMenu"): "main_menu",
    ("CompleteCard", "Nextlevel"): "next_level",
    ("CompleteCard", "PlayAgain"): "play_again",
    ("MorphButton", "Text (TMP)"): "morph",
    ("JumpButton", "Text (TMP)"): "jump",
    ("MainMenu", "Text (TMP)"): "main_menu",
    ("Nextlevel", "Text (TMP)"): "next_level",
    ("PlayAgain", "Text (TMP)"): "play_again",
}


def parse_blocks(content: str) -> list[tuple[str, str, dict[str, str]]]:
    blocks: list[tuple[str, str, dict[str, str]]] = []
    parts = re.split(r"(?=^--- !u!)", content, flags=re.MULTILINE)
    for part in parts:
        if not part.strip():
            continue
        header_match = re.match(r"^--- !u!(\d+) &(\d+)\n(\w+):", part)
        if not header_match:
            continue
        type_id, block_id, kind = header_match.groups()
        name_match = re.search(r"^\s+m_Name: (.+)$", part, re.MULTILINE)
        blocks.append(
            (
                type_id,
                block_id,
                {
                    "kind": kind,
                    "m_Name": name_match.group(1).strip() if name_match else "",
                    "_raw": part,
                },
            )
        )
    return blocks


def decode_unity_text(raw: str) -> str:
    raw = raw.strip()
    if raw.startswith('"') and raw.endswith('"'):
        try:
            return bytes(raw[1:-1], "utf-8").decode("unicode_escape")
        except UnicodeDecodeError:
            return raw[1:-1]
    return raw


def build_scene_maps(blocks: list[tuple[str, str, dict[str, str]]]):
    game_objects: dict[str, dict] = {}
    transforms: dict[str, dict] = {}
    components_by_go: dict[str, list[str]] = {}
    mono_behaviours: dict[str, dict] = {}

    for type_id, block_id, fields in blocks:
        kind = fields.get("kind")
        if kind == "GameObject":
            name = fields.get("m_Name", "")
            component_ids = re.findall(r"component: \{fileID: (\d+)\}", fields["_raw"])
            game_objects[block_id] = {"name": name, "components": component_ids}
            components_by_go[block_id] = component_ids
        elif kind == "RectTransform":
            father_match = re.search(r"m_Father: \{fileID: (\d+)\}", fields["_raw"])
            go_match = re.search(r"m_GameObject: \{fileID: (\d+)\}", fields["_raw"])
            if go_match:
                transforms[block_id] = {
                    "game_object": go_match.group(1),
                    "father": father_match.group(1) if father_match else "0",
                }
        elif kind == "MonoBehaviour":
            go_match = re.search(r"m_GameObject: \{fileID: (\d+)\}", fields["_raw"])
            identifier_match = re.search(r"m_EditorClassIdentifier: (.+)", fields["_raw"])
            text_match = re.search(r"m_text: (.+)", fields["_raw"])
            key_match = re.search(r"localizationKey: (.+)", fields["_raw"])
            mono_behaviours[block_id] = {
                "game_object": go_match.group(1) if go_match else None,
                "identifier": identifier_match.group(1).strip() if identifier_match else "",
                "text": decode_unity_text(text_match.group(1)) if text_match else "",
                "localization_key": key_match.group(1).strip() if key_match else "",
                "raw": fields["_raw"],
            }

    transform_by_go = {}
    for transform_id, data in transforms.items():
        transform_by_go[data["game_object"]] = transform_id

    father_by_go = {}
    for go_id, transform_id in transform_by_go.items():
        father_transform = transforms.get(transform_id, {}).get("father", "0")
        if father_transform == "0":
            father_by_go[go_id] = None
        else:
            father_by_go[go_id] = transforms.get(father_transform, {}).get("game_object")

    return game_objects, mono_behaviours, father_by_go, components_by_go


def get_path(go_id: str, game_objects: dict, father_by_go: dict) -> list[str]:
    path: list[str] = []
    current = go_id
    while current and current in game_objects:
        path.append(game_objects[current]["name"])
        current = father_by_go.get(current)
    return list(reversed(path))


def resolve_key(go_id: str, tmp_block: dict, game_objects: dict, father_by_go: dict) -> str | None:
    path = get_path(go_id, game_objects, father_by_go)
    name = game_objects[go_id]["name"]
    text = tmp_block["text"]

    if text in SKIP_TEXTS:
        return None

    if text in LEVEL_TEXT_TO_KEY:
        return LEVEL_TEXT_TO_KEY[text]

    if len(path) >= 2:
        context = (path[-2], path[-1])
        if context in PARENT_CONTEXT_KEY:
            return PARENT_CONTEXT_KEY[context]

    if name in NAME_TO_KEY and NAME_TO_KEY[name]:
        return NAME_TO_KEY[name]

    if name == "Title" and "CompleteCard" in path:
        return "level_complete"
    if name == "Title" and "SettingsPanel" in path:
        return "settings"

    static_map = {
        "Ayarlar": "settings",
        "Dil": "language",
        "Müzik": "music",
        "SFX": "sfx",
        "Mass Control": "game_title",
        "Bölüm Tamamlandı!": "level_complete",
        "Sıradaki Bölüm": "next_level",
        "Ana Menü": "main_menu",
        "Tekrar Oyna": "play_again",
        "Boyut\nDeğiştir": "morph",
        "Boyut\nDeğiştir\n": "morph",
        "Zıplama": "jump",
    }
    if text in static_map:
        return static_map[text]

    # Dynamic summary text under level complete card
    if "CompleteCard" in path and name == "Text (TMP)" and "Yıldız" in text:
        return None

    return None


def has_localized_text(go_id: str, mono_behaviours: dict, components_by_go: dict) -> bool:
    for component_id in components_by_go.get(go_id, []):
        mono = mono_behaviours.get(component_id)
        if mono and LOCALIZED_IDENTIFIER in mono["identifier"]:
            return True
    return False


def patch_scene(path: Path) -> int:
    content = path.read_text(encoding="utf-8")
    blocks = parse_blocks(content)
    game_objects, mono_behaviours, father_by_go, components_by_go = build_scene_maps(blocks)

    additions: list[str] = []
    game_object_patches: dict[str, str] = {}
    next_id = 9200000000

    for block_id, mono in mono_behaviours.items():
        if TMP_IDENTIFIER not in mono["identifier"]:
            continue
        go_id = mono["game_object"]
        if not go_id or go_id not in game_objects:
            continue
        if has_localized_text(go_id, mono_behaviours, components_by_go):
            continue

        key = resolve_key(go_id, mono, game_objects, father_by_go)
        if not key:
            continue

        next_id += 1
        loc_id = str(next_id)
        block = (
            f"--- !u!114 &{loc_id}\n"
            "MonoBehaviour:\n"
            "  m_ObjectHideFlags: 0\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            f"  m_GameObject: {{fileID: {go_id}}}\n"
            "  m_Enabled: 1\n"
            "  m_EditorHideFlags: 0\n"
            f"  m_Script: {{fileID: 11500000, guid: {LOCALIZED_TEXT_GUID}, type: 3}}\n"
            "  m_Name: \n"
            "  m_EditorClassIdentifier: Assembly-CSharp::LocalizedText\n"
            f"  localizationKey: {key}\n"
        )
        additions.append(block)
        game_object_patches[go_id] = loc_id

    if not additions:
        return 0

    updated = content
    for go_id, loc_id in game_object_patches.items():
        pattern = (
            rf"(--- !u!1 &{go_id}\nGameObject:[\s\S]*?m_Component:\n"
            rf"(?:  - component: {{fileID: \d+}}\n)+)"
        )
        match = re.search(pattern, updated)
        if not match:
            print(f"WARNING: Could not patch GameObject {go_id} in {path.name}")
            continue
        insert = f"  - component: {{fileID: {loc_id}}}\n"
        replacement = match.group(1) + insert
        updated = updated.replace(match.group(1), replacement, 1)

    updated = updated.rstrip() + "\n" + "".join(additions)
    path.write_text(updated, encoding="utf-8")
    return len(additions)


def main() -> int:
    root = Path(__file__).resolve().parents[1]
    scenes = sorted((root / "Assets" / "Scenes").glob("*.unity"))
    total = 0
    for scene in scenes:
        count = patch_scene(scene)
        print(f"{scene.name}: added {count} LocalizedText component(s)")
        total += count
    print(f"Total added: {total}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
