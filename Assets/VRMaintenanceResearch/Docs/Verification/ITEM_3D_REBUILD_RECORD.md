# ITEM_3D rebuild record

**Date:** 2026-08-04 (Asia/Bangkok)
**Rejected commit:** `b6b5777aa90f5133542a98cadbc19af9b912c082` — *Replace maintenance placeholders with licensed models*
**Backups:** tag `reject/item3d-b6b5777`, branch `backup/item3d-rejected-b6b5777` (both at the rejected commit; no hard reset was used)
**Restored from:** `22c71580d67760a1595f570f59747815474b0299` — *Checkpoint Quest readability before model integration*

## Why the integration was rejected

The case interior was overcrowded, parts overlapped or floated, scale was inconsistent,
and the tool and spare areas did not read as trays. Confirmed against the rejected
renders, preserved in `Docs/Screenshots/Rejected_ITEM3D/`:

- the graphics card floated tilted through the drive cage;
- the cooler stood detached against the rear wall instead of on the processor;
- the power supply hung below the case;
- two drives sat outside the shell;
- memory rendered as slivers, and data leads floated free.

## Root cause

`ResearchBuildKit.ImportedVisual` fitted **each axis independently**. Asking for the
0.248 × 0.022 × 0.305 m board at 0.085 × 0.255 × 0.265 applied scale factors of 0.28,
11.6 and 1.07 to a single mesh — a 41× anisotropy. Every imported part was sheared, and
shear is what read on screen as floating, clipping and disagreeing scale.

Two further causes were found by measurement during the rebuild:

- the storage source is **two M.2 drives**, which is where the duplicate drives came from;
- the power-supply source carries a loose screw 16 units out from a body only 10 deep,
  which both floats outside the case and shrinks the unit by inflating the fitted bounds.

## What changed

`ImportedVisual` now fits **uniformly** (tightest axis wins), so it cannot distort a
source. It also takes an explicit `drop` list for parts of a source that are not the
thing being modelled. Collider fitting reuses the builders' existing `SetCollider`,
which resizes an object's own BoxCollider and never replaces a collider type.

`ResearchAssemblyAudit` was added. It measures containment, intersection, scale,
duplicates, hierarchy depth, collider fit, material and triangle budget **in the case's
own frame** — the case is yawed −70°, so world-space AABBs are meaningless here. Every
defect in the rejected build is one of those numbers.

## Model decisions

Corrections to `ITEM_3D_MODEL_AUDIT.md`, all from Unity-side measurement:

| Source | Decision | Measured reason |
|---|---|---|
| Motherboard `anakart_quest.glb` | **Used** | 0.248 × 0.022 × 0.305 m on import — already true ATX size, fitted at native scale |
| CPU `ryzen_5_5600.glb` | **Used** | 40 × 40 × 7 mm, correct |
| Cooler `amdwraithstealthnocable.glb` | **Used** | fits to 49 × 98 × 91 mm, correct for a Wraith Stealth |
| RAM `..._ddr4_quest.glb` | **Used** ×2 in case, ×1 spare | correct DIMM aspect |
| PSU `psu_power_supply_unit.glb` | **Used** | axes are width/depth/height, not width/height/depth; drops `Object_78` |
| Storage `ssd-kit.glb` | **Used, one drive** | two M.2 drives (80 × 22 × 2.4 mm); drops `Circle.002` |
| Fan `120mm_computer_fans.glb` | **Used**, Computer only | 1 : 1 : 0.215, correct 120 mm proportions |
| Screwdriver | **Used** (pre-existing wrapper) | measures 200 mm in scene |
| GPU `gpu_quest.glb` (RTX 4060 Ti) | **Rejected** | backplate 26.5 × 2.49 — 10.6 : 1 where a real card is ~2.2 : 1. At a true 245 mm length it reads as a 22 mm blade |
| GPU `graphicscard.glb` (black) | **Rejected** | correctly shaped but 113,728 triangles |
| Case, wall cable, archived CPU | **Rejected** | unchanged from the original audit (triangles, wrong part, undetermined licence) |

The graphics card therefore stays handmade: 632 triangles, correctly proportioned, and
consistent with the case's semi-realistic style.

## Rebuild stages

1. **Case and board.** Board mounted on the motherboard interactable itself, replacing a
   green stand-in plate that sat 2 mm off it and hid it. Its rear-panel edge — carrying
   the I/O cluster and the PCIe bracket ends — is 305 mm and stands vertical against the
   rear panel, so the board is 305 tall × 244 deep. Handmade board dressing and rear I/O
   ports were deleted, not layered underneath.
2. **Processor, cooler, memory.** Placed from an orthographic board map
   (`Docs/Screenshots/Staging` during the rebuild): socket at dx +0.012 / dz +0.069, DIMM
   slots at dx +0.071 and +0.086. Two DIMMs in the dual-channel pair.
3. **Card, supply, drive, fan.** Card in the measured PCIe slot; supply in the basement,
   upright; one M.2 drive resting on a tray shelf; 120 mm fan on the rear panel. The 3.5"
   hard disk was removed — the bench is specified to hold one drive.
4. **ATX loom.** One 24-pin run swept along a cubic Bezier from the supply's gland along
   the floor and up the case front to the plug hanging short of its header. No glow, no
   colour, no label, no exaggerated gap. The board's front edge faces away to the
   participant's right from the standing pose, so the fault is not obvious on arrival.
5. **Bench.** Oversized engraved captions replaced by small placards on tray rims — one
   change to `BenchDressing.Zone`, which every scene calls, so `TOOLS`, `SPARE PARTS`,
   `SERVICE AREA`, `INSTALLED COMPONENT` and `REMOVED PARTS` are all covered. Work order
   reduced ~15% with its copy held at the validator's 0.30 readability floor.

A duplicate-tray defect was introduced and fixed during stage 5: the lab furniture
already provides `Parts Tray` and `Tool Tray`, and building a second pair underneath them
buried everything resting between the two floors. Parts now sit in the furniture's trays.

## Verification

Two source rebuilds (`REBUILD ALL`) and two validation passes:

- `ITEM3D_Rebuild_PassA.txt` — **ALL SCENES PASS**
- `VisualAudit_Validation.txt` (pass B) — **ALL SCENES PASS**
- Rebuilds are idempotent: `Desktop Case` subtree measured `tris=90215 mats=63
  renderers=236` on two consecutive rebuilds.

Three `WARN` lines in VRTraining are a pre-existing data mismatch the validator itself
labels as not changed by this pass.

**Assembly audit** (`Assembly_Audit.txt`): no unexpected intersections, nothing outside
the case, no duplicate parts, and `cameras=0 lights=0 animators=0 audio=0` under the
case — no imported extras reached the scene.

**Identity:** stable IDs across all four scenes are byte-identical to the pre-integration
baseline (`git diff` of every `stableObjectId`). No duplicate IDs. Repair IDs unchanged:
`computer.main-power-connector`, `fan.working-fuse`.

**Systems:** exactly one `EventSystem` and one `XRUIInputModule` per task scene, one
camera per scene.

**Console:** zero errors. Two pre-existing `CS0618` obsolete-API warnings
(`TMP_Text.enableWordWrapping`) predate this work.

**Runtime (Play Mode):**

| Scene | Result |
|---|---|
| Computer | test → fail (task stays active) → ATX repair → test → **Completed** → reset → **Active** |
| Fan | test → fail (task stays active) → fuse repair → test → **Completed** → reset → **Active** |
| Training | Continue stays locked through three skills, unlocks on the fourth, relocks on reset |

Recorded in `Runtime_Checks_ComputerRepairTask.txt`, `Runtime_Checks_FanRepairTask.txt`,
`Runtime_Checks_VRTraining.txt`.

**No fuse is labelled** good, bad, broken, faulty or correct. A scan of every visible
`TMP_Text` in both task scenes returns one hit — "does not operate correctly" in the
fan's reported symptom — which is the symptom, not a part label.

## Budget

`Desktop Case` subtree: **90,215 triangles**, 63 materials, 236 renderers.
Largest contributors: cooler 37,975; board 18,856; processor 13,426; supply 12,167;
fan 4,755; memory 396 each; card 632; drive 236.

Embedded textures are at or below 2048 px. The cooler is the most expensive item
relative to its on-screen size, and the processor's 13,426 triangles sit almost entirely
hidden beneath it; both are candidates if a triangle budget has to be found later.

## Boundary

This is internal editor-side measurement, static analysis and offscreen render review.
Captures are produced by `Camera.Render()` to a RenderTexture, which never draws gizmos.
**No Meta Quest hardware profiling and no human pilot data were collected.** Quest
suitability here means triangle, material, texture and shader budgets checked in-editor —
not a measured frame rate on device.
