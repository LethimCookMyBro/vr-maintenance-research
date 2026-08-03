# ITEM_3D participant-comprehension validation record

**Validation date:** 2026-08-04 (Asia/Bangkok)  
**Scope:** licensed ITEM_3D visual replacement and participant-comprehension evidence only  
**Result:** PASS for local source validation, available desktop Play Mode checks, and internal visible-only QA

## Baseline and preserved original

Command:

```powershell
git rev-parse HEAD
```

Input: pre-import checkpoint on `visual-polish-claude`  
Literal output:

```text
22c71580d67760a1595f570f59747815474b0299
```

Exit status: `0`

Preserved archive:

```text
C:\Users\User\.codex\visualizations\2026\08\03\019fc7b2-e25f-7482-b38f-a4fb4366fd70\item-3d-preimport\model-integration-prechange-20260803-234250.zip
SHA256 733913605C080A7D6A08DE56A15D8DBE707ADA3F104B4E256ED6CFE4B1E69D11
```

Archive command: `Get-FileHash -Algorithm SHA256 <archive>`  
Exit status: `0`

## Source rebuild and validator passes

Exact modified-state command for both passes:

```text
ResearchVisualPipeline.RebuildAll(); ResearchVisualValidator.ValidateAll();
```

Pass A input: current project source at `2026-08-03T23:58:49.8964536+07:00`  
Pass A literal output:

```text
=== ComputerRepairTask ===
  PASS
=== FanRepairTask ===
  PASS
=== VRTraining ===
  PASS
  WARN task part 'training.cube-a' declared by TrainingDevelopment has no matching StableObjectId in the scene (pre-existing data mismatch, not changed by the visual pass)
  WARN task part 'training.cube-b' declared by TrainingDevelopment has no matching StableObjectId in the scene (pre-existing data mismatch, not changed by the visual pass)
  WARN task part 'training.cylinder' declared by TrainingDevelopment has no matching StableObjectId in the scene (pre-existing data mismatch, not changed by the visual pass)
=== ResearcherSetup ===
  PASS
ALL SCENES PASS
```

Pass A exit status: `0`

Pass B input: same source, independent rebuild at `2026-08-04T00:01:19.9445955+07:00`  
Pass B literal output: identical to Pass A, including the three documented pre-existing Training data warnings and `ALL SCENES PASS`.  
Pass B exit status: `0`

Primary records:

- `ITEM_3D_PassA.txt`
- `ITEM_3D_PassB.txt`

Post-prune command: `ResearchVisualValidator.ValidateAll();` after removing unreferenced raw duplicates.  
Literal terminal line: `ALL SCENES PASS`  
Exit status: `0`

## Play Mode/runtime checks

Computer, exit status `0`:

```text
RUNTIME ComputerITEM3D|playing=True|initial=Active|manualOpen=True|manualClose=True|preRepair=Active|repairId=computer.main-power-connector|postRepair=Completed|reset=Active|attempt=1->2|parts=13|rootColliders=13|visuals=11/11|visualMeshes=102|importedColliders=0
```

Fan, corrected final probe exit status `0`:

```text
RUNTIME FanITEM3D_CORRECTED|playing=True|state=Active|attempt=2|parts=15|rootColliders=15|screwdriverImported=True|downloadedCaseFanAbsent=True|serviceBay=True|fuseHolder=True
```

The first Fan query used the stale name `Fan Service Bay`; the existing object is `Service Bay`. The corrected query required no source or scene change.

Training, corrected final probe exit status `0`:

```text
RUNTIME TrainingITEM3D_CORRECTED|playing=True|state=Active|attempt=1|resetKnownPoses=True|readerOpened=True|readerClosedByReset=True|continueHiddenAfterReset=True|targets=3|rootColliders=3|pass=True
```

The first Training aggregate probe compared reset poses with physics-settled live transforms. The corrected probe compared against the source-authored poses and passed; no source change was required.

Primary records:

- `ITEM_3D_Runtime_Computer.txt`
- `ITEM_3D_Runtime_Fan.txt`
- `ITEM_3D_Runtime_Training.txt`

## Console and assertion gate

Final commands:

```text
Unity.ReadConsole(Action=Get, Types=[Error])
Unity.ReadConsole(Action=Get, Types=[All], FilterText=m_DisallowAutoRefresh)
```

Literal outputs:

```text
Retrieved 0 log entries.
Retrieved 0 log entries.
```

Both command exit statuses: `0`

## Fresh screenshot evidence

All listed files are 1600x900 and were captured at 2026-08-04 00:03 Asia/Bangkok.

| Evidence | SHA-256 |
|---|---|
| `Approach_Computer_ComponentClose.png` | `881C33DB494FD53F6B58270505A243C399C6DE53782375BFC47C8D85986FCD9B` |
| `Approach_Computer_GpuRamSsdPsu.png` | `60988BB4066661215C80C35DE27E2E2BC4DEA6A3E4F969C42E7D07995647B390` |
| `Approach_Fan_Front.png` | `9B0F85F7C5B86002C518290979E86C65A609435CD0E61853E6E5F398B41FDA48` |
| `Approach_Training_WorkstationDetail.png` | `8CD0CAFC3B2F3880200D041B078C24FECD591F12CE5641AEA6490647B5A573AB` |
| `Approach_Computer_TaskBrief.png` | `DD793F407ADC68E4293C21DAB01C463C803C978C35B02B9C86DD1A6670B0F4FA` |
| `ITEM3D_Final_Computer_ReaderOpen.png` | `354644E00975D352211216327C171EAF8DDD808C5A9761AE31F905C98CE054B0` |
| `Approach_Computer_DockCompact.png` | `3E398771669347C38E04C082BFDB09AD7E51C2F681F9A665983FF64C864A3D52` |
| `Approach_Computer_SparesTray.png` | `5D340C8668A758B8584D00207B6E9B4D5B041CEE69255494C54EF0953CBBE01F` |
| `Approach_Computer_InspectControl.png` | `72E9AC6877EF114A18F7E2724469EA108E3462D1C866CE34C1FD9DF2964D9DC2` |

## Independent visible-only comprehension QA

This is internal AI visual QA, not human-pilot data.

1. Computer reads as an open desktop PC: **PASS**.
2. GPU, paired RAM, CPU cooler, PSU, and two M.2 SSDs are recognizable; CPU is appropriately occluded under the cooler: **PASS**.
3. Fan remains a serviceable desk fan: **PASS**.
4. Training communicates Pick Up, Compare, and Turn: **PASS**.
5. Task brief communicates no-power diagnosis/repair without revealing the fault: **PASS**.
6. Reader guide is legible without workstation overlap: **PASS**.
7. Source choices, separated spares, and INSPECT control are visible across the targeted views: **PASS**.

## Research boundary

`P1`, `P2`, and `P3` remain blank in `PILOT_COMPREHENSION_CHECK.md`:

```text
| P1 | | | |
| P2 | | | |
| P3 | | | |
```

No Meta Quest device result and no human-pilot result is claimed.
