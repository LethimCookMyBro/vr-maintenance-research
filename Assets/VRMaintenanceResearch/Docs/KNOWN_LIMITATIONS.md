# Known Limitations

## Research scope

- This is a research-development prototype, not a validated learning intervention.
- No Meta Quest 3 hardware run has occurred.
- Silent development media remains English placeholder content. Thai and Japanese reader fields render through local font fallbacks, but the wording still awaits approved translation and equivalence review.
- The self-authored MP4s are functionally verified in Windows Editor but Windows Media Foundation emits a color-primaries fallback warning; no research-script error followed the normal completion lifecycle.
- Low activity is interaction inactivity, not a clinical or cognitive measure.
- Current Play Mode runs prove schema, flow, simulator integration, and source media behavior; they do not prove participant behavior, hardware tracking quality, or research outcomes.
- The eight invalid v1 information assets remain locally visible until a permitted Unity Editor deletion action is available; only `_v2` assets are used by task definitions. They are untracked and unstaged.

## Build and platform

- The Windows artifact is a `Mono2x` Development build. The serialized Standalone backend remains `IL2CPP`, but the IL2CPP build path awaits the Visual Studio C++ toolchain and Windows SDK.
- The desktop runtime reports OpenXR form-factor unavailable without a connected headset; the Windows simulator run does not establish Quest 3 hardware behavior.
- The Windows build emits 486 package/build warnings, mainly shader and Performance Test preprocessor messages; no research-script Console error accompanies the build or runtime flow.

## Visual redesign, 2026-08-02

- **The redesigned scenes have not been run in the standalone player.** The Windows build succeeds, but every visual and interaction check in this pass was done in the Unity Editor.
- **No Quest 3 performance claim is made.** The scenes are *prepared* for Quest — shared URP materials, 2 real-time directional lights, no post-processing change, no transparency beyond the existing UI, 1024 px maximum texture, static batching flags on all environment geometry — but none of this has been measured on hardware. The triangle and material counts quoted here on 2026-08-02 (~20 k per task scene, 17 materials) were superseded by the model integration and recognition passes; as of `0248328` it is 120,385 triangles / 97 materials for `ComputerRepairTask` and 37,898 / 43 for `FanRepairTask`.
- **The participant start pose changed** from `(0, 0, 0)` to `(0, 0, -1.6)` in all three participant scenes. Movement CSV coordinates recorded before 2026-08-02 are therefore not spatially comparable with later ones. The logging schema is unchanged.
- **Device and component transforms changed.** Stable IDs, scripts, interactable components, collider components, task references and completion logic are all preserved, but positions, rotations and scales were re-authored so the equipment is human-scale and rests on a workbench. `PROTOCOL_CHANGE_LOG.md` records every change; an advisor should confirm the new arrangement does not alter intended task difficulty.
- **The fan front guard is now a removed part lying on the bench** rather than a mounted guard. Mounted, its collider blocked controller rays to `fan.blade`. The disassembled arrangement is deliberate and keeps every component reachable.
- **Interaction was validated through the component API, not by hand.** Grab, socket placement, controller-ray hover and poke were confirmed present and wired, and the XR simulator rig, rays and controller visuals render, but a human did not drive a controller through the full task in this pass.
- **The `ResearcherSetup` interface is built at runtime in code**, not as an editable prefab. This removes prefab-wiring drift but means the layout cannot be adjusted in the Inspector; edit `ResearcherSetupController.cs`.
- **The task status board and training board are runtime-built world-space canvases.** They do not appear in Editor scene view screenshots; the Play Mode captures in `Docs/Screenshots` show them.
- **Translated wording is not yet approved.** The information-source captions, panel titles and body text select Thai/Japanese fields when configured, with actual Editor captures showing no missing-glyph boxes; linguistic equivalence review remains outstanding.
- **The information-source layout changed to a fixed left-side station.** The updated reader and four equal source cards preserve the logged `information_source_layout_id`; advisor review is still needed before data collection because relative salience changed.
- **Two Poly Haven textures were downloaded but only partially used.** `beige_wall_001` contributes its normal map only; its diffuse was dropped because the warm beige conflicted with the neutral off-white target. The unused roughness maps were deleted rather than left as dead assets.

## Final spatial verification update - 2026-08-03

- A fresh Windows Mono Development build launches on this desktop, but its Player log reports `XR_ERROR_FORM_FACTOR_UNAVAILABLE` without an available headset form factor. This establishes startup only; it does not establish the standalone interactive flow.
- The compact source station, reader, status card, simulator-HUD gate, and F9-only researcher controls were runtime-checked in the Unity Editor. Quest 3 validation and full standalone interaction remain pending; Thai/Japanese glyph rendering was checked in the Editor, not on hardware.

## Data comparability - 2026-08-08

- **Telemetry recorded before commit `743b1c3` attributes in-machine interactions to
  the wrong object and must not be compared with data recorded after it.**
  `XRBaseInteractable` auto-collects `GetComponentsInChildren<Collider>()` when its own
  collider list is empty, and both builders reparent the in-machine interactables under
  their device so local coordinates stay readable. The device therefore claimed its
  children's colliders — `Desktop Case` seven, `Electric Fan Body` six — and because the
  parent registers first, `TryGetInteractableForCollider` returned the **device** for a
  ray aimed at any part inside the machine. Every hover and grab on the ATX connector,
  the fuse holder, the board, the supply or the case fan was logged against
  `computer.case` or `fan.body`. Nothing looked wrong from outside: the correct repair
  object sits out on the bench in both scenes, so the loop completed and the play-mode
  checks passed. The only symptom was an XRI warning at `OnEnable`. Affected columns are
  `object_id` and `object_category` on hover, grab and component events; task timing,
  completion and information-source rows are unaffected.
- `VRTraining` kept the same defect until 2026-08-08, because only the two workstation
  builders called `BindOwnColliders`. Training telemetry recorded before that date has
  the same caveat for `training.training-cylinder`.
- **Telemetry recorded before the collider-size fix of 2026-08-08 names the wrong object
  in 31 of 54 aims and must not be compared with data recorded after it.** This is the
  second half of the `743b1c3` defect: that commit fixed *which* interactable owns a
  collider, this one fixes *how big* the collider is. `SetCollider` existed in four
  copies and three of them resized a `BoxCollider` and returned in silence on anything
  else, so eleven parts built from capsule primitives kept the primitive's own
  1 000 x 2 000 x 1 000 mm collider and two status lamps kept a 1 m sphere. A ray from
  the participant's eye to the centre of what a part draws therefore resolved to a
  different part in **31 of 54 aims**; `computer.cooling-fan` absorbed every misdirected
  aim on the computer bench and `fan.blade` fifteen of seventeen on the fan bench.
  Affected columns are the same as above — `object_id` and `object_category` on hover,
  grab and component events. Task timing, completion and information-source rows are
  unaffected, because the loop is driven by id and never by a ray.

  No participant data exists for either half of this defect: no session has been
  recorded on hardware. The caveat is stated so that any pilot capture taken from an
  Editor build before this date is discarded rather than pooled.
- **A session recorded before 2026-08-08 with `developmentMode` set to false could not
  have been completed.** The researcher panel returned early in that configuration and
  it is the only caller of `CompleteCurrentTaskAndAdvance` and `SafetyStop`, so there
  was no route from the first task to the second. Any such session folder is a partial
  record by construction.
- The participant start pose changed on 2026-08-02 and device transforms were
  re-authored, so movement coordinates are not spatially comparable across that date
  either. That limitation is recorded above and is unchanged.

## Measurement definitions and asymmetries - 2026-08-08

- **`unsuccessful_action_count` is a sum across four event types and is not comparable
  between the two benches on its own.** It counts `IncorrectToolSelected`,
  `IncorrectComponentInteraction`, `DeviceTestFailed` and `UnsuccessfulAction` together.
  The four per-type counts appended to `task_summary.csv` at `derivation_version` 1.1
  are the columns to compare; the sum is retained for continuity.
- The two benches do not offer the same failure opportunities. The computer bench can
  record an incorrect component interaction through `computer.ram`, seated in the
  board's fourth memory slot; the fan bench cannot, because `fan.faulty-fuse` was stowed
  during the diagnostic framing pass and was that scene's only other `RepairAction`.
  Neither bench offers an incorrect tool: both tool trays hold one screwdriver. So in
  practice the fan's failure count can only come from failed device tests, while the
  computer's has two sources. **This asymmetry was not equalised.** Manufacturing a
  second wrong part on the fan bench to balance a count would reintroduce the assembly
  reading that the framing pass removed. `fan.faulty-fuse` is deactivated, not deleted;
  reactivating the GameObject restores it exactly, and the builders refresh a stowed
  part before putting it back.
- Every interactable lifts its own colour while an interactor is on it, with one tint
  for all of them. It marks which objects respond to the controller, not which one is
  the answer, but it does narrow the search space to the interactive set and should be
  described as an affordance cue in any write-up.

## Part recognition and remaining graybox - 2026-08-08

- **The two conditions are still not equal in mesh provenance, and the gap is now
  smaller but not closed.** `ComputerRepairTask` carries six licensed meshes for its
  components - motherboard, CPU, cooler, memory, drive, supply, case fan - plus the
  screwdriver. `FanRepairTask` carries the screwdriver and, since this pass, three XRI
  example control meshes. Everything else on the fan bench is still built from Unity
  primitives, because no CC0 or Unity-Companion mesh of a desk fan, a 6 x 30 fuse or an
  appliance fuse carrier exists in the repository and none was found on 2026-08-08 that
  met the project's licence standard. What changed is that the fan bench's primitives
  are now at the real parts' dimensions rather than at arbitrary ones. **Any write-up
  should describe the fan condition as primitive-built and the computer condition as
  mesh-built**, because that asymmetry sits on the variable the study measures.
- **Deliberately still graybox:** the room shell, the workbench, the trays, the placards,
  the walls, the floor and the ceiling. Nothing in the task requires a participant to
  identify the furniture, and distractor count is a research variable that `de7d5fd`
  set on purpose.
- **Colliders are now noticeably larger than several of the visuals they wrap.**
  `fan.working-fuse` and `fan.faulty-fuse` keep a 110 x 50 x 50 mm box around a 30 mm
  cartridge; `fan.fuse-holder` keeps the same box around a 44 mm carrier. This was a
  deliberate choice: the collider is the participant's ray target and the substrate of
  every recorded hover and grab, so shrinking it to match the new visuals would have
  changed hit rates between the verified baseline and now. The practical effects are (a)
  small parts stay easy to hit, which is a usability benefit but not a realism one, and
  (b) inside the fan's service bay the fuse-holder and internal-wire boxes overlap, as
  they did before this pass, so a ray at the boundary between them can resolve to either.
  Neither is new; both should be revisited together if collider geometry is ever
  re-authored.
- **Correction to the bullet above, 2026-08-08: eleven colliders are not "noticeably
  larger", they are 1 000 x 2 000 x 1 000 mm, and that part was never deliberate.**
  `SetCollider` in both workstation builders resizes `BoxCollider` only and returns
  without doing anything when the collider is a `CapsuleCollider`
  (`ComputerWorkstationBuilder.cs:878`, `FanWorkstationBuilder.cs:696`). Every
  interactable built from a Unity capsule primitive therefore kept the primitive's
  default collider — radius 0.5, height 2, at unit scale — while its visible body was
  rebuilt at true scale. The eleven: `computer.cooling-fan`,
  `computer.external-power-cable`, `computer.tool.screwdriver`, `fan.blade`, `fan.body`,
  `fan.fastener`, `fan.front-cover`, `fan.internal-wire`, `fan.motor-module`,
  `fan.power-cord`, `fan.tool.screwdriver`.

  Measured consequence: a ray from the participant's eye to the centre of what a part
  draws resolved to a **different** part in **31 of 54 aims** across the two benches.
  `computer.cooling-fan` absorbed every misdirected aim on the computer bench and
  `fan.blade` fifteen of seventeen on the fan bench.

  **Fixed 2026-08-08.** There is now one `SetCollider`, in `ResearchBuildKit`, and every
  builder goes through it: whatever collider an object arrives with, it leaves with a
  `BoxCollider` of the size the builder asked for, and a replacement names the object on
  the console instead of passing in silence. Four hand-rolled copies were deleted
  (`ComputerWorkstationBuilder`, `FanWorkstationBuilder`, `TrainingSceneBuilder`, and the
  box-only branch in `BenchDressing.PlaceScrewdriver`, which is why both benches stood a
  1 x 2 x 1 m grab volume on the tool tray). Three further colliders that did not cover
  their own part were corrected in the same pass: `computer.external-power-cable` and
  `fan.power-cord` were centred on their origin rather than on the coil they draw, and
  `Desktop Case` was one solid box the size of the whole tower, which put the case in
  front of all six components inside it — the `743b1c3` symptom expressed as geometry. It
  is now five boxes, one per closed face, open on the side whose panel is off.

  Result: **31 of 54 misattributed before, 11 after**, and the widest a grab volume now
  reaches past the part it belongs to is 52 mm, against roughly a metre before. No
  `stableObjectId`, transform, part count or task definition changed; the 31 ids are
  byte-identical to the previous commit.

  **The remaining 11 are occlusion, not misattribution.** In every one of them the
  pointer correctly reports the geometry that is actually in front of the part, from the
  two poses the check uses. They are: `computer.case` (2 — its own bounds centre is the
  air inside the open shell, and the motherboard is what is really there),
  `computer.psu-switch` (2 — the rocker is on the case's rear face, pointing away from
  both poses), `computer.side-panel` and `fan.front-cover` (1 each — stowed on the lower
  shelf, with the workbench top between them and the higher pose), `fan.fuse-holder` and
  `fan.internal-wire` (2 each — the service bay is behind the propeller), and
  `fan.power-cord` (1 — the coil sits behind the fan from the bench pose). A sweep of 56
  standing poses confirms **every part is selectable from somewhere**; the weakest is
  `fan.internal-wire` at 4 of 56. Closing these would mean moving parts or reshaping the
  propeller, which is a scene decision, not a collider bug — it stays decision **ช** in
  `SUPERVISOR_REVIEW_PACKAGE.md` and check 2 in `QUEST3_NEXT_STEPS.md`.

  See `Verification/Ray_Aim_Attribution.txt`, reproducible from *Tools → VR Maintenance
  Research → Visual Audit → Report Ray Aim Attribution*. Two scene-integrity tests now
  guard this: `NoInteractableClaimsMoreSpaceThanItOccupies` (deterministic, green, fails
  the moment a grab volume reaches more than 100 mm past its part) and
  `EveryInteractableAnswersTheRayAimedAtIt` (the 54-aim check, currently red on the 11
  occlusions above).

  Why no earlier check saw it: every existing check reaches an interactable by name or by
  reference and never casts a ray. The scene-integrity tests read components, the visual
  validator reads appearance, and the play-mode runtime checks and full-flow walkthroughs
  call `MaintenanceTaskController.RecordInteraction` directly with the object they looked
  up by id. `743b1c3` fixed which collider belongs to which interactable; it did not
  change how big any collider is.
- **The unplugged ATX connector now leans 26° out of vertical, toward the open side
  panel, where before it leaned 18° the other way.** This makes the twenty-four bores
  that identify it visible from the participant's approach instead of pointing at the
  ceiling. It does not shorten the search: the plug is still behind the board's front
  edge from the standing pose. An advisor should confirm the new hang does not alter
  intended task difficulty, the same way the 2026-08-02 transform changes were flagged.
- **The two device-test controls no longer look alike.** Both were the same three
  stacked cylinders; the computer bench now has a push button and the fan bench a rotary
  dial, matching what each one's stable id calls it. This is a between-condition visual
  difference in a control that is functionally identical in both scenes. It was made
  because an unlabelled flat disc read as an indicator lamp as easily as a control; it
  should be described, not assumed neutral.
- **A 6 x 30 mm fuse is small.** At real size the element - the whole diagnostic cue - is
  a 0.7 mm wire with a 3.4 mm gap when blown. It is legible in Editor captures at
  inspection range (`Docs/Screenshots/Audit/After_Fan_ElementBlown_Macro.png`) but
  whether it resolves on a Quest 3 panel at a participant's working distance has not
  been measured on hardware. If a pilot shows it does not, the element thickness is the
  knob to turn, and it must be turned on both fuses together.

## Headset comfort and control boundaries - 2026-08-08

- `ResearcherSetup` has no XR Origin and its `Setup Camera` carries no
  `TrackedPoseDriver`. **Nothing should load that scene while a participant is wearing
  the headset**: the view would not follow their head. The session therefore ends in
  the finished task scene, and returning to setup is a researcher action taken once
  the headset is off. The same applies to `ReturnToSetup()` — it is on the desktop
  panel for that reason and must not be wired to anything the participant can reach.
- The status board deliberately carries no control. The participant removes the
  headset between the two tasks for NASA-TLX, so any button there could load the
  second task before the questionnaire was administered. If the protocol ever moves
  NASA-TLX to the end of the session, this is the decision to revisit.
- Mouse interaction (`OnMouseEnter`/`OnMouseDown`) is disabled outside development
  mode. It remains the way the scenes are driven without a headset, and it is how the
  desktop simulator checks in `KEYBOARD_MOUSE_CONTROLS.md` are performed — those must
  be run in development mode.

## Content and configuration - 2026-08-08

- `ResearchTaskDefinition.thaiTitle`, `japaneseTitle`, `thaiParticipantInstructions` and
  `japaneseParticipantInstructions` are **read by nothing**. The work order's Thai and
  Japanese wording is hardcoded in `LocalizedTaskBrief.cs`. The fields were left empty
  rather than filled in, because filling them would imply content that no code reads.
  The information sources are different: `InformationSourceController` does read
  `thaiTitle`/`japaneseTitle` and the corresponding content fields from
  `InformationSourceDefinition`, and all referenced sources carry all three languages.
- `LocalizedTaskBrief` finds its text with `transform.Find("Heading")` and
  `transform.Find("Body")`. Both return null silently and `Refresh` then returns without
  touching anything, so a rebuild that renamed either child would leave every Thai and
  Japanese participant reading the English brief with nothing logged. A scene integrity
  test now asserts both children exist with a `TMP_Text`.
- Translated wording still awaits approval and linguistic equivalence review. This is
  unchanged from 2026-08-02.
- The information reader is a fixed left-side station and is deliberately **not**
  repositionable by the participant. `information_source_layout_id` is a logged variable
  and relative salience is part of what the study measures, so making the reader movable
  is a protocol decision, not a usability fix.
- Build Settings still include `Assets/XRI_Examples/Scenes/XRI_Examples_Main.unity` at
  index 4. Nothing in the research code loads it, but it ships inside a research build.
  It was left in place because the project's stated invariant is that the original XRI
  example scenes and assets remain untouched; removing it is the researcher's call.
- The application identifier is `com.unity.xr.interaction.examples` on both Android and
  Standalone — Unity's identifier for the example package, not a research application
  id. On Android it also determines where `Application.persistentDataPath` puts session
  data. Changing it is an app-identity decision and was not made here.

## Participant heads-up display - 2026-08-09

The participant now sees a head-referenced display carrying elapsed time, a
completion percentage and a three-line objectives checklist. **None of the three
existed in the protocol the earlier data and the earlier task design were built
around.** The project owner asked for all three, having been told what they cost;
this section is the record of that cost, not an objection to it.

Each element is a separate switch on `ResearchSessionConfig`
(`showTimer`, `showProgress`, `showObjectives`), all three default to on, and all
three are written into `session_manifest.csv` as `show_timer`, `show_progress` and
`show_objectives`, appended after `manifest_updated_utc`. **A session is only
interpretable against the display it was collected under, so no analysis should
pool sessions without first checking those three columns.**

### What each one changes

- **Visible elapsed time changes behaviour under pressure.** A clock the participant
  can read turns an untimed diagnostic task into a self-paced timed one. The direction
  is not neutral and not uniform: some participants will hurry and stop searching
  earlier, others will slow down once they see how much of the 900 s limit is left.
  Every duration measure in `task_summary.csv` — `completion_time_seconds`,
  `total_information_viewing_duration_seconds`, the four per-source durations,
  `total_low_activity_duration_seconds` — is therefore measured under a different
  task than it was before. `low_activity_period_count` is the most exposed: a visible
  clock is a reason to keep moving, and the 30 s inactivity threshold was calibrated
  without one.
- **Objectives that tick themselves are correctness feedback.** The three rows are
  states the controller already tracked, but showing them means the participant learns
  *at the moment they act* that the repair they just performed was the right one. The
  study measures diagnosis, and diagnosis under confirmation is a different task from
  diagnosis without it. Concretely: `incorrect_component_interaction_count` and
  `device_test_failed_count` can no longer be read as a search process, because after
  the first tick the participant knows the search is over. The wording never names a
  part, a tool or a procedure — the rows are past-tense states, not instructions — so
  the display does not shorten the search *before* the first correct action. It removes
  the uncertainty *after* it.
- **A completion percentage tells the participant how close they are to done.** 0 / 33 /
  67 / 100 is a progress signal the task never gave. Seeing 33 % after a first failed
  device test says "two of three things remain", which is information about the
  structure of the task that a participant was previously expected to infer. It also
  interacts with the timer: a participant who can see both will estimate whether they
  are on schedule, which is a judgement the protocol did not ask them to make.
- **The three interact.** They are not three independent additions to one baseline.
  A pilot that turns on all three is not comparable with one that turns on the timer
  alone, and neither is comparable with data collected before this commit.

### How to turn it off

Three ways, cheapest first.

1. **Per session, no rebuild.** In `ResearcherSetup`, section 3 "Session options" now
   carries three checkboxes: *Participant display: elapsed time*, *…: completion
   percentage*, *…: objectives checklist*. Clearing a box before pressing Start Session
   removes that element for that session, and the manifest records the choice. This is
   the answer to "the pilot says it is too much".
2. **The timer only.** `ResearchTaskDefinition.showTimerToParticipant` gates the clock a
   second time, per task. It was `false` on both maintenance definitions and **was set
   to `true` in this pass**, because otherwise the session-level `showTimer` could never
   display anything. Setting it back to `false` on
   `ComputerMaintenanceDevelopment.asset` and `FanMaintenanceDevelopment.asset` restores
   the pre-2026-08-09 behaviour for the clock regardless of the session switch. This is
   the only task-definition field this pass changed; no id, fault, part list, repair
   object, time limit or content string was touched.
3. **Permanently, for everyone.** Change the three field initialisers in
   `ResearchSessionConfig` (`Scripts/Core/ResearchTypes.cs`) from `true` to `false`, or
   remove the `ParticipantHud` component from the task controller in
   `ComputerRepairTask` and `FanRepairTask`. With all three switches clear the component
   builds nothing at all — it returns from `Start` before creating a canvas — so there
   is no residual cost or occlusion.

### Display limitations

- **It is head-locked, not world-fixed.** The canvas is parented to the main camera at
  1.15 m, so it follows head rotation exactly. That is what makes the corners behave
  like screen corners, and it is also the arrangement most likely to cause discomfort
  over a long session. It has not been worn on hardware; a Quest 3 comfort check is
  outstanding.
- **It occludes.** The objectives block sits in the upper right of the participant's
  view at all times, so it covers whatever is there — including the wall-mounted status
  board when they turn towards it. At 1.15 m it is also nearer than the workbench, so
  standing at the bench puts the lower blocks in front of bench geometry.
- **It carries no raycaster and no control**, deliberately: a controller ray passes
  straight through it to the bench, and there is nothing on it a participant can press.
  It writes no research event and changes no task state.
- The three milestones are read from `MaintenanceTaskController.DeviceTested`,
  `RepairPerformed` and `State`, which are the same transitions the event stream already
  records as `DeviceTestStarted`, the correct `RepairAction` and `TaskCompleted`. No new
  event type, no new column in `session_events.csv`, and no change to the existing
  manifest columns.
- It is in the two maintenance scenes only. `VRTraining` has its own board and measures
  four unrelated skills; `ResearcherSetup` has no participant.

## Panel legibility after the dark-glass restyle - 2026-08-09

- **Every participant-facing panel inverted from dark-on-light to light-on-dark.** The
  work order, the information reader and its selector cards, the wall notice board, the
  status board and the training board are now a dark translucent slab at about 88 %
  opacity with a rounded edge. Copy colours moved with them — `#F2F5F8` for headings,
  `#DCE4EE` and `#C6D2E0` for body — which is roughly 14:1 and 9-11:1 against the
  composited surface, so all of it stays past the 4.5:1 floor the notice board was
  already held to. **The measured contrast is calculated, not measured on a headset.**
  Reading time is a variable in this study; if a pilot reads more slowly than the
  earlier white-panel captures suggested, this restyle is the first thing to suspect and
  the tints in `ResearchUiStyleBuilder.Surfaces()` are the single place to change.
- The panels are now genuinely transparent, so the lab wall shows through them. That is
  additional fill-rate on Quest and has not been profiled on hardware.
- No panel changed position, size or wording. The information dock in particular keeps
  its authored geometry, because `information_source_layout_id` is a logged variable.

## Industrial room dressing - 2026-08-09

- **The room is no longer visually neutral, and distractor count is a research
  variable.** `de7d5fd` set the sparse room on purpose; this pass adds 68 background
  objects to it — racking with crates, guardrails, floor marking, a wall control
  cabinet with a lit display and three status lamps, wordless safety signage and a
  two-tone wall. None of it is interactable and none of it can be aimed at, but a
  busier room is a different search environment from an empty one, and the change was
  made across all four scenes so at least it is the same busier room everywhere.
- **The signage carries no text on purpose.** The three wall signs are the ISO shapes —
  hazard diamond, mandatory disc, safe-condition square — with no glyph on any of them.
  A legible line of English on a wall in a room read from any angle would be a fifth
  information source in a study whose information sources are the controlled variable.
- **Nothing in the dressing has a collider**, so no controller ray can resolve to it.
  `LabIndustrialDressing.AssertInert` fails the build on the console if one ever
  appears, and the 54-aim ray check is unchanged at the same 11 pre-existing occlusions.
- **Cost, measured, not estimated:** +1,904 triangles, +68 renderers, +4 to +6 materials
  and **+6 estimated draw batches** per scene, the last because every dressing object is
  marked batching-static and shares the existing palette. `ComputerRepairTask` is
  117,989 triangles and 96 materials, still under the 120,385 / 97 recorded at `0248328`.
  These are editor-side counts; **no measurement has been taken on a Quest 3.**
- The green ESD matting on the bench comes from the `LabEnvironment` prefab and is
  therefore in all four scenes, including `ResearcherSetup`. The darker per-scene
  service pad still sits on top of it in the three participant scenes.
