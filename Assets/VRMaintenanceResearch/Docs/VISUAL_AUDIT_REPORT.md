# Visual / UI Audit — VR Maintenance Research

Date: 2026-08-03 · Branch: `visual-polish-claude`

All screenshots are in `Docs/Screenshots/Audit/`, rendered from fixed camera poses by
`Tools/VR Maintenance Research/Visual Audit/Capture BEFORE|AFTER`, so before and after
are the same pose, FOV and resolution. Machine checks are in
`Docs/Verification/VisualAudit_Validation.txt`.

Reproduce the whole thing with two menu items:

1. `Tools/VR Maintenance Research/Visual Audit/REBUILD ALL`
2. `Tools/VR Maintenance Research/Visual Audit/Validate Scenes`

---

## 1. Root causes found

Four defects explained most of what looked wrong across every scene:

| # | Root cause | Effect |
|---|---|---|
| 1 | No reflection probe and no tonemapper; ambient at 1.0 doing all the lighting | Every surface rendered the same chalky grey and lit areas clipped to pure white. "Untextured greybox" look. |
| 2 | All materials shared near-identical roughness | Metal, plastic, PCB and rubber never separated. |
| 3 | Information sources parented on the bench's back edge at head height | Manuals sat directly over the spare-parts tray — the blocking problem. |
| 4 | Placeholder objects kept their own primitive mesh while their scale was normalised | An 11 m screwdriver and a 2 m cylinder where a fan blade belonged. |

---

## 2. Scene-by-scene

### ComputerRepairTask

| Issue found | Change made |
|---|---|
| Case was a black open frame with a floating green board — read as a doorway, not a PC | Rebuilt as an open mid-tower at 3/4 angle: front bezel with 5.25" bays, power/reset/USB, rear I/O and expansion slots, motherboard tray, CPU + cooler, 4 DIMM slots (2 populated), GPU, VRM/chipset/caps, SATA ports, drive cage, PSU, rear exhaust fan |
| Fault not visible — nothing showed what was wrong | 24-pin ATX header modelled on the board edge with the loom **visibly unplugged** and hanging clear of it |
| Ambiguous props (red blob, grey sphere, white box) | Red blob was the mis-scaled screwdriver; fixed. Sphere/box came from the legacy `Visual Desktop PC Assembly`, deleted |
| Bench 4.6 m wide, task occupied the middle third | Bench cut to 3.6 m; test station pulled from x=1.95 to x=1.50 |
| No zoning — objects looked scattered | Anti-static service mat under the case; SPARE PARTS / TOOLS / SERVICE AREA / TEST engraved zone labels; divided spares tray; tool set (driver, pliers, screw cup, wrist strap) |
| Removed side panel cluttered the bench | Stowed on the bench's lower shelf, which also clears the dock's sight line |
| Distractor module looked like a task part | Restyled as a sealed spare with a warning seal, moved to the tray's far end |

Before → After: `Before_Computer_ParticipantEye.png` → `After_Computer_ParticipantEye.png`
(also `_Workstation`, `_Overview`, `_SideProfile`)

### FanRepairTask

| Issue found | Change made |
|---|---|
| Fan was 1.12 m tall with its base 0.2 m **below** the bench surface — floating and dominating the room | Rebuilt as a ~0.5 m desk fan standing on the mat |
| Fan read as a lamp: all-white head, solid disc face | Dark navy motor housing against five rounded light blades; hub and spinner now visible |
| Guard was modelled on the fan *and* staged on the shelf | Guard exists once, removed and stowed on the lower shelf; mount lugs left on the head |
| Fuse (the fault) hidden at the rear | Service cut-out on the housing side facing the participant, exposing the fuse board, holder and wiring |
| Both fuses looked identical | Both fuses now share the same neutral body, caps and rating; only close inspection reveals the intact versus broken internal element |
| Motor module was a grey puck | Laminated motor can with shaft, terminal block and leads |
| Same mis-scaled screwdriver | Fixed |

Before → After: `Before_Fan_ParticipantEye.png` → `After_Fan_ParticipantEye.png`

### VRTraining

| Issue found | Change made |
|---|---|
| Three untextured primitives (blue cube, white cube, grey cylinder) communicated nothing | Part A and Part B are colour-coded modules with grips and label plates; the cylinder is a knob with grip ridges and a pointer |
| Socket was a flat slab | Recessed cradle with a dashed accent outline, corner studs and a PLACE PART HERE caption |
| No sense of order | Numbered bench signage: 1 PICK UP · 2 COMPARE · 3 TURN |
| Tool tray held tools in what is actually the placement target | Tools suppressed for training; right zone relabelled PLACE PART HERE |
| Reset control was an unlabelled puck | RESET caption added |

Before → After: `Before_Training_ParticipantEye.png` → `After_Training_ParticipantEye.png`

### ResearcherSetup

| Issue found | Change made |
|---|---|
| Bare room with an empty bench behind the form | Operator station: two monitors with UI-like content, keyboard, mouse, notepad and pen, headset and two controllers |
| Blank Information Station slab | Replaced by the wall notice board (below) |

Before → After: `Before_ResearcherSetup_ParticipantEye.png` → `After_ResearcherSetup_ParticipantEye.png`

**Note:** the researcher form itself is a `ScreenSpaceOverlay` canvas built at runtime in
`ResearcherSetupController.Start()`. It does not exist in edit mode, so it is **not** in
these screenshots — it needs a Play-mode capture, which is not covered here.

### All scenes — lab shell

- Blank 2.55 m Information Station slab → **Lab Notice Board** carrying bay identity
  (BAY 01/02/03), SAFETY, ESD CONTROL and PROCEDURE cards with real copy, and a footer.
  Moved to the right-hand wall so it does not stack against the dock on the left.
- Lighting: ambient 1.0 → 0.78, key/fill retuned, two ceiling point lights plus one bench
  key spot (3 additional lights, under URP's per-object limit of 4), realtime reflection
  probe, and a post-processing volume (Neutral tonemapping, −0.35 EV, +12 contrast, subtle
  bloom and vignette). Post-processing had to be enabled on the cameras explicitly — URP
  defaults it off, which is why the lab was clipping to white.
- Materials: metal/plastic/PCB/rubber/glass separated by metallic and smoothness; new
  surfaces for case steel, copper, gold, silicon, cabling, anti-static and status lamps.

---

## 3. Manual / guide placement

**Compact state** — four selector cards in a single row on an arm-mounted dock clamped to
the bench's left end, at approximately 40° to the participant's left.
**Expanded state** — selecting a source expands the reader *over* the same row, so the dock
never grows and cannot creep back over the bench. Close returns to compact.

Fixes applied to the readers themselves:
- Readers faced **backwards** — their content is on the +Z face, the cards' on −Z. Without a
  180° flip the participant saw a blank backing plate. (This was the "mirrored/back-facing UI"
  item.)
- Readers were **blank in edit mode** — `InformationSourceController` only fills the labels in
  `Awake()`. Copy is now written at build time too. (The "unloaded guide content" item.)
- The Close button shipped labelled **"Next"** and sat on top of the title. Relabelled and
  pinned to the header's far right; title inset and left-aligned.
- The visual guide's sprite rendered 1.4× wider than its own panel and overhung both sides.
  Now fitted inside the frame.
- Readers were each measured separately and came out different sizes; now one shared scale.

### Proof the manuals no longer block

- `After_Computer_ReaderOpen.png`, `After_Fan_ReaderOpen.png`, `After_Training_ReaderOpen.png`
  — participant pose with a guide **open**; the workstation, trays, test station and notice
  board are all fully visible.
- Machine check, `Docs/Verification/VisualAudit_Validation.txt`: **ALL SCENES PASS**. Two
  assertions back this up —
  - `CheckDockClearsBench` — no selector or opened-reader geometry intersects the volume
    enclosing the task interactables.
  - `CheckSightLines` — a ray from the participant's eye to every bench interactable is not
    intersected by any information-source geometry.

---

## 4. UI style — external asset

**Imported and applied: Kenney UI Pack 2.0 — CC0 1.0 Universal.**

| Field | Value |
|---|---|
| Source | <https://kenney.nl/assets/ui-pack> |
| Download | `https://kenney.nl/media/pages/assets/ui-pack/f651646eab-1718203990/kenney_ui-pack.zip` (1.17 MB) |
| License | CC0 1.0 Universal — verified in the pack's own `License.txt` (creation date 12-06-2024) |
| Attribution | Not required; credited voluntarily |
| Local copy | `ThirdParty/Kenney/UIPack/` including `LICENSE.txt` |
| Files used | 13 of 1343 |
| Style | Flat/soft-shadow modern UI, rounded rectangles with a raised "depth" edge — **not** fantasy or stone |

Applied by `Tools/.../Apply UI Style Pack`:

| Surface | Sprite |
|---|---|
| Reader panel body, notice cards | `panel_surface` (Grey depth flat) |
| Selector card face, dock slots, reader header | `panel_flat` |
| Primary controls (Next, Play, Restart) | `button_accent` (Blue depth flat) |
| Secondary controls (Prev, Close, Pause, Stop) | `button_neutral` |
| Header rules, accent bars | `divider` |
| Headings and signage | `KenneyFutureNarrow.ttf` → `TMP_KenneyFutureNarrow.asset` |

**Typography is deliberately split.** Kenney Future Narrow is an all-caps display face: set as
body copy it rendered paragraphs as shouty blocks and its `X` reads as an `H` ("NEXT" → "NEHT").
It is therefore used for headings, bench signage and step markers only; body copy and button
labels stay on the readable text face. That is a display-face + text-face pairing, not a
half-finished swap.

Kenney Game Icons remain as **supporting glyphs only** (source-type icons on the selector
cards), as required — they are no longer carrying the visual language.

Proof: `Proof_UI_ReaderStyled.png` (rounded panel with depth edge and drop shadow, display
heading, readable body, blue primary / grey secondary buttons) and `Proof_UI_CompactRow.png`.

**Not used:** the Unity Asset Store "3D Modern Menu UI" you named. Asset Store downloads
require an authenticated Unity account and license acceptance, which I cannot do on your
behalf. Kenney UI Pack was chosen as the closest CC0 equivalent — same flat-modern language,
no license encumbrance, and consistent with the project's existing CC0-only asset policy.
If you want the Asset Store pack specifically, import it with your account and the same
`ResearchUiStyleBuilder` mapping can be repointed at its sprites.

---

## 5. Project-wide checklist

| Check | Result |
|---|---|
| Awkward panel overlap | Fixed — reader covers the selector row exactly; Close no longer overlaps the title |
| Blocking task objects | Fixed — asserted by `CheckDockClearsBench` and `CheckSightLines` |
| Inconsistent button styles | Fixed — one primary and one secondary style from the pack, applied by role |
| Inconsistent typography and spacing | Fixed — one display face, one text face, shared sizes and letterspacing |
| Blank or unloaded guide content | Fixed — copy written at build time; asserted by `CheckNoBlankReaders` |
| Mirrored / back-facing / duplicate UI | Fixed — reader 180° flip; duplicate fan guard and legacy PC assembly deleted |
| Unclear interaction focus | Fixed — service mat, zone labels, recessed card slots, accent-coded primary actions |
| Icons not matching the theme | Kenney Game Icons demoted to supporting glyphs under the Kenney UI Pack language |
| Panels too large / too small | Dock housing sized to its contents; reader fitted to the row; notice board sized to its wall |
| Task objects not communicating purpose | Fixed in all three task scenes (see §2) |

---

## 6. Regression guard

`Tools/.../Validate Scenes` writes `Docs/Verification/VisualAudit_Validation.txt` and fails loudly on:

- **oversized props** — catches the scale bugs (thresholds set below the 2 m and 11 m cases found);
- **dock intrusion** into the task volume;
- **blocked sight lines** from the participant pose;
- **blank readers**;
- **missing task parts** — declared-vs-present `StableObjectId` check.

Current result: **ALL SCENES PASS**.

---

## 7. Open items

1. **Pre-existing data mismatch (warning, not changed).** `TrainingDevelopment.asset` declares
   `training.cube-a` / `-b` / `-cylinder`, but the scene objects use
   `training.training-cube-a` / `-b` / `-cylinder`. The objects exist and render correctly —
   the IDs simply disagree. Fixing it means editing `StableObjectId`s, which changes what the
   research log records, so it is your call, not a visual decision.

2. **Researcher form not captured.** It is a runtime `ScreenSpaceOverlay` canvas and needs a
   Play-mode screenshot to audit. The scene *backdrop* behind it has been rebuilt.

3. **Incident during this pass, now fixed.** Builders reparent interactables under their device
   for readable local coordinates; `ResetVisual` wiped children, so a *second* run of a builder
   deleted task interactables (motherboard, PSU, fan blade, fuse holder and others) and saved
   that to disk. `ComputerRepairTask.unity` and `FanRepairTask.unity` were restored with
   `git checkout`, `ResetVisual` now detaches task-carrying children instead of destroying them,
   and the missing-part assertion above was added so this cannot recur silently. The pipeline has
   since been run twice in a row and validated clean — but please spot-check those two scenes.

---

## 8. Comprehension pass

The audit above answered "does the scene look finished". It did not answer "does a
stranger know what to do", and on that question both benches failed. This pass is
scoped to participant comprehension. Everything in it is visual, textual or
affordance work — **no StableObjectId, event name, task rule, socket, collider
role or reset path was changed.**

### What was actually wrong

| Scene | Failure | Why it mattered |
|---|---|---|
| Fan | The head was four full-diameter bars crossing at the hub — a white plus sign on a stick. | The single largest defect in the project. A participant cannot diagnose a fan they cannot recognise as a fan. |
| Fan | Both spare fuses carried a red or green rating band. | The diagnosis was legible from across the room, so the task collapsed to fetching the green one. |
| Fan | The removed guard lay flat on a shelf as a plain white disc. | Read as a dish, and took with it the strongest cue that the object on the mat is a fan with its cage off. |
| Computer | Every component was sized in the *case's* axes, not the *board's*. | DIMMs came out as 5 mm slivers lying on the board; the graphics card stood on edge like a second motherboard. The interior read as coloured stripes on a green rectangle. |
| Computer | The CPU cooler was a top-down fin disc. | Drew a black disc with a hub — a record player. Also left the cavity flat, because nothing stood up off the board. |
| Both | Decorative tools sat in the tool tray beside the one real tool. | Three screwdriver-shaped objects, one of which answers a controller ray. |
| Both | No hover or selection feedback anywhere. | Nothing distinguished an interactive object from scenery until it had already been grabbed. |
| Both | No statement of the task, and a wall card listing a five-step procedure. | Participants had no goal, and the only instruction present prescribed the order of actions the study exists to observe. |

### What changed

**Fan.** Propeller rebuilt as five rounded, offset paddles around a clear hub so
the silhouette reads as a normal desk fan without looking bent or broken. Rear guard
cage added — that is what keeps the circular silhouette now that the front cage is
off the machine. Weighted base, telescoping column with a lock knob, tilt yoke,
finned motor barrel, control pod with a printed speed legend, cord gland with the
lead running out to the coil on the bench. Service bay is now a real recess with
four walls and a cover hinged open on its front edge, holding a fuse board,
terminal block and the holder. The fitted fuse sits in its spring clips with a
broken element and a light stain — visible on inspection, invisible from the
participant's start pose. Both spare fuses were stripped of their coloured bands
and are now identical twins at any distance.

**Computer.** Board-local frame documented, and every component re-sized in it.
Tower CPU cooler (cold plate, heatpipes, stacked fin block, 92 mm fan). DIMMs
standing out of their slots with heatspreader combs. Graphics card lying
horizontally as it does in a tower, with fan bays cut into the shroud edge, a
backplate label and a rear bracket. Drive bay carrying a 3.5" disk *and* a 2.5"
SSD, both faces out. Fixed cable loom climbing the case front from the PSU gland,
with drive, card and front-panel runs — so the unplugged 24-pin plug has something
to be unplugged *from*. That plug is now a proper connector: keyed shroud, latch,
two rows of twelve sockets, hung a hand's width from its header. Case shell,
interior liners and board green all lifted out of near-black, which is what made
the cavity render as a cave.

**Both benches.** `ResearchInteractable` tints its own renderers while focused —
one tint for every interactable, drawn with a `MaterialPropertyBlock` so no
material is written to and no instance is created. Decorative screwdriver and
pliers deleted. The device-test control is at unit scale, has a real button, and
the pedestal carries an INSPECT plate; the bench zone label matches it. A
bench-mounted **work order** states the reported symptom, says the goal is to find
the cause and fix it, points at the dock and the INSPECT control — and stops
there. The wall's numbered PROCEDURE card was replaced with BENCH LAYOUT.

### New regression guards

`Validate Scenes` gained four assertions, so this cannot quietly rot:

- `CheckTaskBrief` — the brief exists, is non-empty, and does not name the answer
  or number the steps;
- `CheckInspectControl` — the device test is at unit scale, visible, and labelled;
- `CheckFaultNotAdvertised` — no fault-site part uses a signal colour, and the two
  spare fuses are built from the same material set;
- `CheckDecorationIsInert` — no builder-made dressing carries a collider.

### Final internal validation status

**Internal participant-comprehension redesign and validation completed; real
first-time human pilot P1–P3 still required before participant testing
readiness.**

The final source builders were rebuilt and validated twice. Pass A and Pass B
preserved every task object ID, collider count, XR component count, source count,
and input-module count. Play Mode checks exercised manual open/close,
selection-tint enter/exit, Inspect Fail before repair, the existing correct repair
action, Inspect Pass after repair, and development reset for both Computer and
Fan. Each telemetry file contained exactly one `TaskCompleted`, one
`DeviceTestFailed`, and one `DeviceTestPassed` event with unique, monotonic event
sequence numbers. The final clean Unity Console contained zero errors and zero
warnings.

Fresh screenshots are the `Final_*` and current `Approach_*` files in
`Docs/Screenshots/Audit/`. These were reviewed internally by AI only. They are not
human pilot observations. The remaining acceptance boundary is the blank P1–P3
record in `PILOT_COMPREHENSION_CHECK.md`, plus real-device Quest checks where
applicable.
