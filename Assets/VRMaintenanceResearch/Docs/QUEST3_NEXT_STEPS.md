# Meta Quest 3 — on-device checklist

**Written:** 2026-08-08 · commit `0248328` · branch `visual-polish-claude`

Nothing in this document is a claim about the headset. **No part of this project has ever
run on a Quest 3.** An Android player was built (`b152597`) and never installed.
Everything below is a check to perform, not a result.

Work top to bottom. Checks 1–3 gate everything after them: if the build will not install,
or the pointer will not select, nothing further is measurable.

Record results in `Verification/Quest3_Session_<date>.md`. Where a check has a
**If it fails** line, that names the exact file and line to change, so a failure turns
straight into work.

---

## Before you start

| | |
|---|---|
| **APK** | `Builds/Quest3/VRMaintenanceResearch.apk` — 179,766,537 bytes, built 2026-08-08 |
| **Build record** | `Verification/Quest3_Build.txt` |
| **Rebuild if the branch has moved since** | Unity → File → Build Settings → Android → Build. Takes about 46 minutes. |
| **Needed** | A Quest 3, a USB-C cable, developer mode on the headset, `adb` on the PC |
| **Time** | Allow 3 hours for checks 1–10 with one person, plus 30 minutes per extra person for check 9 |

> **Set `developmentMode` off** in the researcher setup screen before any check that
> produces data you intend to keep. With it on, data goes to a `Development` folder and
> the researcher's mouse can write into the participant's event stream (`50ad6fa`).

---

## 1. Install the APK and reach the first scene

**How.** Connect the headset by USB-C, accept the *Allow USB debugging* prompt inside the
headset, then:

```bash
adb install -r Builds/Quest3/VRMaintenanceResearch.apk
```

Launch it from the headset's library under **Unknown Sources**. The app is listed as
**XRI Examples** by **Unity Technologies** — it does *not* carry a research name.

**What to look at.** The researcher setup screen appears, drawn in the headset, and the
participant code field accepts input.

**Pass.** The app installs, launches, and the setup screen is readable and usable.

**If it fails.**
- *Install refuses with `INSTALL_FAILED_UPDATE_INCOMPATIBLE`* — an older build with the
  same identifier is present. `adb uninstall com.unity.xr.interaction.examples`, then
  install again.
- *Black screen or immediate exit* — capture `adb logcat -s Unity:V` from launch and read
  the first exception. The three errors in the build record are a Unity OpenXR package
  bug at `MetaQuestFeature.cs:554` and are not expected to stop the app; anything else is
  new.
- *The app appears under a Unity example name and that is unacceptable for a participant
  session* — the identifier is `com.unity.xr.interaction.examples` in
  `ProjectSettings/ProjectSettings.asset:169`. Changing it is a project-settings decision,
  recorded in `KNOWN_LIMITATIONS.md`.

---

## 2. The pointer selects the part you point at

**This is the most important check in the document, and it is expected to fail.**

Measured in the editor on 2026-08-08: pointing at the visible centre of a part selects a
**different** part in **31 of 54 aims** across the two benches
(`Verification/Ray_Aim_Attribution.txt`). Eleven parts carry a click target 1 000 mm
across and 2 000 mm tall while their visible bodies are 11–571 mm. Two of them —
`computer.cooling-fan` and `fan.blade` — sit in front of everything else and absorb
almost every aim.

**How.** Start a session with `developmentMode` **on** so you can read the event stream
afterwards. In each task scene, point at and select each of these in turn, saying aloud
which one you aimed at:

*Computer:* `computer.internal-cable` (the unplugged connector in the machine),
`computer.ram` (the memory on the board), `computer.main-power-connector` (the spare in
the tray), `computer.motherboard`, `computer.psu-switch`.
*Fan:* `fan.working-fuse` (the spare in the tray), `fan.fuse-holder`,
`fan.internal-wire`, `fan.power-switch`, `fan.body`.

Do it twice: once standing at the start position, once leaning in over the bench.

**What to look at.** The `events.csv` for that task. Each selection writes a row naming
the object id the software decided on.

**Pass.** Every row names the part you aimed at.

**Expected result.** It will not. In particular `fan.working-fuse` — the **correct
repair** on the fan bench — resolved to `fan.front-cover` from both editor poses, so
**the fan task may not be completable by pointing at all.** Establish that first: if you
cannot complete the fan task, stop and report it, because nothing after this check is
meaningful.

**If it fails.** The fix is one line in each of two files: the click-target sizing helper
handles box shapes only and silently skips capsule shapes —
`Editor/ComputerWorkstationBuilder.cs:878` and `Editor/FanWorkstationBuilder.cs:696`.
Extending it to capsules, then rebuilding both benches, resizes eleven targets and
changes nothing else.
**Do not make that change without a decision** — resizing a click target changes which
object id lands in the event stream, which is a research variable. It is question **ช**
in `SUPERVISOR_REVIEW_PACKAGE.md`.

---

## 3. The pointer reaches the buttons on the information dock

Never measured. The editor can confirm that every button sits on a world-space surface
that accepts pointer input; it cannot confirm that a controller ray lands on one.

**How.** From the participant start position, without walking:

1. Point at each of the four source cards on the dock at your left and select it.
2. With a reader open, press **Prev**, **Next** and **Close**.
3. On the video source, press **Play**, **Pause**, **Stop**, **Restart** and **+10 s**.
4. In the training room, complete the four skills and press **Continue**.

**What to look at.** Whether the button highlights under the ray before you press, and
whether the press registers first time.

**Pass.** Every button highlights when aimed at and responds on the first press, from the
start position, without leaning in.

**Reference sizes**, measured from the start pose:

| Target | Width | Distance | Apparent width |
|---|---|---|---|
| Source card (×4) | 244 mm | 2.80–2.85 m | 4.9°–5.0° |
| Prev / Next | 184 mm | 2.68 m | 3.9° |
| Close | 157 mm | 2.76 m | 3.3° |
| Play / Pause / Stop / Restart / +10 s | 148 mm | 2.67–2.71 m | 3.1°–3.2° |

For scale: a thumbnail at arm's length is about 1.5°. Nothing here is smaller than two
thumbnails, so a failure is a pointer or raycast problem, not a size problem.

**If it fails.**
- *Nothing highlights anywhere* — the pointer is not reaching world-space surfaces at
  all. Check that the **Near-Far Interactor** on each hand is enabled and that its far
  cast is on.
- *Cards highlight, reader buttons do not* — the reader panel is switched off until the
  card is selected; confirm the reader actually opened.
- *Highlights but does not press* — a select-action binding problem, not geometry.
- *Only the small video buttons fail* — this is the one place where making the target
  bigger is the answer; the reader layout is in
  `Editor/InformationDockBuilder.cs`, and reader buttons are **not** research click
  targets, so resizing them does not touch any research variable.

---

## 4. The 0.7 mm fuse element is readable

Never measured. The entire fan task rests on this. The two fuses are identical — same
glass, same ferrules, same printed rating — and differ **only** in the element inside:
one continuous wire, or two stubs with a 3.4 mm gap. If the element cannot be resolved on
the display, the fan task has no visible answer.

**How.** In the fan scene, walk up to the bench and look at:

1. the fuse **fitted in the holder** in the service bay — its element is broken;
2. the **spare fuse in the left-hand tray** — its element is intact.

Look at each from three distances: the start position (2.8 m), standing at the bench
(about 0.5 m), and holding it as close as the headset will focus.

**What to look at.** Whether you can tell, without being told, which one has a gap in the
wire.

**Pass.** At the bench, a person who has not been told which is which can say which fuse
is blown. It is **acceptable and intended** that this is impossible from the start
position — a participant who can name the blown fuse from across the bench has not
diagnosed anything.

**If it fails** — the element is invisible even close up:

Change **one constant**: `Editor/FanWorkstationBuilder.cs:623`

```
const float k_FuseElement = 0.0007f;
```

One function, `BuildFuse`, draws both fuses and reads this constant on both branches
(line 655 for the intact element, line 666 for the two stubs). **Changing it thickens the
intact element and the broken stubs by the same amount, in the same step, for all three
fuses in the scene** — the fitted one, the spare, and the stowed one. There is no way to
thicken one and not the other, which is exactly the property this check needs: the two
fuses must stay identical in everything except whether the wire is continuous.

Try 0.0010 first, then 0.0014. Rebuild the fan workstation, then re-run this check and
the full verification suite. Record the value you settled on and the distance at which it
became readable.

**Do not** add a second cue — no discolouration, no stain, no colour change on the blown
fuse. An earlier version had an opaque dark blob inside the blown fuse's glass, visible
from across the bench; it handed over the diagnosis and was removed in `0248328`.

Reference images taken in the editor: `Screenshots/Audit/After_Fan_ElementGood_Macro.png`
and `Screenshots/Audit/After_Fan_ElementBlown_Macro.png`.

---

## 5. Thai and Japanese text is legible on the display

Never measured. Approving a translation on a monitor does not establish that it renders
on the headset — Thai stacks vowel and tone marks above and below the line, and both can
be lost first when a display runs out of resolution.

**How.** Run three short sessions, one per language, set on the researcher setup screen.
In each, read aloud from the start position and then from the bench:

| What | Where | Which languages |
|---|---|---|
| Work order heading and body | Bench panel, left of the machine | All three |
| Source card captions — `คู่มือ` / `マニュアル` etc. | The four cards on the left dock | All three |
| Source title and body | Inside an opened reader | All three |
| Finished notice | Status board, after completing or aborting a task | All three |

**What to look at.**
- **Thai:** whether tone marks and upper/lower vowels are distinct or merge into the
  glyph body. Look specifically at `เมื่อเครื่องพร้อมแล้ว` in the work order — it stacks
  marks on three consecutive characters.
- **Japanese:** whether kanji strokes separate at reading distance. Look at
  `装置は組み立て済みで` in the work order body.
- **Both:** whether any character renders as an empty box. A box means the font fallback
  did not load, which is a bug, not a legibility problem.

**Pass.** Every string is readable at the distance the participant will read it from: the
work order and the card captions from the start position, the reader body from the bench.
No empty boxes anywhere.

**If it fails.**
- *Empty boxes* — a font fallback failure, not legibility. The fallbacks are registered in
  `Scripts/InformationSources/InformationSourceController.cs:305` and the status board
  registers them itself at `Scripts/UI/TaskStatusBoard.cs:138`. Note which string and
  which scene.
- *Marks merge but the text is otherwise readable* — record it and raise it as a font-size
  decision, not a translation one. Point sizes are in the builders:
  work order body `Editor/TaskBriefBuilder.cs:87` (0.30), card captions
  `Editor/InformationDockBuilder.cs`, reader body via the source panel.
- *Only Thai fails, or only Japanese* — the two use different font assets
  (`Resources/Fonts/TMP_NotoSansThai_v2`, `TMP_NotoSansJP_v2`); note which.

The wording itself is a separate job for a human reader — `TRANSLATION_REVIEW.md`. A
translation approved there could still be unreadable here, and vice versa.

---

## 6. Tracking, grab, socket, poke, haptics and audio

Never measured. These are the toolkit's own interactions; the research code sits on top of
them.

Run this in the **training room**, which exists to exercise exactly these four skills, and
then confirm on one task bench.

| # | What | How | Pass |
|---|---|---|---|
| 6.1 | Head tracking | Look around the lab, lean in and back, turn 360° | The view follows the head with no visible lag and no jump. No drift after 10 minutes. |
| 6.2 | Controller tracking | Hold both controllers out, cross them, bring them behind your back and out again | Both pointers stay attached to the controllers and recover within a second when they come back into view |
| 6.3 | Grab | Pick up **PART A** and the **REFERENCE** cube with each hand | Both grab on grip, hold while you move, and drop on release |
| 6.4 | Socket | Place a cube in the **PLACE TO COMPARE** tray | It seats in the socket and the training board's second requirement ticks |
| 6.5 | Poke | Touch the training dial with a fingertip / controller tip rather than the ray | The poke registers and the third requirement ticks |
| 6.6 | Turn the dial | Grip the **DIAL** cylinder and rotate it | It turns and the requirement ticks |
| 6.7 | Haptics | Grab and release, and press a reader button | A short pulse on the correct controller |
| 6.8 | Audio | Complete a task | Any audio cue plays from the correct direction |
| 6.9 | Device test — button | Computer bench: press `computer.power-button` | Presses first time |
| 6.10 | Device test — dial | Fan bench: turn `fan.speed-selector` | Turns first time |

**6.9 and 6.10 are a matched pair and must be compared.** The two conditions use
different controls for the same step — a push button and a rotary dial (question **จ** in
`SUPERVISOR_REVIEW_PACKAGE.md`). Time both and count failed attempts on each. If the dial
takes materially longer or fails more often, that is a systematic difference between the
two conditions and must be reported, because it would otherwise show up in the results as
a property of the fan task.

**If it fails.** In the editor these confirm 4 warnings on entering play mode, all from
running without a headset (`XR: Error setting active audio output driver`, `Failed to get
haptic capabilities of XRSimulatedController`). Those should disappear on real hardware.
If haptics still report unavailable *on the headset*, that is new — capture
`adb logcat -s Unity:V`.

---

## 7. Frame timing

Never measured. No performance claim has ever been made about this project, and none may
be made until this check is done.

**How.** Install Meta's **OVR Metrics Tool** on the headset and enable the persistent
overlay, or use `adb shell logcat -s VrApi` and read the `FPS=` field. Then:

1. Stand in each of the four scenes for 60 seconds without moving.
2. Walk the full approach — start position to bench to dock and back — in each task scene.
3. Open a video source and let it play to the end.
4. Complete a full session, both tasks.

Record for each: average FPS, worst FPS, and stale-frame count.

**Pass.** A steady 72 FPS with no stale frames during normal interaction. Brief dips on
scene load are acceptable and should be noted rather than counted.

**Reference geometry**, for judging whether a dip is content or code:
ComputerRepairTask 120,385 triangles / 97 materials; FanRepairTask 37,898 / 43.
The computer bench is roughly three times the fan bench, so if only one scene drops
frames it should be that one.

**If it fails.**
- *Both scenes drop frames equally* — look at the lighting rig and the URP asset
  (`UniversalRP-HighQuality`), not the benches.
- *Only the computer scene* — it carries the licensed interior models. Report the number;
  reducing them is a decision, because the two benches' visual detail is already unequal
  and that inequality is noted in `Verification/PART_RECOGNITION_RECORD.md`.
- *Dips when a video plays* — note the source and the video file.
- Note that the APK also ships Unity's own `XRI_Examples_Main` scene. It is never loaded
  by the research code, so it costs install size, not frames.

---

## 8. Comfort, reach and locomotion at different heights

Never measured, and it cannot be measured by one person.

Every participant starts at the same place, 2.05 m from the bench edge and 1.05 m outside
the marked work zone, so **every task begins with locomotion**. The bench top is at
0.92 m and the parts sit between 0.93 m and 1.35 m.

**How.** With **at least three people of clearly different heights** — aim for roughly
150 cm, 170 cm and 185 cm — each doing the following without coaching:

| # | What | Pass |
|---|---|---|
| 8.1 | Set the headset's floor height, then stand at the start position | The floor reads as the floor; the bench top looks waist-to-chest high, not chin high or knee high |
| 8.2 | Travel to the bench | Reaches the bench without disorientation and without leaving the play area |
| 8.3 | Reach the parts at the back of the bench (`fan.power-cord` at z = 1.24, `computer.external-power-cable` at z = 1.24) | Reachable without climbing or clipping through the bench |
| 8.4 | Look into the computer's open side and find the unplugged connector | Possible without crouching uncomfortably or standing on tiptoe |
| 8.5 | Read the work order and the dock from the start position | Both legible without walking |
| 8.6 | 15 minutes continuous | No nausea, no eye strain. Ask directly; do not wait to be told. |

**What to record.** Each person's height, whether each row passed, and any verbatim
complaint. A comfort problem that appears at one height and not another is the finding.

**If it fails.**
- *Bench too high or too low for shorter or taller people* — the bench top is one
  constant, `Editor/BenchDressing.cs:13` (`BenchTop = 0.92f`), which every builder reads.
  The participant start pose is authored in the scenes themselves, on the XR Origin's
  `Camera Offset`, at (0, 1.361, −1.6) in all three participant scenes. Changing either
  changes every distance in the review package's §2 table, so it needs a decision.
- *Anyone reports nausea* — stop that person's session, record it, and note which
  locomotion they were using. This gates data collection.
- *Anyone cannot reach a part* — name the part. Do **not** move it without a decision;
  positions are question **ก**.

---

## 9. A full session, in the headset, end to end, with data

**How.** With `developmentMode` **off** and a real participant code, run a whole session
in the headset: setup → training → task → (headset off for NASA-TLX) → task → finish.
Run it **twice**, once in each task order, matching
`Verification/Full_Flow_Walkthrough_ComputerThenFan.txt` and
`Verification/Full_Flow_Walkthrough_FanThenComputer.txt`.

**What to look at.**

| | Pass |
|---|---|
| The participant never has to take the headset off except for NASA-TLX | No point in the flow requires the researcher to touch the headset |
| The status board carries **no** buttons | The participant cannot advance themselves and so cannot skip NASA-TLX |
| The finished notice appears in the participant's language | And the session stays in the scene it finished in — it must not load the setup screen |
| The researcher can advance and can return to setup from the desktop panel | Both work while the participant waits |

**Then pull the data off the headset:**

```bash
adb pull /sdcard/Android/data/com.unity.xr.interaction.examples/files/VRMaintenanceResearchData
```

Sessions written with `developmentMode` off land in a `Sessions` folder; with it on, in
`Development`. Check that `session_manifest.csv`, `task_summary.csv`,
`session_events.csv` and each task's `events.csv` and `movement.csv` exist and are
non-empty, that `platform` reads `Android`, and that head and both hand poses carry real
coordinates **throughout** the session and not only at startup.

**If it fails.**
- *No data folder on the headset* — the app could not write to its own storage. Capture
  `adb logcat -s Unity:V` and look for the path it tried.
- *Movement rows have empty pose columns partway through* — tracking was lost. Note when,
  and what the participant was doing.
- *`first_meaningful_action` reads about 0.014 s* — expected, and already an open item: it
  is an incidental pointer hover at spawn, not participant behaviour. Record it; do not
  fix it here.

---

## 10. What the recorded data says the participant did

**How.** Take the events file from one headset session in check 9 and read it beside your
own memory of the session.

**What to look at.**

| | Pass |
|---|---|
| Which part each hover and selection names | Matches what the participant actually pointed at — this is check 2 seen from the data side |
| Hover counts | Not doubled. A hover by both controllers on one object currently records two events; note how often that happened |
| `interactor=` | No row reads `interactor=mouse`. If any does, the researcher's mouse wrote into the participant's stream and `developmentMode` was left on |
| `incorrect_component_interaction_count` | Will be 0 on the fan task no matter what the participant did — that is structural, not an error (question **ค**) |

**If it fails.** Do not correct the data. Record which rows are wrong and why; the
decisions are in `SUPERVISOR_REVIEW_PACKAGE.md`.

---

## Reporting

Write up as `Verification/Quest3_Session_<date>.md` with, for each check: **pass / fail /
not run**, what you observed, and any measurement. State the headset's build number and
the APK's build date.

**Do not** describe Quest 3 validation as complete until checks 1–10 have been observed
and recorded. Until then, everything in this project's documentation that touches the
headset is a plan, not a result.

---

## Earlier status, kept for reference

### 2026-08-08 — what the software side established

- A participant can complete setup, training, both tasks and the end of the session
  without leaving the headset, with `developmentMode` off. Both task orders pass:
  `Verification/Full_Flow_Walkthrough_*.txt`.
- Every button on that path sits on a world-space surface with a tracked-device raycaster
  and a graphic that accepts pointer input. **Whether a controller ray actually lands on
  them is check 3 and is not established.**
- OpenXR configuration for Android is correct for this headset: Meta Quest Support
  enabled with Quest 3 (`eureka`) targeted, Oculus Touch and Meta Quest Touch Plus
  controller profiles both enabled, IL2CPP, ARM64, Vulkan, linear colour, URP, single
  pass instanced. The Standalone profile for Quest Link is enabled too, at multi-pass.
- **An Android player was produced**: `Succeeded`, 179 MB APK, 46 minutes —
  `Verification/Quest3_Build.txt`. It was not installed and not run. The three errors in
  that build are one Unity OpenXR package bug (`MetaQuestFeature.cs:554`), not a project
  setting; validation was re-run separately and is clean for both Android and Standalone:
  `Verification/OpenXR_Validation.txt`.
- The active build target was returned to Windows afterwards, and the platform state the
  Android build wrote into version-controlled files was reverted.

### 2026-08-02

The recovered Windows development build verified the desktop simulator and the logging
path. It does not establish headset tracking, controller behaviour or OpenXR hardware
readiness. The original six-point list from that date is superseded by checks 1–10 above,
which say the same things with a method and a pass condition attached.
