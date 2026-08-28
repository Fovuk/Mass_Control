#!/usr/bin/env python3
"""Add QuitButton and QuitConfirmPopup to MainMenu.unity."""

from __future__ import annotations

from pathlib import Path

SCENE = Path(__file__).resolve().parents[1] / "Assets" / "Scenes" / "MainMenu.unity"

BUTTON_SPRITE = "{fileID: -5518990023937178466, guid: 3d8cc8dcd10411b4dac6a55be0fbca75, type: 3}"
PANEL_SPRITE = "{fileID: -5518990023937178466, guid: c82c93d99bf781e49877a2f13e180daa, type: 3}"
OVERLAY_SPRITE = "{fileID: 10907, guid: 0000000000000000f000000000000000, type: 0}"
FONT_BODY = "{fileID: 11400000, guid: 2e498d1c8094910479dc3e1b768306a4, type: 2}"
FONT_TITLE = "{fileID: 11400000, guid: 0bfb1f34031e05b4b99aceac64b99e36, type: 2}"
MAT_BODY = "{fileID: 2180264, guid: 2e498d1c8094910479dc3e1b768306a4, type: 2}"
MAT_TITLE = "{fileID: 8292986130934440167, guid: 0bfb1f34031e05b4b99aceac64b99e36, type: 2}"

GUID_BUTTON = "4e29b1a8efbd4b44bb3f3716e73f07ff"
GUID_IMAGE = "fe87c0e1cc204ed48ad3b37840f39efc"
GUID_TMP = "f4688fdb7df04437aeb418b961361dc5"
GUID_LOCALIZED = "d9e5b3c2f4a6478901bcdef234567890"
GUID_QUIT_UI = "e1f2a3b4c5d6478990abcdef98765432"

PANEL_RT = "168052136"
CANVAS_RT = "1062219228"


def tmp_block(comp_id: str, go_id: str, text: str, font_asset: str, mat: str, size: int, bold: bool = False) -> str:
    style = 17 if bold else 1
    color = "4294425911" if font_asset == FONT_TITLE else "4294967295"
    rgb = "{r: 0.21568629, g: 0.7411765, b: 0.9686275, a: 1}" if font_asset == FONT_TITLE else "{r: 1, g: 1, b: 1, a: 1}"
    return f"""--- !u!114 &{comp_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_TMP}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Unity.TextMeshPro::TMPro.TextMeshProUGUI
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_text: {text}
  m_isRightToLeft: 0
  m_fontAsset: {font_asset}
  m_sharedMaterial: {mat}
  m_fontSharedMaterials: []
  m_fontMaterial: {{fileID: 0}}
  m_fontMaterials: []
  m_fontColor32:
    serializedVersion: 2
    rgba: {color}
  m_fontColor: {rgb}
  m_enableVertexGradient: 0
  m_colorMode: 3
  m_fontColorGradient:
    topLeft: {{r: 1, g: 1, b: 1, a: 1}}
    topRight: {{r: 1, g: 1, b: 1, a: 1}}
    bottomLeft: {{r: 1, g: 1, b: 1, a: 1}}
    bottomRight: {{r: 1, g: 1, b: 1, a: 1}}
  m_fontColorGradientPreset: {{fileID: 0}}
  m_spriteAsset: {{fileID: 0}}
  m_tintAllSprites: 0
  m_StyleSheet: {{fileID: 0}}
  m_TextStyleHashCode: -1183493901
  m_overrideHtmlColors: 0
  m_faceColor:
    serializedVersion: 2
    rgba: 4294967295
  m_fontSize: {size}
  m_fontSizeBase: {size}
  m_fontWeight: 400
  m_enableAutoSizing: 0
  m_fontSizeMin: 18
  m_fontSizeMax: 72
  m_fontStyle: {style}
  m_HorizontalAlignment: 2
  m_VerticalAlignment: 512
  m_textAlignment: 65535
  m_characterSpacing: 0
  m_characterHorizontalScale: 1
  m_wordSpacing: 0
  m_lineSpacing: 0
  m_lineSpacingMax: 0
  m_paragraphSpacing: 0
  m_charWidthMaxAdj: 0
  m_TextWrappingMode: 1
  m_wordWrappingRatios: 0.4
  m_overflowMode: 0
  m_linkedTextComponent: {{fileID: 0}}
  parentLinkedComponent: {{fileID: 0}}
  m_enableKerning: 0
  m_ActiveFontFeatures: 6e72656b
  m_enableExtraPadding: 0
  checkPaddingRequired: 0
  m_isRichText: 1
  m_EmojiFallbackSupport: 1
  m_parseCtrlCharacters: 1
  m_isOrthographic: 1
  m_isCullingEnabled: 0
  m_horizontalMapping: 0
  m_verticalMapping: 0
  m_uvLineOffset: 0
  m_geometrySortingOrder: 0
  m_IsTextObjectScaleStatic: 0
  m_VertexBufferAutoSizeReduction: 0
  m_useMaxVisibleDescender: 1
  m_pageToDisplay: 1
  m_margin: {{x: 0, y: 0, z: 0, w: 0}}
  m_isUsingLegacyAnimationComponent: 0
  m_isVolumetricText: 0
  m_hasFontAssetChanged: 0
  m_baseMaterial: {{fileID: 0}}
  m_maskOffset: {{x: 0, y: 0, z: 0, w: 0}}
"""


def localized_block(comp_id: str, go_id: str, key: str) -> str:
    return f"""--- !u!114 &{comp_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_LOCALIZED}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::LocalizedText
  localizationKey: {key}
"""


def button_onclick_set_active(target_go: str, active: bool) -> str:
    return f"""  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: {target_go}}}
        m_TargetAssemblyTypeName: UnityEngine.GameObject, UnityEngine
        m_MethodName: SetActive
        m_Mode: 6
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: {1 if active else 0}
        m_CallState: 2"""


def button_onclick_method(target_comp: str, method: str) -> str:
    return f"""  m_OnClick:
    m_PersistentCalls:
      m_Calls:
      - m_Target: {{fileID: {target_comp}}}
        m_TargetAssemblyTypeName: QuitGameUI, Assembly-CSharp
        m_MethodName: {method}
        m_Mode: 1
        m_Arguments:
          m_ObjectArgument: {{fileID: 0}}
          m_ObjectArgumentAssemblyTypeName: UnityEngine.Object, UnityEngine
          m_IntArgument: 0
          m_FloatArgument: 0
          m_StringArgument: 
          m_BoolArgument: 0
        m_CallState: 2"""


def image_block(comp_id: str, go_id: str, sprite: str, sliced: bool = True) -> str:
    img_type = 1 if sliced else 0
    multiplier = 0.7 if sprite == PANEL_SPRITE else 1
    return f"""--- !u!114 &{comp_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {sprite}
  m_Type: {img_type}
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: {multiplier}
"""


def button_block(comp_id: str, go_id: str, image_comp: str, onclick_yaml: str) -> str:
    return f"""--- !u!114 &{comp_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_BUTTON}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Button
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {image_comp}}}
{onclick_yaml}
"""


def go_block(go_id: str, name: str, components: list[str], active: int = 1) -> str:
    comp_lines = "\n".join(f"  - component: {{fileID: {c}}}" for c in components)
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
{comp_lines}
  m_Layer: 5
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: {active}
"""


def rt_block(rt_id: str, go_id: str, father: str, anchor_min, anchor_max, pos, size, pivot, children=None) -> str:
    children = children or []
    child_lines = "\n".join(f"  - {{fileID: {c}}}" for c in children)
    children_section = f"  m_Children:\n{child_lines}" if children else "  m_Children: []"
    return f"""--- !u!224 &{rt_id}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
{children_section}
  m_Father: {{fileID: {father}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: {anchor_min[0]}, y: {anchor_min[1]}}}
  m_AnchorMax: {{x: {anchor_max[0]}, y: {anchor_max[1]}}}
  m_AnchoredPosition: {{x: {pos[0]}, y: {pos[1]}}}
  m_SizeDelta: {{x: {size[0]}, y: {size[1]}}}
  m_Pivot: {{x: {pivot[0]}, y: {pivot[1]}}}
"""


def cr_block(cr_id: str, go_id: str) -> str:
    return f"""--- !u!222 &{cr_id}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_CullTransparentMesh: 1
"""


def labeled_text(name: str, ids: dict[str, str], father_rt: str, layout: dict, key: str, text: str, font_asset: str, mat: str, size: int, bold: bool) -> list[str]:
    return [
        go_block(ids["go"], name, [ids["rt"], ids["cr"], ids["tmp"], ids["loc"]]),
        rt_block(ids["rt"], ids["go"], father_rt, layout["anchor_min"], layout["anchor_max"], layout["pos"], layout["size"], layout["pivot"]),
        cr_block(ids["cr"], ids["go"]),
        tmp_block(ids["tmp"], ids["go"], text, font_asset, mat, size, bold),
        localized_block(ids["loc"], ids["go"], key),
    ]


def action_button(name: str, ids: dict[str, str], text_ids: dict[str, str], father_rt: str, pos, size, key: str, text: str, onclick_yaml: str) -> list[str]:
    blocks = [
        go_block(ids["go"], name, [ids["rt"], ids["cr"], ids["img"], ids["btn"]]),
        rt_block(ids["rt"], ids["go"], father_rt, (0.5, 0.5), (0.5, 0.5), pos, size, (0.5, 0.5), [text_ids["rt"]]),
        cr_block(ids["cr"], ids["go"]),
        image_block(ids["img"], ids["go"], BUTTON_SPRITE),
        button_block(ids["btn"], ids["go"], ids["img"], onclick_yaml),
        go_block(text_ids["go"], "Text (TMP)", [text_ids["rt"], text_ids["cr"], text_ids["tmp"], text_ids["loc"]]),
        rt_block(text_ids["rt"], text_ids["go"], ids["rt"], (0, 0), (1, 1), (0, 4), (0, -8), (0.5, 0.5)),
        cr_block(text_ids["cr"], text_ids["go"]),
        tmp_block(text_ids["tmp"], text_ids["go"], text, FONT_BODY, MAT_BODY, 32, True),
        localized_block(text_ids["loc"], text_ids["go"], key),
    ]
    return blocks


def build_yaml() -> str:
    parts: list[str] = []

    parts += [
        go_block("3200000100", "QuitButton", ["3200000101", "3200000102", "3200000103", "3200000104"]),
        rt_block("3200000101", "3200000100", PANEL_RT, (1, 1), (1, 1), (-36, -36), (240, 80), (1, 1), ["3200000111"]),
        cr_block("3200000102", "3200000100"),
        image_block("3200000103", "3200000100", BUTTON_SPRITE),
        button_block("3200000104", "3200000100", "3200000103", button_onclick_set_active("3200000200", True)),
        go_block("3200000110", "Text (TMP)", ["3200000111", "3200000112", "3200000113", "3200000114"]),
        rt_block("3200000111", "3200000110", "3200000101", (0, 0), (1, 1), (0, 4), (0, -8), (0.5, 0.5)),
        cr_block("3200000112", "3200000110"),
        tmp_block("3200000113", "3200000110", "Çıkış", FONT_BODY, MAT_BODY, 32, True),
        localized_block("3200000114", "3200000110", "quit_game"),
    ]

    parts += [
        go_block("3200000200", "QuitConfirmPopup", ["3200000201", "3200000202"], active=0),
        rt_block("3200000201", "3200000200", CANVAS_RT, (0, 0), (1, 1), (0, 0), (0, 0), (0.5, 0.5), ["3200000211", "3200000221"]),
        f"""--- !u!114 &3200000202
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 3200000200}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_QUIT_UI}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::QuitGameUI
  cancelButton: {{fileID: 3200000244}}
  confirmButton: {{fileID: 3200000254}}
""",
        go_block("3200000210", "Overlay", ["3200000211", "3200000212", "3200000213"]),
        rt_block("3200000211", "3200000210", "3200000201", (0, 0), (1, 1), (0, 0), (0, 0), (0.5, 0.5)),
        cr_block("3200000212", "3200000210"),
        f"""--- !u!114 &3200000213
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 3200000210}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {GUID_IMAGE}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: UnityEngine.UI::UnityEngine.UI.Image
  m_Material: {{fileID: 0}}
  m_Color: {{r: 0, g: 0, b: 0, a: 0.55}}
  m_RaycastTarget: 0
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {OVERLAY_SPRITE}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
""",
        go_block("3200000220", "ConfirmPanel", ["3200000221", "3200000222", "3200000223"]),
        rt_block(
            "3200000221",
            "3200000220",
            "3200000201",
            (0.5, 0.5),
            (0.5, 0.5),
            (0, 0),
            (680, 380),
            (0.5, 0.5),
            ["3200000231", "3200000236", "3200000241", "3200000251"],
        ),
        cr_block("3200000222", "3200000220"),
        image_block("3200000223", "3200000220", PANEL_SPRITE),
    ]

    parts += labeled_text(
        "Title",
        {"go": "3200000230", "rt": "3200000231", "cr": "3200000232", "tmp": "3200000233", "loc": "3200000234"},
        "3200000221",
        {"anchor_min": (0.5, 1), "anchor_max": (0.5, 1), "pos": (0, -80), "size": (620, 80), "pivot": (0.5, 0.5)},
        "quit_confirm_title",
        "Emin misin?",
        FONT_TITLE,
        MAT_TITLE,
        56,
        True,
    )
    parts += labeled_text(
        "Message",
        {"go": "3200000235", "rt": "3200000236", "cr": "3200000237", "tmp": "3200000238", "loc": "3200000239"},
        "3200000221",
        {"anchor_min": (0.5, 0.5), "anchor_max": (0.5, 0.5), "pos": (0, 20), "size": (620, 60), "pivot": (0.5, 0.5)},
        "quit_confirm_message",
        "Oyun kapatılacak.",
        FONT_BODY,
        MAT_BODY,
        30,
        False,
    )
    parts += action_button(
        "CancelButton",
        {"go": "3200000240", "rt": "3200000241", "cr": "3200000242", "img": "3200000243", "btn": "3200000244"},
        {"go": "3200000245", "rt": "3200000246", "cr": "3200000247", "tmp": "3200000248", "loc": "3200000249"},
        "3200000221",
        (-130, -120),
        (220, 80),
        "no",
        "Hayır",
        button_onclick_method("3200000202", "HidePopup"),
    )
    parts += action_button(
        "ConfirmButton",
        {"go": "3200000250", "rt": "3200000251", "cr": "3200000252", "img": "3200000253", "btn": "3200000254"},
        {"go": "3200000255", "rt": "3200000256", "cr": "3200000257", "tmp": "3200000258", "loc": "3200000259"},
        "3200000221",
        (130, -120),
        (220, 80),
        "yes",
        "Evet",
        button_onclick_method("3200000202", "ConfirmQuit"),
    )

    return "\n".join(parts) + "\n"


def patch_scene() -> None:
    content = SCENE.read_text(encoding="utf-8")
    if "QuitButton" in content:
        print("QuitButton already exists, skipping.")
        return

    yaml_chunk = build_yaml()
    content = content.replace(
        "  m_Children:\n  - {fileID: 2035927482}\n  - {fileID: 788512488}\n  - {fileID: 3100000101}\n  - {fileID: 3200000101}",
        "  m_Children:\n  - {fileID: 2035927482}\n  - {fileID: 788512488}\n  - {fileID: 3100000101}\n  - {fileID: 3200000101}",
        1,
    )
    content = content.replace(
        "  m_Children:\n  - {fileID: 168052136}\n  - {fileID: 3100000201}",
        "  m_Children:\n  - {fileID: 168052136}\n  - {fileID: 3100000201}\n  - {fileID: 3200000201}",
        1,
    )
    marker = "--- !u!1660057539 &9223372036854775807"
    content = content.replace(marker, yaml_chunk + marker, 1)
    SCENE.write_text(content, encoding="utf-8")
    print("Added QuitButton and QuitConfirmPopup to MainMenu.unity")


if __name__ == "__main__":
    patch_scene()
