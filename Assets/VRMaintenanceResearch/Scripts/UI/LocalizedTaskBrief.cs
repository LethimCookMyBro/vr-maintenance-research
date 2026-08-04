using TMPro;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>Applies the session language to the neutral maintenance Work Order.</summary>
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
            if (language == ResearchLanguage.Thai)
            {
                heading.text = computer ? "คอมพิวเตอร์  ·  ใบสั่งงาน" : "พัดลมตั้งโต๊ะ  ·  ใบสั่งงาน";
                body.text = computer
                    ? "<b>อาการที่รายงาน</b>\nคอมพิวเตอร์เปิดไม่ติด\n\n<b>เป้าหมาย</b>\nเครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง ให้หาสาเหตุและซ่อมให้เรียบร้อย\n\n<b>แหล่งข้อมูล</b>\nอยู่ทางซ้ายของคุณ\n\nเมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>"
                    : "<b>อาการที่รายงาน</b>\nพัดลมตั้งโต๊ะทำงานผิดปกติ\n\n<b>เป้าหมาย</b>\nเครื่องประกอบเสร็จแล้วและเปิดฝาไว้เพื่อซ่อมบำรุง ให้หาสาเหตุและซ่อมให้เรียบร้อย\n\n<b>แหล่งข้อมูล</b>\nอยู่ทางซ้ายของคุณ\n\nเมื่อเครื่องพร้อมแล้ว ให้กด <b>INSPECT</b>";
                return;
            }

            if (language == ResearchLanguage.Japanese)
            {
                heading.text = computer ? "コンピューター  ·  作業指示" : "卓上扇風機  ·  作業指示";
                body.text = computer
                    ? "<b>報告された症状</b>\nコンピューターの電源が入りません。\n\n<b>目標</b>\n装置は組み立て済みで、点検のために開けてあります。原因を特定し、修理してください。\n\n<b>情報源</b>\n左側にあります。\n\n装置の準備ができたら、<b>INSPECT</b> を押してください。"
                    : "<b>報告された症状</b>\n卓上扇風機が正常に動作しません。\n\n<b>目標</b>\n装置は組み立て済みで、点検のために開けてあります。原因を特定し、修理してください。\n\n<b>情報源</b>\n左側にあります。\n\n装置の準備ができたら、<b>INSPECT</b> を押してください。";
                return;
            }

            heading.text = englishHeading;
            body.text = englishBody;
        }
    }
}
