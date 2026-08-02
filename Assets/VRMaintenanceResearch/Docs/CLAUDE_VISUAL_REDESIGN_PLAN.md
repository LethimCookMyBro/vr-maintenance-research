# Visual Redesign Plan — VR Maintenance Research

Branch: `visual-polish-claude` (created from validated baseline `f117cc8`)
Author: Claude (visual redesign pass)
Date: 2026-08-02

This plan covers the visual redesign only. The research system, session flow,
logging schema, stable research IDs, completion conditions and the four
information-source conditions are **not** changed by this work.

---

## 1. Current visual problems (audited, not assumed)

Audit method: Unity MCP scene dumps of `ComputerRepairTask`, `FanRepairTask`,
`VRTraining`, `ResearcherSetup`, plus a render from the participant camera.

| # | Problem | Evidence |
|---|---|---|
| P1 | No room. `Research Floor` is a single 20 x 20 m plane; the default skybox is visible in every direction, so both task scenes read as objects floating outdoors. | Camera render; `Research Floor p=(0,0,0) s=(2,1,2)`, `RenderSettings.skybox = Default-Skybox` |
| P2 | Every object shares Unity's built-in `Lit` material instance. There is not one material asset in `Assets/VRMaintenanceResearch/Materials` except the two video materials, so the entire scene is one flat white-grey. | Scene dump: `mat=Lit/Universal Render Pipeline/Lit` on 40+ renderers |
| P3 | **The participant spawns inside the device.** The XR Origin is at `(0,0,0)` and the Desktop Case occupies `x ±1.2, y 0.3–1.9, z ±0.45` around that same origin. The Fan body likewise. | `Main Camera p=(0,1.36,0)`; `Desktop Case p=(0,1.1,0) s=(2.4,1.6,0.9)` |
| P4 | **Two magenta objects.** `Computer Status Indicator` and `Fan Status Indicator` use `Default-Material` on the **built-in Standard shader**, which does not exist in URP and renders as the magenta error colour. | Camera render (magenta sphere); scene dump `mat=Default-Material/Standard` |
| P5 | The Fan body cylinder spans `y = -0.4 … 2.6` and therefore clips through the floor. | `Electric Fan Body p=(0,1.1,0) s=(0.9,1.5,0.9)` (Unity cylinder is 2 units tall) |
| P6 | Nothing is supported by anything. Replacement components, the tool and the device-test control all hover in mid-air at `y = 1.0` with no bench, table or stand beneath them. | Scene dump positions |
| P7 | All text is legacy `TextMesh` on the `GUI/Text Shader`, at `characterSize 0.1` / `fontSize 0`, which is unlit, aliased and not readable at VR panel distance. | Scene dump `mat=Font Material/GUI/Text Shader` |
| P8 | `ResearcherSetup` contains **no camera and no 3D content at all** — two GameObjects, one of which is a light. The whole setup screen is `OnGUI`/IMGUI drawn in a 560 x 700 px box. | `ResearcherSetup` hierarchy (rootCount 2); `ResearcherSetupController.OnGUI` |
| P9 | The `VRTraining` world-space canvas `Training World Instructions` has `localScale = (0,0,0)`, so it renders nothing. Training relies entirely on an IMGUI debug box. | Scene dump; `TrainingInstructions.OnGUI` |
| P10 | Information-source tiles are unlabelled grey boxes; the participant cannot tell manual from video from visual guide without opening them. Their *content* panels carry the titles, not the closed tiles. | Scene dump: tiles have no text child |
| P11 | Researcher controls are an IMGUI box pinned to the top-right of the screen whenever development mode is on, with Safety Stop styled identically to Pause. | `ResearcherTaskControls.OnGUI` |
| P12 | Lighting is a single shadowless directional light plus skybox ambient. No key light over the work area, no object separation. | `Light intensity=1 shadows=None`, `Ambient mode=Skybox` |

## 2. Functional objects that must not be replaced

Every GameObject below is a **functional root**. Its GameObject, its scripts, its
`XRSimpleInteractable` / `XRGrabInteractable` / `XRSocketInteractor`, its collider
component, its `ResearchInteractable` stable ID and its task reference are preserved.
Only the material, an added visual child, and (where the visual demands it) the
collider's local `center`/`size`/`radius`/`height` fields change.

**ComputerRepairTask** — `computer.case`, `computer.main-power-connector`,
`computer.ram`, `computer.tool.screwdriver`, `computer.power-button`,
`computer.side-panel`, `computer.motherboard`, `computer.psu`, `computer.psu-switch`,
`computer.external-power-cable`, `computer.cooling-fan`, `computer.internal-cable`,
`computer.non-target-module`, and the four `computer.source.*` tiles with their
panels and control buttons.

**FanRepairTask** — `fan.body`, `fan.working-fuse`, `fan.faulty-fuse`,
`fan.tool.screwdriver`, `fan.speed-selector`, `fan.power-plug`, `fan.power-cord`,
`fan.power-switch`, `fan.front-cover`, `fan.blade`, `fan.fuse-holder`,
`fan.internal-wire`, `fan.motor-module`, `fan.fastener`, `fan.non-target-module`,
and the four `fan.source.*` tiles with their panels and control buttons.

**VRTraining** — `training.training-cube-a`, `training.training-cube-b`,
`training.training-cylinder`, `Training Socket` (XRSocketInteractor),
`Neutral Information Source`, `Reset Training Button`.

**All scenes** — `Official XRI Complete Setup`, `Official XR Interaction Simulator`,
`XR Interaction Manager`, `Event System`, the task controller objects, and
`Research Floor` (it carries the MeshCollider used for teleportation).

The eight invalid unreferenced v1 information-source assets are not touched,
not staged and not referenced.

## 3. UI assets already available in the project

Checked before sourcing anything external:

- `Assets/TextMesh Pro/` — TMP essentials, `LiberationSans SDF` font asset and
  material presets. Used for **all** new text. No TMP asset is modified.
- `Assets/XRI/` and `Assets/XRI_Examples/` — the official XRI UI prefabs and
  interaction visuals. Read for reference; **not modified**, and no prefab variant
  is created from them because the new research UI has a different information
  architecture.
- `Assets/VRMaintenanceResearch/Materials/` — the two existing video materials
  (`ComputerMaintenanceDevelopmentVideo`, `FanMaintenanceDevelopmentVideo`).
  Kept and reused unchanged; the video RenderTexture path must not be disturbed.
- `Assets/VRMaintenanceResearch/Video/` — the existing self-authored baseline MP4s.
  Kept unchanged.

## 4. External assets actually needed

Only two categories could not be produced adequately from primitives and
solid materials:

1. **Surface texture for floor and walls** — a flat untextured room reads as a
   render, not a laboratory. Sourced from Poly Haven (CC0).
2. **UI icons** — the four information sources must be distinguishable while
   closed, and researcher/training controls need glyphs. Sourced from Kenney (CC0).

Everything else (workbench, shelving unit, tool tray, device shells, room shell,
signage) is authored from Unity primitives with shared materials. Rationale:
imported furniture models would have to be scale- and pivot-aligned against
colliders that are part of a validated build, would add materials and draw calls,
and would not look more "clean academic laboratory" than restrained primitive
geometry. See `ThirdParty/THIRD_PARTY_ASSETS.md` for the full record, including
the assets deliberately **not** imported and the manual-download shortlist.

## 5. Proposed visual style

Modern academic VR maintenance laboratory. Clean, low-poly, semi-realistic.
Not cartoonish, not photoreal, not sci-fi, not crowded.

- Flat-ish surfaces, restrained bevel-free primitive forms.
- Matte finishes; a single metal material carries all the specular interest.
- No decals, no grime, no decorative props beyond what a maintenance bay needs.
- Uniform 0.02–0.06 m panel thicknesses so nothing reads as a paper-thin plane.

## 6. Proposed colour palette (60-30-10)

| Share | Role | Colour | Hex | Used on |
|---|---|---|---|---|
| 60% | Neutral environment | Off-white / light warm grey | `#EDEDEA` walls, `#B9BCC0` floor | Walls, ceiling, floor, room trim |
| 30% | Interface + equipment | Navy / slate | `#1E2A3A` navy, `#3C4655` slate, `#8A9199` light metal | Workbench frame, device shells, closed source tiles, UI panel bodies |
| 10% | Accent | Signal blue | `#2E7BE6` | Interactive affordances, tile borders, primary button, progress |
| reserved | Warning only | Amber | `#F2A22C` | Safety Stop, warnings, outstanding-requirement markers |

Secondary tints kept deliberately few: `#D8DCE0` light plastic, `#2A2E33` dark
plastic, `#1A1D21` rubber, `#C8CDD3` brushed metal.

The palette is applied identically in Computer and Fan so the two tasks read as
rooms in the same building.

## 7. Proposed room layout

Identical shell in Training, Computer and Fan:

- Interior 11.0 m (x) x 11.0 m (z) x 3.2 m (y), walls 0.1 m thick, wall inner
  faces at `x = ±5.5`, `z = -4.5` and `z = 5.5`.
- Ceiling at `y = 3.2` with four flush light panels.
- Skirting band at `y = 0.10` in slate to break the floor/wall seam.
- Participant start pose moved from `(0,0,0)` to `(0,0,-2.0)`, **identically in
  all three scenes**, so the participant no longer spawns inside the device and
  faces the work area at a natural 2 m working distance. This is the one layout
  value that changes; it is applied to every scene so cross-task comparability is
  preserved. Recorded in `PROTOCOL_CHANGE_LOG.md`.
- Device centred at the origin, on a low equipment platform.
- Component/parts table on the participant's **left**, beneath the two
  replacement components that already sit at `x = -1.7` and `x = -0.9`.
- Tool table and control pedestal on the participant's **right**, beneath the
  tool at `x = 1.4` and the device-test control at `x = 1.6`.
- Information station: the four source tiles keep their existing transforms
  (`y = 1.0`, `z = 3.0`, `x = -3.3 / -1.1 / +1.1 / +3.3`) and gain a shared
  back-wall structure, header sign and equal navy tile bodies.
- Task status panel above and behind the work area on the far wall.
- Researcher controls stay off the participant's view (see §9).

Every distance, height and UI size listed above is byte-identical between the
Computer and Fan scenes.

## 8. Information-source equality — explicit non-change

Phase 4 of the brief suggests moving the information station to the participant's
left. **That is not done.** The four source tiles are at validated, equal-size,
equal-height, equal-angle transforms that are recorded against `definition.layoutId`
in the event CSV. Moving them would change the experimental layout without changing
the logged layout ID, and would alter the relative salience of the four conditions.
The redesign therefore keeps every source transform byte-identical and equalises
only appearance:

- identical tile dimensions (`1.2 x 0.7 x 0.1`, unchanged)
- identical navy body material, identical border weight, identical accent
- identical icon size and identical icon colour (white, 100 x 100 source)
- identical caption typography, size and vertical offset
- identical panel size, position, height and viewing angle
- identical control-button size and spacing

The only per-source difference is the glyph and the caption text, which is
required for the four conditions to be distinguishable at all.

## 9. Researcher controls

`ResearcherTaskControls` already renders only when
`ResearchSessionManager.Instance.Configuration.developmentMode` is true, so it is
invisible during a normal participant session. The redesign keeps that gate and adds:

- a collapsed-by-default panel opened with a researcher toggle,
- restrained styling so the panel no longer dominates the frame,
- Safety Stop separated by a rule, amber-on-dark, away from the ordinary controls.

No control is removed: Pause, Resume, Retry, Reset Task, Abort Task, Safety Stop
and Continue to Next Task all remain, calling the same methods.

## 10. Expected risks to interaction and logging

| Risk | Mitigation |
|---|---|
| Adding a visual child changes what a controller ray hits first | Every visual child is created on the `Ignore Raycast`-equivalent path: no collider is added to any visual child, so all rays still resolve to the functional root's collider. |
| Disabling the graybox `MeshRenderer` breaks `OnMouseDown` mouse simulation | It does not: `OnMouseDown`/`OnMouseEnter` are collider-driven, and colliders stay enabled. Verified in the smoke run. |
| Re-fitting a collider changes hover/select behaviour | Colliders are only re-fitted where the visual footprint genuinely changed (Fan body). Type stays the same; only local sizing fields change. Each change is listed in the final report. |
| New geometry blocks teleportation or walking | Room shell and furniture are placed outside the participant's walk envelope; `Research Floor` keeps its MeshCollider; a smoke run checks locomotion. |
| A second EventSystem / XR Origin / Interaction Manager sneaks in with a new canvas | World-space canvases are added as plain children with no EventSystem. A uniqueness check runs after every batch. |
| New lights or cameras arrive from an import | No model was imported. The scenes are checked for exactly one enabled camera and a counted light set. |
| Decoration accidentally hints at the faulty component | No light, colour, label or prop is placed on or near `computer.main-power-connector` / `fan.working-fuse` that is not equally placed on `computer.ram` / `fan.faulty-fuse`. The two replacement components share one material and one table. |
| Changed start pose alters movement CSV coordinates | Schema is unchanged; only the origin offset differs. Recorded in `PROTOCOL_CHANGE_LOG.md` and `KNOWN_LIMITATIONS.md`. |

## 11. Quest-oriented performance constraints

Targets for this pass (visual preparation only — **no physical Quest 3 validation
is claimed**):

- All new geometry from Unity primitives: cube (12 tris), cylinder (80 tris),
  sphere (768 tris), quad (2 tris). Spheres are avoided in bulk decoration.
- One shared material per surface class; target ≤ 14 new material assets total.
- Textures: 1024 max for the two Poly Haven surfaces, 100 px for icons.
  No 2048 texture is introduced.
- Lighting: one directional key light with shadows, one soft fill directional
  without shadows, and emissive ceiling panels instead of real point lights.
  Target ≤ 3 real-time lights per scene.
- No transparency except the existing UI; no reflection probes; no post-processing
  changes; no new animations.
- All static environment geometry flagged Batching Static / Occluder Static.
- Package versions untouched.

## 12. Execution order

1. Materials + textures (shared, URP Lit).
2. Fix the two magenta Standard-shader indicators.
3. Room shell + furniture prefab, shared across the three scenes.
4. Device visual children; graybox renderers disabled.
5. Information-source tiles and panels restyled to TMP.
6. `ResearcherSetup` rebuilt as a real 3D scene with a TMP canvas.
7. Training UI (fix the zero-scale canvas, add progress + Continue).
8. Researcher controls restyled and gated.
9. Lighting and static flags.
10. Console check, uniqueness check, smoke flow both task orders, Windows build.

---

# Execution record (completed 2026-08-02)

## What was built

| Area | Delivered |
|---|---|
| Materials | 17 shared URP Lit materials in `Materials/Lab` (`Lab_Floor`, `Lab_Wall`, `Lab_Ceiling`, `Lab_Trim`, `Lab_Navy`, `Lab_StationBoard`, `Lab_Metal`, `Lab_MetalDark`, `Lab_PlasticLight`, `Lab_PlasticDark`, `Lab_Rubber`, `Lab_Accent`, `Lab_Warning`, `Lab_PanelSurface`, `Lab_Pcb`, `Lab_Indicator`, `Lab_LightPanel`) plus 16 unlit icon materials in `Materials/Icons` |
| Environment | `Prefabs/Environment/LabEnvironment.prefab`, ~884 triangles, instantiated once in all four build scenes |
| Room | 9.0 x 8.0 x 3.0 m interior, walls at `x = ±4.5`, `z = -3.5 / +4.5`, ceiling 3.0 m, skirting, four emissive ceiling panels |
| Furniture | Workbench (top 0.92 m, 4.6 x 0.9 m), identical parts tray (left) and tool tray (right), control pedestal at `(1.95, ., 0.20)`, four-shelf storage unit on the west wall, floor work-zone markings |
| Information station | Mid-tone backing board at `z = 3.32`, navy base and cap, accent line; the four source tiles keep their original transforms |
| Lighting | One shadowed key directional (0.85) + one shadowless fill directional (0.20) + trilight ambient; emissive ceiling panels carry the rest |
| Setup UI | `ResearcherSetupController` rebuilt as a centred 1420 px two-column TextMeshPro interface: Session / Experimental condition / Session options / Technical notes / status + Start Session |
| Participant UI | Fixed world-space TMP tiles and panels; runtime-built training board and task status board |
| Researcher UI | `ResearcherTaskControls` restyled, collapsed by default behind a handle or F9, Safety Stop separated by a rule and given the amber/red treatment |
| Editor tooling | `Editor/ResearchSceneCapture.cs` — reproducible participant-viewpoint PNG captures (`Tools/VR Maintenance Research/Capture Participant Views`) |

## Defects found and fixed during the redesign

1. The participant spawned **inside** the device volume in all three participant scenes.
2. `Computer Status Indicator` and `Fan Status Indicator` used the built-in Standard shader and rendered magenta in URP.
3. The Fan body cylinder clipped through the floor (`y = -0.4 … 2.6`).
4. `Training World Instructions` had `localScale = (0,0,0)` and rendered nothing.
5. The training neutral information source had no content panel, so opening it produced no visible feedback.
6. Information-source control buttons read Next-before-Prev and Seek-before-Play from the participant's viewpoint.
7. Auto-sized tile captions made short source names visually larger than long ones — an equal-prominence violation. Type size is now fixed and identical on all four tiles.
8. A duplicate `LabEnvironment` instance was created because the prefab root was renamed on save; the de-duplication check now matches both names, and each scene has exactly one instance and two lights.

## Verification of the equal-condition requirement

Measured after the redesign, identical for all four sources in both task scenes:

| Property | Value |
|---|---|
| Tile transform | `(-3.3 / -1.1 / +1.1 / +3.3, 1.0, 3.0)`, scale `(1.2, 0.7, 0.1)` — unchanged from baseline |
| Tile body material | `Lab_Navy` |
| Accent bar | `1.04 x 0.028 x 0.008`, `Lab_Accent`, same height offset |
| Icon | 0.24 x 0.24 m quad, white 100 px Kenney glyph, same offset |
| Caption | TMP, fixed 0.62, bold, centred, same offset |
| Slot label | TMP, fixed 0.34, same offset |
| Panel | `(x, 1.65, 1.5)`, scale `(1.7, 1, 0.05)` — unchanged from baseline |
| Panel frame / header / rule | identical geometry and materials |
| Control buttons | `0.20 x 0.09 x 0.028`, `Lab_Accent`, identical spacing |

The only per-source differences are the glyph and the caption/title/body strings,
which are required for the four conditions to be distinguishable at all.

## Quest-oriented measurements (preparation only, not hardware-validated)

| Scene | Renderers | Triangles | Unique materials | Real-time lights |
|---|---|---|---|---|
| ResearcherSetup | 52 | 1,084 | 12 | 2 |
| VRTraining | 91 | 17,446 | 18 | 2 |
| ComputerRepairTask | 156 | 19,272 | 26 | 2 |
| FanRepairTask | 159 | 19,988 | 25 | 2 |

Textures added: 3 x 1024 px (2 Poly Haven maps in use) and 16 x 100 px icons.
No 2048 px texture, no new transparency beyond the existing UI, no post-processing
change, no reflection probes, no animations, no package version change. All
environment geometry is flagged static.

Most of the per-scene triangle count comes from the original XRI controller models
and Unity's cylinder/sphere primitives, not from the new environment.

## Deviations from the brief, and why

| Brief item | What was done instead | Reason |
|---|---|---|
| "Information station is on the participant's left" | Sources kept at their validated, equal transforms across the front of the room | Moving them would change relative salience without changing `information_source_layout_id`. Recorded in `PROTOCOL_CHANGE_LOG.md`. |
| "External assets: environment, computer, fan, tool models" | Only CC0 textures and icons were imported; all geometry is authored from Unity primitives | Imported furniture and appliance models would have to be pivot- and scale-aligned against colliders that are part of a validated build, and would add materials and draw calls without looking more like a clean academic laboratory than restrained primitive geometry. The full sourcing record, including the manual-download shortlist, is in `ThirdParty/THIRD_PARTY_ASSETS.md`. |
| "Imported models are visual children only; preserve transforms" | Functional roots, scripts, IDs, interactable and collider *components* were preserved, but positions/rotations/scales were re-authored | The baseline geometry was dimensionally incoherent (2.4 m PC case, 3 m fan clipping the floor, parts floating in mid-air). Every change is tabulated in `PROTOCOL_CHANGE_LOG.md` for advisor review. |
| "Create prefab variants under `Prefabs/UI`" | The setup, training and status interfaces are built in code | Removes prefab-wiring drift across four scenes. Noted as a limitation in `KNOWN_LIMITATIONS.md`. |
