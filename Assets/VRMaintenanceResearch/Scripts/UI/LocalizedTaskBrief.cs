using TMPro;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Applies the session language to the bench-mounted Work Order.
    ///
    /// The two briefs differ by task as well as by language, which is why this is not
    /// handled by the general scene sweep in <see cref="LocalizedSceneText"/>: what
    /// Task A asks for is not what Task B asks for. Proposal 9.3.1 makes Task A a
    /// component replacement carried out from the manual, 9.3.2 makes Task B a
    /// diagnosis among several possible causes, and 9.3 requires B to carry the
    /// heavier decision load. The wording below is the only place a participant is
    /// told which of the two they are doing, so the distinction has to survive
    /// translation intact — in all three languages Task A says follow the manual and
    /// fit the correct part, and never says find the cause, while Task B says several
    /// parts could be responsible and asks for the cause.
    ///
    /// The strings are static and public because two other things need them: the
    /// visual validator measures all three languages against the panel, since a brief
    /// that overflows is a brief the participant cannot finish reading, and
    /// `Docs/TRANSLATION_REVIEW.md` quotes them for the reviewer required by 9.13.
    /// </summary>
    public sealed class LocalizedTaskBrief : MonoBehaviour
    {
        string englishHeading;
        string englishBody;

        void Awake()
        {
            englishHeading = transform.Find("Heading")?.GetComponent<TMP_Text>()?.text;
            englishBody = transform.Find("Body")?.GetComponent<TMP_Text>()?.text;
        }

        void Start() => Refresh();

        public void Refresh()
        {
            var task = FindFirstObjectByType<MaintenanceTaskController>();
            if (task == null || task.Definition == null)
                return;

            var language = ResearchSessionManager.Instance != null
                ? ResearchSessionManager.Instance.Configuration.language
                : ResearchLanguage.English;

            var heading = transform.Find("Heading")?.GetComponent<TMP_Text>();
            var body = transform.Find("Body")?.GetComponent<TMP_Text>();
            if (heading == null || body == null)
                return;

            var computer = task.Definition.taskId == ResearchTaskId.Computer;
            if (language == ResearchLanguage.English)
            {
                // The English is read back from the scene rather than from the table
                // below, because the scene is what the builder wrote and what the audit
                // captures show. If the two ever disagree, the scene wins.
                heading.text = englishHeading;
                body.text = englishBody;
                return;
            }

            InformationSourceController.EnsureLocalizedFontFallbacks();
            heading.text = Heading(computer, language);
            body.text = Body(computer, language);
        }

        public static string Heading(bool computer, ResearchLanguage language)
        {
            switch (language)
            {
                case ResearchLanguage.Thai:
                    return computer ? "คอมพิวเตอร์  ·  ใบสั่งงาน" : "พัดลมตั้งโต๊ะ  ·  ใบสั่งงาน";
                case ResearchLanguage.Japanese:
                    return computer ? "コンピューター  ·  作業指示" : "卓上扇風機  ·  作業指示";
                default:
                    return computer ? "COMPUTER  ·  WORK ORDER" : "DESK FAN  ·  WORK ORDER";
            }
        }

        /// <summary>
        /// Four headings, in the same order, in every language and both tasks. Only the
        /// symptom line and the goal line carry task-specific content; the closing
        /// INSPECT sentence is identical across both, because 11.7 standardises the
        /// procedure and a difference there would be a difference in procedure rather
        /// than in task.
        ///
        /// `INSPECT` stays English in all three: it is the word printed on the control
        /// the sentence is pointing at.
        /// </summary>
        public static string Body(bool computer, ResearchLanguage language)
        {
            switch (language)
            {
                case ResearchLanguage.Thai:
                    return computer
                        ? "<b>อาการที่รายงาน</b>\nคอมพิวเตอร์เปิดไม่ติด\n\n<b>เป้าหมาย</b>\nเครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง ให้ทำตามคู่มือและเปลี่ยนส่วนประกอบให้ถูกชิ้น\n\n<b>แหล่งข้อมูล</b>\nอยู่ทางซ้ายของคุณ\n\nเมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>"
                        : "<b>อาการที่รายงาน</b>\nพัดลมตั้งโต๊ะทำงานผิดปกติ\n\n<b>เป้าหมาย</b>\nเครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง สาเหตุเป็นไปได้หลายอย่าง ให้หาสาเหตุและซ่อมให้เรียบร้อย\n\n<b>แหล่งข้อมูล</b>\nอยู่ทางซ้ายของคุณ\n\nเมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>";
                case ResearchLanguage.Japanese:
                    return computer
                        ? "<b>報告された症状</b>\nコンピューターの電源が入りません。\n\n<b>目標</b>\n装置は組み立て済みで、点検のために開けてあります。マニュアルに従って、正しい交換部品を取り付けてください。\n\n<b>情報源</b>\n左側にあります。\n\n装置の準備ができたら、<b>INSPECT</b> を押してください。"
                        : "<b>報告された症状</b>\n卓上扇風機が正常に動作しません。\n\n<b>目標</b>\n装置は組み立て済みで、点検のために開けてあります。原因はいくつか考えられます。原因を特定し、修理してください。\n\n<b>情報源</b>\n左側にあります。\n\n装置の準備ができたら、<b>INSPECT</b> を押してください。";
                default:
                    return computer
                        ? "<b>REPORTED ISSUE</b>\nThe computer does not power on.\n\n<b>GOAL</b>\nThe unit is assembled and open for service. Follow the manual and fit the correct replacement component.\n\n<b>INFORMATION SOURCES</b>\nAvailable on your left.\n\nPress <b>INSPECT</b> when the unit is ready."
                        : "<b>REPORTED ISSUE</b>\nThe desk fan does not operate correctly.\n\n<b>GOAL</b>\nThe unit is assembled and open for service. Several parts could be responsible. Find the cause and repair it.\n\n<b>INFORMATION SOURCES</b>\nAvailable on your left.\n\nPress <b>INSPECT</b> when the unit is ready.";
            }
        }
    }
}
