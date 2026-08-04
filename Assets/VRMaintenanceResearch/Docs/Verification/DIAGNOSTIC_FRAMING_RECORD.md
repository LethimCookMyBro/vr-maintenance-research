# Diagnostic framing pass

Both maintenance scenes were reading as assembly tasks. This pass makes each
bench state one goal — *this unit is broken, find the fault and fix it* — and
fixes the wall board, whose text was present but invisible.

No StableObjectId, ResearchInteractable, collider, socket or task definition was
changed. Verified below.

## 1. Wall board text — diagnosis

Reported as "missing". Measured in the editor before any change:

| Element | World z | Note |
|---|---|---|
| Notice card `Face` plate | 3.1660 – 3.1840 | opaque, `UI_PanelSurface` |
| Card `Heading` glyphs | 3.1750 | dead centre **inside** the plate |
| Board `Bay Title` glyphs | 3.1590 | in front of its band — this one was visible |

The participant stands at z ≈ −1.6 looking toward +z, so each plate's near face
sat **9 mm closer to the eye than its own text** and the depth test discarded
every glyph. All four cards rendered as blank white rectangles.

So: the content was not empty, and no reference had failed. `Label()` positions
were the fault — the header band avoided it with a −0.016 offset, `BuildCard`
placed its label at the card origin.

Second defect found in the same place: the cards only ever carried a heading.
There was no body copy to show even if it had rendered.

**Fixed** — both labels now stand 14 mm proud of the plate, each card carries two
lines of body copy, and the type is sized to fill the card (heading 0.34, body
0.30, up from 0.20/0.15 — at the old size it was unreadable from the 4.8 m start
pose). Contrast is `#14202E` and `#2B3949` on `#F4F6F8`, roughly 15:1 and 11:1.

Board copy is lab procedure only — safety, bench layout, ESD, fault reporting.
Nothing on it names a component or a symptom: it is read from anywhere in the
room and a notice naming either fault would answer the task from the doorway.

## 2. Computer bench — repair, not assembly

The machine was already assembled. What said otherwise was the bench.

| Was | Now |
|---|---|
| Bare DDR4 DIMM on an antistatic pad in the spares tray | seated in the board's fourth memory slot |
| Sealed anonymous module in the spares tray | stowed |
| Pale "Parts Pad" under the DIMM | removed |
| Side panel loose on the lower shelf | same shelf, now captioned REMOVED PARTS |

A bare board part on a pad next to an open case is what an assembly bench looks
like. The tray now holds one part — the replacement 24-pin lead — and the tool
tray one screwdriver.

`computer.ram` was not deleted: it is the same object with the same id, the same
`RepairAction` kind and the same collider, seated on the board. Its telemetry
role is unchanged and arguably sharper — selecting it still logs
`IncorrectComponentInteraction`, but now that means "pulled the memory on a
machine that will not power on", which is a real misdiagnosis, rather than
"picked up a spare from a box".

The work order gained one line in all three languages:

> The unit is assembled and open for service. Find the cause and repair it.

It names no component and no procedure; the validator's leak check still passes.

## 3. Case shell

The wrapper, the `computer.case` id and the grab collider are untouched. The
visual shell was rebuilt: the old one was a five-sided grey box with two 5.25"
optical bays — a beige-era tower drawn around a licensed B450 board, a Wraith
cooler and a modular supply.

- Front: full-height mesh intake behind a chamfered graphite bezel, brushed edge
  strip, modern I/O (power, 2 × USB-A, USB-C, audio). Optical bays gone.
- Closed side: glass panel in a steel frame — the twin of the one on the shelf.
- Roof: vent field over the rear two thirds. Rear: 7 slot covers with screws,
  exhaust grille, PSU cutout. Feet raised over a bottom intake filter.
- Interior: matte graphite liners plus a rear wall.

Materials moved with it. `Lab_CaseSteel`, `Lab_CasePanel` and `Lab_CaseInterior`
were mid grey at 0.55–0.85 metallic, which under the ceiling panels rendered as
near-white: the tower read as a pale carton and the machine inside it as debris
in a box. They are now painted steel and powder coat at the value real chassis
are.

`Lab_PlasticDark` dropped from 0.52 to 0.26 smoothness. At 0.52 every
upward-facing dark surface returned a highlight strong enough to read light grey.

Two things inside the case were fixed while measuring the above:

- The handmade graphics card's outer edge was a 222 mm strip of `Lab_MetalDark`
  square-on to the open panel. That strip — not the backplate — was what made the
  card read as a bright shelf spanning the cavity. Now matte. Its near-white
  backplate label went the same way.
- The drive cage — a 120 mm pale box standing on the case floor — is gone. The
  licensed part is an M.2 stick, so it now mounts flat on the board. It was also
  being fitted wrong: `ImportedVisual` fits uniformly on the tightest axis and the
  box it was given was not proportional to the mesh, so an 80 mm drive was coming
  out 36 mm long and under a millimetre thick. Corrected to the mesh's own
  1.190 × 0.104 × 4.289 ratio at a true 80 × 22 × 2 mm, and turned −90° rather
  than +90° so the printed face shows: the kit's artwork is on the mesh's −Y side
  and was rendering mirrored.

A basement divider was built and then removed. From the participant's approach,
which looks straight into the open side, a full-width shelf at that height reads
as a drawer pulled out of the machine.

## 4. Fan bench

One assembled fan, one open service bay, one fitted fuse, one spare, one tool.

| Stowed | Why |
|---|---|
| `fan.faulty-fuse` | a second loose cartridge, also blown — a second diagnosis stacked on the first, and from the bench it read as a box of fuses to fit |
| `fan.motor-module` | largest object on the bench, not the fault, mentioned by no information source; a spare motor beside a part-stripped fan says "assemble this" |
| `fan.non-target-module` | as the computer bench |

The mains lead was two objects reading as two problems: the plug stood 0.32 m
clear of its own coil. The plug now sits at the coil's edge with a tail into it,
and the coil's other end runs to the fan's cord gland — one lead, unplugged.

The spare fuse rests on a pale pad. A cartridge fuse is 30 mm of clear glass;
alone on the floor of a 0.5 m tray it was invisible from the start pose, and a
tray captioned SPARE PARTS that looks empty says the wrong thing.

The service-area caption was `INSTALLED COMPONENT`, sitting on the mat under the
whole fan and naming the machine a component. It now matches the computer bench:
`SERVICE AREA`.

### Consequence to be aware of

`fan.faulty-fuse` was this scene's only `RepairAction` other than the correct
one, so with it stowed the fan task can no longer record an
`IncorrectComponentInteraction`. That is the direct cost of "one installed fuse,
one spare fuse". The computer scene still records one, via `computer.ram`.

Nothing was deleted: reactivating the GameObject restores the part exactly, and
the builders are idempotent across it — `ResearchBuildKit.FindAny` sees inactive
objects, so a rebuild refreshes a stowed part before putting it back.

## 5. Collider shadowing — found while verifying, fixed

Not part of the brief, but it defeated the telemetry the brief says to preserve,
so it is fixed here.

`XRBaseInteractable` auto-collects `GetComponentsInChildren<Collider>()` when its
collider list is empty, and both builders deliberately reparent the in-machine
interactables under their device so local coordinates stay readable. The device
therefore claimed its children's colliders — `Desktop Case` held seven, `Electric
Fan Body` six — and because the parent registers first,
`XRInteractionManager.TryGetInteractableForCollider` returned the **device** for a
ray aimed at any part inside the machine.

Every hover and grab on the ATX connector, the fuse holder, the board, the supply
or the case fan was being recorded against `computer.case` / `fan.body`.

It was invisible: the correct repair object sits out on the bench in both scenes,
so the loop completed and the play-mode check passed. The only symptom was an XRI
warning at `OnEnable`.

This predates this pass — the reparenting is long-standing — but seating
`computer.ram` on the board added one more shadowed part to it, so it could not be
left.

**Fixed** — `ResearchBuildKit.BindOwnColliders()` writes each interactable's
collider list explicitly to its own collider, which makes the nesting irrelevant.
Both builders call it. Verified: 13/13 and 15/15 interactables bound, zero
unbound, and no "already registered" warning in either scene at play-mode entry.

## 6. Verification

Editor-side only. No Quest hardware profiling and no human pilot data.

**Visual validator** — `Tools ▸ Visual Audit ▸ Validate Scenes`

```
=== ComputerRepairTask ===  PASS
=== FanRepairTask ===       PASS
=== VRTraining ===          PASS
=== ResearcherSetup ===     PASS
ALL SCENES PASS
```

Three `WARN` lines remain against `TrainingDevelopment` for `training.cube-a`,
`training.cube-b` and `training.cylinder`. They are a pre-existing task-definition
mismatch, unchanged by this pass and reported as warnings by design.

**Repair loop, play mode** — `Run Runtime Checks (Play Mode)`, both task scenes.
A visual audit cannot see this: a scene can look right and still have a rebuild
that detached the repair object from its task.

| | Computer | Fan |
|---|---|---|
| required repair id | `computer.main-power-connector` | `fan.working-fuse` |
| test before repair | Active — PASS | Active — PASS |
| repair object found | Main Power Connector (RepairAction) | Working Replacement Fuse (RepairAction) |
| test after repair | Completed — PASS | Completed — PASS |
| reset | Active — PASS | Active — PASS |

**Stable ids** — every `stableObjectId` in all four scenes is byte-identical to
the pre-change baseline:

```
diff <(git show HEAD:<scene> | grep stableObjectId | sort) \
     <(grep stableObjectId <scene> | sort)
```

Identical for `ComputerRepairTask`, `FanRepairTask`, `VRTraining` and
`ResearcherSetup`.

**Interactable inventory** — every id still present with its collider; four
objects inactive and no others:

- Computer: 13 interactables, `computer.non-target-module` inactive.
- Fan: 15 interactables, `fan.faulty-fuse`, `fan.motor-module` and
  `fan.non-target-module` inactive.

**Collider ownership** — 13/13 and 15/15 interactables carry an explicit collider
list matching their own colliders; none auto-collect a child's.

**Idempotency** — the builders were run twice end to end. Stowed parts stay
stowed and are refreshed before being put back, because `FindAny` sees inactive
objects. No duplicate geometry, no missing-object warnings.

Zero console errors and zero builder warnings across the full rebuild.
