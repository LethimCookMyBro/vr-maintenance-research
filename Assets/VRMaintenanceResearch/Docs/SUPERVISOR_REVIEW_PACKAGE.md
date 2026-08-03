# Supervisor Review Package — Visual Redesign

**Branch:** `visual-polish-claude`
**Compared against:** commit `f117cc8` (last pre-redesign state) → working tree at commit `0371ecd`
**Prepared:** 2026-08-02
**Status:** review package. Nothing has been merged or pushed, and no scene, prefab or
script was modified while preparing it.

All measurements below were read out of the scenes themselves (Unity `Transform.position`,
`lossyScale`, `Renderer.bounds`, `Collider` settings), not from the earlier change log.
Where a measurement contradicts what `PROTOCOL_CHANGE_LOG.md` already records, the
contradiction is stated explicitly in [§7](#7-points-where-the-existing-change-log-needs-correcting).

---

## 1. What this package contains

| # | Deliverable | Section | State |
|---|---|---|---|
| 1 | Before/after screenshots, 8 subjects | [§2](#2-screenshots) | Complete |
| 2 | Transform / initial-state change table with impact columns | [§4](#4-transform-and-initial-state-change-table) | Complete |
| 3 | Partly disassembled initial state, Computer and Fan | [§5](#5-partly-disassembled-initial-state) | Complete |
| 4 | Windows standalone run of the full flow + CSV | [§6](#6-windows-standalone-verification) | Complete — flow and CSVs verified, **one blocking defect found** |
| 5 | Package warnings vs `Assets/VRMaintenanceResearch` warnings | [§8](#8-warning-origin-separation) | Complete |
| 6 | No merge, push or scene change | [§9](#9-repository-state) | Confirmed |

> **Read §7.4 first.** The standalone run found an unhandled exception thrown every frame
> in all three participant scenes, introduced by this redesign, which repeatedly forces the
> development console over the participant's view. It must be fixed before any participant
> session. The fix touches the scenes, so it has deliberately not been made here.

---

## 2. Screenshots

All new captures are in `Docs/Screenshots/Review/`. Every before/after pair was rendered
from an **identical camera pose** so the two images are directly comparable; the pose is
listed with each view. Renders are 1600 × 900.

### 2.1 How the "before" images were produced

The four pre-redesign scenes were extracted read-only from commit `f117cc8` into a
temporary folder, opened, captured, and the folder deleted. **The live scenes were never
opened for edit, never modified and never saved.** No prefab used by the pre-redesign
scenes was changed by the redesign (the redesign only *added* `LabEnvironment.prefab`), so
the "before" images show the pre-redesign state faithfully.

Two caveats on reading the "before" images:

* The pre-redesign scenes shared **one** material across every object, so untextured
  grey-on-grey is the actual former appearance, not a rendering fault. This is why
  `Before_*_StartPosition.png` (plan view) is close to unreadable — that flatness *is* the
  before state.
* The magenta dot visible in `Before_Computer_*` and `Before_Fan_*` is one of the two
  objects that rendered magenta under URP (built-in Standard shader). It is one of the
  defects the redesign fixed.

### 2.2 Subject index

| Subject | Before | After | Camera pose |
|---|---|---|---|
| ResearcherSetup (runtime UI) | `Before_ResearcherSetup_UI.png` | `After_ResearcherSetup_UI.png` | Runtime screen capture, not a scene camera |
| ResearcherSetup (scene) | `Before_ResearcherSetup_Overview.png` | `After_ResearcherSetup_Overview.png` | pos (−3.6, 2.8, −3.6) → look (0, 1.1, 0.6), 55° |
| VRTraining | `Before_Training_Overview.png`, `Before_Training_ParticipantEye.png` | `After_Training_Overview.png`, `After_Training_ParticipantEye.png`, `After_Training_UI.png` | overview as above; eye view from the scene's own camera pose |
| Computer task | `Before_Computer_Overview.png`, `Before_Computer_ParticipantEye.png` | `After_Computer_Overview.png`, `After_Computer_ParticipantEye.png` | as above |
| Fan task | `Before_Fan_Overview.png`, `Before_Fan_ParticipantEye.png` | `After_Fan_Overview.png`, `After_Fan_ParticipantEye.png` | as above |
| Participant starting position | `Before_*_StartPosition.png`, `Before_*_Elevation.png` | `After_*_StartPosition.png`, `After_*_Elevation.png` | orthographic plan from (0, 2.90, 0.70), size 2.85; orthographic elevation from (−4.40, 1.50, 0.70) looking east, size 1.90 |
| Information-source layout | `Before_*_InfoLayout.png` | `After_*_InfoLayout.png` | pos (0, 1.6, −2.6) → look (0, 1.35, 3.0), 60° |
| Device initial state | `Before_*_Device.png` | `After_*_Device.png` | pos (−1.9, 2.0, −1.7) → look (0, 1.15, 0.7), 50° |
| Removable / repair components | `Before_*_Components.png` | `After_*_Components.png` | pos (0, 2.15, −1.15) → look (0, 0.95, 0.95), 62° |
| Information panel, opened | — | `After_InformationPanel_Open.png` | pre-existing capture, retained for reference |

The **participant start marker** is the red capsule + floor disc drawn into the plan,
elevation and overview renders only. It is 1.70 m tall, 0.36 m wide, drawn at the XR
Origin's `Camera Offset` ground projection. It is a `HideFlags.HideAndDontSave` object
created at render time and destroyed immediately — it is not part of any scene.

In `Before_Computer_Overview.png` and `Before_Computer_Elevation.png` only the bottom of
the marker is visible: the rest is **inside** the 2.4 m desktop case. In
`Before_Fan_Elevation.png` the marker is completely hidden inside the 3 m fan body. This is
the "participant spawned inside the device volume" defect, shown directly.

Two artefacts to ignore in the *after* renders: the small white object at the participant
position is the XRI rig's controller-visual meshes sitting at the rig origin in edit mode
(they follow the headset at runtime), and the ceiling is cropped out of the plan views
because the plan camera sits at y = 2.90 m, just under the 3.00 m ceiling soffit.

---

## 3. Reference geometry (after)

| Item | Value |
|---|---|
| Room | 9.20 m × 8.20 m, 3.00 m clear height |
| Workbench top | centre (0, 0.88, 0.90), 4.60 × 0.08 × 0.90 → work surface at **y = 0.92**, front edge at **z = 0.45** |
| Participant start (all three participant scenes) | XR Origin `Camera Offset` at (0, 1.361, **−1.6**) |
| Participant → bench front edge | **2.05 m** |
| Marked work-zone floor line | front edge at z = −0.55, i.e. the participant starts **1.05 m behind the marked work zone** |
| Information tiles | x = −3.3 / −1.1 / +1.1 / +3.3, y = 1.0, z = 3.0, tile 1.2 × 0.7 m (**unchanged in world coordinates**) |
| Information panels | same x, y = 1.65, z = 1.5, inactive at start (**unchanged**) |

Pre-redesign, the participant start was (0, 1.361, **0**) and there was no workbench,
no room shell and no floor marking.

---

## 4. Transform and initial-state change table

Distances are horizontal distance from the participant's own start position in that build,
so they are directly comparable across the redesign even though the world origin offset
changed. Angular size is the horizontal angle the object's widest face subtends from the
start eye position.

### 4.1 Participant, environment and information layout

| Change | Before | After | Walking | Reaching | Visibility | Source salience | Difficulty | Movement log | Completion sequence |
|---|---|---|---|---|---|---|---|---|---|
| Participant start pose | (0, 1.361, 0) | (0, 1.361, −1.6) | **Yes** — start is now 2.05 m from the bench edge and 1.05 m outside the marked work zone; nothing is reachable without locomoting | **Yes** — nearest interactable goes from 0.75 m (Fan) / 1.13 m (Computer) to 2.30 m; no target is within arm's reach at spawn | **Yes** — participant is no longer inside the device mesh | **Yes** — see next row | Neutral to slightly higher (locomotion added before any manipulation) | **Yes** — all logged head/controller world coordinates shift by −1.6 m in z relative to the participant; pre/post sessions are not spatially comparable | No change to the rule; adds travel time before the first interaction |
| Information tiles | x ±1.1 / ±3.3, y 1.0, z 3.0 | **identical world coordinates** | Distance to nearest tile 3.20 → **4.73 m**; farthest 4.46 → **5.66 m** | n/a (tiles are ray-selected) | Tile angular width: inner 21.2° → **14.5°**, outer 15.3° → **12.1°** | **Yes, and not in the direction the change log states.** Outer:inner distance ratio 1.394 → **1.197**, i.e. the four sources became *more* equidistant. Head-turn to the outer tile 47.7° → **35.7°** | Slightly higher — smaller tiles | Longer approach to a source raises movement rows per source visit | Unchanged |
| Training information source | (2.6, 1.0, 1.4), scale 1.25 × 0.80 × 0.12 | (0, 1.0, 3.0), scale 1.20 × 0.70 × 0.10 | 2.95 → **4.60 m** | n/a | Now front-and-centre instead of 62° to the participant's right | Now matches task-source geometry exactly | Lower — no longer off to one side | Yes, same origin-shift effect | Unchanged |
| Room shell, lighting, floor markings, furniture | none | Added: 4 walls + ceiling, 2-light rig + ceiling panels, work-zone floor lines, workbench, trays, pedestal, storage unit, information station | Adds physical boundaries; does not restrict the 2.05 m approach | No | Substantially higher — surfaces, contrast and shading now differentiate objects | Indirect — the information station backing gives the four tiles a shared frame | Lower (clearer affordances) | No | No |
| Task status board | none | Read-only board above the information station | No | No | New always-visible surface | No — carries no source content | Slightly lower (attempt number is visible) | No — writes no events | **No** — mirrors state, never names the faulty component |
| Researcher controls | Always-visible IMGUI panel | Collapsed by default, opened with the handle or **F9**; Safety Stop separated | No | No | Removes a persistent overlay from the participant view | No | No | No | No — every control calls the same task method |
| Training instruction panel | World canvas with `localScale (0,0,0)` — rendered nothing | Fixed world-space board with live progress and a gated Continue button | No | No | **Yes** — previously invisible | No | Lower — the three training requirements are now legible | No | Continue is gated on the same three requirements |
| Source control buttons | Next left of Prev; Seek left of Play | Mirrored about the panel centre | No | No | No | No — positions stay symmetric and identical across all four sources and both tasks | Lower (reading order corrected) | No | No |

### 4.2 Computer task objects

`d` = horizontal distance from participant start. `∠` = angular width from the start eye position.

| Stable ID | Before pos → After pos | d before → after | Widest dimension before → after | ∠ before → after | Notes |
|---|---|---|---|---|---|
| `computer.case` | (0, 1.1, 0) → (0, 1.2, 1.15) | 0.00 → 2.75 m | 2.40 → **0.46 m** | participant was inside it → 9.6° | Tower now stands on the bench |
| `computer.side-panel` | (−1.55, 0.95, 1.35) → (−1.72, 0.945, 1.15) | 2.06 → 3.24 m | 1.20 → 0.50 m | 32.0° → 8.8° | Upright slab → flat plate lying on the bench |
| `computer.motherboard` | (0, 0.95, 1.42) → (−0.45, 0.935, 1.05) | 1.42 → 2.69 m | 1.25 → 0.40 m | 47.4° → 8.5° | Laid flat on the bench |
| `computer.psu` | (1.1, 0.95, 1.28) → (0.45, 1.0, 1.1) | 1.69 → 2.74 m | 0.46 → 0.26 m | 15.3° → 5.4° | On the bench beside the tower |
| `computer.psu-switch` | (1.1, 1.38, 1.05) → (0.45, 1.02, 0.965) | 1.52 → 2.60 m | 0.18 → **0.05 m** | 6.8° → **1.1°** | Small target — see §7.3 |
| `computer.cooling-fan` | (−0.82, 1.0, 1.4) → (0.12, 0.945, 0.7) | 1.62 → 2.30 m | 0.36 → 0.20 m | 12.7° → 5.0° | Front row of the bench |
| `computer.internal-cable` | (0.42, 1.45, 1.12) → (−0.12, 0.945, 0.7) | 1.20 → 2.30 m | 0.22 → 0.10 m | 10.5° → 2.5° | Front row of the bench |
| `computer.external-power-cable` | (1.75, 0.18, 1.1) → (0.82, 0.935, 0.7) | 2.07 → 2.44 m | 1.38 → 0.44 m | 37.0° → 10.2° | Was on the floor; now on the bench |
| `computer.main-power-connector` **(correct repair)** | (−1.7, 1.0, 0.9) → (−1.28, 0.975, 0.95) | 1.92 → 2.85 m | 0.45 → **0.15 m** | 13.3° → **3.0°** | In the parts tray |
| `computer.ram` (incorrect) | (−0.9, 1.0, 0.9) → (−0.92, 0.975, 0.95) | 1.27 → 2.71 m | 0.45 → 0.15 m | 19.9° → 3.2° | In the parts tray, beside the connector |
| `computer.tool.screwdriver` | (1.4, 1.0, 0.8) → (1.1, 0.962, 0.95) | 1.61 → 2.78 m | 1.10 → 0.26 m | 37.8° → 5.4° | In the tool tray |
| `computer.power-button` **(device test)** | (1.6, 1.0, −0.2) → (1.95, 0.95, 0.2) | 1.61 → 2.65 m | 0.30 → 0.11 m | 10.6° → 2.4° | On the separate control pedestal |
| `computer.non-target-module` | (−1.05, 0.55, 0.42) → (1.7, 0.99, 1.15) | 1.13 → 3.23 m | 0.30 → 0.20 m | 15.1° → 3.6° | Moved from the participant's left to the right |

Repair-site → device-test travel: **3.48 m before → 3.32 m after** (effectively unchanged).

### 4.3 Fan task objects

| Stable ID | Before pos → After pos | d before → after | Widest dimension before → after | ∠ before → after | Notes |
|---|---|---|---|---|---|
| `fan.body` | (0, 1.1, 0) → (0, 1.28, 1.02) | 0.00 → 2.62 m | 3.00 m tall, clipping through the floor → 0.30 m head | participant was inside it → 6.6° | Head on a stand, on the bench |
| `fan.blade` | (0, 1.2, 0.98) → (0, 1.28, 0.88) | 0.98 → 2.48 m | 0.54 → 0.22 m | 30.9° → 5.1° | On the participant-facing side of the head |
| `fan.front-cover` | (0, 1.2, 1.15) → (−0.55, 0.95, 0.7) | 1.15 → 2.36 m | 0.84 → 0.36 m | 40.2° → 8.7° | **Detached and laid on the bench** — the only true initial-state change |
| `fan.fuse-holder` | (0.75, 0.55, 0.82) → (0.2, 1.24, 1.02) | 1.11 → 2.63 m | 0.35 → 0.09 m | 17.7° → 2.0° | Mounted on the head |
| `fan.internal-wire` | (0.92, 0.82, 0.98) → (0.12, 1.13, 1.02) | 1.34 → 2.62 m | 0.68 → 0.17 m | 28.6° → 3.7° | Exposed on the head |
| `fan.motor-module` | (0, 0.95, 0.75) → (0.55, 0.97, 0.7) | 0.75 → 2.36 m | 0.50 → 0.16 m | 36.9° → 3.9° | Removed, on the bench |
| `fan.fastener` | (−0.72, 0.82, 0.94) → (−0.16, 1.2, 0.94) | 1.18 → 2.55 m | 0.10 → **0.022 m** | 4.9° → **0.49°** | Smallest target in the project — see §7.3 |
| `fan.power-switch` | (0.18, 0.52, 1.05) → (0, 0.972, 0.88) | 1.07 → 2.48 m | 0.24 → 0.10 m | 12.8° → 2.3° | On the stand base |
| `fan.power-cord` | (−0.95, 0.18, 0.82) → (−0.5, 0.942, 1.26) | 1.25 → 2.90 m | 1.16 → 0.44 m | 49.5° → 8.7° | Was on the floor; now on the bench |
| `fan.power-plug` | (−1.45, 0.18, 0.8) → (−0.86, 0.962, 1.26) | 1.66 → 2.99 m | 0.24 → 0.09 m | 8.2° → 1.7° | On the bench |
| `fan.working-fuse` **(correct repair)** | (−1.7, 1.0, 0.9) → (−1.28, 0.975, 0.95) | 1.92 → 2.85 m | 0.45 → 0.15 m | 13.3° → 3.0° | In the parts tray |
| `fan.faulty-fuse` (incorrect) | (−0.9, 1.0, 0.9) → (−0.92, 0.975, 0.95) | 1.27 → 2.71 m | 0.45 → 0.15 m | 19.9° → 3.2° | In the parts tray, beside the working fuse |
| `fan.tool.screwdriver` | (1.4, 1.0, 0.8) → (1.1, 0.962, 0.95) | 1.61 → 2.78 m | 1.10 → 0.26 m | 37.8° → 5.4° | In the tool tray |
| `fan.speed-selector` **(device test)** | (1.6, 1.0, −0.2) → (1.95, 0.95, 0.2) | 1.61 → 2.65 m | 0.30 → 0.11 m | 10.6° → 2.4° | On the control pedestal |
| `fan.non-target-module` | (1.36, 0.38, 0.44) → (1.7, 0.99, 1.15) | 1.43 → 3.23 m | 0.26 → 0.20 m | 10.4° → 3.6° | On the bench |

### 4.4 Training task objects

| Stable ID | Before → After | d before → after | Size before → after |
|---|---|---|---|
| `training.training-cube-a` | (−1.2, 1, 0) → (−0.45, 1, 0.95) | 1.20 → 2.59 m | 0.30 → 0.12 m cube |
| `training.training-cube-b` | (0, 1, 0) → (0, 1, 0.95) | 0.00 → 2.55 m | 0.30 → 0.12 m cube |
| `training.training-cylinder` | (1.2, 1, 0) → (0.45, 1, 0.95) | 1.20 → 2.59 m | Ø0.30 × 0.60 → Ø0.11 × 0.12 m |

The three training grabbables keep their `XRGrabInteractable` and non-kinematic
`Rigidbody`; they were verified to settle on the workbench collider at y = 0.996 rather
than falling through.

### 4.5 What did **not** change

Verified by direct comparison of the two scene sets:

* Every stable research ID (13 Computer, 15 Fan, 3 Training) — identical sets, no additions,
  removals or renames.
* Every `objectCategory`, `ResearchInteractionKind` and `isCorrect` flag.
* Every interactable's task reference.
* Interactor component type on every object (`XRSimpleInteractable` for all task objects,
  `XRGrabInteractable` for the three training objects).
* Collider *type* on every object (Box / Capsule / Sphere as before); only the scale changed.
* `activeSelf` / `activeInHierarchy` / `Renderer.enabled` on every interactable — all
  active and enabled at start, before and after.
* The four information-source definition assets bound to each scene (`*_v2`), and the
  content panels' inactive-at-start state.
* Information tile and panel world transforms.
* The logging schema, the CSV column set, task-relative timestamps, retry behaviour and
  the completion rule (correct component, then device test).

---

## 5. Partly disassembled initial state

Both devices are presented **already opened up**, so that every logged component is
individually ray-reachable and none is hidden inside a closed shell. Nothing is disabled
or hidden — every component listed is active, rendered and interactable from the first
frame of the task.

### 5.1 Computer — state of each part at task start

| Stable ID | Object | Initial state | Where |
|---|---|---|---|
| `computer.case` | Desktop tower | **Open** — stands on the bench with its side panel off | Bench centre, (0, 1.2, 1.15) |
| `computer.side-panel` | Side panel | **Removed and detached** — lying flat on the bench, left end | (−1.72, 0.945, 1.15) |
| `computer.motherboard` | Motherboard | **Removed and detached** — laid flat on the bench | (−0.45, 0.935, 1.05) |
| `computer.psu` | Power supply | **Removed and detached** — on the bench beside the tower | (0.45, 1.0, 1.1) |
| `computer.psu-switch` | PSU switch | **Installed** on the front face of the removed PSU | (0.45, 1.02, 0.965) |
| `computer.cooling-fan` | Cooling fan | **Removed and detached** — front row of the bench | (0.12, 0.945, 0.7) |
| `computer.internal-cable` | Internal cable connector | **Removed and detached** — front row of the bench | (−0.12, 0.945, 0.7) |
| `computer.external-power-cable` | External power cable | **Detached** — coiled on the bench, not plugged in | (0.82, 0.935, 0.7) |
| `computer.main-power-connector` | Replacement main power connector | **Spare, not installed** — in the parts tray | (−1.28, 0.975, 0.95) |
| `computer.ram` | RAM module | **Spare, not installed** — in the parts tray beside the connector | (−0.92, 0.975, 0.95) |
| `computer.non-target-module` | Non-target module | **Detached** — on the bench, right of the tower | (1.7, 0.99, 1.15) |
| `computer.tool.screwdriver` | Screwdriver | **Available** — in the tool tray | (1.1, 0.962, 0.95) |
| `computer.power-button` | Power button (device test) | **Available** — on the control pedestal, right of the bench | (1.95, 0.95, 0.2) |

**Nothing is installed inside the tower at task start.** The correct repair
(`computer.main-power-connector`) and the plausible distractor (`computer.ram`) both begin
as loose spares lying side by side in the parts tray — the participant does not have to
remove anything before fitting the correct part. This arrangement is **unchanged** from the
pre-redesign scene; only the coordinates and scales differ.

### 5.2 Fan — state of each part at task start

| Stable ID | Object | Initial state | Where |
|---|---|---|---|
| `fan.body` | Fan head and stand | **Open** — front cover off, blade and internals exposed | Bench centre, (0, 1.28, 1.02) |
| `fan.front-cover` | Front guard | **Removed and detached** — lying flat on the bench, left of the fan | (−0.55, 0.95, 0.7) |
| `fan.blade` | Blade | **Installed and exposed** — on the participant-facing side of the head | (0, 1.28, 0.88) |
| `fan.fuse-holder` | Fuse holder | **Installed and exposed** — on the head, upper right | (0.2, 1.24, 1.02) |
| `fan.internal-wire` | Internal wire | **Installed and exposed** — on the head, below the fuse holder | (0.12, 1.13, 1.02) |
| `fan.motor-module` | Motor module | **Removed and detached** — on the bench, right of the fan | (0.55, 0.97, 0.7) |
| `fan.fastener` | Fastener | **Loose** — resting beside the head | (−0.16, 1.2, 0.94) |
| `fan.power-switch` | Power switch | **Installed** on the stand base | (0, 0.972, 0.88) |
| `fan.power-cord` | Power cord | **Detached** — on the bench behind the fan, not plugged in | (−0.5, 0.942, 1.26) |
| `fan.power-plug` | Power plug | **Detached** — on the bench, at the end of the cord | (−0.86, 0.962, 1.26) |
| `fan.working-fuse` | Working replacement fuse | **Spare, not installed** — in the parts tray | (−1.28, 0.975, 0.95) |
| `fan.faulty-fuse` | Faulty fuse | **Not installed** — in the parts tray, beside the working fuse | (−0.92, 0.975, 0.95) |
| `fan.non-target-module` | Non-target module | **Detached** — on the bench, right | (1.7, 0.99, 1.15) |
| `fan.tool.screwdriver` | Screwdriver | **Available** — in the tool tray | (1.1, 0.962, 0.95) |
| `fan.speed-selector` | Speed selector (device test) | **Available** — on the control pedestal | (1.95, 0.95, 0.2) |

**The faulty fuse does not begin fitted in the fuse holder.** Both fuses start loose, side
by side in the parts tray. The participant therefore never has to extract a failed part —
the task reduces to selecting the correct one of two visually similar spares and then
running the device test. This is **unchanged** from the pre-redesign scene, but it is worth
an explicit decision before data collection, because it removes a removal step that the
task description implies.

---

## 6. Windows standalone verification

**Executed. The complete flow ran outside the Unity Editor and produced correct CSV
output — and it surfaced a blocking defect, §7.4.**

Artefact: `Builds/Windows/VRMaintenanceResearch/VRMaintenanceResearch.exe`, Development +
Allow Debugging, launched windowed at 1280 × 720. No rebuild was needed: the scenes were
last saved at 15:43 and the player data (`globalgamemanagers`, `level0`–`level4`) was
written at 15:48, so this binary already contains the redesigned scenes.

Session `20260802T142853Z_7e839ab4`, participant code `REVIEW_STANDALONE`, Thai group /
English / Computer → Fan, training required, DEVELOPMENT_TEST mode, XR simulator mode.

### 6.1 Flow executed

| Step | Observed | Screenshot |
|---|---|---|
| ResearcherSetup | Redesigned two-column TextMeshPro screen rendered correctly in the player; participant code accepted; Start Session worked | `r1-setup.png`, `r2-code.png` |
| → VRTraining | Loaded; world-space training board rendered with all three requirement checkboxes and **Continue (locked)**, confirming the gate works outside the Editor | `r3-training.png` |
| Researcher panel | Opened via the on-screen handle; training controls present | `r4-training-panel.png` |
| → ComputerRepairTask | Loaded; task status board read **"Computer Maintenance Task / Status: in progress / Attempt 1"**; lab, bench, tower and all four information tiles rendered | `r5-computer.png` |
| Researcher controls | Pause → Resume → Abort Task; panel state tracked `Active` → `Aborted`; board changed to **"Stopped by researcher"**; **Continue to Next Task** appeared only after the task ended | `r6-computer-panel.png`, `r7-computer-aborted.png` |
| → FanRepairTask | Loaded; board read **"Fan Maintenance Task / in progress / Attempt 1"** | `r8-fan.png` |
| Researcher controls | Retry → Abort Task | `r9-fan-retry.png`, `r10-fan-aborted.png` |
| → ResearcherSetup | Session ended and returned to setup with the generated Session ID displayed | `r11-return-setup.png` |
| Exit | Closed cleanly with Alt+F4 | — |

Screenshots, the manifest, the task summary and a log excerpt are in
`Docs/Screenshots/Review/Standalone/`. The run was driven with the researcher controls
only, per the agreed scope — a `Completed` repair outcome outside the Editor would need
hand-driven XR simulator interaction, because the panel exposes Pause, Resume, Retry, Reset
Task, Abort Task, Safety Stop and Continue to Next Task but **no force-complete**.

### 6.2 CSV output on disk

`C:\Users\User\AppData\LocalLow\Unity Technologies\XRI Examples\VRMaintenanceResearchData\Development\20260802T142853Z_7e839ab4`

| File | Rows (incl. header) | Result |
|---|---|---|
| `session_manifest.csv` | 2 | 1 row; `session_completion_status = Completed`, `logging_status = active`, `platform = WindowsPlayer`, `unity_version = 6000.3.20f1`, `xri_version = 3.4.0`, all three layout IDs populated, `movement_sampling_hz = 10` |
| `task_summary.csv` | 4 | 3 rows: **Training `Completed`** (128.37 s), **Computer `Aborted`** (59.50 s, `abort_status = true`), **Fan `Aborted`** (35.42 s, `abort_status = true`, `retry_count = 1`) |
| `session_events.csv` | 3 | Session start/end |
| `Training/events.csv` | 8 | — |
| `Training/movement.csv` | 3 790 | 3 devices × 10 Hz × ~126 s ✓ |
| `Computer/events.csv` | 10 | `TaskLoaded`, `TaskStarted`, hover, `LowActivityStarted`, `TaskPaused`, `LowActivityEnded`, `TaskResumed`, `TaskAborted` — sequence numbers continuous across tasks (9–17) |
| `Computer/movement.csv` | 1 717 | 3 devices × 10 Hz × ~57 s ✓ |
| `Fan/events.csv` | 9 | includes `RetryStarted` |
| `Fan/movement.csv` | 1 051 | 3 devices × 10 Hz × ~35 s ✓ |
| `technical_log.txt` | 0 bytes | no technical notes recorded |

Schema, task-relative timestamps, continuous session-wide event sequence numbering,
low-activity detection, one summary row per task attempt and the pause/resume/retry/abort
transitions all behave outside the Editor exactly as they do in Play Mode.

Behaviour note: **Retry increments `retry_count` but not `task_attempt_id`.** The Fan row
is `task_attempt_id = 1, retry_count = 1`. `Reset Task` is the control that starts a new
attempt row. This matches the earlier Editor runs and is not a redesign change, but the two
controls are easy to confuse and the distinction should be in the researcher procedure.

Confirmation of §7.6: `Computer/movement.csv` row 1 records the headset at
`(0.000000, 1.361440, −1.600000)` with `coordinate_space_id = task-local`. The values are
world coordinates and carry the −1.6 m start offset.

---

## 7. Points where the existing change log needs correcting

### 7.1 "Information-source tiles and panels — unchanged"

`PROTOCOL_CHANGE_LOG.md` records the tiles as deliberately unmoved so that relative
salience is preserved. The tiles' **world** transforms are indeed unchanged, but the
participant moved 1.6 m back, so every participant-relative quantity changed:

| Measure | Before | After |
|---|---|---|
| Distance to inner tiles (x = ±1.1) | 3.20 m | 4.73 m |
| Distance to outer tiles (x = ±3.3) | 4.46 m | 5.66 m |
| Outer : inner distance ratio | 1.394 | **1.197** |
| Inner tile angular width | 21.2° | 14.5° |
| Outer tile angular width | 15.3° | 12.1° |
| Head turn to an outer tile | 47.7° | 35.7° |

The four sources became **more nearly equidistant and more nearly equal in angular size**,
and all four moved further into the forward field of view. For a study whose independent
variable is information-source type, this is arguably an improvement — but it is a change
to relative salience, not the absence of one, and the change log currently states the
opposite. It should be recorded and approved rather than left as "unchanged".

### 7.2 "Fan front guard — its collider blocked controller rays to `fan.blade`"

Measured pre-redesign geometry: the guard sat at z = 1.15 and the blade at z = 0.98, i.e.
the **blade was nearer the participant than the guard**. Both colliders are capsules that
Unity clamps to spheres at those scales — guard radius 0.42 m spanning z 0.73–1.57, blade
radius 0.27 m spanning z 0.71–1.25. A ray aimed at the blade centre would have hit the
blade first (0.71 before 0.73); only off-axis aim, more than 0.27 m from the blade centre,
would have hit the guard instead.

So the stated reason is only partly supported. Detaching the guard is still defensible —
it is what makes the fan read as an opened maintenance scene — but the justification in the
change log should be corrected to "off-axis rays could hit the guard instead of the blade",
not "the guard blocked the blade".

### 7.3 Target size is not mentioned at all, and it dropped sharply

Components were re-authored to human scale, which shrank them well below the reduction the
change log implies. From the participant's start pose:

| Object | Angular width before | Angular width after | Factor |
|---|---|---|---|
| `fan.fastener` | 4.9° | **0.49°** | 10× smaller |
| `computer.psu-switch` | 6.8° | 1.1° | 6× smaller |
| `fan.power-plug` | 8.2° | 1.7° | 5× smaller |
| `fan.fuse-holder` | 17.7° | 2.0° | 9× smaller |
| `computer.main-power-connector` (correct repair) | 13.3° | 3.0° | 4× smaller |
| `fan.working-fuse` (correct repair) | 13.3° | 3.0° | 4× smaller |

Even standing at the bench edge (≈ 0.5 m from a part), `fan.fastener` subtends about 2.5°.
Ray-pointing accuracy and hover/selection event rates are directly affected, so this is a
task-difficulty change that needs sign-off, and it is a plausible source of accidental
`IncorrectComponentInteraction` events. It is not currently recorded anywhere.

### 7.4 Blocking defect — `ResearcherTaskControls` throws every frame in all three participant scenes

Found by the standalone run in §6. This is the most serious item in the package.

```
ArgumentOutOfRangeException: Specified argument was out of the range of valid values.
Parameter name: key: 290
  at UnityEngine.InputSystem.Keyboard.get_Item (UnityEngine.InputSystem.Key key)
  at TMUVR.MaintenanceResearch.ResearcherTaskControls.Update ()
     … Assets/VRMaintenanceResearch/Scripts/UI/ResearcherTaskControls.cs:45
```

**Cause.** The redesign added `[SerializeField] Key toggleKey = Key.F9;`, where `Key` is
`UnityEngine.InputSystem.Key`. All three participant scenes serialise `toggleKey: 290`:

```
Assets/VRMaintenanceResearch/Scenes/VRTraining.unity:759:        toggleKey: 290
Assets/VRMaintenanceResearch/Scenes/ComputerRepairTask.unity:7809: toggleKey: 290
Assets/VRMaintenanceResearch/Scenes/FanRepairTask.unity:667:      toggleKey: 290
```

290 is `UnityEngine.KeyCode.F9`, the **legacy** input enum. `InputSystem.Key.F9` is a
different, much smaller value, so `keyboard[290]` is out of range and
`ResearcherTaskControls.Update()` throws on **every frame** the component is enabled.

**Introduced by this redesign.** Neither the field nor the serialised value exists at
`f117cc8`: the pre-redesign `ResearcherTaskControls.cs` had no key field at all.

**Measured impact in the standalone run:** **129 081 exceptions in a 4½-minute session** —
one per frame, confirmed three ways in the player log (message, parameter line and
`ResearcherTaskControls.Update` stack frame all appear exactly 129 081 times). The log grew
to 81 MB / 904 155 lines from that session alone. In a development build this repeatedly
forces the Unity **Development Console overlay open across the participant's view** —
visible in every task screenshot in §6.1 — during a live session. In a release build the
exception and its stack trace would still be written to the player log every frame.

**Functional consequences:**

* The **F9 shortcut does not work at all** in any scene. The panel can only be opened with
  the on-screen handle. `KEYBOARD_MOUSE_CONTROLS.md` and the researcher procedure currently
  imply F9 works.
* `Update()` aborts at its first statement each frame. That statement is the only thing in
  `Update()`, so no other logic is lost — but the exception is unhandled and continuous.
* Nothing else in the session is affected: the flow, state machine and CSV output in §6 are
  all correct despite the error storm.

**Why this was not caught earlier.** `TEST_REPORT.md` records "zero Console errors" for the
redesign. The exception only occurs in Play Mode / the player, and the Editor Console
collapses identical entries, so it is easy to miss among a busy Console.

This is a one-line fix, but it is a **scene** change as well as a code change, so it has
deliberately **not** been made — no scenes were altered while preparing this package.
It should be scheduled before any participant session.

### 7.5 The first-action metric fires 14 ms into every task

Also found by the standalone run. `task_summary.csv` records, for both tasks:

| Task | `first_meaningful_action` | `…_timestamp_seconds` | `action_occurred_before_first_information_access` |
|---|---|---|---|
| Computer | `DeviceHovered` | **0.013926** | `true` |
| Fan | `ComponentHovered` | **0.013359** | `true` |

Nobody did anything in those 14 ms. The participant spawns at (0, 1.361, −1.6) facing +Z
with the controller rays pointing forward, so the ray lands on `computer.case` /
`fan.blade` on the first frame after `TaskStarted` and an incidental hover is logged as the
first meaningful action.

Two consequences for the analysis:

* `first_meaningful_action_timestamp_seconds` is not measuring participant behaviour; it is
  measuring the spawn geometry, and will read ≈ 0.014 s for every participant.
* `action_occurred_before_first_information_access` will therefore be `true` for every
  session in which any information source is opened at all — which is precisely the
  variable the study is designed to measure.

This is very likely **pre-existing** rather than redesign-introduced (before the redesign
the participant spawned inside the device mesh, which would also hover immediately), but
the new start pose does not fix it and the new geometry preserves it. Either the hover
event should be excluded from the "meaningful action" definition, or the initial ray should
not be aimed at the device.

Related: the same hover is logged **twice** at an identical timestamp (event sequence 11
and 12 for Computer, 20 and 21 for Fan) because both the left and right controller hover
the same object. Hover counts are therefore doubled whenever both rays are on one target.

### 7.6 Movement CSV frame label

`ResearchLogService.LogMovement` writes `pose.position`, which is a **world** position, but
the CSV's frame column is the literal string `"task-local"`. The values have always been
world-space, so this is a pre-existing labelling defect rather than something the redesign
introduced — but combined with the −1.6 m origin shift it will mislead anyone comparing
sessions recorded before and after 2026-08-02. Either the column value or the schema
documentation should be corrected before data collection.

---

## 8. Warning origin separation

### 8.1 Compiler warnings — current source

There is no assembly definition under `Assets/VRMaintenanceResearch`, so the research
scripts compile into `Assembly-CSharp` together with every other `Assets/` script. A
research-owned compiler warning surfaces in exactly the same place as any other `Assets/`
warning and is distinguished only by the file path in the message.

| Origin | Errors | Warnings |
|---|---|---|
| `Assets/VRMaintenanceResearch` | **0** | **0** |
| Other `Assets/` | 0 | 0 |
| `Packages/` and `Library/PackageCache` | 0 | 0 |

Evidence, in order of strength:

* The one research-owned compiler warning that existed during the redesign —
  `ResearchUiKit.cs(75,13): warning CS0618: 'TMP_Text.enableWordWrapping' is obsolete` — is
  gone from the source, which now uses `textWrappingMode` (verified by reading the file).
  It is the **only** distinct `warning CS` message that `Assets/VRMaintenanceResearch` has
  ever produced in this Editor session's log.
* The current `Assembly-CSharp.dll` was compiled at 15:51:46, after that fix. **No
  `warning CS` or `error CS` line appears anywhere in the Editor log after that compile.**
* A recompilation was requested during this review; Unity found the assemblies already up
  to date and rebuilt nothing, and the Console reports 0 errors and 0 warnings.

### 8.2 Build warnings — the 486 reported by the last Windows build

Counted directly from the Editor log window covering that build (lines 616 000–696 868,
ending at the build report), excluding asset-import chatter. Total: **491 warning lines,
485 distinct shader warnings + 6 duplicate emissions of 1 compiler warning**, which is how
Unity arrives at a reported count of 486.

| Origin | Warning lines | Distinct messages | Class |
|---|---|---|---|
| `Packages/com.unity.ai.inference` — `ConvGeneric.compute` | 460 | 5 | `TRANSPOSE_OUTPUT variants not supported with NON_UNIFORM_CONVGROUP_PER_OC`, once per compiled kernel variant |
| `Packages/com.unity.ai.inference` — `ScatterND`, `ConvTranspose`, `GridSample`, `ScatterElements`, `SliceSet`, `Pad` | 25 | 8 | Signed/unsigned mismatch, integer modulus/divide performance, loop-variable shadowing |
| **Packages subtotal** | **485** | **13** | all shader-compilation warnings |
| `Assets/VRMaintenanceResearch` | 6 | **1** | `ResearchUiKit.cs(75,13) CS0618` obsolete TMP API, emitted once per compile pass, from **before** the fix |
| Other `Assets/` | 0 | 0 | — |

**485 of the 486 build warnings originate in `Packages/com.unity.ai.inference`** (Unity's
AI Inference / Sentis package). They are shader-compilation warnings for compute kernels
the research scenes never use; the package is pulled in by the project, not by this work.

The one research-owned build warning is the obsolete-TMP-API warning, and it is already
fixed in source. The shipped `VRMaintenanceResearch.exe` was built at 15:48 from an
`Assembly-CSharp` compiled before the 15:51 fix, so **that specific binary still contains
the pre-fix call**. The difference is an obsolete-API alias with identical behaviour, so it
does not affect the standalone verification in §6 — but a rebuild would drop the count from
486 to 485.

### 8.3 Runtime warnings in Play Mode

Four warnings appear when Play Mode is entered on this machine, all from packages, none
from research code:

| Message | Origin |
|---|---|
| `XR: Error setting active audio output driver. Falling back to default.` (×2) | `com.unity.xr.management`, `com.unity.xr.openxr` |
| `Failed to get haptic capabilities of XRSimulatedController… error code −1` (×2) | `com.unity.xr.interaction.toolkit` |

These are consequences of running without a headset and using the XR simulator. The
OpenXR `XR_ERROR_FORM_FACTOR_UNAVAILABLE` message in the standalone runtime log has the
same cause. Physical Meta Quest 3 validation remains pending and no Quest performance
claim is made.

### 8.4 Caveat on the recompile

The first recompile request used `RequestScriptCompilationOptions.CleanBuildCache`, which
failed after about two minutes with a Tundra build error on
`Unity.RenderPipelines.Core.ShaderLibrary.dll` and two other **package** assemblies. Unity
kept the previously built assemblies and no research assembly was affected. A normal
recompilation was then requested; Unity found all 149 assemblies up to date and rebuilt
nothing, and the Editor is idle with an error-free Console.

Because that clean rebuild did not complete, §8.1 rests on the Editor log and the source
itself rather than on a from-scratch compile. It should be re-confirmed with a clean Editor
restart before the package is signed off.

---

## 9. Repository state

* Branch `visual-polish-claude`, HEAD `0371ecd`. **Nothing merged, nothing pushed.**
* No `.unity` scene file was modified. The four scenes were opened read-only for
  measurement and capture and were never saved; `git status` shows no scene changes.
* No prefab, material, script or ScriptableObject was modified.
* The pre-redesign scene copies used for the "before" captures were extracted to a
  temporary folder outside the research tree and deleted afterwards.
* The standalone run in §6 wrote one new session folder under the user's
  `AppData\LocalLow` research-data directory. It is outside the repository and outside the
  Editor's `Development` runs used for earlier testing; nothing existing was overwritten.
* Files added by this package, all documentation:
  * `Docs/SUPERVISOR_REVIEW_PACKAGE.md` (this file)
  * `Docs/Screenshots/Review/` — 51 before/after PNGs
  * `Docs/Screenshots/Review/Standalone/` — 11 run screenshots, `session_manifest.csv`,
    `task_summary.csv`, `standalone-run-excerpt.log`

---

## 10. Decisions requested

**Must fix before any participant session**

1. **`toggleKey` exception** — §7.4. Set `toggleKey` to a valid `UnityEngine.InputSystem.Key`
   value in all three participant scenes (or guard the lookup in `ResearcherTaskControls`).
   Requires a scene edit, so it is not done here. Rebuild afterwards.
2. **First-action metric** — §7.5. `first_meaningful_action` fires at ≈ 0.014 s in every
   task from an incidental spawn-time ray hover, and
   `action_occurred_before_first_information_access` is consequently `true` in every
   session. Decide whether hover counts as a meaningful action, and whether the initial ray
   should point at the device.

**Protocol decisions**

3. **Participant start pose** — approve 2.05 m from the bench edge and 1.05 m outside the
   marked work zone, or move the start into the work zone. Note that no target is reachable
   at spawn, so every task now begins with locomotion.
4. **Information-source salience** — §7.1. The four sources became more equidistant and more
   nearly equal in angular size. Approve as an improvement, or restore the pre-redesign
   participant-relative geometry by moving the tile row 1.6 m closer.
5. **Target size** — §7.3. Approve the reduced component sizes, or raise a floor on the
   smallest targets (`fan.fastener`, `computer.psu-switch`, `fan.internal-wire`).
6. **Fan fuse initial state** — §5.2. Confirm that both fuses starting loose in the tray,
   with nothing to extract, is the intended task.

**Data-schema decisions**

7. **Movement CSV frame label** — §7.6. Correct the column value to `world`, or convert the
   logged pose to a genuine task-local frame.
8. **Duplicate hover events** — §7.5. Decide whether a left-and-right-controller hover of
   the same object should log one event or two; it currently logs two.

**Remaining verification**

9. A hand-driven `Completed` run outside the Editor (correct component + device test
   through the XR simulator) has still not been done — §6 used the researcher controls, so
   Computer and Fan recorded `Aborted`. Confirm whether that is needed before sign-off.
10. Physical Meta Quest 3 validation remains outstanding. No Quest performance claim is
    made anywhere in this package.
