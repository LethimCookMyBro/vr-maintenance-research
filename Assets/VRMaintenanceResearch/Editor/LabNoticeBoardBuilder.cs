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

            BuildCard(t, new Vector3(-0.445f, 0.095f, 0f), "SAFETY");
            BuildCard(t, new Vector3(0.445f, 0.095f, 0f), "BENCH LAYOUT");
            BuildCard(t, new Vector3(-0.445f, -0.185f, 0f), "ESD CONTROL");
            BuildCard(t, new Vector3(0.445f, -0.185f, 0f), "ERROR CODES");
        }

        static void BuildCard(Transform parent, Vector3 pos, string heading)
        {
            var card = Group($"Notice {heading}", parent, pos);
            Box("Face", card, Vector3.zero, new Vector3(0.82f, 0.23f, 0.018f), "Lab_PanelSurface");
            Box("Accent", card, new Vector3(0f, 0.096f, -0.002f), new Vector3(0.82f, 0.038f, 0.020f), "Lab_Accent");

            var h = Label("Heading", card, Vector3.zero, heading, 0.30f, "#1E2A3A", Vector3.zero, 0.76f);
            h.fontStyle = TMPro.FontStyles.Bold;
            h.characterSpacing = 6f;
        }
    }
}
