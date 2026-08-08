# Translation Review Sheet

**Prepared:** 2026-08-08 · commit `0248328` · branch `visual-polish-claude`
**For:** a reviewer who reads Thai and/or Japanese.

Every piece of text a participant can see, in one table. This sheet **collects**; it does
not translate. No wording in this file was written, corrected, improved or invented for
this document — every cell is copied verbatim from the build. Where a cell reads
**— none in build —**, no Thai or Japanese exists for that string and the participant
sees the English.

## How to use this sheet

1. Read down the **English** column and check the **ไทย** and **日本語** cells beside it.
2. Write corrections in the last column. Do not edit the build.
3. Anything that must not be translated (`INSPECT`, `PART A`, product names) — say so in
   the last column and the string will be marked as deliberately untranslated.

## Reading the cells

- `<br>` is a line break in the running text.
- `<b>…</b>` makes text bold on the panel. Keep the tags in any replacement wording, and
  keep them around the same idea.
- **Do not translate** the words `INSPECT` and `RESET` where they appear inside a
  sentence — they are the labels physically printed on controls in the room, so the
  sentence is pointing at an object the participant can see. If a translated label is
  wanted, the label on the object has to change too; say so in the last column.

## Summary before you start

| | Count |
|---|---|
| Distinct strings listed below | **94** |
| Have Thai **and** Japanese | 25 |
| **English only, no translation exists** | **69** |
| — of those, currently switched off and not visible (row 72) | 4 |
| **English only and visible to a participant** | **65** |

The table has 85 numbered rows. Three of them stand for more than one string: rows 14
and 15 each carry the same body text across four source assets, and row 72 covers four
slot labels. 85 + 3 + 3 + 3 = 94.

The untranslated strings are not stray labels. They include the whole training board,
every bench placard, the whole lab wall notice board, the status board a participant
looks at for the entire task, and every button inside the information readers. A Thai or
Japanese participant currently reads the work order and the source content in their own
language, and everything else in English.

Whether that is acceptable is a protocol decision, not a translation one. It is
raised in `SUPERVISOR_REVIEW_PACKAGE.md`; this sheet only establishes the facts.

---

## The table

| # | Source (file:line, or asset) | English | ไทย | 日本語 | Reviewer's correction |
|---|---|---|---|---|---|
| | **GROUP 1 — WORK ORDER.** The bench panel that states the task. The only text that says what the participant is there to do. English is read from the scene; Thai and Japanese are in code, so a change to one does not change the others. | | | | |
| 1 | `Scripts/UI/LocalizedTaskBrief.cs:38,54` · scene `Task Brief/Heading` (Computer) | `COMPUTER  ·  WORK ORDER` | `คอมพิวเตอร์  ·  ใบสั่งงาน` | `コンピューター  ·  作業指示` | |
| 2 | `Scripts/UI/LocalizedTaskBrief.cs:38,54` · scene `Task Brief/Heading` (Fan) | `DESK FAN  ·  WORK ORDER` | `พัดลมตั้งโต๊ะ  ·  ใบสั่งงาน` | `卓上扇風機  ·  作業指示` | |
| 3 | `Scripts/UI/LocalizedTaskBrief.cs:40,49,55` · scene `Task Brief/Body` (Computer) | `<b>REPORTED ISSUE</b><br>The computer does not power on.<br><br><b>GOAL</b><br>The unit is assembled and open for service. Find the cause and repair it.<br><br><b>INFORMATION SOURCES</b><br>Available on your left.<br><br>Press <b>INSPECT</b> when the unit is ready.` | `<b>อาการที่รายงาน</b><br>คอมพิวเตอร์เปิดไม่ติด<br><br><b>เป้าหมาย</b><br>เครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง ให้หาสาเหตุและซ่อมให้เรียบร้อย<br><br><b>แหล่งข้อมูล</b><br>อยู่ทางซ้ายของคุณ<br><br>เมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>` | `<b>報告された症状</b><br>コンピューターの電源が入りません。<br><br><b>目標</b><br>装置は組み立て済みで、点検のために開けてあります。原因を特定し、修理してください。<br><br><b>情報源</b><br>左側にあります。<br><br>装置の準備ができたら、<b>INSPECT</b> を押してください。` | |
| 4 | `Scripts/UI/LocalizedTaskBrief.cs:41,50,55` · scene `Task Brief/Body` (Fan) | `<b>REPORTED ISSUE</b><br>The desk fan does not operate correctly.<br><br><b>GOAL</b><br>The unit is assembled and open for service. Find the cause and repair it.<br><br><b>INFORMATION SOURCES</b><br>Available on your left.<br><br>Press <b>INSPECT</b> when the unit is ready.` | `<b>อาการที่รายงาน</b><br>พัดลมตั้งโต๊ะทำงานผิดปกติ<br><br><b>เป้าหมาย</b><br>เครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง ให้หาสาเหตุและซ่อมให้เรียบร้อย<br><br><b>แหล่งข้อมูล</b><br>อยู่ทางซ้ายของคุณ<br><br>เมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>` | `<b>報告された症状</b><br>卓上扇風機が正常に動作しません。<br><br><b>目標</b><br>装置は組み立て済みで、点検のために開けてあります。原因を特定し、修理してください。<br><br><b>情報源</b><br>左側にあります。<br><br>装置の準備ができたら、<b>INSPECT</b> を押してください。` | |
| | **GROUP 2 — INFORMATION SOURCE TITLES.** The heading at the top of each reader panel. Four sources per task; the source *type* is the study's independent variable, so wording differences between the four matter. | | | | |
| 5 | `ScriptableObjects/InformationSources/ComputerProductManual_v2.asset:18,20,21` | `Computer Product Manual` | `คู่มือผลิตภัณฑ์คอมพิวเตอร์` | `コンピューター製品マニュアル` | |
| 6 | `ScriptableObjects/InformationSources/ComputerTextGuide_v2.asset:18,20,21` | `Computer Troubleshooting Guide` | `คู่มือแก้ไขปัญหาคอมพิวเตอร์` | `コンピューター トラブルシューティングガイド` | |
| 7 | `ScriptableObjects/InformationSources/ComputerVideo_v2.asset:18,20,21` | `Computer Instructional Video` | `วิดีโอแนะนำการซ่อมคอมพิวเตอร์` | `コンピューター修理手順ビデオ` | |
| 8 | `ScriptableObjects/InformationSources/ComputerVisualGuide_v2.asset:18,20,21` | `Computer Visual Guide` | `คู่มือภาพขั้นตอนซ่อมคอมพิวเตอร์` | `コンピューター修理ビジュアルガイド` | |
| 9 | `ScriptableObjects/InformationSources/FanProductManual_v2.asset:18,20,21` | `Fan Product Manual` | `คู่มือผลิตภัณฑ์พัดลม` | `扇風機製品マニュアル` | |
| 10 | `ScriptableObjects/InformationSources/FanTextGuide_v2.asset:18,20,21` | `Fan Troubleshooting Guide` | `คู่มือแก้ไขปัญหาพัดลม` | `扇風機トラブルシューティングガイド` | |
| 11 | `ScriptableObjects/InformationSources/FanVideo_v2.asset:18,20,21` | `Fan Instructional Video` | `วิดีโอแนะนำการซ่อมพัดลม` | `扇風機修理手順ビデオ` | |
| 12 | `ScriptableObjects/InformationSources/FanVisualGuide_v2.asset:18,20,21` | `Fan Visual Guide` | `คู่มือภาพขั้นตอนซ่อมพัดลม` | `扇風機修理ビジュアルガイド` | |
| 13 | `ScriptableObjects/InformationSources/TrainingNeutralManual.asset:18,20,21` | `Neutral Training Manual` | **— none in build —** | **— none in build —** | |
| | **GROUP 3 — INFORMATION SOURCE CONTENT.** The body text inside each reader. All four computer sources carry **identical** body text, and all four fan sources carry identical body text — that is deliberate (content is held equal so that only the source *type* differs), so the same four rows repeat. Please check each language once. | | | | |
| 14 | `ComputerProductManual_v2.asset:19,22,23` — and identically in `ComputerTextGuide_v2`, `ComputerVideo_v2`, `ComputerVisualGuide_v2` (same lines) | `Before opening, disconnect power. Trace the external power cable to the PSU and main connector. Identify the motherboard power connector, reconnect it, then press the power button for the neutral device test.` | `ก่อนเปิดฝา ให้ถอดแหล่งจ่ายไฟ ตรวจเส้นทางสายไฟภายนอกไปยัง PSU และขั้วต่อหลัก ระบุขั้วต่อไฟของเมนบอร์ด เสียบกลับให้แน่น แล้วกดปุ่มเปิดเครื่องเพื่อทดสอบอุปกรณ์` | `開ける前に電源を切り、外部電源ケーブルから PSU と主電源コネクターまでを確認します。マザーボード電源コネクターを特定して接続し直し、電源ボタンを押して装置を確認します。` | |
| 15 | `FanProductManual_v2.asset:19,22,23` — and identically in `FanTextGuide_v2`, `FanVideo_v2`, `FanVisualGuide_v2` (same lines) | `Disconnect power before opening the guard. Trace the power cord to the fuse holder, identify the working replacement fuse, install it, then use the speed control for the neutral device test.` | `ก่อนเปิดตะแกรง ให้ถอดปลั๊ก ตรวจสายไฟไปยังช่องฟิวส์ ระบุฟิวส์สำรองที่ใช้งานได้ ติดตั้งฟิวส์ แล้วใช้ปุ่มปรับความเร็วเพื่อทดสอบอุปกรณ์` | `ガードを開ける前に電源プラグを抜きます。電源コードからヒューズホルダーまでを確認し、使用できる交換ヒューズを特定して取り付け、速度調整で装置を確認します。` | |
| 16 | `TrainingNeutralManual.asset:19` | `Development training content only; no maintenance solution is included.` | **— none in build —** | **— none in build —** | |
| | **GROUP 4 — SOURCE TILE CAPTIONS.** The word on the face of each of the four tiles, before it is opened. This is what the participant chooses between, so it is the most consequential short text in the study. | | | | |
| 17 | `Scripts/InformationSources/InformationSourceController.cs:270,280,288` | `Manual` | `คู่มือ` | `マニュアル` | |
| 18 | `Scripts/InformationSources/InformationSourceController.cs:271,281,289` | `Troubleshooting` | `แก้ไขปัญหา` | `トラブル対応` | |
| 19 | `Scripts/InformationSources/InformationSourceController.cs:272,282,290` | `Video` | `วิดีโอ` | `ビデオ` | |
| 20 | `Scripts/InformationSources/InformationSourceController.cs:273,283,291` | `Visual Guide` | `คู่มือภาพ` | `ビジュアルガイド` | |
| | **GROUP 5 — STATUS BOARD.** Mounted above the bench, in view for the whole task. Only the finished notice is translated; the rest is English in every condition. | | | | |
| 21 | `Scenes/ComputerRepairTask.unity:27507` (`taskTitle`) | `Computer Maintenance Task` | **— none in build —** | **— none in build —** | |
| 22 | `Scenes/FanRepairTask.unity:1477` (`taskTitle`) | `Fan Maintenance Task` | **— none in build —** | **— none in build —** | |
| 23 | `Scripts/UI/TaskStatusBoard.cs:126,147` | `Status: Not started` | **— none in build —** | **— none in build —** | |
| 24 | `Scripts/UI/TaskStatusBoard.cs:126,148` | `Status: In progress` | **— none in build —** | **— none in build —** | |
| 25 | `Scripts/UI/TaskStatusBoard.cs:126,149` | `Status: Paused` | **— none in build —** | **— none in build —** | |
| 26 | `Scripts/UI/TaskStatusBoard.cs:126,150` | `Status: Completed` | **— none in build —** | **— none in build —** | |
| 27 | `Scripts/UI/TaskStatusBoard.cs:126,151` | `Status: Time limit reached` | **— none in build —** | **— none in build —** | |
| 28 | `Scripts/UI/TaskStatusBoard.cs:126,152` | `Status: Stopped by researcher` | **— none in build —** | **— none in build —** | |
| 29 | `Scripts/UI/TaskStatusBoard.cs:126,153` | `Status: Safety stop` | **— none in build —** | **— none in build —** | |
| 30 | `Scripts/UI/TaskStatusBoard.cs:126,154` | `Status: Reset` | **— none in build —** | **— none in build —** | |
| 31 | `Scripts/UI/TaskStatusBoard.cs:117` (shown if the board loses its task) | `Status: unavailable` | **— none in build —** | **— none in build —** | |
| 32 | `Scripts/UI/TaskStatusBoard.cs:127` | `Attempt 1` (number increments) | **— none in build —** | **— none in build —** | |
| 33 | `Scripts/UI/TaskStatusBoard.cs:100,101,102` — **the one instruction addressed to the participant** | `This task is finished.<br>Please wait for the researcher.` | `งานนี้เสร็จแล้ว<br>กรุณารอผู้วิจัย` | `この作業は終了しました。<br>研究者をお待ちください。` | |
| | **GROUP 6 — TRAINING BOARD.** Built at run time in the training room. Entirely English. The training room is where a participant learns the controls, so this is the first text they read. | | | | |
| 34 | `Scripts/UI/TrainingInstructions.cs:129` | `Neutral XR Training` | **— none in build —** | **— none in build —** | |
| 35 | `Scripts/UI/TrainingInstructions.cs:133` | `<b>Look</b> headset   <b>Point</b> controller ray   <b>Grip</b> grab and release   <b>Trigger</b> select` | **— none in build —** | **— none in build —** | |
| 36 | `Scripts/UI/TrainingInstructions.cs:138` | `This scene contains no Computer or Fan maintenance solution.` | **— none in build —** | **— none in build —** | |
| 37 | `Scripts/UI/TrainingInstructions.cs:145` | `Pick up a training object` | **— none in build —** | **— none in build —** | |
| 38 | `Scripts/UI/TrainingInstructions.cs:146` | `Place an object in the comparison tray` | **— none in build —** | **— none in build —** | |
| 39 | `Scripts/UI/TrainingInstructions.cs:147` | `Turn the training dial` | **— none in build —** | **— none in build —** | |
| 40 | `Scripts/UI/TrainingInstructions.cs:148` | `Open the neutral information panel` | **— none in build —** | **— none in build —** | |
| 41 | `Scripts/UI/TrainingInstructions.cs:151,192` | `Complete all four skills. <b>RESET</b> returns the training objects.` | **— none in build —** | **— none in build —** | |
| 42 | `Scripts/UI/TrainingInstructions.cs:192` | `All four skills complete.` | **— none in build —** | **— none in build —** | |
| 43 | `Scripts/UI/TrainingInstructions.cs:155,191` | `Continue` | **— none in build —** | **— none in build —** | |
| | **GROUP 7 — TRAINING ROOM SIGNAGE.** Printed on the objects and the bench in the training room. | | | | |
| 44 | `Editor/TrainingSceneBuilder.cs:51` · scene `Training Cube A/Visual/Part Label` | `PART A` | **— none in build —** | **— none in build —** | |
| 45 | `Editor/TrainingSceneBuilder.cs:64` · scene `Training Cube B/Visual/Part Label` | `REFERENCE` | **— none in build —** | **— none in build —** | |
| 46 | `Editor/TrainingSceneBuilder.cs:79` · scene `Training Cylinder/Visual/Part Label` | `DIAL` | **— none in build —** | **— none in build —** | |
| 47 | `Editor/TrainingSceneBuilder.cs:112` · scene `Training Socket/Visual/Socket Label` | `PLACE TO COMPARE` | **— none in build —** | **— none in build —** | |
| 48 | `Editor/TrainingSceneBuilder.cs:125` · scene `Training Signage/Step 1  PICK UP/Text` | `1  PICK UP` | **— none in build —** | **— none in build —** | |
| 49 | `Editor/TrainingSceneBuilder.cs:126` · scene `Training Signage/Step 2  COMPARE/Text` | `2  COMPARE` | **— none in build —** | **— none in build —** | |
| 50 | `Editor/TrainingSceneBuilder.cs:127` · scene `Training Signage/Step 3  TURN/Text` | `3  TURN` | **— none in build —** | **— none in build —** | |
| 51 | `Editor/TrainingSceneBuilder.cs:130` · scene `Training Signage/Reset Caption` | `RESET` | **— none in build —** | **— none in build —** | |
| | **GROUP 8 — BENCH PLACARDS.** Small standing signs on the tray rims and the bench mat. Present in every scene. | | | | |
| 52 | `Editor/BenchDressing.cs:49` · scene `Workstation Dressing/Placard SPARE PARTS/Caption` (both task scenes) | `SPARE PARTS` | **— none in build —** | **— none in build —** | |
| 53 | `Editor/BenchDressing.cs:50` · scene `Workstation Dressing/Placard TOOLS/Caption` (both task scenes) | `TOOLS` | **— none in build —** | **— none in build —** | |
| 54 | `Editor/BenchDressing.cs:27,51` · scene `Workstation Dressing/Placard SERVICE AREA/Caption` (all three participant scenes) | `SERVICE AREA` | **— none in build —** | **— none in build —** | |
| 55 | `Editor/BenchDressing.cs:49` · scene `Workstation Dressing/Placard PARTS BIN/Caption` (training room) | `PARTS BIN` | **— none in build —** | **— none in build —** | |
| 56 | `Editor/BenchDressing.cs:50` · scene `Workstation Dressing/Placard PLACE PART HERE/Caption` (training room) | `PLACE PART HERE` | **— none in build —** | **— none in build —** | |
| 57 | scene `Workstation Dressing/Placard REMOVED PARTS/Caption` (Computer), `Fan Removed Parts Rack/Placard REMOVED PARTS/Caption` (Fan) | `REMOVED PARTS` | **— none in build —** | **— none in build —** | |
| 58 | scene `Operator Station/Placard HEADSET/Caption` (setup scene) | `HEADSET` | **— none in build —** | **— none in build —** | |
| 59 | scene `Operator Station/Placard RESEARCHER CONSOLE/Caption` (setup scene) | `RESEARCHER CONSOLE` | **— none in build —** | **— none in build —** | |
| | **GROUP 9 — DEVICE-TEST SIGN.** Stands over the control the participant presses or turns to finish the task. `INSPECT` is also the word used inside the work order, rows 3 and 4. | | | | |
| 60 | `Editor/BenchDressing.cs:156` · scene `Inspect Station Sign/Caption` (both task scenes) | `INSPECT` | **— none in build —** | **— none in build —** | |
| 61 | `Editor/BenchDressing.cs:160` · scene `Inspect Station Sign/Sub Caption` (both task scenes) | `PRESS TO CHECK THE UNIT` | **— none in build —** | **— none in build —** | |
| | **GROUP 10 — INFORMATION DOCK AND READER CONTROLS.** The header above the four tiles, and the buttons inside an opened reader. Every one is English in every condition. | | | | |
| 62 | `Editor/InformationDockBuilder.cs:154` · scene `Information Dock/Header` (all three participant scenes) | `INFORMATION SOURCES` | **— none in build —** | **— none in build —** | |
| 63 | scene `*.control.Prev/GEN Label` (all eight readers) | `Prev` | **— none in build —** | **— none in build —** | |
| 64 | scene `*.control.Next/GEN Label` (all eight readers) | `Next` | **— none in build —** | **— none in build —** | |
| 65 | `Editor/InformationDockBuilder.cs:263` · scene `*.control.Close/GEN Label` (all eight readers) | `Close` | **— none in build —** | **— none in build —** | |
| 66 | scene `*.control.Play/GEN Label` (both video readers) | `Play` | **— none in build —** | **— none in build —** | |
| 67 | scene `*.control.Pause/GEN Label` (both video readers) | `Pause` | **— none in build —** | **— none in build —** | |
| 68 | scene `*.control.Stop/GEN Label` (both video readers) | `Stop` | **— none in build —** | **— none in build —** | |
| 69 | scene `*.control.Restart/GEN Label` (both video readers) | `Restart` | **— none in build —** | **— none in build —** | |
| 70 | scene `*.control.Seek+10/GEN Label` (both video readers) | `+10 s` | **— none in build —** | **— none in build —** | |
| 71 | `Scripts/InformationSources/InformationSourceController.cs:323` · scene `GEN Video Status/GEN Label` | `00:00 / 01:00` (a running timer) | **— none in build —** | **— none in build —** | Numeric. Confirm the `mm:ss / mm:ss` form reads correctly in both languages. |
| 72 | scene `*/GEN Slot` (all nine sources) | `SOURCE A` / `SOURCE B` / `SOURCE C` / `SOURCE D` | **— none in build —** | **— none in build —** | **Currently switched off — not visible to a participant.** Listed so that switching it on is a deliberate act. |
| | **GROUP 11 — LAB WALL NOTICE BOARD.** On the back wall of every scene, readable from anywhere in the room. Deliberately generic lab procedure — it must never hint at either fault. Entirely English. | | | | |
| 73 | `Editor/LabNoticeBoardBuilder.cs:35` · scene `Lab Notice Board/Bay Title` (Computer) | `BAY 02  ·  COMPUTER SERVICING` | **— none in build —** | **— none in build —** | |
| 74 | `Editor/LabNoticeBoardBuilder.cs:36` · scene `Lab Notice Board/Bay Title` (Fan) | `BAY 03  ·  APPLIANCE SERVICING` | **— none in build —** | **— none in build —** | |
| 75 | `Editor/LabNoticeBoardBuilder.cs:37` · scene `Lab Notice Board/Bay Title` (Training) | `BAY 01  ·  ORIENTATION` | **— none in build —** | **— none in build —** | |
| 76 | `Editor/LabNoticeBoardBuilder.cs:38` · scene `Lab Notice Board/Bay Title` (Setup) | `MAINTENANCE RESEARCH LAB` | **— none in build —** | **— none in build —** | |
| 77 | `Editor/LabNoticeBoardBuilder.cs:61` · scene `Lab Notice Board/Notice SAFETY/Heading` (all four scenes) | `SAFETY` | **— none in build —** | **— none in build —** | |
| 78 | `Editor/LabNoticeBoardBuilder.cs:62` · scene `Lab Notice Board/Notice SAFETY/Body` (all four scenes) | `Isolate at the wall before opening any enclosure.<br>Wait for indicators to go dark.` | **— none in build —** | **— none in build —** | |
| 79 | `Editor/LabNoticeBoardBuilder.cs:63` · scene `Lab Notice Board/Notice BENCH LAYOUT/Heading` (all four scenes) | `BENCH LAYOUT` | **— none in build —** | **— none in build —** | |
| 80 | `Editor/LabNoticeBoardBuilder.cs:64` · scene `Lab Notice Board/Notice BENCH LAYOUT/Body` (all four scenes) | `Spares tray left  ·  tools right.<br>Removed parts go on the lower shelf.` | **— none in build —** | **— none in build —** | |
| 81 | `Editor/LabNoticeBoardBuilder.cs:65` · scene `Lab Notice Board/Notice ESD CONTROL/Heading` (all four scenes) | `ESD CONTROL` | **— none in build —** | **— none in build —** | |
| 82 | `Editor/LabNoticeBoardBuilder.cs:66` · scene `Lab Notice Board/Notice ESD CONTROL/Body` (all four scenes) | `Wrist strap to the bench stud.<br>Handle boards by the edges only.` | **— none in build —** | **— none in build —** | |
| 83 | `Editor/LabNoticeBoardBuilder.cs:67` · scene `Lab Notice Board/Notice REPORT A FAULT/Heading` (all four scenes) | `REPORT A FAULT` | **— none in build —** | **— none in build —** | |
| 84 | `Editor/LabNoticeBoardBuilder.cs:68` · scene `Lab Notice Board/Notice REPORT A FAULT/Body` (all four scenes) | `Log the unit number and the symptom.<br>Leave the work order on the bench.` | **— none in build —** | **— none in build —** | |
| | **GROUP 12 — PRINTED ON THE EQUIPMENT.** Text that is physically part of a machine in the room. | | | | |
| 85 | `Editor/FanWorkstationBuilder.cs:281` · scene `Electric Fan Body/Visual/Control Pod/Speed Legend` | `O F F    1    2    3` | **— none in build —** | **— none in build —** | The legend beside the fan's speed slider. Numerals only apart from `OFF`. |

> Rows 14, 15 and 72 each stand for four strings; every other numbered row is one
> string. That is how 85 rows become the 94 in the summary above.

---

## Questions for the reviewer

These are not translation questions. Answer them in the last column of the relevant row,
or here.

1. **Row 13 and row 16 — the training room's information source has no Thai or
   Japanese.** Every task source has all three languages; the training source has
   English only. The training room is where the participant learns to open a source.
   Should it be translated, or is English acceptable there?

2. **Rows 21–32 — the status board is English throughout except for the finished
   notice.** A participant looks at `Status: In progress` and `Attempt 2` for the whole
   task. Translate, or leave?

3. **Rows 34–43 — the training board is entirely English**, including the control
   instructions (`Look`, `Point`, `Grip`, `Trigger`) that teach the participant how to
   use the headset.

4. **`INSPECT` (rows 3, 4, 60) and `RESET` (rows 41, 51).** The translated work order
   keeps the English word `INSPECT` because it is the word printed on the sign over the
   control. Same for `RESET` on the training board. Confirm this is the intended
   handling, or say which should be translated on both the sentence and the sign.

5. **Rows 14 and 15 — content is identical across all four source types by design.**
   Please confirm the Thai and Japanese are also identical across the four, and that this
   is what equivalence should mean here. If any of the four ought to read differently in
   translation, that would break the equivalence the study depends on.

6. **Rows 5–12 — the source titles differ between the four types in all three
   languages.** Check that the *degree* of difference matches across languages: if the
   Thai titles are more distinguishable from one another than the English ones, that
   affects which source a participant picks, and choice of source is the study's main
   outcome.

7. **Row 71 — the video timer.** Confirm `mm:ss / mm:ss` is read correctly in Thai and
   Japanese, or say what form to use.

---

## What this sheet does not cover

- **The researcher's setup screen.** Operated by the researcher before the participant
  puts the headset on, and (since `a32041c`) never shown to a participant. English only,
  deliberately.
- **The researcher's in-session control panel.** Same: it appears on the researcher's
  monitor, not in the headset.
- **File names, object ids and recorded data values** (`computer.ram`,
  `IncorrectComponentInteraction`, CSV column names). These are never shown to a
  participant and must not be translated — translating them would break the data.
- **Whether the Thai and Japanese glyphs render legibly on the Meta Quest 3 display.**
  That is a hardware check and has never been done — `QUEST3_NEXT_STEPS.md`, check 5.
  A translation approved here could still be unreadable on the device.

## How this sheet was built

Every English string was read out of the built scenes themselves, by walking all four
scenes and collecting every text component with content, rather than by reading the
builder scripts — so what is listed is what is actually in the build, not what the code
intends. Run-time text (the status board, the training board) was read from the code
that produces it, and is marked as such. Thai and Japanese were copied from the asset
files and from the two code files that hold hardcoded translations
(`LocalizedTaskBrief.cs`, `TaskStatusBoard.cs`, `InformationSourceController.cs`).
