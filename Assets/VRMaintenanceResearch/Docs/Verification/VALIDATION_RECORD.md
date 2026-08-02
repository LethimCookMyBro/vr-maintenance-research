# VR Maintenance Research Validation Record

Date: 2026-08-02
Branch: visual-polish-claude
Unity: 6000.3.20f1
XRI: 3.4.0

## Scope and phase commits

Baseline commit: `0371ecd` (`docs: record workbench physics settle check`)
Implementation tip before this record: `6b91871`

1. `8aaec9a` - `fix: stabilize research runtime and task data`
2. `65fe8a5` - `feat: ground desktop XR and polish participant UI`
3. `8405856` - `feat: add licensed maintenance device models`
4. `6b91871` - `docs: deliver localized maintenance content`
5. This record, patch, and rollback script are the final validation phase.

No merge or push was run. The Quest 3 hardware path remains pending and unverified.

## Protected baseline

Checkpoint: `D:\TMU_VR\XR-Interaction-Toolkit-Examples-checkpoint-20260802-220535`
Original Windows player: `D:\TMU_VR\XR-Interaction-Toolkit-Examples-checkpoint-20260802-220535\BuildBaseline-20260802-230000\VRMaintenanceResearch\VRMaintenanceResearch.exe`
Original player SHA256: `F5AF8F2582C77647AA735CDD9F4D9CAE9FF79AAB0910F9524CF71018F28D8B50`

The working tree was copied and hashed before edits. Unrelated pre-existing settings, package, Adaptive Performance, v1 information assets, supervisor package, and old screenshot files were kept outside the phase commits.

## Modified artifact and patch

Modified artifact tree:
`D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch`

Patch artifact:
`D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Verification\verified-change.patch`

Patch generation command and input:
```text
cmd.exe /c "git -C D:\TMU_VR\XR-Interaction-Toolkit-Examples diff --binary 0371ecd 6b91871 -- Assets/VRMaintenanceResearch > Assets/VRMaintenanceResearch/Docs/Verification/verified-change.patch"
```

Patch summary command:
```text
git -C D:\TMU_VR\XR-Interaction-Toolkit-Examples apply --stat -- D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Verification\verified-change.patch
```

Literal result:
```text
74 files changed, 2958 insertions(+), 164 deletions(-)
PATCH_STAT_EXIT=0
```

Clean-baseline patch application check:
```text
git -C D:\TMU_VR\XR-Interaction-Toolkit-Examples worktree add --detach D:\TMU_VR\XR-Interaction-Toolkit-Examples-patch-check-20260802 0371ecd
git -C D:\TMU_VR\XR-Interaction-Toolkit-Examples-patch-check-20260802 apply --check -- D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Verification\verified-change.patch
git -C D:\TMU_VR\XR-Interaction-Toolkit-Examples worktree remove --force D:\TMU_VR\XR-Interaction-Toolkit-Examples-patch-check-20260802
```

Literal result:
```text
HEAD is now at 0371ecd docs: record workbench physics settle check
PATCH_STAT_EXIT=0
WORKTREE_ADD_EXIT=0
BASE_APPLY_CHECK_EXIT=0
WORKTREE_REMOVED=True
```

A direct reverse check against the already modified tree reported failures for the two newly added PDF binary files. The corrected clean-baseline check above passed with exit status 0.

## Runtime and data verification

Key script validation command form:
```text
Unity_ValidateScript(Level=standard, IncludeDiagnostics=true, Uri=<Assets-relative script path>)
```

Literal result for `InformationSourceController.cs`, `XRResearchRuntimeDiagnostic.cs`, `GroundedDesktopXRWalker.cs`, `ComfortFollowPanel.cs`, and `ResearchLogService.cs`:
```text
No diagnostics.
```

The remaining touched scripts returned no compile errors; the validator only reported generic existing warning classes for string concatenation and nullable `GetComponent` use.

Scene wiring probe command form:
```text
Unity_RunCommand(Code=<scene wiring probe>, Title="Verify participant scene wiring")
```

Literal result:
```text
VRTraining; root=True; controllerRoot=True; movementRoot=True; toggle=102; walker=True; diagnostic=True; eventSystems=1; xrModules=1; legacyModules=0; content=development-1; interactables=3; ids=training.training-cube-a,training.training-cube-b,training.training-cylinder
ComputerRepairTask; root=True; controllerRoot=True; movementRoot=True; toggle=102; walker=True; diagnostic=True; eventSystems=1; xrModules=1; legacyModules=0; content=research-v2; interactables=13; ids=computer.case,computer.cooling-fan,computer.external-power-cable,computer.internal-cable,computer.main-power-connector,computer.motherboard,computer.non-target-module,computer.power-button,computer.psu,computer.psu-switch,computer.ram,computer.side-panel,computer.tool.screwdriver
FanRepairTask; root=True; controllerRoot=True; movementRoot=True; toggle=102; walker=True; diagnostic=True; eventSystems=1; xrModules=1; legacyModules=0; content=research-v2; interactables=15; ids=fan.blade,fan.body,fan.fastener,fan.faulty-fuse,fan.front-cover,fan.fuse-holder,fan.internal-wire,fan.motor-module,fan.non-target-module,fan.power-cord,fan.power-plug,fan.power-switch,fan.speed-selector,fan.tool.screwdriver,fan.working-fuse
```

Play-mode diagnostic output:
```text
Grounded desktop locomotion enabled: simulator vertical input disabled; gravity path retained.
XR UI diagnostic: eventSystems=1; xrModules=1; legacyModules=0; trackedRaycasters=1; graphicRaycasters=1.
```

Post-stop error query:
```text
Unity_ReadConsole(Action=Get, Types=Error)
```

Literal result: `0 error entries`.

## Content and media verification

Literal content check:
```text
INFO_V2 all 8 localized_nonempty=true, placeholder=0, has_question=false, has_mojibake=false
PNG_OK Computer_Visual_Guide.png 1600x900
PNG_OK Fan_Visual_Guide.png 1600x900
PDF_OK Computer_Maintenance_Manual.pdf pages=1
PDF_OK Fan_Maintenance_Manual.pdf pages=1
ffprobe both: codec_name=h264,width=1280,height=720,duration=60.000000
```

Scene reference probe:
```text
ComputerRepairTask; source=computer.source.manual; definition=...ComputerProductManual_v2.asset; localized=True; video=<none>
ComputerRepairTask; source=computer.source.text; definition=...ComputerTextGuide_v2.asset; localized=True; video=<none>
ComputerRepairTask; source=computer.source.video; definition=...ComputerVideo_v2.asset; localized=True; video=Assets/VRMaintenanceResearch/Video/Final/ComputerInstructional_60s.mp4
ComputerRepairTask; source=computer.source.visual; definition=...ComputerVisualGuide_v2.asset; localized=True; video=<none>
FanRepairTask; source=fan.source.manual; definition=...FanProductManual_v2.asset; localized=True; video=<none>
FanRepairTask; source=fan.source.text; definition=...FanTextGuide_v2.asset; localized=True; video=<none>
FanRepairTask; source=fan.source.video; definition=...FanVideo_v2.asset; localized=True; video=Assets/VRMaintenanceResearch/Video/Final/FanInstructional_60s.mp4
FanRepairTask; source=fan.source.visual; definition=...FanVisualGuide_v2.asset; localized=True; video=<none>
```

Inactive visual guide probe:
```text
ComputerRepairTask; visualGuideArt=1; sprite=Assets/VRMaintenanceResearch/Information/VisualGuides/Computer_Visual_Guide.png; activeInHierarchy=False
FanRepairTask; visualGuideArt=1; sprite=Assets/VRMaintenanceResearch/Information/VisualGuides/Fan_Visual_Guide.png; activeInHierarchy=False
```

## Windows build and smoke launch

The original output was preserved before build work. An in-place IL2CPP build stopped at host toolchain discovery because the installed Visual Studio instance had no compatible C++ compiler and Windows SDK. A fresh Mono2x output was used for the desktop smoke only, then the project backend was restored to IL2CPP.

Unity build output:
```text
Build result=Succeeded; errors=0; warnings=486; size=288910879; Scripting backend restored=IL2CPP
```

Player verification:
```text
BUILD_PATH=D:\TMU_VR\XR-Interaction-Toolkit-Examples\Builds\Windows\VRMaintenanceResearch-Mono\VRMaintenanceResearch.exe
BUILD_SIZE=667136
BUILD_SHA256=0D5D37BDE5619A3C9373D63F7E799C500DAC263939DFB00387656176798A89A6
LAUNCH_PID=56664
LAUNCH_RUNNING_AFTER_8S=True
LAUNCH_EXITED_AFTER_CHECK=True
```

This is a Windows process-start smoke only. It is not Quest 3 evidence.

## Rollback verification

Rollback script:
`D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Verification\rollback-to-checkpoint.ps1`

Dry-run command:
```text
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Verification\rollback-to-checkpoint.ps1" -WhatIf
```

Literal result:
```text
REPO=D:\TMU_VR\XR-Interaction-Toolkit-Examples
CHECKPOINT_SOURCE=D:\TMU_VR\XR-Interaction-Toolkit-Examples-checkpoint-20260802-220535\Assets\VRMaintenanceResearch
CURRENT_TARGET=D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch
QUARANTINE_TARGET=D:\TMU_VR\VRMaintenanceResearch-rollback-20260802-232738
WHATIF_MOVE=D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch -> D:\TMU_VR\VRMaintenanceResearch-rollback-20260802-232738
WHATIF_COPY=D:\TMU_VR\XR-Interaction-Toolkit-Examples-checkpoint-20260802-220535\Assets\VRMaintenanceResearch -> D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch
ROLLBACK_WHATIF_EXIT=0
```

The default rollback moves the current `Assets/VRMaintenanceResearch` tree to a timestamped sibling quarantine and copies the checkpoint tree back. It does not delete the quarantined tree. Close Unity before a real rollback.

## Review evidence

Runtime review images:
- `D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Screenshots\Review\Training_Runtime_Compact.png`
- `D:\TMU_VR\XR-Interaction-Toolkit-Examples\Assets\VRMaintenanceResearch\Docs\Screenshots\Review\Computer_Runtime_StatusBoard.png`

The images were reopened and visually inspected. The training image shows the compact neutral board and controller visuals. The Computer image shows the compact task status board and source cards.

Known remaining boundary: Quest 3 hardware validation, advisor approval, and real-device audio validation remain follow-up work. The 486 build warnings are shader/import warnings; the player build completed with zero build errors.
## Verification artifact import

Unity refresh command:
```text
Unity_RunCommand(Code=AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport), Title="Refresh validation artifacts")
```

Literal result:
```text
isCompilationSuccessful=true
isExecutionSuccessful=true
[Log] Verification folder refreshed: [1a2f429f71d099e47b4a3a86a592c673]
[Log] Rollback script refreshed: [48acac8dd5de9284f97e6d3e7bd33258]
[Log] Patch refreshed: [ede6e17c0f3bb3c45973a79006165b90]
```
