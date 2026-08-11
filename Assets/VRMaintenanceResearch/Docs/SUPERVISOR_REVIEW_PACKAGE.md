# Supervisor Review Package

**Branch:** `visual-polish-claude` · **Prepared:** 2026-08-11 (Round 7)
**Replaces** the package dated 2026-08-08 in full. That version described the build at
commit `0248328` and carried a correction banner on top of it; three of its questions
asked about things that no longer exist, and a reader following it would have been told
the opposite of what the build now does. It has been rewritten rather than annotated.

**Status:** every question below is open. Nothing here has been signed. No participant
data has been collected, and nothing on this branch has been merged to `main`.

---

## How to use this document

Each numbered section describes one decision, states what the build does now, states what
it costs either way, and ends with a single question you can answer **yes** or **no**.
The answer sheet in [§20](#20-answer-sheet) has a row, a yes box, a no box and a
signature line for every one of them.

Nothing in this document is filled in on your behalf. Where a section says what happens
if you answer no, that is a statement of consequence, not a suggestion.

**The words that carry weight**

| Word | What it means here |
|---|---|
| **scene** | One loadable room. There are four: the researcher's setup screen, the training room, the computer bench and the fan bench. |
| **object id** (`computer.ram`) | The permanent name a part carries in the recorded data. It never changes, so sessions can be compared. |
| **click target** | An invisible shape wrapped round a part. The software decides what you pointed at by testing the pointer against these shapes, **not** against the part you can see. |
| **event** | One line in the recorded data: a time, an object id, and what happened. |
| **`task_summary.csv`** | The one-row-per-task results file the analysis reads. |
| **builder** | A script that constructs a bench from scratch, so it can be rebuilt identically at any time. |
| **the proposal** | Research Proposal version 1.0, dated 18 June 2569, the document this build is measured against. |

---

## 1. The decisions, in one page

| # | Section | The question |
|---|---|---|
| **ก** | [§2](#2--every-part-has-moved-since-you-last-saw-a-position-table) | Do you accept the current positions and sizes of all 31 parts as the layout used for data collection? |
| **ฌ** | [§3](#3--task-a-is-a-replacement-task-again-and-task-b-has-several-candidate-causes) | Do you accept the two work-order goal sentences, and the three parts restored to the fan bench? |
| **ญ** | [§4](#4--both-tasks-stop-at-600-s-the-proposal-says-960-and-1200) | Do you accept 600 s on both tasks in place of the proposal's 960 s and 1200 s? |
| **ฏ** | [§5](#5--the-retry-sentence-is-in-neither-work-order) | Do you accept that neither work order mentions retrying, even though the proposal mentions it for Task B only? |
| **ฐ** | [§6](#6--the-participant-sees-a-clock-a-percentage-and-a-checklist) | Do you accept the three heads-up display elements being on during data collection? |
| **ฎ** | [§7](#7--the-work-order-panel-is-23-per-cent-larger-than-the-one-that-was-cut-back) | Do you accept the enlarged work-order panel? |
| **ซ** | [§8](#8--the-four-information-sources-are-always-in-the-same-order-and-a-way-to-vary-it-now-exists) | Do you accept a fixed source order, leaving position confounded with source type — or should the counterbalancing be switched on? |
| **ฑ** | [§9](#9--every-thai-and-japanese-string-is-a-draft) | Do you accept that no session may run until a Thai and a Japanese reader have signed the translation sheet? |
| **ฒ** | [§10](#10--first-person-recording-now-exists-and-the-consent-wording-has-not-caught-up) | Do you accept that the ethics and consent materials must be updated before any session, to describe a capability that now exists? |
| **ง** | [§11](#11--the-part-that-is-the-answer-to-task-a-was-turned-to-face-the-participant) | Do you accept the fault plug turned toward the participant's approach rather than away from it? |
| **จ** | [§12](#12--the-two-device-test-controls-do-not-look-alike) | Do you accept a push button in one condition and a dial in the other? |
| **ณ** | [§13](#13--the-inspect-control-no-longer-glows) | Do you accept the INSPECT control being paint-matched to the room's other panels? |
| **ด** | [§14](#14--two-pieces-of-room-dressing-were-removed) | Do you accept the room without the guardrails and the bench matting? |
| **ช** | [§15](#15--the-click-targets-were-resized-and-a-gate-now-holds-them-there) | Do you accept the resized click targets as the ones data will be recorded through? |
| **ฉ** | [§16](#16--data-recorded-before-743b1c3-filed-internal-parts-under-the-machines-name) | Do you accept that no session recorded before `743b1c3` can be pooled with later ones? |
| **—** | [§17](#17--four-standing-questions-unchanged-since-2026-08-02) | Four standing questions, unchanged and still unanswered. |

---

## 2. ก — Every part has moved since you last saw a position table

**What changed.** Between Round 1 and Round 5 every part on both benches was rebuilt from
licensed models or from real dimensions, and Round 6 moved three fan parts back onto the
bench. No table of the current positions has been through a review.

Most parts are now **much smaller** than the shapes they replaced, because they are at the
sizes the real objects are: a 6 × 30 mm fuse cartridge is a 6 × 30 mm fuse cartridge.

**What this does to the research.** Apparent size and reach distance both changed for
nearly every part, and both affect how long a participant takes to find and select
something. The two benches were re-measured against each other so that neither is
systematically harder to work at, but that balance has not been checked by a person in a
headset.

**What is not in question.** All 31 object ids are byte-identical to the baseline, no part
was added to or removed from either bench, and no click target was re-owned.

Positions: `Verification/PART_RECOGNITION_RECORD.md` and
`Verification/ITEM_3D_REBUILD_RECORD.md`. Pictures:
`Screenshots/Audit/Round7After_Computer_StartPose.png` and `…_Fan_StartPose.png`.

> ### Question ก
> **Do you accept the current positions and sizes of all 31 parts as the layout that data
> will be collected on?**
> If **no**, name the parts to change; every one is a value in a bench builder, and the
> bench is rebuilt from that value.

---

## 3. ฌ — Task A is a replacement task again, and Task B has several candidate causes

**Why this came up.** Proposal 9.3.1 makes Task A a component-replacement task and 9.3.2
makes Task B a fault-diagnosis task with **several possible causes**; 9.3 then says
plainly that Task B must involve more diagnostic decision-making than Task A.

An earlier round had set both work orders to the same sentence — *find the cause and
repair it* — and had switched three parts off the fan bench. With both benches asking for
a diagnosis and the fan bench reduced to a single candidate part, **the study's task
variable had become a constant.** Round 6 undid that.

| | Task A — Computer | Task B — Fan |
|---|---|---|
| Work order goal now | *Follow the manual and fit the correct replacement component.* | *Several parts could be responsible. Find the cause and repair it.* |
| The word "cause" | absent | present |
| Candidates on the bench | one replacement path | three to rule out, plus the fault site |

Three parts were switched back on, none of them where it had been: `fan.faulty-fuse` in
the spares tray 120 mm from the working fuse, `fan.motor-module` on the service mat right
of the unit, `fan.non-target-module` on the mat to its left. The two fuse cartridges are
identical in glass, ferrules and printed rating — which is sound can only be settled by
picking each up and looking at the element, and that inspection *is* the diagnosis 9.3.2
asks for.

**What this does to the research, and it is a gain.** `fan.faulty-fuse` is a repair action
that is not the required one, so the fan task can record an incorrect-component
interaction again — the direct counterpart of `computer.ram` on the computer bench. Both
benches now offer exactly one wrong repair and one failed device test as failure routes.
The asymmetry that made `incorrect_component_interaction_count` unusable on the fan task
is gone.

No object id, click target, interaction kind, required repair or fault id changed.

> ### Question ฌ
> **Do you accept the two work-order goal sentences, and the three parts restored to the
> fan bench?**
> If **no**, say which sentence or which part; the sentences live in one method in all
> three languages, and each part is one switch.

---

## 4. ญ — Both tasks stop at 600 s; the proposal says 960 and 1200

**This is the largest deliberate difference from the proposal in the build.**

| | Task A | Task B |
|---|---|---|
| Proposal 11.4.2 / 11.8.4 / 11.8.7 | **960 s** (16 min), expected 10 min | **1200 s** (20 min), expected 12 min |
| Build now | **600 s** | **600 s** |

**Why the build does this.** The proposal's caps are pre-study planning estimates and its
own expected durations sit well below them. The task content is now designed to be
finished in five to six minutes, and 600 s is a stop for a stalled participant rather than
a target. Nothing in the room displays the limit, and the participant's clock counts
elapsed time, not remaining time.

**What it costs, and both parts matter.**

- A participant who would have finished at 11 minutes under the proposal's Task B cap is
  recorded as `TimedOut` at 10.
- **The proposal gave Task B 25 per cent more time than Task A and the build gives them
  the same.** That allowance presumably reflected Task B being the harder task. A single
  limit removes it. The argument the other way is that one limit is one fewer difference
  between the two conditions, and 11.7 asks for the procedure to be standardised. This is
  a research decision and the build does not settle it.

If the answer is to restore the proposal's figures, the value to change is
`maximumTimeSeconds` in the two task definition assets and nothing else.

> ### Question ญ
> **Do you accept 600 s on both tasks, in place of the proposal's 960 s for Task A and
> 1200 s for Task B?**
> If **no**, state the two numbers to use.

---

## 5. ฏ — The retry sentence is in neither work order

Proposal 9.3.2 says Task B may be corrected and retried. 9.3.1 says nothing of the sort
about Task A. **In the build, retry is available in both tasks** — and *number of retries
is a primary outcome under 10.2.1.*

That leaves three options and no neutral one:

| Option | What it does |
|---|---|
| Sentence in Task B's brief only, as the proposal reads | Tells one condition about an affordance the other also has, on the exact variable being measured. Task B participants would retry more because they were told they could. |
| Sentence in both briefs | Departs from the proposal's wording, but gives both conditions the same information about the same affordance. |
| **Sentence in neither brief — what the build does** | Both conditions discover retry the same way: by failing a device test and finding they can act again. Departs from the proposal's wording in the other direction. |

The build takes the third option because it keeps the two conditions equal on a primary
outcome. It is not the proposal's wording, and it is not obviously right.

> ### Question ฏ
> **Do you accept that neither work order mentions retrying?**
> If **no**, say whether the sentence goes in Task B only, as the proposal reads, or in
> both briefs.

---

## 6. ฐ — The participant sees a clock, a percentage and a checklist

The participant wears a head-referenced display carrying **elapsed time**, a **completion
percentage** (0 / 33 / 67 / 100) and a **three-line objectives checklist** that ticks
itself as steps complete. **None of the three existed in the protocol the task design was
built around.** All three are on by default.

Each is a separate switch and each is written into `session_manifest.csv` as `show_timer`,
`show_progress` and `show_objectives`, so **a session is only interpretable against the
display it was collected under, and no analysis may pool sessions without checking those
three columns.**

**What each one changes.**

- **A readable clock turns an untimed diagnostic task into a self-paced timed one.** The
  direction is not uniform — some participants hurry and stop searching earlier, others
  slow down. Every duration in `task_summary.csv` is therefore measured under a different
  task than it was before. `low_activity_period_count` is the most exposed: its 30 s
  inactivity threshold was calibrated without a visible clock.
- **A checklist that ticks itself is correctness feedback.** The participant learns *at
  the moment they act* that the repair was the right one. `incorrect_component_interaction_count`
  and `device_test_failed_count` can no longer be read as a search process, because after
  the first tick the search is over. The rows never name a part, a tool or a procedure, so
  the display does not shorten the search *before* the first correct action — it removes
  the uncertainty after it.
- **A completion percentage tells the participant how much task structure remains.**
  Seeing 33 % after a failed device test says "two of three things remain", which
  participants were previously expected to infer.
- **The three interact.** A session with all three is not comparable with one that has the
  timer alone.

Turning any of them off needs no rebuild: three checkboxes in the researcher setup screen,
cleared before Start Session, and the manifest records the choice.

Full detail: `KNOWN_LIMITATIONS.md`, *Participant heads-up display*.

> ### Question ฐ
> **Do you accept all three display elements being on during data collection?**
> If **no**, name which of the three to clear — timer, percentage, checklist — and whether
> that holds for the pilot only or for the whole study.

---

## 7. ฎ — The work order panel is 23 per cent larger than the one that was cut back

**Found while checking the translations, and it was a defect.** The panel had been measured
and sized in English only. The English body copy measured 0.400 m against a 0.410 m box and
fitted; the same panel measured **0.490 m in Thai and 0.570 m in Japanese**, so the closing
*Press INSPECT* line was cut off by the plate **in both of the languages this study is
about**.

Shortening a translation to fit was not available: 9.5 requires the same information in all
three. Two cheaper fixes were measured and rejected — widening to 1.100 m takes Japanese to
0.470 but leaves Thai at 0.490, because Thai has no spaces to wrap on; dropping line
spacing to zero saves 10 mm and costs readability. The panel grew, mostly sideways, to
**1.085 × 0.620 m**, and its anchor moved 70 mm left.

**What this does to the research.** The work order is now the second-largest flat surface
in the room, and it is 23 per cent larger in area than the version an earlier round cut
back for pulling the eye off the bench. That is a real salience change. It applies equally
in all three languages and both tasks, so it cannot differ between the conditions being
compared — but it is a bigger sign than the one that was judged too big. The alternative
was a work order that two of the three language conditions could not read to the end.

The validator now measures all three languages on every run, so this cannot regress
silently.

> ### Question ฎ
> **Do you accept the enlarged work-order panel?**
> If **no**, the alternative is shorter copy in all three languages, which means rewriting
> the one text that states the task.

---

## 8. ซ — The four information sources are always in the same order, and a way to vary it now exists

**This is the longest-standing open item, and it now has a switch attached to it.**

The dock sorts its four cards by source type, so the row reads

> **manual → troubleshooting guide → video → visual guide**

left to right, in both tasks, for every participant, always.

**Position in the row is therefore perfectly confounded with source type.** A participant
who reaches for the leftmost card because it is leftmost cannot be told apart from one who
reaches for the manual because it is the manual — and *which source a participant chooses*
is a primary outcome under proposal 10.2.2.

The dock itself is good for the study in every other respect: the four cards are within
1.8 % of each other in distance and 2 % in apparent size, so no source is nearer, larger or
easier to reach than another. Only their order is fixed.

**What was built in Round 7.** Four orders — a cyclic Latin square in which each source
type appears exactly once in each of the four slots — each with its own
`information_source_layout_id`. A participant is assigned one deterministically from their
participant code, so the assignment is reproducible from the recorded data alone and needs
no separate schedule file. The id is written to `session_manifest.csv` and to every event
row. Nothing about the dock's position, angle, card size, spacing or distance changes; only
which card sits in which of the four existing slots.

**It is switched off.** The build ships with the fixed order and records the same layout id
it always has. Turning it on is one boolean, and it is not being turned on here.

**How the results read if it stays off.**

- Any difference between sources — how often each is opened, how long it is read, which is
  opened first — **cannot be attributed to source type**. Leftmost-and-nearest and "the
  manual" are the same thing in this build, and no statistical control separates them
  afterwards, because there is no variation to model.
- What still holds is the between-group comparison: if Thai and Japanese participants
  differ in how they use the leftmost card, that difference is not explained by its
  position, because its position is identical for both groups. **Objective 5.1 and
  hypothesis 7.1 survive. The parts of 5.2 and 10.2.2 that concern choice among sources do
  not.**
- It would then have to be written up as a stated design limitation. A reader who sees four
  source types and one layout id will otherwise assume the order was varied.

**If it is switched on**, the design itself is still yours: four orders divide 8
participants per group evenly where 24 cannot, and a cyclic square balances position but
preserves relative adjacency — a Williams square would balance adjacency too. Changing that
means editing one table.

> ### Question ซ
> **Do you accept a fixed source order for every participant, leaving position confounded
> with source type — or should the counterbalancing be switched on?**
> Answering **yes** keeps the build as it ships and requires the confound to be stated as a
> limitation in the write-up. Answering **no** means switching it on, and naming which
> design: the cyclic square that is built, or another.

---

## 9. ฑ — Every Thai and Japanese string is a draft

Every participant-visible string is now drafted in all three languages, except six that are
deliberately untranslated because they are printed on physical objects or are numerals:
`INSPECT`, `RESET`, `Grip`, `Trigger`, `+10 s`, and the fan's `O F F 1 2 3` legend.

**Every row of `TRANSLATION_REVIEW.md` is marked DRAFT.** No Thai reader and no Japanese
reader has checked any of it. Proposal 9.13 requires an expert outside the research team to
check the instruments for suitability, clarity of language and fit with the objectives
before real data collection, and 9.5 requires the Thai and Japanese manuals to carry
equivalent meaning, structure, numbers and instructions.

A mechanical check enforces the numeric half of 9.5 — every digit in an English string must
appear in both translations. **It cannot check meaning**, which is exactly what 9.13 is for.

Until that check happens, the language condition is a condition in name only: the strings
are in the right language and nobody has confirmed they say the right thing.

> ### Question ฑ
> **Do you accept that no participant session may run until a Thai reader and a Japanese
> reader have signed `TRANSLATION_REVIEW.md`?**
> If **no**, state what standard of checking is sufficient instead.

---

## 10. ฒ — First-person recording now exists, and the consent wording has not caught up

**This was a defect, and it was the serious kind.** The configuration carried
`firstPersonRecordingConsent` and `firstPersonRecordingEnabled`, they had a checkbox on the
setup screen, they were validated against each other, and they were written into every
`session_manifest.csv`. **There was no capture code anywhere in the project.** A session run
with both switches on produced a manifest stating that first-person recording was consented
to and enabled, beside a folder containing no footage. **The manifest could lie**, and an
ethics submission quoting proposal 9.11 would have described something that did not exist.

It exists now: one file per attempt, in the same session folder as the CSVs, during the two
maintenance tasks only. No file is created unless consent **and** enable are both set.
Training is never recorded. Any failure writes one `TechnicalError` into the event stream.

**What this does to the research:** nothing on the measured variables — it adds no cue, no
control and no feedback. What it changes is the ethics position. The proposal already
describes the recording (9.11) and requires that participants be told beforehand and may
decline without consequence. The consent materials must now describe a capability that is
actually present.

> ### Question ฒ
> **Do you accept that the ethics and consent materials must be updated to match this
> capability before any session runs?**
> If **no**, the alternative is to hold both switches off for the whole study, in which case
> the proposal's 9.11 should be amended rather than the consent form.

---

## 11. ง — The part that is the answer to Task A was turned to face the participant

`computer.internal-cable` — **the fault in Task A** — was rebuilt as a true 24-pin connector
and **rotated from −18° to +26°**, so its bores face the participant's approach instead of
facing away.

**What this does to the research.** It moves the answer's *visibility* without moving the
answer. It should shift the split between finding the fault by **looking**, by **poking**
and by **reading the manual** — and that split is an outcome under 10.2.2, not a
side-effect. Nothing marks the part as the answer: the spare lead and the unplugged one
share body, size and rail colours.

> ### Question ง
> **Do you accept the fault plug turned toward the participant's approach?**
> If **no**, the rotation is one value in the computer bench builder.

---

## 12. จ — The two device-test controls do not look alike

The final step of both tasks is the same action — confirm with Inspect — but the control is
now a **push button** on the computer bench and a **rotary dial** on the fan bench. They
were previously the same three-cylinder shape in both conditions.

**Why.** Each control now looks like what its object id calls it: `computer.power-button`
and `fan.speed-selector`. A flat disc read as an indicator lamp as easily as a control.

**What this does to the research.** The two conditions now differ in the **hand action of
the final step**: pressing versus turning. That is a real difference between the conditions
on the step that ends the task, and completion time includes it. It is small, but it is not
nothing, and it is not something the proposal asks for.

> ### Question จ
> **Do you accept a push button in one condition and a dial in the other?**
> If **no**, both can be the same shape, at the cost of one of them not matching its own id.

---

## 13. ณ — The INSPECT control no longer glows

**Round 7, and it touches a primary outcome.**

The one control a participant must press to end a task was the brightest saturated object in
the room: a 190 mm emissive disc on the pedestal cap plus an emissive button cap, both lit
from inside. It does not reveal the fault — but it says *press this first*, and **where a
participant goes first is a primary outcome under 10.2.2.**

| Surface | Before | Now |
|---|---|---|
| Pedestal cap ring | `Lab_Accent`, saturation 0.80, value 0.90, emitting | `Lab_MetalDark`, saturation 0.12, **not emitting** |
| Button cap | `Lab_Accent`, saturation 0.80, value 0.90, emitting | `Lab_Trim`, saturation 0.39, value 0.36, **not emitting** |
| *For comparison — dock cards* | `Lab_Navy`, saturation 0.50, value 0.22 | unchanged |
| *For comparison — notice board* | `Lab_StationBoard`, saturation 0.29, value 0.28 | unchanged |

The button cap now sits inside the range the room's other panels occupy, and nothing at the
station emits light. **The sign is kept**: the plate still reads *INSPECT* over *PRESS TO
CHECK THE UNIT*. The control is found by reading, which is the behaviour 10.2.2 is about,
rather than by being the only lit thing in the room. Paint only — no collider, position,
scale or id changed, and it applies to both benches.

**The cost.** A control that does not announce itself is a control some participants will
take longer to find, and that time lands in `completion_time_seconds`. The trade is between
a clean first-action measure and a discoverable final step.

> ### Question ณ
> **Do you accept the INSPECT control paint-matched to the room's other panels?**
> If **no**, the value to change is one colour name in the bench dressing builder.

---

## 14. ด — Two pieces of room dressing were removed

A yellow guardrail ran down each side aisle and green ESD matting ran the length of the
bench. Both are gone, because neither could exist without standing inside something else — a
new check found **87 intersections across the four scenes**, and there are now none.

The guardrails stood 107 mm in front of the storage unit on one side and 62 mm in front of
the racking on the other, so they fenced off shelves a participant is meant to walk up to,
and this room has no fall, machine or vehicle lane for a guardrail to separate anyone from.
The matting passed 4 mm through both trays and the service pad; stacking it correctly would
have meant lifting the trays and every part resting in them, which is a change to where task
apparatus sits, made for a decoration.

**What this does to the research.** Nothing removed is selectable, carries a collider, or is
an information source, and no id, collider, task definition, event type or CSV column
changed. The honest caveat is that the room is now slightly **barer** along both side aisles,
and scene richness is not a variable this study controls. It is identical in both tasks and
both language conditions, so it cannot differ between the conditions being compared.

Before and after from the start pose: `Screenshots/Audit/Round7Before_*` against
`Round7After_*`.

> ### Question ด
> **Do you accept the room without the guardrails and the bench matting?**
> If **no**, either can be rebuilt, but each needs somewhere to stand that is not inside
> something else.

---

## 15. ช — The click targets were resized, and a gate now holds them there

**The previous package asked whether to do this. It has been done, so the question has
changed.**

Eleven parts carried a click target 1000 mm across and 2000 mm tall while their visible
bodies were between 11 mm and 571 mm, because the size helper in both bench builders
silently skipped anything that was not a box shape. **31 of 54 test aims resolved to a
different part than the one aimed at**, including the fault in Task A, the only wrong-part
action in the study, and the correct repair on the fan bench.

They were resized to match their parts. The widest excess either bench now carries is 52 mm,
on the 200 mm screwdriver. Every object id, event type and data column is unchanged; only
the size of eleven invisible shapes changed.

**Why this still needs your signature.** Resizing a click target changes **which object id
lands in the event stream for a given pointing action**. The parts are the same parts, but
what the data records when a participant points at a crowded area is not what it would have
recorded before. That is a change to what the data means, and it was made without a
decision.

**What Round 7 added.** The old check failed only when a part could not be selected **at
all** — the far end of a long slope, and the slope had already been walked once: putting the
fan's motor module in the wrong place took `fan.power-plug` from 94 of 135 test aims to 49
and `fan.power-cord` from 49 to 20, and every check stayed green. A person noticed by reading
the numbers, which is not a check.

The aim counts for all 31 parts are now committed to a baseline file, and the gate fails when
any part drops more than 25 per cent below it, when a part cannot be selected at all, or when
the part list stops matching the baseline. **The baseline is never rewritten automatically** —
only a person choosing the menu item writes it, so a regression cannot quietly become the new
normal. Verified by reproducing the original defect: the gate now reports it as −97.9 % and
−53.1 %.

Measurement: `Verification/Ray_Aim_Attribution.txt` and `Verification/Ray_Aim_Baseline.tsv`.

> ### Question ช
> **Do you accept the resized click targets as the ones data will be recorded through?**
> If **no**, name the parts whose targets should differ; the sizes are values in the two
> bench builders, and the baseline is re-recorded deliberately afterwards.

---

## 16. ฉ — Data recorded before `743b1c3` filed internal parts under the machine's name

Because internal parts are built as children of their machine, and each machine's own
click-target list was left empty, the interaction toolkit filled that list with every target
underneath it. The machine registered first, so **every hover and selection of an internal
part was recorded against the machine's id** — `computer.case` for six parts, `fan.body` for
five. The only outward symptom was one warning line at scene load, and because both correct
repair parts sit outside their machine, the repair loop always completed.

**It is fixed.** But data recorded before that commit cannot be pooled with data recorded
after it for any analysis naming an individual part, and **it is not recoverable** — the rows
do not record which child was under the pointer.

No participant data exists, so at present this costs nothing. It needs recording so that it
cannot be discovered later.

> ### Question ฉ
> **Do you accept that no session recorded before `743b1c3` may be pooled with later ones?**

---

## 17. — Four standing questions, unchanged since 2026-08-02

These have been open through seven rounds and none has been answered.

| # | The question | Why it matters |
|---|---|---|
| **1** | Does an incidental pointer hover count as a meaningful first action? | The first-action metric fires about 14 ms into every task from a hover at spawn, so `action_occurred_before_first_information_access` is `true` in every session in which any source is opened — which is the variable the study is designed to measure. |
| **2** | Should the movement file's frame label be corrected to `world`? | The column reads `task-local` and the values are world coordinates. The data is right and the label is wrong. |
| **3** | Should a two-controller hover of one object record one event or two? | It currently records two, so hover counts double whenever both pointers rest on one target. |
| **4** | Do you accept the participant start pose, 2.05 m from the bench edge and 1.05 m outside the marked work zone? | Every task therefore begins with locomotion, which is in every duration measure. |

> ### Questions 1–4
> Each is answerable yes or no on the sheet below. **Question 1 is the one that changes an
> outcome variable rather than a label.**

---

## 18. What is no longer a question, and why

The previous package asked three things that no longer describe the build. They are listed
here so that nothing looks quietly dropped.

| Retired | Was | Why it is gone |
|---|---|---|
| **ข** | *Do you accept diagnosis, with fewer parts on the bench, as the task both conditions present?* | Round 6 reversed it. Task A is a replacement task again and the fan parts are back — the question now asks the opposite thing, as **ฌ** in §3. |
| **ค** | *Do you accept that a wrong-part action is only recordable on the computer task?* | Closed. `fan.faulty-fuse` is back on the bench, so the fan task records incorrect-component interactions again. Both benches now have one wrong repair and one failed device test. |
| **ช** (old form) | *Should the eleven oversized click targets be resized before any participant session?* | They were resized. The question is now whether you accept the resized targets, in §15. |

---

## 19. What was verified for this package, and what was not

**Verified in the editor on 2026-08-11, on branch `visual-polish-claude`:**

| Check | Result |
|---|---|
| Scene integrity, 9 tests — including the new ray-aim baseline gate | **9 / 9 pass** |
| Scene validator, 4 scenes | **All pass, no warnings** |
| Foundation edit-mode tests | **All pass** |
| Repair loop, computer bench, in play mode | **Pass** — fail → repair → pass → reset |
| Repair loop, fan bench, in play mode | **Pass** — fail → repair → pass → reset |
| Training room, four skills and the gated Continue | **Pass**, and relocks after reset |
| Full session, both task orders | See `Verification/Full_Flow_Walkthrough_*.txt` |
| Prop intersections, 4 scenes | **None** — was 87 |
| Ray aim attribution, 31 parts | No part unreachable; no part below its baseline |
| Object ids | All 31 byte-identical to the baseline |
| Room dressing | Carries no collider and no interactable |
| Console | No errors |

**Not verified, and claimed nowhere in this document:**

- **Anything on a physical Meta Quest 3.** A build exists; it has never been installed or
  run. No frame rate, comfort, legibility or tracking claim is made anywhere here. The
  head-locked display in §6 is the most exposed: it sits 1.15 m from the eye and has never
  been worn.
- **Whether any of this helps a real participant.** No person has been through any version
  of either bench.
- **Whether the Thai and Japanese wording is correct.** See §9.

---

## 20. Answer sheet

Nothing below is filled in. One signature per decision.

| # | Question | Yes | No | Signature | Date |
|---|---|---|---|---|---|
| ก | Accept the current positions and sizes of all 31 parts? | ☐ | ☐ | | |
| ฌ | Accept the two work-order goal sentences and the three restored fan parts? | ☐ | ☐ | | |
| ญ | Accept 600 s on both tasks instead of 960 s and 1200 s? | ☐ | ☐ | | |
| ฏ | Accept that neither work order mentions retrying? | ☐ | ☐ | | |
| ฐ | Accept the timer, the percentage and the checklist being on? | ☐ | ☐ | | |
| ฎ | Accept the enlarged work-order panel? | ☐ | ☐ | | |
| ซ | Accept a fixed source order, leaving position confounded with source type? | ☐ | ☐ | | |
| ฑ | Accept that no session runs until both translations are signed? | ☐ | ☐ | | |
| ฒ | Accept that consent materials must be updated before any session? | ☐ | ☐ | | |
| ง | Accept the fault plug turned toward the participant's approach? | ☐ | ☐ | | |
| จ | Accept a push button in one condition and a dial in the other? | ☐ | ☐ | | |
| ณ | Accept the INSPECT control paint-matched to the other panels? | ☐ | ☐ | | |
| ด | Accept the room without the guardrails and the bench matting? | ☐ | ☐ | | |
| ช | Accept the resized click targets as the ones data is recorded through? | ☐ | ☐ | | |
| ฉ | Accept that pre-`743b1c3` sessions cannot be pooled with later ones? | ☐ | ☐ | | |
| 1 | Does an incidental pointer hover count as a meaningful first action? | ☐ | ☐ | | |
| 2 | Correct the movement file's frame label to `world`? | ☐ | ☐ | | |
| 3 | Should a two-controller hover record one event instead of two? | ☐ | ☐ | | |
| 4 | Accept the participant start pose, 2.05 m from the bench? | ☐ | ☐ | | |

**Where an answer is "no", the change it implies:**

_________________________________________________________________________________

_________________________________________________________________________________

_________________________________________________________________________________

**Reviewer name:** _______________________________

**Signature:** _______________________________  **Date:** _______________

---

*Prepared on branch `visual-polish-claude`. Not merged to `main`. The companion record of
what changed and when is [`PROTOCOL_CHANGE_LOG.md`](PROTOCOL_CHANGE_LOG.md); the standing
list of costs the build carries is [`KNOWN_LIMITATIONS.md`](KNOWN_LIMITATIONS.md).*
