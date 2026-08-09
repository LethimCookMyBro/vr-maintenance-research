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
| Poly Haven textures and models (concrete floor, beige wall, screwdriver, power box, ceiling fan) | CC0 1.0 | `ThirdParty/PolyHaven/LICENSE.txt` |
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

Kenney and Quaternius CC0 model packs were considered for the racking, crates
and wall fittings and rejected. They are genuinely CC0 and genuinely good, but
they are stylised low-poly, and this project's task components are photographic
CC BY scans of real hardware. Mixing the two would have made the parts a
participant must identify look *less* real by contrast, which is the one thing
this pass must not do. Primitive geometry painted with the existing lab palette
sits in the same visual register as the room already had.
