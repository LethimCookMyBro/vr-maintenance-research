using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// The Information Station backing was a 2.5 m blank slab behind the bench —
    /// the single largest empty surface in every scene, and the source of the
    /// "blank / unloaded guide content" reading. The information sources now live
    /// on the arm-mounted dock, so this becomes what a real lab wall carries:
    /// bay identity, safety notices and an ESD reminder.
    /// </summary>
    public static class LabNoticeBoardBuilder
    {
        // Matches Station Backing in LabEnvironment: centre (-2.2, 1.28, 3.22), 2.55 x 1.42.
        static readonly Vector3 k_BoardFace = new Vector3(2.10f, 1.36f, 3.175f);

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Notice Boards")]
        public static void BuildAll()
        {
            foreach (var scenePath in ResearchSceneSet.AllScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Build(BayNameFor(scenePath));
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Debug.Log("[NoticeBoard] rebuilt in all scenes");
        }

        static string BayNameFor(string scenePath)
        {
            if (scenePath == ResearchSceneSet.Computer) return "BAY 02  ·  COMPUTER SERVICING";
            if (scenePath == ResearchSceneSet.Fan) return "BAY 03  ·  APPLIANCE SERVICING";
            if (scenePath == ResearchSceneSet.Training) return "BAY 01  ·  ORIENTATION";
            return "MAINTENANCE RESEARCH LAB";
        }

        public static void Build(string bayName)
        {
            var station = GameObject.Find("Station Backing");
            if (station == null)
            {
                Debug.Log("[NoticeBoard] no Station Backing in this scene");
                return;
            }

            var root = Root("Lab Notice Board", k_BoardFace);
            var t = root.transform;

            // Compact environmental context: deliberately tertiary to the machine,
            // Work Order and source dock.
            Box("Header Band", t, new Vector3(0f, 0.330f, 0f), new Vector3(1.78f, 0.14f, 0.020f), "Lab_Navy");
            Box("Header Rule", t, new Vector3(0f, 0.252f, -0.002f), new Vector3(1.78f, 0.010f, 0.022f), "Lab_Accent");
            var title = Label("Bay Title", t, new Vector3(0f, 0.330f, -0.016f), bayName, 0.31f, "#F2F5F8", Vector3.zero, 1.66f);
            title.fontStyle = TMPro.FontStyles.Bold;
            title.characterSpacing = 8f;

            BuildCard(t, new Vector3(-0.450f, 0.108f, 0f), "SAFETY",
                "Isolate at the wall before opening any enclosure.\nWait for indicators to go dark.");
            BuildCard(t, new Vector3(0.450f, 0.108f, 0f), "BENCH LAYOUT",
                "Spares tray left  ·  tools right.\nRemoved parts go on the lower shelf.");
            BuildCard(t, new Vector3(-0.450f, -0.212f, 0f), "ESD CONTROL",
                "Wrist strap to the bench stud.\nHandle boards by the edges only.");
            BuildCard(t, new Vector3(0.450f, -0.212f, 0f), "REPORT A FAULT",
                "Log the unit number and the symptom.\nLeave the work order on the bench.");
        }

        /// <summary>
        /// One notice: heading strip over two lines of body copy.
        ///
        /// Both labels stand 14 mm proud of the plate. They used to be placed at the
        /// card's local origin — dead centre of an 18 mm plate — so the plate's own
        /// front face sat 9 mm nearer the participant than the glyphs and the depth
        /// test discarded them. Every card rendered as a blank white rectangle, which
        /// is the "wall board text is missing" report: the strings were always there
        /// and the references were fine, the geometry hid them.
        ///
        /// Body copy is lab procedure only. Nothing here may hint at either fault:
        /// the board is read from anywhere in the room, and a notice naming a
        /// connector or a fuse would answer the task from the doorway.
        /// </summary>
        static void BuildCard(Transform parent, Vector3 pos, string heading, string body)
        {
            var card = Group($"Notice {heading}", parent, pos);
            Box("Face", card, Vector3.zero, new Vector3(0.84f, 0.27f, 0.018f), "Lab_PanelSurface");
            Box("Accent", card, new Vector3(0f, 0.116f, -0.002f), new Vector3(0.84f, 0.038f, 0.020f), "Lab_Accent");

            // #14202E on Lab_PanelSurface (#F4F6F8) is about 15:1 — well past the 4.5:1
            // floor, which matters because this board is read across the room.
            // Sized to fill the card rather than to fit it. At 0.20/0.15 the copy
            // occupied about a third of the plate and was unreadable from the
            // participant's start pose 4.8 m away; the card was always this big.
            //
            // The heading sits clear of the accent strip above it, and the body's box
            // starts below the heading's glyphs: the body is top-aligned in a 0.15-tall
            // rect, so its anchor is its centre and the two will overlap if the anchor
            // is not at least half that box below the heading.
            var h = Label("Heading", card, new Vector3(0f, 0.062f, -0.014f), heading, 0.34f, "#14202E", Vector3.zero, 0.80f);
            h.fontStyle = TMPro.FontStyles.Bold;
            h.characterSpacing = 6f;

            var copy = Label("Body", card, new Vector3(0f, -0.056f, -0.014f), body, 0.30f, "#2B3949", Vector3.zero, 0.78f);
            copy.alignment = TMPro.TextAlignmentOptions.Top;
            copy.textWrappingMode = TMPro.TextWrappingModes.Normal;
            copy.rectTransform.sizeDelta = new Vector2(0.78f, 0.15f);
            copy.lineSpacing = 2f;
        }
    }
}
