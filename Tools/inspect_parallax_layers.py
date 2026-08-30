#!/usr/bin/env python3
import re
from pathlib import Path

text = Path(r"d:\ilk-oyunumm\Assets\Scenes\Level_01.unity").read_text(encoding="utf-8")

parts = re.split(r"(?=^--- !u!)", text, flags=re.M)
docs = {}
for part in parts:
    m = re.match(r"^--- !u!(\d+) &(\d+)\n", part)
    if m:
        docs[int(m.group(2))] = (int(m.group(1)), part)

root_go_id = None
for doc_id, (type_id, content) in docs.items():
    if type_id == 1 and "\n  m_Name: ParallaxBackground\n" in content:
        root_go_id = doc_id
        break

print("root_go_id", root_go_id)
root_content = docs[root_go_id][1]
comps = [int(x) for x in re.findall(r"component: \{fileID: (\d+)\}", root_content)]
print("root comps", comps)

root_tid = None
for c in comps:
    if c in docs and docs[c][0] == 4:
        root_tid = c
        break

tcontent = docs[root_tid][1]
print("root pos", re.search(r"m_LocalPosition: .+", tcontent).group(0))
print("root scale", re.search(r"m_LocalScale: .+", tcontent).group(0))
kids_section = tcontent.split("m_Children:")[1].split("m_Father:")[0]
child_tids = [int(x) for x in re.findall(r"- \{fileID: (\d+)\}", kids_section)]
print("child transforms", child_tids)
print()

for tid in child_tids:
    tcontent = docs[tid][1]
    go_id = int(re.search(r"m_GameObject: \{fileID: (\d+)\}", tcontent).group(1))
    gcontent = docs[go_id][1]
    name = re.search(r"m_Name: (.+)", gcontent).group(1)
    active = re.search(r"m_IsActive: (.+)", gcontent).group(1)
    gcomps = [int(x) for x in re.findall(r"component: \{fileID: (\d+)\}", gcontent)]
    pos = re.search(r"m_LocalPosition: (.+)", tcontent).group(1)
    scale = re.search(r"m_LocalScale: (.+)", tcontent).group(1)
    father = re.search(r"m_Father: \{fileID: (\d+)\}", tcontent).group(1)
    print(f"=== {name} go={go_id} active={active} ===")
    print(f"  localPos {pos}")
    print(f"  localScale {scale}")
    print(f"  father={father} expected_root={root_tid}")
    for c in gcomps:
        if c not in docs:
            print(f"  MISSING doc {c}")
            continue
        typ, content = docs[c]
        if typ == 4:
            print(f"  Transform {c}")
        elif typ == 212:
            print(
                "  SpriteRenderer enabled=",
                re.search(r"m_Enabled: (.+)", content).group(1),
                "order=",
                re.search(r"m_SortingOrder: (.+)", content).group(1),
                "layerID=",
                re.search(r"m_SortingLayerID: (.+)", content).group(1),
            )
            print("   sprite=", re.search(r"m_Sprite: (.+)", content).group(1))
            print("   size=", re.search(r"m_Size: (.+)", content).group(1))
        elif typ == 114:
            ident = re.search(r"m_EditorClassIdentifier: (.+)", content)
            guid = re.search(r"guid: ([a-f0-9]+)", content)
            print(
                "  Mono enabled=",
                re.search(r"m_Enabled: (.+)", content).group(1),
                ident.group(1) if ident else "?",
                "guid=",
                guid.group(1) if guid else "?",
            )
            if "ParallaxLayer" in content:
                print("   factor=", re.search(r"parallaxFactor: (.+)", content).group(1))
                print("   vert=", re.search(r"verticalParallaxFactor: (.+)", content).group(1))
        else:
            print(f"  type {typ} id {c}")
    print()

root_pos = re.search(
    r"m_LocalPosition: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}", docs[root_tid][1]
)
rx, ry, rz = map(float, root_pos.groups())
print("World positions (root + local):")
for tid in child_tids:
    tcontent = docs[tid][1]
    go_id = int(re.search(r"m_GameObject: \{fileID: (\d+)\}", tcontent).group(1))
    name = re.search(r"m_Name: (.+)", docs[go_id][1]).group(1)
    pos = re.search(
        r"m_LocalPosition: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}", tcontent
    )
    lx, ly, lz = map(float, pos.groups())
    print(f"  {name}: world=({rx+lx:.4f}, {ry+ly:.4f}, {lz:.4f})")

print("\nCamera:")
for doc_id, (typ, content) in docs.items():
    if typ == 1 and "\n  m_Name: Main Camera\n" in content:
        for c in re.findall(r"component: \{fileID: (\d+)\}", content):
            c = int(c)
            if c in docs and docs[c][0] == 4:
                print(re.search(r"m_LocalPosition: .+", docs[c][1]).group(0))
            if c in docs and docs[c][0] == 20:
                print(re.search(r"orthographic size: .+", docs[c][1]).group(0))
