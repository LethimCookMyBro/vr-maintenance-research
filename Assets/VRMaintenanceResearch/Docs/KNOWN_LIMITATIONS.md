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
- **No Quest 3 performance claim is made.** The scenes are *prepared* for Quest — 17 shared URP materials, ~20 k triangles per task scene, 2 real-time directional lights, no post-processing change, no transparency beyond the existing UI, 1024 px maximum texture, static batching flags on all environment geometry — but none of this has been measured on hardware.
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
