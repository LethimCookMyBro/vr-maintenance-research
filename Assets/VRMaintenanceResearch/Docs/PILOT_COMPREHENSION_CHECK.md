# Pilot comprehension check — protocol and record

Purpose: establish that a first-time participant understands **what the machine is,
what the task is, and what they can touch**, before the study starts collecting
data. Until that holds, the study measures how well people decode placeholder
geometry, not how they diagnose a fault.

This check is deliberately separate from the visual audit. The visual audit asks
"does the scene look finished". This asks "does a stranger know what to do".

---

## 1. Who to run it with

At least **three people who have not seen the project**, and who are not part of
the research team. Do not use anyone who has been shown a screenshot, a build, or
this document. Prior exposure destroys the measurement — you cannot un-know what
a green rectangle is once someone tells you.

Domain knowledge is not required and should not be screened for. A participant
who does not know what a motherboard is should still be able to say "that's a
computer, and something inside it is unplugged or broken".

---

## 2. Procedure

Run each person separately. **Give no help until the whole checklist is done** —
that is the point of it. Note what they say, not whether they were eventually
right.

1. Fit the headset with the task scene loaded and the participant at the start
   pose. Say only: *"Have a look around and tell me what you see."*
2. Let them look for 60 seconds without prompting. Silence is data.
3. Work through the five questions below, in order, verbatim.
4. Record answers in the score sheet, in the participant's own words.
5. Only after Q5, answer any questions they have. Note anything you had to
   explain — every one of those is a defect in the scene, not in the person.

---

## 3. The five checks

| # | Question to ask | Passes when |
|---|---|---|
| 1 | "What is the machine in front of you?" | They say computer / PC / desktop, or fan / desk fan, unprompted and without hedging. "Some kind of box"? Fail. |
| 2 | "What do you think you're being asked to do here?" | They describe finding out what is wrong and fixing it, in their own words. Reciting the brief back verbatim does not count — ask them to say it another way. |
| 3 | "Point at anything you could read for information. Anything you could pick up and use. Any spare parts. And the control that checks the machine." | They locate the information dock, the tool tray, the spares tray and the INSPECT control. Four separate answers, no coaching. |
| 4 | "Go ahead and start." | They begin acting on the machine without asking the researcher what to do first. Any question of the form "am I allowed to…" or "what do I press…" is a fail. |
| 5 | *(researcher's own judgement, recorded at the end)* | Any confusion they showed was about **the fault** — where it is, what is wrong, which part to use. Confusion about what an object *is*, whether something can be grabbed, or what a control does, is a fail. |

Check 5 is the one that matters. Checks 1–4 exist to make check 5 meaningful.

---

## 4. Score sheet

Copy one block per participant.

```
Participant ...........  Date ..........  Scene: Computer / Fan
Prior VR use: none / some / regular      Prior repair experience: none / some / trade

Q1 machine identified as: ......................................  PASS / FAIL
    time to answer: ...... s      hedged? Y / N

Q2 task in their words: ........................................  PASS / FAIL
    ................................................................

Q3 found:  manual/dock [ ]   tools [ ]   spare parts [ ]   INSPECT [ ]   PASS / FAIL
    anything they pointed at that is NOT interactive: ..............

Q4 started unaided:                                               PASS / FAIL
    first action taken: ............................................
    questions asked before starting: ...............................

Q5 confusion was about the fault, not the models or the UI:       PASS / FAIL
    what they misread: .............................................

Anything the researcher had to explain:
    ................................................................
```

---

## 5. Misunderstandings log

Every misunderstanding gets logged here with what was changed in response, or an
explicit note that it was accepted. This is the record that the scene was revised
against real confusion rather than against taste.

| # | Source | Misunderstanding | Change made |
|---|---|---|---|
| M1 | internal review of `Pass1_Fan_ParticipantEye.png` | The fan was read as a signpost / lamp / weather vane. Four blades were modelled as full-diameter bars crossing at the hub, so the head drew a plus sign. | Replaced with five rounded, offset paddles around a clear hub. The final front view reads as a normal desk fan without bent or broken-looking blades; the rear guard preserves the circular silhouette. |
| M2 | internal review of `Pass1_Fan_Workstation.png` | The removed fan guard lay flat on the lower shelf and read as a white plate or dish. | Rebuilt as a wire cage — segmented rim, inner ring, radial spokes, hub cap — and stood upright on the shelf facing the participant. |
| M3 | internal review of `Approach_Computer_OpenSide.png` | The CPU cooler read as a record player: a black disc with a hub, lying flat on the board. | Replaced with a tower cooler — cold plate, heatpipes, stacked fin block, 92 mm fan on the front face. Also gives the cavity depth, which was the reason the whole interior looked flat. |
| M4 | internal review of `Approach_Computer_OpenSide.png` | Components were sized in the case's axes rather than the board's, so DIMMs were 5 mm slivers lying on the board and the graphics card stood on edge like a second motherboard. | Board-local frame documented and every component re-sized in it: DIMMs stand out of their slots, the card lies horizontally as it does in a tower. |
| M5 | internal review of `Approach_Computer_OpenSide.png` | The brightest object in the case was a yellow front-panel ribbon; the eye landed on wiring instead of on components. | Ribbon recoloured to black and reduced. |
| M6 | internal review of `Pass1_*` | Both benches carried a decorative screwdriver and pliers next to the one screwdriver that can actually be picked up — three tools, one of which answers. | Decorative driver and pliers removed. The tool tray now holds a foam cut-out and the real tool; only consumables and PPE remain as dressing. |
| M7 | internal review of `Pass1_*` | Nothing distinguished an interactive object from scenery until it was grabbed. | `ResearchInteractable` now tints its own renderers while focused, with one tint for every interactable so it signals "this responds" and never "this is the answer". |
| M8 | internal review of the notice board | The wall carried a numbered PROCEDURE card — inspect, consult a source, repair, test — which prescribes the order of actions the study is trying to observe. | Replaced with a BENCH LAYOUT card that says where things are and nothing about sequence. |
| M9 | internal review of `Pass1_Fan_*` | The blown fuse and the good spare were marked with red and green bands, readable from across the room. | Both fuses now carry identical bodies, caps and printed ratings. The difference — a broken element and a light stain inside the glass — is only visible close up, and `CheckFaultNotAdvertised` in the validator fails the build if a signal colour returns to a fault-site part. |
| M10 | independent fresh-eyes screenshot review | Fan blades still appeared overlapped or damaged, and the service view was crowded. | Replaced the blade bars with five rounded paddles; added fixed front, service-bay, and fuse-detail capture poses. |
| M11 | independent fresh-eyes screenshot review | The open Fan reader showed mirrored duplicate text. | Build-time reader prefilling now keeps the TMP copy active and deactivates the legacy `TextMesh` copy; the validator fails if legacy reader text is active. |
| M12 | independent fresh-eyes screenshot review | Training objects read as abstract blocks and the intended actions were not readable from participant eye height. | Raised three front-facing plaques: `1 PICK UP`, `2 COMPARE`, and `3 TURN`; added participant-eye and workstation-detail captures. |
| M13 | source and runtime review | Hover tint existed, but select did not retain the same focus tint. | Wired `selectEntered`/`selectExited` to the existing reference-counted `Focus` path. Play Mode confirmed the tint changes and restores through public XRI events without modifying shared materials. |
| M14 | independent fresh-eyes screenshot review | Important components, fault sites, and feedback were not all evidenced in dedicated close views. | Added source-backed ATX, fuse, task-brief, Training workstation, and Training feedback poses. The new views expose geometry for inspection without adding arrows, fault labels, or signal colours. |

> Rows M1–M14 come from **internal review of rendered captures and Play Mode checks, not from human
> pilots.** They are recorded here because the same log has to carry both, and
> because each one was a real defect found and fixed. Human pilot rows start at
> P1 and are filled in by the researcher.

| # | Participant | Misunderstanding | Change made |
|---|---|---|---|
| P1 | | | |
| P2 | | | |
| P3 | | | |

---

## 6. Exit criterion

The application is **not** ready for participant testing until all three
participants pass all five checks, and every row in the misunderstandings log has
either a change made or a written reason for accepting it.

Current status: **Desktop and internal participant-comprehension refinement
completed. Meta Quest hardware confirmation and real first-time human pilot
P1–P3 remain required before participant data collection.** P1–P3 remain blank
because no human pilot has been run.
