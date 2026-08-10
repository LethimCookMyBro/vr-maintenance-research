# Protocol Change Log

Every change on this branch that could affect what the study measures, in the order it
happened. Entries are decisions or pending decisions, not a build history — the commit
messages carry the engineering detail.

The full argument for each open item, with pictures and a closed question, is in
[`SUPERVISOR_REVIEW_PACKAGE.md`](SUPERVISOR_REVIEW_PACKAGE.md). **Nothing in this file
has been approved.**

---

## Standing items, carried from before the redesign

| Area | Earlier material may describe | Current direction | Status |
|---|---|---|---|
| Task A | Component replacement only | **Component replacement, per proposal 9.3.1.** An earlier round widened this to "inspection, diagnosis, repair and retries", which erased the difference from Task B; Round 6 put it back | Fault and completion rule to confirm before data collection |
| Task B | Fault diagnosis only | **Fault diagnosis among several possible causes, per proposal 9.3.2**, with more decision-making than Task A as 9.3 requires | As above |
| Information | One internal manual, forced usage | Four independent concurrent source types; no recommended source, no forced order | Thai and Japanese now drafted for the whole build; **every row is DRAFT** pending the 9.13 reviewer — see [`TRANSLATION_REVIEW.md`](TRANSLATION_REVIEW.md) |
| Validation | Inspect action as main validation | Observable interaction and device-test outcomes, raw events retained | Stop and scoring rules to confirm |
| Timing | Earlier fixed timings | **600 s on both tasks**, against the proposal's 960 and 1200 — see Round 6.2 | Open: needs sign-off |
| First-person recording | Described in proposal 9.11 | **Implemented in Round 6.3.** It had been a manifest column with no capture code behind it | Consent wording in the ethics materials must match a capability that now exists |
| Analytics | Broad learner labels | Observable behaviour only; transparent counts and timestamps derived from raw events | No personality or nationality conclusion is automated |

### Documents that must be updated before data collection

- Participant instructions and researcher procedure.
- Thai and Japanese translations, and the approved information content.
- Ethics and consent materials, especially first-person recording language.
- Counterbalancing schedule and source-layout assignment schedule.

---

## 2026-08-02 — Visual redesign

The redesign changed no research behaviour but changed the physical layout of both task
scenes. The logging schema, object ids, completion conditions, retry behaviour and the
four information-source conditions were unchanged.

| Item | Before | After | Reason |
|---|---|---|---|
| Participant start pose | `(0, 0, 0)`, inside the machine | `(0, 0, −1.6)` in all three participant scenes | The participant used to spawn inside the computer case / fan body. Identical in all three scenes, so cross-task comparability holds. |
| Device and part transforms | Free-floating grey shapes at mixed scales | Human-scale equipment on a shared 0.92 m bench | The old scene had a 2.4 m computer case and a 3 m fan clipping through the floor. |
| Information tiles and panels | `(x, 1.0, 3.0)` and `(x, 1.65, 1.5)` | Unchanged in world coordinates | Recorded against `information_source_layout_id`; moving them would alter relative salience. |
| Source control buttons | Next left of Prev; Seek left of Play | Mirrored about the panel centre | Reading-order defect. Symmetric and identical across all four sources and both tasks. |
| Fan front guard | Mounted in front of the blade | Removed and laid on the bench | Keeps every part pointable and reads as a maintenance scene. |
| Computer internals | Scattered in mid-air | Laid out on the bench | Same reason. |
| Status board | None | Read-only world-space board | Mirrors task state and attempt number only; writes no events, never names the fault. |

**Recorded at the time, still true:** movement-file coordinates from sessions before
2026-08-02 are not spatially comparable with later ones, because the origin offset
changed. The schema is unchanged.

**Corrected on 2026-08-08, twice over:**

1. This table's claim that the information tiles were "unchanged" was true of their world
   coordinates only. Because the participant moved 1.6 m back, every participant-relative
   quantity changed — the four sources became more nearly equidistant (outer:inner
   distance ratio 1.394 → 1.197) and more nearly equal in apparent size.
2. The tiles did not stay there either. They were moved the next day — see the
   2026-08-03 entry below.

---

## 2026-08-03 — Round 0: comprehension and readability

`501bc01`, `4f4e452`, `22c7158`

| Change | Detail | Research effect |
|---|---|---|
| Thai and Japanese work order | `LocalizedTaskBrief.cs` added. The work order — the only text that states the task — is now shown in the session language | The one text a participant must understand is in their language. Wording still unchecked by a Thai or Japanese reader; see [`TRANSLATION_REVIEW.md`](TRANSLATION_REVIEW.md) |
| Training board rewritten | Four skill requirements with a gated Continue | The training room now teaches the four interactions the tasks need |
| **All four information sources moved** | Off the back wall, onto a single dock at the participant's **left**, angled 38° toward them. Cards 1.2 m → 0.244 m wide | See below |

**The information source move, measured:**

| Measure | 2026-08-02 | Now |
|---|---|---|
| Position | Back wall, x = −3.3 / −1.1 / +1.1 / +3.3, z = 3.0 | Left dock, (−2.12, 1.30, 0.30) → (−1.43, 1.30, 0.84) |
| Distance from the start pose | 4.73 m to 5.66 m | **2.80 m to 2.85 m** |
| Farthest : nearest | 1.197 | **1.018** |
| Apparent width | 14.5° / 12.1° | **4.9°–5.0°, all four** |
| Spread | Left and right of the participant | **All four on the left** |

**Research effect, two parts:**

* The four sources are now almost perfectly matched in distance (1.8% spread) and
  apparent size (2%). For a study whose independent variable is source type, that is a
  clear improvement — no source is nearer, larger or easier to reach than another.
* **Position in the row is now completely confounded with source type.** The dock sorts
  the row by source type, so the order is always *manual → troubleshooting → video →
  visual guide*, left to right, in both tasks, for every participant. There is one layout
  identifier in the build (`sources-layout-development-a`) and no second one, so
  `information_source_layout_id` will record the same value for every session and the
  bias cannot be modelled out afterwards. No assignment schedule is implemented.

**Status:** open item ซ.

---

## 2026-08-04 — Round 1: licensed models

`b6b5777`, `79cc5e6`

Hand-built stand-in shapes replaced with licensed 3-D models; the part of that
integration that did not hold up was reverted the same day, keeping the licensed sources.

**Research effect:** parts took the models' true proportions, so positions and sizes
moved away from the 2026-08-02 table. Superseded by Round 5 for most parts.
**Status:** covered by open item ก.

---

## 2026-08-04 — Round 2: computer interior rebuilt

`e7cd4d4`, `298a538`, `c02bb64`

The inside of the computer rebuilt in five stages: board, processor, cooler, memory,
then card, supply, drive, power loom and bench.

**Research effect:** the computer bench gained a great deal of visual detail that the
fan bench did not have. That inequality was one of the reasons for Round 5.
**Status:** covered by open item ก. Record: `Verification/ITEM_3D_REBUILD_RECORD.md`.

---

## 2026-08-04 — Round 3: diagnostic framing, and click-target ownership

`de7d5fd`, `743b1c3`

**`de7d5fd` — both benches re-framed from "assemble this" to "diagnose this".**

| Change | Detail |
|---|---|
| `computer.ram` | Moved out of the spares tray and **seated in the board's fourth memory slot**. Same id, same recorded event type, same click target. Selecting it now means pulling memory from a machine that will not power on. |
| `computer.non-target-module` | **Stowed** |
| `fan.faulty-fuse` | **Stowed.** One fuse is now fitted in the holder and one spare is in the tray. |
| `fan.motor-module`, `fan.non-target-module` | **Stowed** |
| Work order | Gained one line in all three languages: the unit is assembled and open for service |
| Lab wall notice board | Four notice cards had always rendered as blank white rectangles — each card's opaque face sat 9 mm nearer the eye than its own lettering. Fixed, and body copy added. Copy is lab procedure only and names neither fault. |

Stowing switches a part off without deleting it. The id, settings and position are
retained and one step restores it.

**Research effect, stated at the time and unresolved:** `fan.faulty-fuse` was the fan
scene's only repair-action part besides the correct one, **so the fan task can no longer
record an incorrect-component action at all.** The computer task still can, through
`computer.ram`. See open item ค for exactly which `task_summary.csv` columns this
affects.
**Status:** open items ข and ค. Record: `Verification/DIAGNOSTIC_FRAMING_RECORD.md`.

**`743b1c3` — each machine stopped answering for its own internal parts.**

Because internal parts are built as children of their machine, and the machine's own
click-target list was left empty, the interaction toolkit filled that list with every
target underneath it. The machine registered first, so **every hover and selection of an
internal part was recorded against the machine's id** — `computer.case` for six parts,
`fan.body` for five. The only outward symptom was one warning line at scene load; both
correct repair parts sit outside their machine, so the repair loop always completed.

**Research effect:** data recorded before this commit cannot be pooled with data recorded
after it for any analysis that names an individual part. It is not recoverable — the
rows do not record which child was under the pointer. No participant data exists, so at
present this costs nothing.
**Status:** open item ฉ.

---

## 2026-08-08 — Round 4: headset-only flow, regression checks, first Quest build

`6683949`, `24c5399`, `6845fec`, `efe4e0a`, `b152597`

| Change | Research effect |
|---|---|
| A participant can finish a task and move on without removing the headset | Removes a procedural interruption that was not part of the design |
| Scene-regression checks added, seven tests | Catches duplicated ids, a part answering for its children's targets, and a task whose required repair part left the scene |
| A whole session driven end to end with development mode **off**, both task orders | The configuration a real session runs in was previously never exercised |
| Status documents corrected to describe what the code does | No research effect |
| An Android player actually produced for Quest 3 — 179 MB, 46 minutes | **Never installed, never run.** No hardware claim follows from it. |

**Status:** no open decision. Records: `Verification/Full_Flow_Walkthrough_*.txt`,
`Verification/Quest3_Build.txt`, `Verification/OpenXR_Validation.txt`.

---

## 2026-08-08 — Round 5: session integrity, and part recognition

`50ad6fa`, `a32041c`, `43e1708`, `230ddfe`, `0248328`

**`50ad6fa` — the researcher's mouse could write into the participant's data.**
The deployment is a PC running Quest Link, so the game view sits on the researcher's
monitor while the participant works. A mouse click over a part recorded a hover, a grab
or an information-source open in the participant's event stream. That path is now off
during a participant session and available only when no session exists, so editor
tooling still works.
**Research effect:** affected rows are identifiable and filterable — they carry
`interactor=mouse` — so nothing already recorded is lost. **Status:** closed.

**`a32041c` — advancing moved off the participant's board.**
Two protocol facts changed the right design: the participant removes the headset between
the two tasks for NASA-TLX, and the setup scene has no head tracking. A *Continue* button
on the participant's status board would have let them load the second task before the
questionnaire was administered; and ending the session by loading the setup scene put a
participant still wearing the headset in front of a view that did not follow their head,
showing them the configuration screen and their own participant code.
The board now carries **no control at all** and shows a finished notice in the
participant's language. The session ends where it finished. Advancing and returning to
setup are the researcher's, from the desktop panel.
**Research effect:** protects the NASA-TLX step. **Status:** closed.

**`0248328` — the parts a participant must be able to name were rebuilt.**

| Part | Change | Research effect |
|---|---|---|
| `computer.internal-cable` — **the fault in Task A** | Rebuilt as a true 24-pin connector, and **rotated from −18° to +26°**, so its bores face the participant's approach instead of facing away | Moves the answer's visibility without moving the answer. Should shift the split between finding the fault by *looking*, by *poking* and by *reading* — and that split is an outcome. **Open item ง.** |
| `computer.main-power-connector`, and the socket on the board | Built from one description, at real dimensions, so plug, socket and spare match | Task A is noticing that one of these is not in the other; they were previously three separate builds at three different sizes |
| `fan.working-fuse`, `fan.faulty-fuse`, and the fitted fuse | All now 6 × 30 mm glass cartridges, 0.7 mm element. The blown one differs **only** in the element | Previously an opaque dark blob inside the blown fuse's glass was visible from across the bench and handed over the diagnosis. The printed rating moved off the middle of the glass, where it covered the element on both fuses |
| `computer.power-button`, `fan.speed-selector` | Now a **push button** and a **dial** — previously the same three-cylinder shape in both conditions | The two conditions now differ in the hand action of the final step: pressing versus turning. **Open item จ.** |
| `fan.power-switch` | Now a slider | Was three flat boxes |
| `fan.fuse-holder`, fan service bay, `fan.power-plug`, `fan.power-cord`, `fan.internal-wire`, `fan.fastener` | Rebuilt at real dimensions | Most parts are now much smaller. **Open item ก.** |

Nothing marks the answer in either condition: the spare lead and the unplugged one share
body, size and rail colours; the good fuse and the blown one share glass, ferrules and
printed rating. Nothing was added to or removed from either bench, no click target was
resized, moved or re-owned, no task definition or data column changed, and all 31 object
ids are byte-identical to the baseline.
**Status:** open items ก, ง, จ. Record: `Verification/PART_RECOGNITION_RECORD.md`.

**`230ddfe`** — a stale consolidated third-party index was removed; each asset keeps its
own licence text beside the files it covers. No research effect.

---

## 2026-08-08 — Found while preparing the review package, not fixed

**Pointing at a part often selects a different part.**

A pointer ray cast from the participant's eye to the centre of what each part draws
resolves to a **different** part in **31 of 54 aims** across the two benches. Eleven
parts carry a default click target 1 000 mm across and 2 000 mm tall while their visible
bodies are between 11 mm and 571 mm; the size helper in both bench builders only handles
box shapes and silently skips capsule shapes.

Among the parts that cannot be aimed at: the fault in Task A (`computer.internal-cable`),
the only wrong-part action in the study (`computer.ram`), and the correct repair on the
fan bench (`fan.working-fuse`).

No existing check could see it: they all reach a part by name or by reference and never
point at anything. The play-mode repair-loop checks call the task controller's
record-interaction method directly with the part they looked up.

**Not fixed.** Resizing a click target changes which id lands in the event stream, which
is a research decision.
**Research effect if left:** per-part interaction counts from both benches would be close
to meaningless, and the fan task may not be completable by pointing.
**Status:** open item ช — the one item where the recommended answer is "change it".
Measurement: `Verification/Ray_Aim_Attribution.txt`, reproducible from
*Tools → VR Maintenance Research → Visual Audit → Report Ray Aim Attribution*.

---

## 2026-08-11 — Round 6: aligned to Research Proposal 1.0 (18 June 2569)

This round was worked against the proposal document itself rather than against the
build's own history, so for the first time the entries below say **what the proposal
requires** and **where the build now differs from it**. Five of the six differences are
deliberate and stated here for approval; one is a defect that was found and fixed.

### 6.1 The two tasks are different tasks again — proposal 9.3

**What the proposal says.** 9.3.1 makes Task A *งานเปลี่ยนส่วนประกอบ*: the participant uses
the in-VR manual to identify and replace the correct component, then confirms with
Inspect. 9.3.2 makes Task B *งานวินิจฉัยข้อขัดข้อง*: the participant uses the manual to
identify the fault **from several possible causes**, repairs it, confirms with Inspect,
and may correct and retry on failure. 9.3 then states plainly that Task B must involve
more diagnostic decision-making than Task A.

**What the build said.** Since `de7d5fd` both work orders read *"The unit is assembled and
open for service. Find the cause and repair it."* — the same goal sentence on both
benches, in all three languages. That commit was framing both scenes away from an
assembly reading and it succeeded at that, but it took the distinction between the two
tasks with it. With both benches asking for a diagnosis and the fan bench reduced to a
single candidate part, the study's task variable had collapsed.

| | Task A — Computer | Task B — Fan |
|---|---|---|
| Work order goal, before | Find the cause and repair it | Find the cause and repair it |
| Work order goal, now | *Follow the manual and fit the correct replacement component.* | *Several parts could be responsible. Find the cause and repair it.* |
| The word "cause" | **absent** | present |
| Candidate parts on the bench | one replacement path | three candidates to rule out, plus the fault site |

All three languages carry the same distinction; the wording is in
`TRANSLATION_REVIEW.md` rows 3 and 4 for the reviewer, and all three now live in one
method so they cannot drift apart again.

**The fan bench.** `fan.faulty-fuse`, `fan.motor-module` and `fan.non-target-module` were
switched off by `de7d5fd`; they are back, and none went back where it came from:

| Part | Where it is now | Why there |
|---|---|---|
| `fan.faulty-fuse` | spares tray, on its own pad, 120 mm from the working fuse | Two cartridges identical in glass, ferrules and printed rating. Which is sound can only be settled by picking each up and looking at the element — that inspection *is* the diagnosis 9.3.2 asks for. Two seated positions, not a heap. |
| `fan.motor-module` | service mat, right of the unit, against its pedestal | Reads as the motor out of this unit and available to check, rather than as a spare motor waiting to go into something — which is the reading that got it stowed. |
| `fan.non-target-module` | service mat, left of the unit | A sealed module a participant can reach, read and reject. Opposite the motor, so the two are on either side of the machine rather than in a row. |

The SPARE PARTS tray holds exactly one thing to fit. **One part to fit, several points to
check.**

**Research effect, and it is a gain.** `fan.faulty-fuse` is a `RepairAction` that is not
the required one, so the fan task can record an `IncorrectComponentInteraction` again —
fitting a cartridge whose element has parted, which is a real misdiagnosis and the direct
counterpart of `computer.ram` on the computer bench. Verified in play mode: acting on it
does not complete the task, and the correct fuse does. **This closes open item ค and the
`unsuccessful_action_count` asymmetry recorded in `KNOWN_LIMITATIONS.md`** — both benches
now offer exactly one wrong repair and one failed device test as failure routes.

No `stableObjectId`, collider, `ResearchInteractionKind`, `requiredRepairObjectId` or
`activeFaultId` changed. Verified: all 31 ids byte-identical to `HEAD`, every collider
size and centre unchanged, no interactable inactive in any scene.

**Status:** approval wanted on the two goal sentences and on the three restored parts.

### 6.2 Time limits are 600 s on both tasks — proposal 11.4.2 says 960 and 1200

**Deviation, deliberate, and the largest one in this round.**

| | Task A | Task B |
|---|---|---|
| Proposal 11.4.2 / 11.8.4 / 11.8.7 | 16 min = **960 s** (expected 10 min) | 20 min = **1200 s** (expected 12 min) |
| Build before this round | 900 s | 900 s |
| **Build now** | **600 s** | **600 s** |

**Why.** The proposal's figures were pre-study planning estimates, and its own expected
durations (10 and 12 minutes) sit well below its caps. The content is now designed to be
finished in **5–6 minutes**, and 600 s is a timeout that stops a stalled participant
being cut off arbitrarily — **not a target and not a pacing device**. Nothing in the room
displays the limit; the participant's timer counts elapsed time, not remaining time.

**What this costs.** Two things a supervisor should weigh:

- A participant who would have finished at 11 minutes under the proposal's Task B cap is
  now recorded as `TimedOut` at 10. If that happens in the pilot, the number to change is
  `maximumTimeSeconds` in the two task definition assets and nothing else.
- The two tasks now share one limit where the proposal gave Task B 25 per cent more. The
  proposal's asymmetry presumably reflected Task B being the harder task; a single limit
  removes that allowance. The counter-argument is that a shared limit is one fewer
  difference between the two conditions, and 11.7 asks for the procedure to be
  standardised. **This is a research decision and is not settled here.**

Total session time is unaffected in the direction that matters: 11.8 budgets ~75 minutes
and the caps are a worst case, not a plan.

**Status:** open, needs sign-off before collection.

### 6.3 First-person recording is real — proposal 9.11

**This was a defect, not a deviation.** `firstPersonRecordingConsent` and
`firstPersonRecordingEnabled` existed in the configuration, had a checkbox on the setup
screen, were validated against each other, and were written into every
`session_manifest.csv`. **There was no capture code anywhere in the project.** A session
run with both switches on produced a manifest stating that first-person recording was
consented to and enabled, beside a folder containing no footage. The manifest could lie,
and an ethics submission quoting 9.11 would have been describing something that did not
exist.

It exists now: `Scripts/Logging/FirstPersonRecorder.cs`, running during
`ComputerRepairTask` and `FanRepairTask` only, one file per attempt, in the same session
folder as the CSVs, named `{participant}_{Task}_attempt{n}.mjpeg`. No file at all is
created unless consent **and** enable are both set. Training is never recorded. Any
failure writes one `TechnicalError` into the event stream.

**Research effect:** none on the measured variables — it adds no cue, no control and no
feedback, and the participant is told about it before the session per 9.11. The costs are
storage and an unmeasured per-frame GPU cost on Quest 3, both in
`KNOWN_LIMITATIONS.md`.
**Status:** the consent wording in the ethics materials must now match a capability that
is actually present. Still listed under *Documents that must be updated*.

### 6.4 Thai and Japanese now cover the whole build — proposal 9.5, pending 9.13

**Before:** 69 of 94 participant-visible strings were English only, including the entire
training board, every bench placard, the whole lab notice board, and the status board a
participant looks at for the length of the task. A Thai or Japanese participant read the
work order and the source content in their own language and everything else in English.

**Now:** every participant-visible string is drafted in all three languages except six
that are deliberately untranslated — `INSPECT`, `RESET`, `Grip`, `Trigger`, `+10 s` and
the fan's `O F F 1 2 3` legend, all of which are printed on physical objects or are
numerals. The sheet for the 9.13 reviewer is `TRANSLATION_REVIEW.md`, and **every row in
it is marked DRAFT**.

**Research effect.** This cuts in both directions and both should be recorded:

- The condition the proposal describes — participants working in their own language — is
  now actually delivered. Before, comprehension of two thirds of the room's text was
  confounded with English reading ability, which is not a variable this study measures or
  balanced its groups on.
- **Any data collected before this round is not comparable with data collected after it**
  for anything involving reading, searching or hesitating in front of a sign. No
  participant data exists, so this costs nothing today.
- A mechanical check now enforces the numeric half of 9.5: every digit in an English
  string must appear in both translations. It cannot check meaning, which is what 9.13 is
  for.

**Status:** open until the 9.13 reviewer signs the sheet. Nothing here is approved.

### 6.5 Language is chosen on the setup screen and locked for the session — proposal 10.3.2

Language is a between-group condition, so it is set once by the researcher, before the
first task, and cannot change afterwards. English is the default. The participant has no
language control anywhere in the headset and, since `a32041c`, never sees the setup
screen at all.

The hole that is now closed: `ReturnToSetup` brings the researcher back to the setup
screen **between the two tasks**, and the language control there was live. Changing it
would have run Task A in one language and Task B in another under one participant code —
and because `session_manifest.csv` records one language per session, the file would not
have shown that it happened. The control now refuses while the log service is writing and
says why.

**Status:** closed, no decision needed.

### 6.6 The work order panel is 23 per cent larger, because Thai and Japanese did not fit

Found while checking 6.4. The panel was measured and sized in English only. The English
body copy measured 0.400 m against a 0.410 m box and fitted; the same panel measured
**0.490 m in Thai and 0.570 m in Japanese**, so the closing *Press INSPECT* line was cut
off by the plate in both of the languages this study is about. It is the same defect that
was found and fixed for English in `de7d5fd`, repeated for the two languages nobody
measured.

Shortening a translation to fit was not available — 9.5 requires the same information in
all three. Two cheaper fixes were measured and rejected: widening from 0.748 to 1.100 m
takes Japanese to 0.470 but leaves Thai at 0.490, because Thai has no spaces to wrap on;
dropping line spacing to zero saves 10 mm and costs readability. So the panel grew, mostly
sideways, to 1.085 x 0.620 m and the anchor moved 70 mm left.

**Research effect:** the work order is now the second-largest flat surface in the room and
is 23 per cent larger in area than the version that was cut back in an earlier round for
pulling the eye off the bench. That is a real salience change and it applies equally in
all three languages and both tasks. The alternative was a work order that two of the three
language conditions could not read to the end.
The validator now measures all three languages on every run, so it cannot regress silently.

**Status:** open — a supervisor may prefer a smaller panel and shorter copy in all three
languages instead.

### 6.7 Not done, and why

- **Task B's retry sentence.** 9.3.2 says Task B may be corrected and retried; 9.3.1 says
  nothing of the sort about Task A. Retry is available in **both** tasks in the build, and
  *number of retries is a primary outcome* under 10.2.1. Putting the sentence in Task B's
  brief only would tell one condition about an affordance the other also has, on the exact
  variable being measured. It is therefore in **neither** brief, which is where it was
  before this round. **Open: needs a decision, and the decision belongs to the supervisor,
  not to the build.**
- **`computer.non-target-module` stays stowed.** The three restored parts are Task B's
  candidate causes. Task A is a replacement task and does not need a distractor to rule
  out; adding one would move Task A toward the diagnosis reading that 6.1 just took out
  of it.
- **The task definition assets are still named `…Development` and carry
  `taskContentVersion: research-v2`.** Untouched this round.

---

## Still open from 2026-08-02, unchanged since

- The first-action metric fires about 14 ms into every task, from an incidental pointer
  hover at spawn. `action_occurred_before_first_information_access` is therefore `true`
  in every session in which any source is opened — which is the variable the study is
  designed to measure.
- The movement file's frame column reads `task-local` but the values are world
  coordinates.
- A hover of one object by both controllers is recorded as two events, so hover counts
  double whenever both pointers rest on one target.
- The participant start pose, 2.05 m from the bench edge and 1.05 m outside the marked
  work zone, so every task begins with locomotion.
