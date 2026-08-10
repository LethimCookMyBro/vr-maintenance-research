# Translation Review Sheet

**Prepared:** 2026-08-11 · branch `visual-polish-claude`
**For:** a reviewer who reads Thai and/or Japanese.
**Status of every row below: `DRAFT`.** Nothing here has been through the expert review
that proposal 9.13 requires before the instruments are used for real collection.

Every piece of text a participant can see, in one table, in three languages.

**What changed since the 2026-08-08 edition of this sheet.** That edition *collected*
only: 69 of the 94 strings had no Thai and no Japanese at all, and the sheet's job was to
establish that fact. Those 69 have now been drafted and put into the build, so this
edition is a review sheet rather than an inventory. Two other things moved with them:

- **The two work orders now say different things** (rows 3 and 4), because proposal 9.3.1
  makes Task A a component replacement and 9.3.2 makes Task B a diagnosis. They had
  briefly been given the same goal sentence, which erased the distinction. Please read
  those two rows against each other as well as across the languages.
- **The participant heads-up display was added after the last edition** and is new here as
  GROUP 13.

## How to use this sheet

1. Read down the **English** column and check the **ไทย** and **日本語** cells beside it.
2. Write corrections in the last column. Do not edit the build.
3. Every row is `DRAFT`. A row you are content with, say so — silence is not approval.
4. Anything that should **not** be translated, say so in the last column. The rows already
   held that way are listed under *Deliberately untranslated* below.

## Reading the cells

- `<br>` is a line break in the running text.
- `<b>…</b>` makes text bold on the panel. Keep the tags in any replacement wording, and
  keep them around the same idea.
- `{0}` is a number filled in at run time. It must appear once in every language.

## Rules the draft was written to, from proposal 9.5

Proposal 9.5 requires the Thai and Japanese materials to be equivalent in **meaning,
format, numeric data and work instruction**. Three things follow, and a reviewer should
check the draft against them:

1. **Numerals, units and letter identifiers are identical in all three languages** — `4`,
   `10`, `01`, `02`, `03`, `A`, `+10 s`. An automated check enforces this: every digit in
   an English string must appear in both translations
   (`EveryTranslationIsPresentAndKeepsTheEnglishNumerals`). It cannot check that the
   *sense* survived, which is your job.
2. **No string adds or removes information relative to the others.** A translation that is
   more or less specific than the English is a difference in the material, not in the
   language, and it lands on the variable the study measures.
3. **Nothing names the answer.** No string may name the faulty part, the symptom's cause,
   or the repair procedure. The lab notice board is readable from anywhere in either room
   and the training board is read before either task, so a leak in those would answer both
   tasks from the doorway. A separate automated check holds the work order to this.

## Deliberately untranslated, and why

| String | Where | Why |
|---|---|---|
| `INSPECT` | work order rows 3–4, sign rows 60 | The word **printed on the control** the sentence points at. Translating the sentence but not the object would break the reference. To translate it, the sign has to be repainted too — say so and it will be. |
| `RESET` | training board row 41, placard row 51 | Same: printed on a physical control in the training room. |
| `Grip`, `Trigger` | row 35 | The names printed on the controller by its manufacturer. `Look` and `Point` are actions and **are** translated. |
| `+10 s`, `00:00 / 01:00` | rows 70, 71 | Numeral and unit symbols, held identical by rule 1. |
| `O F F    1    2    3` | row 85 | Printed on the fan's own speed slider — a hardware marking, not copy. |
| Object ids, event names, CSV columns | not shown to participants | Translating them would break the data. |

## Summary

| | Count |
|---|---|
| Distinct strings listed below | **97** |
| Have Thai **and** Japanese | **91** |
| Deliberately untranslated (the table above) | 6 |
| **English only by omission** | **0** |

The table has 88 numbered rows. Four of them stand for more than one string: rows 14 and
15 each carry the same body text across four source assets, row 72 covers four slot
labels, and row 88 covers three objective lines. 88 + 3 + 3 + 3 = 97.

---

## The table

| # | Source (file, or asset) | English | ไทย | 日本語 | Status | Reviewer's correction |
|---|---|---|---|---|---|---|
| | **GROUP 1 — WORK ORDER.** The bench panel that states the task. The only text that says what the participant is there to do. **Rows 3 and 4 deliberately differ in the GOAL sentence** — that difference is the study's independent task variable (proposal 9.3). All three languages now live in `LocalizedTaskBrief.Body`, so a change to one is made next to the other two. | | | | | |
| 1 | `LocalizedTaskBrief.Heading` (Computer) | `COMPUTER  ·  WORK ORDER` | `คอมพิวเตอร์  ·  ใบสั่งงาน` | `コンピューター  ·  作業指示` | DRAFT | |
| 2 | `LocalizedTaskBrief.Heading` (Fan) | `DESK FAN  ·  WORK ORDER` | `พัดลมตั้งโต๊ะ  ·  ใบสั่งงาน` | `卓上扇風機  ·  作業指示` | DRAFT | |
| 3 | `LocalizedTaskBrief.Body` (Computer) — **Task A, replacement** | `<b>REPORTED ISSUE</b><br>The computer does not power on.<br><br><b>GOAL</b><br>The unit is assembled and open for service. Follow the manual and fit the correct replacement component.<br><br><b>INFORMATION SOURCES</b><br>Available on your left.<br><br>Press <b>INSPECT</b> when the unit is ready.` | `<b>อาการที่รายงาน</b><br>คอมพิวเตอร์เปิดไม่ติด<br><br><b>เป้าหมาย</b><br>เครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง ให้ทำตามคู่มือและเปลี่ยนส่วนประกอบให้ถูกชิ้น<br><br><b>แหล่งข้อมูล</b><br>อยู่ทางซ้ายของคุณ<br><br>เมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>` | `<b>報告された症状</b><br>コンピューターの電源が入りません。<br><br><b>目標</b><br>装置は組み立て済みで、点検のために開けてあります。マニュアルに従って、正しい交換部品を取り付けてください。<br><br><b>情報源</b><br>左側にあります。<br><br>装置の準備ができたら、<b>INSPECT</b> を押してください。` | DRAFT | **Check especially:** does the goal read as *replace the right part, guided by the manual* — and **not** as *work out what is wrong*? |
| 4 | `LocalizedTaskBrief.Body` (Fan) — **Task B, diagnosis** | `<b>REPORTED ISSUE</b><br>The desk fan does not operate correctly.<br><br><b>GOAL</b><br>The unit is assembled and open for service. Several parts could be responsible. Find the cause and repair it.<br><br><b>INFORMATION SOURCES</b><br>Available on your left.<br><br>Press <b>INSPECT</b> when the unit is ready.` | `<b>อาการที่รายงาน</b><br>พัดลมตั้งโต๊ะทำงานผิดปกติ<br><br><b>เป้าหมาย</b><br>เครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง สาเหตุเป็นไปได้หลายอย่าง ให้หาสาเหตุและซ่อมให้เรียบร้อย<br><br><b>แหล่งข้อมูล</b><br>อยู่ทางซ้ายของคุณ<br><br>เมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>` | `<b>報告された症状</b><br>卓上扇風機が正常に動作しません。<br><br><b>目標</b><br>装置は組み立て済みで、点検のために開けてあります。原因はいくつか考えられます。原因を特定し、修理してください。<br><br><b>情報源</b><br>左側にあります。<br><br>装置の準備ができたら、<b>INSPECT</b> を押してください。` | DRAFT | **Check especially:** does *several parts could be responsible* carry the same weight in all three, and does it name none of them? |
| | **GROUP 2 — INFORMATION SOURCE TITLES.** The heading at the top of each reader panel. Four sources per task; the source *type* is the study's independent variable, so wording differences between the four matter. | | | | | |
| 5 | `ComputerProductManual_v2.asset` | `Computer Product Manual` | `คู่มือผลิตภัณฑ์คอมพิวเตอร์` | `コンピューター製品マニュアル` | DRAFT | |
| 6 | `ComputerTextGuide_v2.asset` | `Computer Troubleshooting Guide` | `คู่มือแก้ไขปัญหาคอมพิวเตอร์` | `コンピューター トラブルシューティングガイド` | DRAFT | |
| 7 | `ComputerVideo_v2.asset` | `Computer Instructional Video` | `วิดีโอแนะนำการซ่อมคอมพิวเตอร์` | `コンピューター修理手順ビデオ` | DRAFT | |
| 8 | `ComputerVisualGuide_v2.asset` | `Computer Visual Guide` | `คู่มือภาพขั้นตอนซ่อมคอมพิวเตอร์` | `コンピューター修理ビジュアルガイド` | DRAFT | |
| 9 | `FanProductManual_v2.asset` | `Fan Product Manual` | `คู่มือผลิตภัณฑ์พัดลม` | `扇風機製品マニュアル` | DRAFT | |
| 10 | `FanTextGuide_v2.asset` | `Fan Troubleshooting Guide` | `คู่มือแก้ไขปัญหาพัดลม` | `扇風機トラブルシューティングガイド` | DRAFT | |
| 11 | `FanVideo_v2.asset` | `Fan Instructional Video` | `วิดีโอแนะนำการซ่อมพัดลม` | `扇風機修理手順ビデオ` | DRAFT | |
| 12 | `FanVisualGuide_v2.asset` | `Fan Visual Guide` | `คู่มือภาพขั้นตอนซ่อมพัดลม` | `扇風機修理ビジュアルガイド` | DRAFT | |
| 13 | `TrainingNeutralManual.asset` | `Neutral Training Manual` | `คู่มือฝึกแบบเป็นกลาง` | `中立トレーニングマニュアル` | DRAFT | **New.** Was English-only. |
| | **GROUP 3 — INFORMATION SOURCE CONTENT.** The body text inside each reader. All four computer sources carry **identical** body text, and all four fan sources carry identical body text — that is deliberate (content is held equal so that only the source *type* differs), so the same rows repeat across four assets each. | | | | | |
| 14 | `ComputerProductManual_v2.asset` — and identically in `ComputerTextGuide_v2`, `ComputerVideo_v2`, `ComputerVisualGuide_v2` | `Before opening, disconnect power. Trace the external power cable to the PSU and main connector. Identify the motherboard power connector, reconnect it, then press the power button for the neutral device test.` | `ก่อนเปิดฝา ให้ถอดแหล่งจ่ายไฟ ตรวจเส้นทางสายไฟภายนอกไปยัง PSU และขั้วต่อหลัก ระบุขั้วต่อไฟของเมนบอร์ด เสียบกลับให้แน่น แล้วกดปุ่มเปิดเครื่องเพื่อทดสอบอุปกรณ์` | `開ける前に電源を切り、外部電源ケーブルから PSU と主電源コネクターまでを確認します。マザーボード電源コネクターを特定して接続し直し、電源ボタンを押して装置を確認します。` | DRAFT | |
| 15 | `FanProductManual_v2.asset` — and identically in `FanTextGuide_v2`, `FanVideo_v2`, `FanVisualGuide_v2` | `Disconnect power before opening the guard. Trace the power cord to the fuse holder, identify the working replacement fuse, install it, then use the speed control for the neutral device test.` | `ก่อนเปิดตะแกรง ให้ถอดปลั๊ก ตรวจสายไฟไปยังช่องฟิวส์ ระบุฟิวส์สำรองที่ใช้งานได้ ติดตั้งฟิวส์ แล้วใช้ปุ่มปรับความเร็วเพื่อทดสอบอุปกรณ์` | `ガードを開ける前に電源プラグを抜きます。電源コードからヒューズホルダーまでを確認し、使用できる交換ヒューズを特定して取り付け、速度調整で装置を確認します。` | DRAFT | |
| 16 | `TrainingNeutralManual.asset` | `Development training content only; no maintenance solution is included.` | `เนื้อหาสำหรับการฝึกในขั้นพัฒนาเท่านั้น ไม่มีวิธีแก้ของงานซ่อมบำรุง` | `開発中のトレーニング用内容のみです。保守作業の答えは含まれていません。` | DRAFT | **New.** Was English-only. |
| | **GROUP 4 — SOURCE TILE CAPTIONS.** The word on the face of each of the four tiles, before it is opened. This is what the participant chooses between, so it is the most consequential short text in the study. | | | | | |
| 17 | `InformationSourceController.CompactSourceLabel` | `Manual` | `คู่มือ` | `マニュアル` | DRAFT | |
| 18 | `InformationSourceController.CompactSourceLabel` | `Troubleshooting` | `แก้ไขปัญหา` | `トラブル対応` | DRAFT | |
| 19 | `InformationSourceController.CompactSourceLabel` | `Video` | `วิดีโอ` | `ビデオ` | DRAFT | |
| 20 | `InformationSourceController.CompactSourceLabel` | `Visual Guide` | `คู่มือภาพ` | `ビジュアルガイド` | DRAFT | |
| | **GROUP 5 — STATUS BOARD.** Mounted above the bench, in view for the whole task. All of it is now translated; before this pass only the finished notice was. | | | | | |
| 21 | scene `ComputerRepairTask` (`taskTitle`) | `Computer Maintenance Task` | `งานซ่อมบำรุงคอมพิวเตอร์` | `コンピューター保守作業` | DRAFT | **New.** |
| 22 | scene `FanRepairTask` (`taskTitle`) | `Fan Maintenance Task` | `งานซ่อมบำรุงพัดลม` | `扇風機保守作業` | DRAFT | **New.** |
| 23 | `ResearchStrings` | `Status: Not started` | `สถานะ: ยังไม่เริ่ม` | `状態: 未開始` | DRAFT | **New.** |
| 24 | `ResearchStrings` | `Status: In progress` | `สถานะ: กำลังดำเนินการ` | `状態: 実行中` | DRAFT | **New.** Shown for most of the task. |
| 25 | `ResearchStrings` | `Status: Paused` | `สถานะ: หยุดชั่วคราว` | `状態: 一時停止中` | DRAFT | **New.** |
| 26 | `ResearchStrings` | `Status: Completed` | `สถานะ: เสร็จสิ้น` | `状態: 完了` | DRAFT | **New.** |
| 27 | `ResearchStrings` | `Status: Time limit reached` | `สถานะ: ครบเวลาที่กำหนด` | `状態: 制限時間に到達` | DRAFT | **New.** |
| 28 | `ResearchStrings` | `Status: Stopped by researcher` | `สถานะ: ผู้วิจัยหยุดการทดลอง` | `状態: 研究者により中止` | DRAFT | **New.** |
| 29 | `ResearchStrings` | `Status: Safety stop` | `สถานะ: หยุดเพื่อความปลอดภัย` | `状態: 安全のため中止` | DRAFT | **New.** |
| 30 | `ResearchStrings` | `Status: Reset` | `สถานะ: เริ่มใหม่` | `状態: リセット` | DRAFT | **New.** |
| 31 | `ResearchStrings` (shown if the board loses its task) | `Status: unavailable` | `สถานะ: ไม่พร้อมใช้งาน` | `状態: 取得できません` | DRAFT | **New.** |
| 32 | `ResearchStrings` | `Attempt {0}` | `ครั้งที่ {0}` | `{0} 回目` | DRAFT | **New.** `{0}` is the attempt number. Japanese puts it before the counter, Thai after — please confirm both read naturally at 1, 2 and 3. |
| 33 | `TaskStatusBoard.FinishedMessage` — **the one instruction addressed to the participant** | `This task is finished.<br>Please wait for the researcher.` | `งานนี้เสร็จแล้ว<br>กรุณารอผู้วิจัย` | `この作業は終了しました。<br>研究者をお待ちください。` | DRAFT | |
| | **GROUP 6 — TRAINING BOARD.** Built at run time in the training room, which is where a participant learns the controls — so this is the first text they read in the headset. Entirely English before this pass. | | | | | |
| 34 | `ResearchStrings` | `Neutral XR Training` | `บทฝึกใช้งาน VR` | `VR 操作トレーニング` | DRAFT | **New.** |
| 35 | `ResearchStrings` | `<b>Look</b> headset   <b>Point</b> controller ray   <b>Grip</b> grab and release   <b>Trigger</b> select` | `<b>มอง</b> แว่น VR   <b>ชี้</b> ลำแสงคอนโทรลเลอร์   <b>Grip</b> จับและปล่อย   <b>Trigger</b> เลือก` | `<b>見る</b> ヘッドセット   <b>向ける</b> コントローラーのレイ   <b>Grip</b> つかむ・放す   <b>Trigger</b> 選択` | DRAFT | **New.** `Grip` and `Trigger` are button names and stay English; `Look` and `Point` are translated. Confirm. |
| 36 | `ResearchStrings` | `This scene contains no Computer or Fan maintenance solution.` | `ฉากนี้ไม่มีวิธีแก้ของงานคอมพิวเตอร์หรือพัดลม` | `このシーンにはコンピューターまたは扇風機の作業の答えは含まれていません。` | DRAFT | **New.** |
| 37 | `ResearchStrings` | `Pick up a training object` | `หยิบวัตถุฝึก` | `練習用の物体を持ち上げる` | DRAFT | **New.** |
| 38 | `ResearchStrings` | `Place an object in the comparison tray` | `วางวัตถุลงในถาดเปรียบเทียบ` | `比較用トレイに物体を置く` | DRAFT | **New.** |
| 39 | `ResearchStrings` | `Turn the training dial` | `หมุนลูกบิดฝึก` | `練習用ダイヤルを回す` | DRAFT | **New.** |
| 40 | `ResearchStrings` | `Open the neutral information panel` | `เปิดแผงข้อมูลกลาง` | `中立情報パネルを開く` | DRAFT | **New.** |
| 41 | `ResearchStrings` | `Complete all four skills. <b>RESET</b> returns the training objects.` | `ทำให้ครบทั้ง 4 ทักษะ  <b>RESET</b> จะนำวัตถุฝึกกลับที่เดิม` | `4 つの操作をすべて完了してください。<b>RESET</b> で練習用の物体が元に戻ります。` | DRAFT | **New.** English spells *four*; both translations use the digit `4`, which the numeral check requires. Say if a spelled form is wanted instead — the English would change with it. |
| 42 | `ResearchStrings` | `All four skills complete.` | `ครบทั้ง 4 ทักษะแล้ว` | `4 つの操作がすべて完了しました。` | DRAFT | **New.** As row 41. |
| 43 | `ResearchStrings` | `Continue` | `ต่อไป` | `次へ進む` | DRAFT | **New.** The button that leaves the training room. |
| | **GROUP 7 — TRAINING ROOM SIGNAGE.** Printed on the objects and the bench in the training room. | | | | | |
| 44 | `TrainingSceneBuilder` · `Training Cube A` | `PART A` | `ชิ้น A` | `部品 A` | DRAFT | **New.** The letter `A` identifies the cube and is held identical. |
| 45 | `TrainingSceneBuilder` · `Training Cube B` | `REFERENCE` | `ชิ้นอ้างอิง` | `参照品` | DRAFT | **New.** |
| 46 | `TrainingSceneBuilder` · `Training Cylinder` | `DIAL` | `ลูกบิด` | `ダイヤル` | DRAFT | **New.** |
| 47 | `TrainingSceneBuilder` · `Training Socket` | `PLACE TO COMPARE` | `วางเพื่อเปรียบเทียบ` | `置いて比較` | DRAFT | **New.** |
| 48 | `TrainingSceneBuilder` · `Step 1  PICK UP` | `1  PICK UP` | `1  หยิบ` | `1  持ち上げる` | DRAFT | **New.** |
| 49 | `TrainingSceneBuilder` · `Step 2  COMPARE` | `2  COMPARE` | `2  เปรียบเทียบ` | `2  比較` | DRAFT | **New.** |
| 50 | `TrainingSceneBuilder` · `Step 3  TURN` | `3  TURN` | `3  หมุน` | `3  回す` | DRAFT | **New.** |
| 51 | `TrainingSceneBuilder` · `Reset Caption` | `RESET` | *(untranslated)* | *(untranslated)* | DRAFT | Printed on the physical control. See *Deliberately untranslated*. |
| | **GROUP 8 — BENCH PLACARDS.** Small standing signs on the tray rims and the bench mat. Present in every scene. | | | | | |
| 52 | `BenchDressing.Zone` (both task scenes) | `SPARE PARTS` | `อะไหล่สำรอง` | `予備部品` | DRAFT | **New.** |
| 53 | `BenchDressing.Zone` (both task scenes) | `TOOLS` | `เครื่องมือ` | `工具` | DRAFT | **New.** |
| 54 | `BenchDressing.Zone` (all three participant scenes) | `SERVICE AREA` | `พื้นที่ซ่อมบำรุง` | `作業エリア` | DRAFT | **New.** |
| 55 | `BenchDressing.Zone` (training room) | `PARTS BIN` | `ถาดอะไหล่` | `部品トレイ` | DRAFT | **New.** |
| 56 | `BenchDressing.Zone` (training room) | `PLACE PART HERE` | `วางชิ้นงานที่นี่` | `ここに部品を置く` | DRAFT | **New.** |
| 57 | scene `Placard REMOVED PARTS` (both task scenes) | `REMOVED PARTS` | `ชิ้นส่วนที่ถอดออก` | `取り外した部品` | DRAFT | **New.** |
| 58 | scene `Operator Station` (setup scene) | `HEADSET` | `แว่น VR` | `ヘッドセット` | DRAFT | **New.** |
| 59 | scene `Operator Station` (setup scene) | `RESEARCHER CONSOLE` | `คอนโซลผู้วิจัย` | `研究者コンソール` | DRAFT | **New.** |
| | **GROUP 9 — DEVICE-TEST SIGN.** Stands over the control the participant presses or turns to finish the task. | | | | | |
| 60 | `BenchDressing.PlaceInspectControl` (both task scenes) | `INSPECT` | *(untranslated)* | *(untranslated)* | DRAFT | Printed on the control. See *Deliberately untranslated*. |
| 61 | `BenchDressing.PlaceInspectControl` (both task scenes) | `PRESS TO CHECK THE UNIT` | `กดเพื่อตรวจสอบเครื่อง` | `押して装置を確認` | DRAFT | **New.** Names the action without naming what to fix. |
| | **GROUP 10 — INFORMATION DOCK AND READER CONTROLS.** The header above the four tiles, and the buttons inside an opened reader. | | | | | |
| 62 | `InformationDockBuilder` · `Information Dock/Header` | `INFORMATION SOURCES` | `แหล่งข้อมูล` | `情報源` | DRAFT | **New.** Same wording as the work order's third heading (rows 3–4), deliberately. |
| 63 | scene `*.control.Prev` (all eight readers) | `Prev` | `ก่อนหน้า` | `前へ` | DRAFT | **New.** |
| 64 | scene `*.control.Next` (all eight readers) | `Next` | `ถัดไป` | `次へ` | DRAFT | **New.** |
| 65 | scene `*.control.Close` (all eight readers) | `Close` | `ปิด` | `閉じる` | DRAFT | **New.** |
| 66 | scene `*.control.Play` (both video readers) | `Play` | `เล่น` | `再生` | DRAFT | **New.** |
| 67 | scene `*.control.Pause` (both video readers) | `Pause` | `หยุดชั่วคราว` | `一時停止` | DRAFT | **New.** Same Thai as `Status: Paused` (row 25) uses; confirm that is right in both places. |
| 68 | scene `*.control.Stop` (both video readers) | `Stop` | `หยุด` | `停止` | DRAFT | **New.** |
| 69 | scene `*.control.Restart` (both video readers) | `Restart` | `เริ่มใหม่` | `最初から` | DRAFT | **New.** Same Thai as `Status: Reset` (row 30); confirm. |
| 70 | scene `*.control.Seek+10` (both video readers) | `+10 s` | *(untranslated)* | *(untranslated)* | DRAFT | Numeral and unit. See *Deliberately untranslated*. |
| 71 | `InformationSourceController` · `GEN Video Status` | `00:00 / 01:00` (a running timer) | *(untranslated)* | *(untranslated)* | DRAFT | Numeric. Confirm the `mm:ss / mm:ss` form reads correctly in both languages. |
| 72 | scene `*/GEN Slot` (all nine sources) | `SOURCE A` / `SOURCE B` / `SOURCE C` / `SOURCE D` | `แหล่งข้อมูล A` / `B` / `C` / `D` | `情報源 A` / `B` / `C` / `D` | DRAFT | **Currently switched off — not visible to a participant.** Translated anyway so switching it on is one decision, not two. |
| | **GROUP 11 — LAB WALL NOTICE BOARD.** On the back wall of every scene, readable from anywhere in the room. Deliberately generic lab procedure — it must never hint at either fault. Entirely English before this pass. | | | | | |
| 73 | `LabNoticeBoardBuilder` (Computer) | `BAY 02  ·  COMPUTER SERVICING` | `ช่อง 02  ·  งานซ่อมคอมพิวเตอร์` | `ベイ 02  ·  コンピューター整備` | DRAFT | **New.** Bay numbers are identifiers and stay as digits. |
| 74 | `LabNoticeBoardBuilder` (Fan) | `BAY 03  ·  APPLIANCE SERVICING` | `ช่อง 03  ·  งานซ่อมเครื่องใช้ไฟฟ้า` | `ベイ 03  ·  電気製品整備` | DRAFT | **New.** |
| 75 | `LabNoticeBoardBuilder` (Training) | `BAY 01  ·  ORIENTATION` | `ช่อง 01  ·  ปฐมนิเทศ` | `ベイ 01  ·  オリエンテーション` | DRAFT | **New.** |
| 76 | `LabNoticeBoardBuilder` (Setup) | `MAINTENANCE RESEARCH LAB` | `ห้องปฏิบัติการวิจัยงานซ่อมบำรุง` | `保守作業研究ラボ` | DRAFT | **New.** |
| 77 | `LabNoticeBoardBuilder` (all four scenes) | `SAFETY` | `ความปลอดภัย` | `安全` | DRAFT | **New.** |
| 78 | `LabNoticeBoardBuilder` (all four scenes) | `Isolate at the wall before opening any enclosure.<br>Wait for indicators to go dark.` | `ตัดไฟที่เต้ารับก่อนเปิดฝาครอบทุกชนิด<br>รอจนไฟแสดงสถานะดับสนิท` | `筐体を開ける前に壁側で電源を遮断してください。<br>表示灯が消えるまで待ってください。` | DRAFT | **New.** Generic lab safety; confirm it names no specific machine. |
| 79 | `LabNoticeBoardBuilder` (all four scenes) | `BENCH LAYOUT` | `ผังโต๊ะทำงาน` | `作業台の配置` | DRAFT | **New.** |
| 80 | `LabNoticeBoardBuilder` (all four scenes) | `Spares tray left  ·  tools right.<br>Removed parts go on the lower shelf.` | `ถาดอะไหล่อยู่ซ้าย  ·  เครื่องมืออยู่ขวา<br>ชิ้นส่วนที่ถอดออกให้วางที่ชั้นล่าง` | `予備部品トレイは左  ·  工具は右。<br>取り外した部品は下段の棚へ。` | DRAFT | **New.** Left/right must match the room in both languages. |
| 81 | `LabNoticeBoardBuilder` (all four scenes) | `ESD CONTROL` | `การป้องกันไฟฟ้าสถิต` | `静電気対策` | DRAFT | **New.** |
| 82 | `LabNoticeBoardBuilder` (all four scenes) | `Wrist strap to the bench stud.<br>Handle boards by the edges only.` | `ต่อสายรัดข้อมือเข้ากับจุดกราวด์ของโต๊ะ<br>จับแผงวงจรที่ขอบเท่านั้น` | `リストストラップは作業台の接地端子へ。<br>基板は縁だけを持ってください。` | DRAFT | **New.** |
| 83 | `LabNoticeBoardBuilder` (all four scenes) | `REPORT A FAULT` | `การแจ้งข้อขัดข้อง` | `不具合の報告` | DRAFT | **New.** |
| 84 | `LabNoticeBoardBuilder` (all four scenes) | `Log the unit number and the symptom.<br>Leave the work order on the bench.` | `บันทึกหมายเลขเครื่องและอาการที่พบ<br>วางใบสั่งงานไว้บนโต๊ะ` | `装置番号と症状を記録してください。<br>作業指示書は作業台に置いてください。` | DRAFT | **New.** |
| | **GROUP 12 — PRINTED ON THE EQUIPMENT.** Text that is physically part of a machine in the room. | | | | | |
| 85 | `FanWorkstationBuilder` · `Speed Legend` | `O F F    1    2    3` | *(untranslated)* | *(untranslated)* | DRAFT | A hardware marking on the fan's own slider. See *Deliberately untranslated*. |
| | **GROUP 13 — PARTICIPANT HEADS-UP DISPLAY.** Added after the previous edition of this sheet, so all of it is new here. Each block is separately switchable from the researcher's setup screen and each switch is recorded in `session_manifest.csv`. | | | | | |
| 86 | `ResearchStrings` · HUD block heading | `TIME` | `เวลา` | `時間` | DRAFT | **New.** Over a `mm:ss` counter. |
| 87 | `ResearchStrings` · HUD block heading | `PROGRESS` | `ความคืบหน้า` | `進捗` | DRAFT | **New.** Over a percentage. |
| 88 | `ResearchStrings` · HUD block heading, and `ParticipantHud.Objectives` | `OBJECTIVES` — over the three lines `Device tested`, `Repair performed`, `Device test passed` | `เป้าหมาย` — `ทดสอบเครื่องแล้ว`, `ดำเนินการซ่อมแล้ว`, `ทดสอบผ่านแล้ว` | `目標` — `動作確認を実施`, `修理を実施`, `動作確認に合格` | DRAFT | **New heading; the three lines existed.** They are written as states reached, not as instructions — please keep that. `เป้าหมาย` is also the work order's GOAL heading (rows 3–4); confirm that repetition is wanted. |

> Rows 14, 15, 72 and 88 each stand for more than one string; every other numbered row is
> one string. That is how 88 rows become the 97 in the summary.

---

## Questions for the reviewer

Answer in the last column of the relevant row, or here.

1. **Rows 3 and 4 are the important ones.** They are the only text that tells a participant
   which of the two tasks they are doing, and proposal 9.3 requires the two to differ in
   how much diagnostic decision-making they ask for. Read them side by side in each
   language: is Task A unambiguously *replace the right part following the manual*, and
   Task B unambiguously *work out which of several parts is at fault*? If the distinction
   is sharper in one language than another, that is a confound between the two groups.

2. **Register.** The whole build addresses the participant in one register. Thai uses plain
   polite forms without particles; Japanese uses ですます. Is that right for university
   students aged 18–35 in both countries, and is it consistent across all 88 rows?

3. **Rows 41 and 42 use the digit `4` where the English spells "four".** The numeral rule
   in 9.5 is enforced mechanically and it needs a digit to check. If a spelled numeral
   reads better in Thai or Japanese, say so and the English will be changed to a digit to
   match.

4. **Row 32, the attempt counter.** `ครั้งที่ {0}` and `{0} 回目` — please check they read
   naturally at 1, 2 and 3, since a participant who retries sees this change.

5. **Repeated Thai across different English words.** `หยุดชั่วคราว` is both `Pause`
   (row 67) and `Status: Paused` (row 25); `เริ่มใหม่` is both `Restart` (row 69) and
   `Status: Reset` (row 30). One is a video control and the other a task state. If a
   participant could confuse them, they need different wording.

6. **Rows 14 and 15 — content is identical across all four source types by design.**
   Confirm the Thai and Japanese are also identical across the four, and that this is what
   equivalence should mean here.

7. **Rows 5–12 — the source titles differ between the four types in all three languages.**
   Check that the *degree* of difference matches across languages: if the Thai titles are
   more distinguishable from one another than the English ones, that affects which source a
   participant picks, and choice of source is a main outcome.

8. **The deliberately untranslated set.** `INSPECT`, `RESET`, `Grip`, `Trigger`, `+10 s`
   and the fan's speed legend stay English because they are printed on objects or are
   numerals. Confirm, or say which should be translated — for the first two, the object has
   to be repainted as well as the sentence.

---

## What this sheet does not cover

- **The researcher's setup screen and in-session control panel.** Operated by the
  researcher, never shown to a participant. English only, deliberately.
- **File names, object ids and recorded data values** (`computer.ram`,
  `IncorrectComponentInteraction`, CSV column names). Never shown to a participant and
  must not be translated.
- **Whether the Thai and Japanese glyphs render legibly on the Meta Quest 3 display.**
  That is a hardware check and has never been done — `QUEST3_NEXT_STEPS.md`, check 5. A
  translation approved here could still be unreadable on the device. What *has* been
  checked, in the Editor, is that every string fits its panel in all three languages: the
  work order overflowed its plate in Thai and Japanese until the panel was enlarged on
  2026-08-11, and the validator now measures all three languages on every run.

## How this sheet was built

The English strings were read out of the built scenes by walking all four scenes and
collecting every text component with content, so what is listed is what is in the build.
Run-time text (the status board, the training board, the heads-up display) was read from
the code that produces it. The Thai and Japanese are drafts written for this pass and
placed in `Scripts/Core/ResearchStrings.cs`, `Scripts/UI/LocalizedTaskBrief.cs`,
`Scripts/InformationSources/InformationSourceController.cs`,
`Scripts/UI/TaskStatusBoard.cs`, `Scripts/UI/ParticipantHud.cs` and the
`InformationSourceDefinition` assets — one place per string, all three languages on
adjacent lines.

A coverage sweep over all four scenes confirms that the only English left on any
participant-facing surface is the six deliberately untranslated strings above.
