"""Inject ParallaxBackground hierarchy into Level_05.unity (Clouds 5 set)."""
from __future__ import annotations

from pathlib import Path

SCENE = Path(r"d:\ilk-oyunumm\Assets\Scenes\Level_05.unity")

LAYERS = [
    {
        "name": "1_0",
        "sprite": (-855298285791758126, "fa90d3f117575dd4d949d74636385bbd"),
        "size": (5.76, 3.24),
        "pos": (-0.0272, 0.017094, 28.6),
        "scale": (6.9678254, 6.0926, 1),
        "factor": 0.85,
    },
    {
        "name": "2_0",
        "sprite": (6937817954938267909, "788e830d509054f40909f338a1383c06"),
        "size": (5.76, 3.24),
        "pos": (0.025, 0.10391, 3.6),
        "scale": (6.968187, 6.11, 1),
        "factor": 0.55,
    },
    {
        "name": "3_0",
        "sprite": (-6470460504253458178, "9093e3b106474f24ab449f6c36fbaf19"),
        "size": (0.95, 0.24),
        "pos": (0.025003, -0.02109, 74.99),
        "scale": (12.435164, 83.29153, 1),
        "factor": 0.92,
    },
    {
        "name": "4_0",
        "sprite": (-4178046375966092550, "42e3cb90047a6a34b9d8d23072b9944f"),
        "size": (5.76, 3.11),
        "pos": (-0.03657, 0.096817, 2),
        "scale": (6.9682956, 6.32, 1),
        "factor": 0.18,
    },
]

ROOT_POS = (-10.77, 36.99, 0)
ROOT_ID = 880005002
ROOT_GO = 880005001
ROOT_SCRIPT = 880005009

LIT_MAT = "{fileID: 2100000, guid: a97c105638bdf8b4a8650670310a4cd3, type: 2}"
PARALLAX_BG_GUID = "f3a8b2c1d4e54f6a9b0c1d2e3f4a5b6c"
PARALLAX_LAYER_GUID = "e8f4a2b1c3d54e6f9a0b1c2d3e4f5a6b"


def sprite_line(file_id: int, guid: str) -> str:
    return f"m_Sprite: {{fileID: {file_id}, guid: {guid}, type: 3}}"


def layer_block(index: int, layer: dict) -> str:
    base = 880005100 + index * 100
    go_id = base + 1
    tr_id = base + 2
    sr_id = base + 3
    mb_id = base + 4
    sx, sy = layer["size"]
    px, py, pz = layer["pos"]
    scx, scy, scz = layer["scale"]
    sp_id, sp_guid = layer["sprite"]

    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {mb_id}}}
  - component: {{fileID: {sr_id}}}
  m_Layer: 0
  m_Name: {layer["name"]}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: -0, y: -0, z: -0, w: 1}}
  m_LocalPosition: {{x: {px}, y: {py}, z: {pz}}}
  m_LocalScale: {{x: {scx}, y: {scy}, z: {scz}}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ROOT_ID}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &{sr_id}
SpriteRenderer:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_ForceMeshLod: -1
  m_MeshLodSelectionBias: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {LIT_MAT}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_GlobalIlluminationMeshLod: 0
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_MaskInteraction: 0
  {sprite_line(sp_id, sp_guid)}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: {sx}, y: {sy}}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 1
  m_SpriteSortPoint: 0
--- !u!114 &{mb_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {PARALLAX_LAYER_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::ParallaxLayer
  parallaxFactor: {layer["factor"]}
  horizontalTileRadius: 2
"""


def root_block() -> str:
    rx, ry, rz = ROOT_POS
    child_ids = [880005100 + i * 100 + 2 for i in range(4)]
    children_yaml = "\n".join(f"  - {{fileID: {cid}}}" for cid in child_ids)

    return f"""--- !u!1 &{ROOT_GO}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ROOT_ID}}}
  - component: {{fileID: {ROOT_SCRIPT}}}
  m_Layer: 0
  m_Name: ParallaxBackground
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{ROOT_ID}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ROOT_GO}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {rx}, y: {ry}, z: {rz}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children:
{children_yaml}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &{ROOT_SCRIPT}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ROOT_GO}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {PARALLAX_BG_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: Assembly-CSharp::ParallaxBackground
"""


def main() -> None:
    text = SCENE.read_text(encoding="utf-8")

    if "m_Name: ParallaxBackground" in text:
        print("ParallaxBackground already exists — aborting.")
        return

    injection = root_block() + "".join(layer_block(i, layer) for i, layer in enumerate(LAYERS))

    marker = "SceneRoots:"
    if marker not in text:
        raise SystemExit("SceneRoots marker not found")

    text = text.replace(marker, injection + marker, 1)

    roots_marker = "SceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:"
    text = text.replace(
        roots_marker,
        roots_marker + f"\n  - {{fileID: {ROOT_ID}}}",
        1,
    )

    out = SCENE.with_suffix(".unity.new")
    out.write_text(text, encoding="utf-8")
    out.replace(SCENE)
    print("Level_05 parallax setup complete (Clouds 5).")


if __name__ == "__main__":
    main()
