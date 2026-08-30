"""Align mobile UI (joystick + jump/morph buttons) with Level_01 design."""
from __future__ import annotations

import re
from pathlib import Path

SCENES_DIR = Path(r"d:\ilk-oyunumm\Assets\Scenes")

ONSCREEN_STICK_BLOCK = re.compile(
    r"--- !u!114 &434504012\n"
    r"MonoBehaviour:\n"
    r"  m_ObjectHideFlags: 0\n"
    r"  m_CorrespondingSourceObject: \{fileID: 0\}\n"
    r"  m_PrefabInstance: \{fileID: 0\}\n"
    r"  m_PrefabAsset: \{fileID: 0\}\n"
    r"  m_GameObject: \{fileID: 434504010\}\n"
    r"  m_Enabled: 1\n"
    r"  m_EditorHideFlags: 0\n"
    r"  m_Script: \{fileID: 11500000, guid: e9d677f1681015749b15c436eec6d880, type: 3\}\n"
    r"  m_Name: \n"
    r"  m_EditorClassIdentifier: Unity\.InputSystem::UnityEngine\.InputSystem\.OnScreen\.OnScreenStick\n"
    r"  m_MovementRange: 50\n"
    r"  m_DynamicOriginRange: 100\n"
    r"  m_ControlPath: <Gamepad>/leftStick\n"
    r"  m_Behaviour: 0\n"
    r"  m_UseIsolatedInputActions: 0\n"
    r"  m_PointerDownAction:\n"
    r"    m_Name: \n"
    r"    m_Type: 0\n"
    r"    m_ExpectedControlType: \n"
    r"    m_Id: 2d7f3ea6-85e9-46a4-822d-f89a00e90c5f\n"
    r"    m_Processors: \n"
    r"    m_Interactions: \n"
    r"    m_SingletonActionBindings: \[\]\n"
    r"    m_Flags: 0\n"
    r"  m_PointerMoveAction:\n"
    r"    m_Name: \n"
    r"    m_Type: 0\n"
    r"    m_ExpectedControlType: \n"
    r"    m_Id: 74af2b0d-f297-4c75-8a99-10e5afae95ac\n"
    r"    m_Processors: \n"
    r"    m_Interactions: \n"
    r"    m_SingletonActionBindings: \[\]\n"
    r"    m_Flags: 0\n",
    re.MULTILINE,
)

VIRTUAL_JOYSTICK_BLOCK = """--- !u!114 &434504012
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 434504010}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: c7d9e1f2a3b44c5d8e9f0a1b2c3d4e5f, type: 3}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::VirtualJoystick
  controlPath: <Gamepad>/leftStick
  handle: {fileID: 1925370530}
  movementRange: 50
"""

SPRITE_JOYSTICK_BG = (
    "  m_Sprite: {fileID: 3441002012680869628, guid: 6027409d8d3bb0749802c16f74a94a58, type: 3}"
)
SPRITE_JOYSTICK_HANDLE = (
    "  m_Sprite: {fileID: -3161942850466663788, guid: 48af46230d6d596418ba1cefd02a50d5, type: 3}"
)
SPRITE_BUTTON = (
    "  m_Sprite: {fileID: 3508328273764669133, guid: ff228bd7ded53c84f892d4ae9250d2b2, type: 3}"
)
DEFAULT_SPRITE = (
    "  m_Sprite: {fileID: 10913, guid: 0000000000000000f000000000000000, type: 0}"
)

COMPONENT_SPRITES = {
    "434504013": SPRITE_JOYSTICK_BG,
    "1925370531": SPRITE_JOYSTICK_HANDLE,
    "1067092661": SPRITE_BUTTON,
    "590908834": SPRITE_BUTTON,
}


def replace_component_sprite(text: str, component_id: str, sprite_line: str) -> str:
    pattern = re.compile(
        rf"(--- !u!114 &{component_id}\nMonoBehaviour:.*?m_OnCullStateChanged:\n"
        rf"    m_PersistentCalls:\n"
        rf"      m_Calls: \[\]\n)"
        rf"  m_Sprite: .*?\n",
        re.DOTALL,
    )
    return pattern.sub(rf"\1{sprite_line}\n", text, count=1)


def fix_scene(path: Path) -> bool:
    text = path.read_text(encoding="utf-8")
    original = text

    if "m_Name: JoystickBackground" not in text:
        print(f"  skip (no mobile UI): {path.name}")
        return False

    if ONSCREEN_STICK_BLOCK.search(text):
        text = ONSCREEN_STICK_BLOCK.sub(VIRTUAL_JOYSTICK_BLOCK, text, count=1)
    elif "Assembly-CSharp::VirtualJoystick" in text:
        print(f"  already fixed joystick: {path.name}")
    else:
        print(f"  warning: OnScreenStick block not found in {path.name}")

    for component_id, sprite_line in COMPONENT_SPRITES.items():
        text = replace_component_sprite(text, component_id, sprite_line)

    text = text.replace(
        "  m_GameObject: {fileID: 1925370529}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
        "  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n"
        "  m_Name: \n"
        "  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n"
        "  m_Material: {fileID: 0}\n"
        "  m_Color: {r: 1, g: 0.55660367, b: 0.95922595, a: 1}",
        "  m_GameObject: {fileID: 1925370529}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n"
        "  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n"
        "  m_Name: \n"
        "  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image\n"
        "  m_Material: {fileID: 0}\n"
        "  m_Color: {r: 1, g: 1, b: 1, a: 1}",
    )

    if text == original:
        print(f"  no changes: {path.name}")
        return False

    out = path.with_suffix(".unity.new")
    out.write_text(text, encoding="utf-8")
    out.replace(path)
    print(f"  updated: {path.name}")
    return True


def main() -> None:
    updated = 0
    for level in range(2, 7):
        scene = SCENES_DIR / f"Level_{level:02d}.unity"
        if not scene.exists():
            continue
        print(scene.name)
        if fix_scene(scene):
            updated += 1
    print(f"Done. Updated {updated} scene(s).")


if __name__ == "__main__":
    main()
