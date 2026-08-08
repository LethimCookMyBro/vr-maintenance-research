# Part recognition pass - 2026-08-08

## Why

The study measures how a participant diagnoses a fault. A participant who cannot say
what a part *is* by looking at it cannot form a hypothesis by looking at it: they are
pushed into poking things at random or reading a manual, which is one of the measured
variables. So legibility of the parts is not decoration here - it sits directly on top
of what the instrument records.

The two benches were also unequal in exactly that respect. The computer bench carried
licensed meshes for the board, the cooler, the memory, the drive, the supply and the
case fan; the fan bench carried one imported mesh (the screwdriver) and primitives for
everything else. Whatever that inequality did to comprehension, it did it to one
condition and not the other.

This pass rebuilt the parts a participant has to recognise in order to diagnose, and
left everything else - room, bench, trays, placards, walls, ceiling - as the graybox it
already was. Nothing was added to or removed from either bench: the object count is what
`de7d5fd` deliberately left.

## What changed, and what it is measured against

| Part | Before | After | Real part it is sized from |
|---|---|---|---|
| `computer.main-power-connector` (spare, tray) | 70 x 24 x 22 mm block, 24 gold pips on the **top** face, two black boxes for a tail | 54 x 13.2 x 15 mm moulded plug, 24 bores in 2 x 12 at 4.2 mm pitch, 20+4 split seam, retention latch, twelve rail-coloured wires gathering into a sleeved loom | Molex Mini-Fit Jr. 24-circuit, 4.2 mm pitch: 50.4 mm across the twelve circuits, 8.4 mm between the two rows, body ≈54 x 13 x 15 mm |
| `computer.internal-cable` (the fault, in the case) | 20 x 64 x 16 mm bar with 12 sockets and one black stub | the same connector geometry, hanging mouth-up off the loom and leaning 26° out of vertical so the bores face the open side panel | as above |
| ATX header on the board (dressing) | 18 x 62 x 14 mm shroud, pins at 4.9 mm pitch and 6.4 mm row spacing | matching 54 x 13.2 mm shrouded header, 24 gold pins in the same 2 x 12 array at 4.2 mm, latch window | as above - the plug and the socket are now one description |
| `fan.working-fuse`, `fan.faulty-fuse`, and the fuse fitted in the holder | 84 mm long, 26 mm across the caps - a torch battery | 30 mm end to end, 6.0 mm glass, 6.4 mm ferrules, 6 mm cap length, 0.7 mm element | 6 x 30 mm glass cartridge (IEC 6.3 x 32 / "6x30"): 30 mm overall, 6.3 mm cap diameter |
| the blown fuse's cue | two element stubs **plus** an opaque 20 x 19 x 19 mm dark blob inside the glass | two element stubs with a 3.4 mm gap, and nothing else | a blown 6 x 30: the element has parted, the glass is otherwise the same |
| `fan.fuse-holder` | 92 mm base under an 84 mm fuse, two clips, no fasteners | 44 mm carrier for a 30 mm cartridge: two sprung clips with lips the cartridge has to pass, a moulded pull tab, and two slotted mounting screws | open-clip PCB fuse carrier for 6 x 30 |
| the fan's service bay (dressing) | 100 x 128 mm opening | 72 x 92 mm opening, board, tracks and terminal strip scaled with it | appliance terminal box holding one 6 x 30 carrier |
| `fan.fastener` (bay cover screw) | 22 mm slotted head | 9 mm pan head with an 11 mm washer | M4 pan-head cover screw |
| `fan.power-plug` | box, three bare tabs | moulded body with a chamfered face, finger grip, three insulated pin shanks with metal pins, strain-relief boot, flex into the coil | 3-pin moulded mains plug, ≈50 x 30 x 32 mm body |
| `fan.power-cord` | three flat discs at three radii - a stack of plates | three rings of tangential segments at 86 / 66 / 46 mm radius, 7 mm flex, with a tail to the fan's cord gland | 7 mm two-core flex, loosely coiled |
| `fan.internal-wire` | two 60 mm runs splayed 18° across the compartment | two 30 mm runs at 7°, from the sleeve to the carrier's terminals | appliance internal leads |
| `computer.power-button` (device-test control) | three stacked cylinders, 120 mm disc | XRI `PushButton.fbx`: plate and mushroom head, fitted to 111 x 100 x 111 mm on a 132 mm collar | industrial panel push button |
| `fan.speed-selector` (device-test control) | the same three cylinders | XRI `Dial.fbx`: plate and knob, fitted to 125 x 90 x 125 mm on the same collar | rotary selector |
| `fan.power-switch` | three flat boxes | XRI `Slider.fbx`: handle standing in a travel slot, fitted to 50 x 18 x 50 mm on a 58 x 40 mm plate; the OFF-1-2-3 legend moved clear of it | slide switch on an appliance base |

Two supporting changes: the fuse pad in the spares tray went from white to the bench's
antistatic blue, because a glass-and-nickel cartridge on a white card is the same value
as the card; and the fuse's printed rating moved from a band round the middle of the
glass to a stamp on one ferrule, because across the middle it lay exactly over the
element on **both** fuses and hid the only thing that tells them apart.

## Mesh provenance

| Mesh | Source | Licence | Downloaded? |
|---|---|---|---|
| `PushButton.fbx`, `Dial.fbx`, `Slider.fbx` | `Assets/XRI_Examples/UI_3D/Models/` | Unity Companion License | no - already in the repository |
| everything else in the table | built from Unity primitives by the scene builders | project's own | no |

Nothing was downloaded in this pass. The two parts that have no mesh anywhere in the
repository - a 24-pin ATX connector and a 6 x 30 fuse - were modelled from primitives at
the real parts' dimensions, because the sources checked on 2026-08-08 offered nothing
usable: Poly Haven has no appliance or connector category, and the Sketchfab matches are
CC-BY at ~1.2 M triangles behind a login. The project's CC0 / Unity-Companion standard
was not lowered to get a mesh.

The XRI meshes are used as meshes only. `XRPushButton`, `XRKnob` and `XRSlider` carry
their own interaction state and would compete with `ResearchInteractable` for the same
events, so no script came across; `ImportedVisual` strips every MonoBehaviour, collider,
rigidbody, animator, light, camera and audio source from an imported instance. No FBX
import setting was touched - the XRI example scenes use the same files - and the fit is
done with scale and rotation on transforms inside our own scenes.

## Quest budget

Counted over every renderer in the scene, enabled or not, which is the rule the
2026-08-08 baseline used.

| Scene | Triangles before | Triangles after | Renderers before | after | Materials before | after |
|---|---|---|---|---|---|---|
| `FanRepairTask` | 36,120 | 37,898 (+1,778, +4.9%) | 442 | 505 | 43 | 43 |
| `ComputerRepairTask` | 118,967 | 120,385 (+1,418, +1.2%) | 531 | 591 | 94 | 97 |

Per part, triangles before → after:

- `computer.main-power-connector` 360 → 732
- `computer.internal-cable` 204 → 492
- ATX header (inside `Desktop Case`) - the case total moved 91,571 → 91,755
- `computer.power-button` 1,008 → 938
- `fan.working-fuse` 276 → 264, `fan.faulty-fuse` 300 → 276
- `fan.fuse-holder` 348 → 544
- `fan.internal-wire` 128 → 36
- `fan.power-plug` 84 → 132
- `fan.power-cord` 332 → 660
- `fan.power-switch` 48 → 336
- `fan.speed-selector` 1,008 → 826
- `fan.body` 7,116 → 7,508 (the resized service bay), `fan.front-cover` unchanged

The three new materials in `ComputerRepairTask` are the ATX loom's rail colours -
`Lab_CableRed`, `Lab_CableYellow` and the one material this pass added,
`Lab_CableOrange`. `FanRepairTask` gained none: the borrowed control meshes are
repainted into the lab palette on every submesh slot, so they add no material of their
own to either scene.

The whole `ComputerRepairTask` scene, room included, is 120 k triangles. No claim is
made about frame time on hardware; this is a triangle and material count taken in the
Editor, and Quest 3 validation is still outstanding.

## What was deliberately not changed

- **`fan.body` and `fan.front-cover` were not rebuilt.** Both already read as what they
  are - `After_Fan_FanFront_Inspect.png` shows a desk fan, `After_Fan_FrontGuard_Inspect.png`
  shows a wire fan guard on the removed-parts rack. The only edit inside the fan body was
  the service bay, and that was forced by the fuse coming down to real size: a 30 mm
  cartridge in a 128 mm compartment reads as something small dropped in a locker.
- **No collider was resized, moved or re-owned.** Several are now generous relative to
  the visual they wrap - `fan.working-fuse` keeps a 110 x 50 x 50 mm box around a 30 mm
  part. Colliders are the participant's ray target and the substrate of the recorded
  hover and grab counts, so they were left exactly as the verified baseline had them.
  See `KNOWN_LIMITATIONS.md`.
- **Nothing was added to or removed from either bench.** Distractor count is a research
  variable and `de7d5fd` set it deliberately.
- **Nothing marks the answer.** No arrow, glow, outline, signal colour, size step or
  placement advantage. The spare ATX lead in the tray and the unplugged one in the case
  are the same white body, the same size and the same rail colours; the good fuse and
  the blown one are the same glass, ferrules and printed rating, and differ only in
  whether the element is continuous. `ResearchVisualValidator.CheckFaultNotAdvertised`
  asserts both of those and passes.
- **The plug in the case leans 26° out of vertical**, where before it leaned 18° the
  other way. That is a legibility change and it is recorded here as one: hanging the
  other way it showed the participant a blank white back, and the twenty-four bores that
  identify it pointed at the ceiling. It is not a shorter path to the fault - the plug is
  still behind the board's front edge from the standing pose and is still found by
  stepping in and looking along the board.

## Evidence

`Docs/Screenshots/Recognition/` holds a Before and an After for every part above, from
two fixed poses each, rendered through the same camera path as the rest of the visual
audit:

- `*_StartPose.png` - from the participant's start pose `(0, 1.36, -1.6)`, the field of
  view narrowed to the part. Same standing position as `ParticipantEye`; what a
  participant resolves from where they arrive.
- `*_Inspect.png` - from where they stand once they have walked in.

`Docs/Screenshots/Audit/After_Fan_ElementGood_Macro.png` and
`After_Fan_ElementBlown_Macro.png` are the diagnosis itself at close range: an unbroken
element, and two stubs with a gap.

## Verification, all after the final rebuild

- Scene Integrity Tests - `ALL 7 PASS`
- Validate Scenes - `ALL SCENES PASS`, no `WARN`
- Foundation Edit Mode Tests - `PASS 6 tests`
- Runtime Checks, `ComputerRepairTask` and `FanRepairTask` - fail, repair, pass, reset
- Training Checks - four skills, Continue unlocks, reset relocks
- Full Flow Walkthrough A-B and B-A - both `WALKTHROUGH PASSED`, zero `FAIL` lines
- Console - zero errors
- All 31 `stableObjectId` values byte-identical to the pre-pass baseline (`md5`
  `df7517e46495d090065459bb2e5d8394` before and after)
- Builder idempotent: a second `REBUILD ALL` produces an identical transform path, local
  position, rotation, scale and material for all 2,114 transforms across the four scenes
