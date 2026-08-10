using System.Collections.Generic;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Every participant-visible string that is not already localised somewhere else,
    /// keyed by the exact English wording that appears in the build.
    ///
    /// Proposal 9.5 requires the Thai and Japanese materials to match in meaning,
    /// format and numeric content. Holding all three languages of a string on one line
    /// is what makes that checkable: a reviewer reads across, not between files, and a
    /// wording change that lands in one language and not the others is visible at the
    /// point of the edit rather than three months later in the data.
    ///
    /// **Every entry here is a DRAFT.** Proposal 9.13 requires an expert outside the
    /// research team to review the instruments before real collection, and none of
    /// this has been through that. `Docs/TRANSLATION_REVIEW.md` is the sheet that goes
    /// to that reviewer and carries the same strings, row for row.
    ///
    /// Two rules the table follows, both from 9.5:
    ///
    ///  - Numerals, units and letter identifiers are byte-identical across the three
    ///    languages — `4`, `10`, `02`, `A`, `+10 s`, `O F F  1  2  3`. A participant
    ///    who counts four skills in English must count four in Thai.
    ///  - No entry names a component, a symptom or a procedure. The lab notice board
    ///    is read from anywhere in either room, so a notice that named either fault
    ///    would answer both tasks from the doorway; the training board is read before
    ///    either task starts. What is written here is lab procedure and controls.
    ///
    /// Deliberately **not** in the table, and left in English on purpose:
    ///
    ///  - `INSPECT` and `RESET`. Both are printed on physical objects in the room, so
    ///    a sentence that uses the word is pointing at something the participant can
    ///    see. Translating the sentence without repainting the object would break that
    ///    reference. Same reasoning as `Grip` and `Trigger`, which are the names of
    ///    controller buttons.
    ///  - `+10 s`, `00:00 / 01:00` and the fan's `O F F  1  2  3` legend. Numerals and
    ///    unit symbols, held identical by the rule above.
    ///  - The researcher's setup screen and in-session panel. Neither is ever shown to
    ///    a participant; they are English by design.
    ///  - Object ids, event names and CSV column names. Translating them would break
    ///    the data.
    /// </summary>
    public static class ResearchStrings
    {
        public static ResearchLanguage Current =>
            ResearchSessionManager.Instance == null
                ? ResearchLanguage.English
                : ResearchSessionManager.Instance.Configuration.language;

        public static string T(string english) => T(english, Current);

        /// <summary>
        /// The English string is both the key and the fallback, so a string that has
        /// not been translated yet renders in English rather than as a blank panel or
        /// a missing-key marker. A participant reading English in a Thai session is a
        /// protocol problem worth catching; an empty sign is a broken build.
        /// </summary>
        public static string T(string english, ResearchLanguage language)
        {
            if (language == ResearchLanguage.English || string.IsNullOrEmpty(english))
                return english;
            if (!Table.TryGetValue(english, out var entry))
                return english;
            return language == ResearchLanguage.Thai ? entry.thai : entry.japanese;
        }

        /// <summary>True when this exact English string has both translations.</summary>
        public static bool Has(string english) => english != null && Table.ContainsKey(english);

        public static IEnumerable<string> Keys => Table.Keys;

        static readonly Dictionary<string, (string thai, string japanese)> Table =
            new Dictionary<string, (string thai, string japanese)>
            {
                // ---- status board -------------------------------------------------
                // The board sits above the bench for the whole task, so these are the
                // strings a participant looks at longest. "Status:" is kept as part of
                // each phrase rather than composed from two pieces, so that what a
                // reviewer approves is the whole line as it will be rendered.
                { "Computer Maintenance Task", ("งานซ่อมบำรุงคอมพิวเตอร์", "コンピューター保守作業") },
                { "Fan Maintenance Task", ("งานซ่อมบำรุงพัดลม", "扇風機保守作業") },
                { "Status: Not started", ("สถานะ: ยังไม่เริ่ม", "状態: 未開始") },
                { "Status: In progress", ("สถานะ: กำลังดำเนินการ", "状態: 実行中") },
                { "Status: Paused", ("สถานะ: หยุดชั่วคราว", "状態: 一時停止中") },
                { "Status: Completed", ("สถานะ: เสร็จสิ้น", "状態: 完了") },
                { "Status: Time limit reached", ("สถานะ: ครบเวลาที่กำหนด", "状態: 制限時間に到達") },
                { "Status: Stopped by researcher", ("สถานะ: ผู้วิจัยหยุดการทดลอง", "状態: 研究者により中止") },
                { "Status: Safety stop", ("สถานะ: หยุดเพื่อความปลอดภัย", "状態: 安全のため中止") },
                { "Status: Reset", ("สถานะ: เริ่มใหม่", "状態: リセット") },
                { "Status: unavailable", ("สถานะ: ไม่พร้อมใช้งาน", "状態: 取得できません") },
                // Japanese puts the counter after the number, Thai before it, so the
                // attempt number is a placeholder rather than a concatenation.
                { "Attempt {0}", ("ครั้งที่ {0}", "{0} 回目") },

                // ---- participant heads-up display ---------------------------------
                { "TIME", ("เวลา", "時間") },
                { "PROGRESS", ("ความคืบหน้า", "進捗") },
                { "OBJECTIVES", ("เป้าหมาย", "目標") },

                // ---- training board -----------------------------------------------
                // The first text a participant reads in the headset. `Grip` and
                // `Trigger` stay English: they are the names printed on the controller.
                { "Neutral XR Training", ("บทฝึกใช้งาน VR", "VR 操作トレーニング") },
                {
                    "<b>Look</b> headset   <b>Point</b> controller ray   <b>Grip</b> grab and release   <b>Trigger</b> select",
                    ("<b>มอง</b> แว่น VR   <b>ชี้</b> ลำแสงคอนโทรลเลอร์   <b>Grip</b> จับและปล่อย   <b>Trigger</b> เลือก",
                     "<b>見る</b> ヘッドセット   <b>向ける</b> コントローラーのレイ   <b>Grip</b> つかむ・放す   <b>Trigger</b> 選択")
                },
                {
                    "This scene contains no Computer or Fan maintenance solution.",
                    ("ฉากนี้ไม่มีวิธีแก้ของงานคอมพิวเตอร์หรือพัดลม",
                     "このシーンにはコンピューターまたは扇風機の作業の答えは含まれていません。")
                },
                { "Pick up a training object", ("หยิบวัตถุฝึก", "練習用の物体を持ち上げる") },
                { "Place an object in the comparison tray", ("วางวัตถุลงในถาดเปรียบเทียบ", "比較用トレイに物体を置く") },
                { "Turn the training dial", ("หมุนลูกบิดฝึก", "練習用ダイヤルを回す") },
                { "Open the neutral information panel", ("เปิดแผงข้อมูลกลาง", "中立情報パネルを開く") },
                {
                    "Complete all four skills. <b>RESET</b> returns the training objects.",
                    ("ทำให้ครบทั้ง 4 ทักษะ  <b>RESET</b> จะนำวัตถุฝึกกลับที่เดิม",
                     "4 つの操作をすべて完了してください。<b>RESET</b> で練習用の物体が元に戻ります。")
                },
                { "All four skills complete.", ("ครบทั้ง 4 ทักษะแล้ว", "4 つの操作がすべて完了しました。") },
                { "Continue", ("ต่อไป", "次へ進む") },

                // ---- training room signage ----------------------------------------
                // `A` stays `A` in all three: it identifies one cube against another,
                // and a participant told to compare A with the reference has to find
                // the same letter on the object.
                { "PART A", ("ชิ้น A", "部品 A") },
                { "REFERENCE", ("ชิ้นอ้างอิง", "参照品") },
                { "DIAL", ("ลูกบิด", "ダイヤル") },
                { "PLACE TO COMPARE", ("วางเพื่อเปรียบเทียบ", "置いて比較") },
                { "1  PICK UP", ("1  หยิบ", "1  持ち上げる") },
                { "2  COMPARE", ("2  เปรียบเทียบ", "2  比較") },
                { "3  TURN", ("3  หมุน", "3  回す") },

                // ---- bench placards ------------------------------------------------
                { "SPARE PARTS", ("อะไหล่สำรอง", "予備部品") },
                { "TOOLS", ("เครื่องมือ", "工具") },
                { "SERVICE AREA", ("พื้นที่ซ่อมบำรุง", "作業エリア") },
                { "PARTS BIN", ("ถาดอะไหล่", "部品トレイ") },
                { "PLACE PART HERE", ("วางชิ้นงานที่นี่", "ここに部品を置く") },
                { "REMOVED PARTS", ("ชิ้นส่วนที่ถอดออก", "取り外した部品") },
                { "HEADSET", ("แว่น VR", "ヘッドセット") },
                { "RESEARCHER CONSOLE", ("คอนโซลผู้วิจัย", "研究者コンソール") },

                // ---- device-test station -------------------------------------------
                // The caption above the button is INSPECT in every language, because
                // INSPECT is what is printed on the button.
                { "PRESS TO CHECK THE UNIT", ("กดเพื่อตรวจสอบเครื่อง", "押して装置を確認") },

                // ---- information dock and reader controls --------------------------
                { "INFORMATION SOURCES", ("แหล่งข้อมูล", "情報源") },
                { "Prev", ("ก่อนหน้า", "前へ") },
                { "Next", ("ถัดไป", "次へ") },
                { "Close", ("ปิด", "閉じる") },
                { "Play", ("เล่น", "再生") },
                { "Pause", ("หยุดชั่วคราว", "一時停止") },
                { "Stop", ("หยุด", "停止") },
                { "Restart", ("เริ่มใหม่", "最初から") },
                // Switched off in the build. Translated anyway so that switching the
                // slot letters back on is one decision and not two.
                { "SOURCE A", ("แหล่งข้อมูล A", "情報源 A") },
                { "SOURCE B", ("แหล่งข้อมูล B", "情報源 B") },
                { "SOURCE C", ("แหล่งข้อมูล C", "情報源 C") },
                { "SOURCE D", ("แหล่งข้อมูล D", "情報源 D") },

                // ---- lab wall notice board -----------------------------------------
                // Read from anywhere in the room, in all four scenes. Bay numbers are
                // identifiers and stay as digits.
                { "BAY 02  ·  COMPUTER SERVICING", ("ช่อง 02  ·  งานซ่อมคอมพิวเตอร์", "ベイ 02  ·  コンピューター整備") },
                { "BAY 03  ·  APPLIANCE SERVICING", ("ช่อง 03  ·  งานซ่อมเครื่องใช้ไฟฟ้า", "ベイ 03  ·  電気製品整備") },
                { "BAY 01  ·  ORIENTATION", ("ช่อง 01  ·  ปฐมนิเทศ", "ベイ 01  ·  オリエンテーション") },
                { "MAINTENANCE RESEARCH LAB", ("ห้องปฏิบัติการวิจัยงานซ่อมบำรุง", "保守作業研究ラボ") },
                { "SAFETY", ("ความปลอดภัย", "安全") },
                {
                    "Isolate at the wall before opening any enclosure.\nWait for indicators to go dark.",
                    ("ตัดไฟที่เต้ารับก่อนเปิดฝาครอบทุกชนิด\nรอจนไฟแสดงสถานะดับสนิท",
                     "筐体を開ける前に壁側で電源を遮断してください。\n表示灯が消えるまで待ってください。")
                },
                { "BENCH LAYOUT", ("ผังโต๊ะทำงาน", "作業台の配置") },
                {
                    "Spares tray left  ·  tools right.\nRemoved parts go on the lower shelf.",
                    ("ถาดอะไหล่อยู่ซ้าย  ·  เครื่องมืออยู่ขวา\nชิ้นส่วนที่ถอดออกให้วางที่ชั้นล่าง",
                     "予備部品トレイは左  ·  工具は右。\n取り外した部品は下段の棚へ。")
                },
                { "ESD CONTROL", ("การป้องกันไฟฟ้าสถิต", "静電気対策") },
                {
                    "Wrist strap to the bench stud.\nHandle boards by the edges only.",
                    ("ต่อสายรัดข้อมือเข้ากับจุดกราวด์ของโต๊ะ\nจับแผงวงจรที่ขอบเท่านั้น",
                     "リストストラップは作業台の接地端子へ。\n基板は縁だけを持ってください。")
                },
                { "REPORT A FAULT", ("การแจ้งข้อขัดข้อง", "不具合の報告") },
                {
                    "Log the unit number and the symptom.\nLeave the work order on the bench.",
                    ("บันทึกหมายเลขเครื่องและอาการที่พบ\nวางใบสั่งงานไว้บนโต๊ะ",
                     "装置番号と症状を記録してください。\n作業指示書は作業台に置いてください。")
                },
            };
    }
}
