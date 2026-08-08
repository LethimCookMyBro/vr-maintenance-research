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
| Task A | Component replacement only | Computer maintenance with inspection, information seeking, diagnosis, repair attempts, error correction and retries | Fault and completion rule to confirm before data collection |
| Task B | Fault diagnosis only | Electric-fan maintenance, same free interaction cycle | As above |
| Information | One internal manual, forced usage | Four independent concurrent source types; no recommended source, no forced order | Thai/Japanese/English equivalence still unchecked — see [`TRANSLATION_REVIEW.md`](TRANSLATION_REVIEW.md) |
| Validation | Inspect action as main validation | Observable interaction and device-test outcomes, raw events retained | Stop and scoring rules to confirm |
| Timing | Earlier fixed timings | Per-task configurable limits; development values are provisional | Research limits and timer visibility to approve |
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
