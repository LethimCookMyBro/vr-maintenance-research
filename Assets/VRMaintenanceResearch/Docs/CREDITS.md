# Credits

Asset provenance for the visual-polish pass of 2026-08-09, and where the
authoritative licence text for everything the project ships already lives.

## What this pass added

**No new third-party asset was downloaded or imported.** Everything the
industrial-lab dressing and the participant display are made of is either a
Unity primitive authored in this repository or an asset the project already
carried under a licence recorded below.

| Added in this pass | Made of | Licence |
|---|---|---|
| Two-tone walls, yellow floor marking, guardrails, steel racking, stacking crates, wall control cabinet, safety signage, ESD bench matting (`Editor/LabIndustrialDressing.cs`) | Unity `Cube` and `Cylinder` primitives, painted with the project's own `Materials/Lab` palette | Project-authored, no third-party content |
| `Lab_SafetyYellow`, `Lab_CrateBlue` materials | URP Lit, flat colour | Project-authored |
| Rounded panel sprite behind every uGUI board (`ResearchUiKit.RoundedSprite`) | Generated in code at runtime, 48 × 48 px | Project-authored, no image file shipped |
| Rounded corners on the world-space panels (work order, information dock, notice board) | Existing Kenney UI Pack 2.0 sprites, retinted | CC0 — see below |

Why the panels reuse the Kenney sprites rather than a new nine-slice image: the
sprites, their import settings and the URP Unlit transparent materials that
carry them were already in the project and already applied to the reader panel.
The reference look needed a tint with an alpha channel, not a new asset.

## Third-party assets the project ships

Each of these carries its own licence file next to it; those files, not this
table, are the record.

| Asset group | Licence | Licence file |
|---|---|---|
| Kenney UI Pack 2.0 (panel sprites, Future Narrow typeface) | CC0 1.0 | `ThirdParty/Kenney/UIPack/LICENSE.txt` |
| Kenney Game Icons | CC0 1.0 | `ThirdParty/Kenney/GameIcons/LICENSE.txt` |
| Poly Haven textures and models (concrete floor, beige wall, screwdriver) | CC0 1.0 | `ThirdParty/PolyHaven/LICENSE.txt` |
| Noto Sans Thai, Noto Sans JP | SIL Open Font License 1.1 | `ThirdParty/Fonts/NotoSansThai-OFL.txt`, `ThirdParty/Fonts/NotoSansJP-OFL.txt` |
| Component meshes — motherboard, CPU, cooler, memory, drive, supply, case fan, screwdriver | **CC BY 4.0**, one of them **CC BY-NC 4.0** | `ThirdParty/ITEM_3D/ATTRIBUTION.md` |

### CC BY attribution

Required attribution for the component meshes is held in full — title, creator,
licence, source URL and SHA-256 — in `ThirdParty/ITEM_3D/ATTRIBUTION.md`. It is
not duplicated here: commit `230ddfe` removed an earlier project-wide summary
precisely because a second copy of a licence list goes stale without anyone
noticing. Note the one non-commercial restriction recorded there: the DDR4
memory model is CC BY-NC 4.0, which limits redistribution of the whole project
to non-commercial use.

## What was deliberately not used

### Poly Haven `power_box_01` — tried on the wall cabinet, rejected on the numbers

The wall control cabinet is nine Unity primitives, and `power_box_01` is
literally a wall-mounted electrical box, so it was built into
`Prefabs/Environment/LabEnvironment.prefab` in place of them and measured from
the participant's start pose against the version that was already there.
`Docs/Screenshots/Review/PowerBoxTrial_*.png` are those two frames.

| ComputerRepairTask, participant start pose | nine primitives | `power_box_01` |
|---|---|---|
| Triangles in the scene | 120,093 | 141,065 (**+20,972**) |
| Triangles submitted per frame | 335,810 | 367,093 (**+31,283**) |
| Vertices per frame | 445,519 | 469,281 |
| Batches | 1,441 | 1,445 |
| Static-batched draw calls | 351 in 11 batches | 334 in 10 batches |
| Visible skinned meshes | 2 | 4 |
| Materials in the scene | 97 | 98, plus a 1024 × 1024 texture |

The static-batch line is the one that decides it. The nine primitives are
`BatchingStatic` and fold into the room's static batch; `power_box_01` ships as
two `SkinnedMeshRenderer`s on a hundred-bone rig for its door and cable runs,
and a skinned mesh is never static-batched, so the swap *removed* a batch's
worth of merged geometry and added a per-frame skinning cost for a
19,196-vertex mesh that never moves.

The picture agrees with the numbers. The asset is a 500 mm domestic meter box
where a 1.08 m industrial cabinet stood; at the participant's six metres the
photoscanned detail the 21,272 triangles pay for resolves to a grey rectangle,
while what the primitives bought — a lit green display and three status lamps
that say the room is live — is gone. Scaling it 2.2× to fill the same wall
would have been a photoscan stretched past the size of the real object, which
is worse than a primitive, not better. It also lands on the wrong side of the
rule below: it is a photographic scan standing next to graybox furniture, in a
room whose whole point is that the *task parts* are the real-looking things.

The nine primitives stay. `power_box_01` and its material are removed from the
project along with their rows in `ThirdParty/PolyHaven/DOWNLOAD_VERIFICATION.csv`
— an unreferenced third-party asset is a provenance obligation with nothing on
the other side of it. The measurements above are the record; the files are one
`git revert` away if the wall ever wants them.

### Poly Haven `ceiling_fan` — removed, no honest place for it

The fan bench repairs a desk fan. A ceiling fan is not a part of it, not a
spare for it and not a tool for it, and the only place it could have gone is
the ceiling as dressing — which is the one thing this particular study must not
do. The fan task asks a participant to name the parts of a fan; a second fan
hanging over the bench is a distractor from the same object category as the
device under test, and that is a confound in the measurement, not a decoration.
16,356 triangles for it besides. Removed with its material and its
`DOWNLOAD_VERIFICATION.csv` rows.

### Stylised CC0 model packs

Kenney and Quaternius CC0 model packs were considered for the racking, crates
and wall fittings and rejected. They are genuinely CC0 and genuinely good, but
they are stylised low-poly, and this project's task components are photographic
CC BY scans of real hardware. Mixing the two would have made the parts a
participant must identify look *less* real by contrast, which is the one thing
this pass must not do. Primitive geometry painted with the existing lab palette
sits in the same visual register as the room already had.
