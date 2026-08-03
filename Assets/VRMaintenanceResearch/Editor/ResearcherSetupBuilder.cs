using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// The researcher scene renders its form on a screen-space canvas built at
    /// runtime, so in the editor it was a bare room with an empty bench behind the
    /// UI. This gives the backdrop a plausible operator station, which is what the
    /// researcher actually sees behind the form.
    /// </summary>
    public static class ResearcherSetupBuilder
    {
        const float k_BenchTop = BenchDressing.BenchTop;

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Researcher Setup")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ResearchSceneSet.ResearcherSetup, OpenSceneMode.Single);

            var root = Root("Operator Station", Vector3.zero);
            var t = root.transform;

            BuildMonitor(t, new Vector3(-0.62f, 0f, 1.06f), -14f);
            BuildMonitor(t, new Vector3(0.62f, 0f, 1.06f), 14f);
            BuildDeskItems(t);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ResearcherSetup] rebuilt");
        }

        static void BuildMonitor(Transform parent, Vector3 pos, float yaw)
        {
            var monitor = Group($"Monitor {pos.x:0.00}", parent, pos, new Vector3(0f, yaw, 0f));

            Box("Foot", monitor, new Vector3(0f, k_BenchTop + 0.010f, 0.06f), new Vector3(0.240f, 0.020f, 0.150f), "Lab_Navy");
            Cyl("Stem", monitor, new Vector3(0f, k_BenchTop + 0.090f, 0.06f), new Vector3(0.040f, 0.070f, 0.040f), "Lab_MetalDark");
            Box("Neck", monitor, new Vector3(0f, k_BenchTop + 0.190f, 0.045f), new Vector3(0.060f, 0.120f, 0.030f), "Lab_MetalDark", new Vector3(-8f, 0f, 0f));

            var head = Group("Head", monitor, new Vector3(0f, k_BenchTop + 0.300f, 0.030f), new Vector3(-8f, 0f, 0f));
            Box("Chassis", head, Vector3.zero, new Vector3(0.560f, 0.330f, 0.020f), "Lab_PlasticDark");
            Box("Screen", head, new Vector3(0f, 0.010f, -0.012f), new Vector3(0.530f, 0.290f, 0.004f), "Lab_Navy");
            Box("Screen Glow", head, new Vector3(0f, 0.010f, -0.015f), new Vector3(0.510f, 0.270f, 0.002f), "Lab_AntiStatic");

            // A few UI-ish bars so the screens are not blank rectangles.
            Box("Header Bar", head, new Vector3(0f, 0.128f, -0.017f), new Vector3(0.510f, 0.026f, 0.002f), "Lab_Accent");
            for (var i = 0; i < 5; i++)
                Box($"Row {i + 1}", head, new Vector3(-0.09f, 0.075f - i * 0.042f, -0.017f), new Vector3(0.330f, 0.014f, 0.002f), "Lab_Trim");
            for (var i = 0; i < 3; i++)
                Box($"Chip {i + 1}", head, new Vector3(0.165f, 0.060f - i * 0.055f, -0.017f), new Vector3(0.120f, 0.030f, 0.002f), "Lab_Trim");

            Box("Stand Badge", head, new Vector3(0f, -0.150f, -0.013f), new Vector3(0.060f, 0.006f, 0.002f), "Lab_MetalDark");
        }

        static void BuildDeskItems(Transform parent)
        {
            // Keyboard
            var kb = Group("Keyboard", parent, new Vector3(0f, k_BenchTop + 0.012f, 0.66f), new Vector3(0f, 0f, 0f));
            Box("Base", kb, Vector3.zero, new Vector3(0.440f, 0.018f, 0.150f), "Lab_PlasticDark", new Vector3(-3f, 0f, 0f));
            for (var row = 0; row < 4; row++)
                for (var col = 0; col < 14; col++)
                    Box($"Key {row}_{col}", kb, new Vector3(-0.198f + col * 0.0305f, 0.013f - row * 0.001f, -0.048f + row * 0.031f), new Vector3(0.024f, 0.006f, 0.022f), "Lab_Trim", new Vector3(-3f, 0f, 0f));

            // Mouse
            var mouse = Group("Mouse", parent, new Vector3(0.34f, k_BenchTop + 0.016f, 0.66f));
            Cyl("Shell", mouse, Vector3.zero, new Vector3(0.060f, 0.016f, 0.095f), "Lab_PlasticDark");
            Box("Split", mouse, new Vector3(0f, 0.016f, -0.024f), new Vector3(0.003f, 0.004f, 0.040f), "Lab_MetalDark");

            // Notebook + pen: the researcher logs by hand alongside the form.
            var pad = Group("Notepad", parent, new Vector3(-0.66f, k_BenchTop + 0.008f, 0.62f), new Vector3(0f, 12f, 0f));
            Box("Paper", pad, Vector3.zero, new Vector3(0.210f, 0.010f, 0.290f), "Lab_PanelSurface");
            Box("Binding", pad, new Vector3(-0.098f, 0.004f, 0f), new Vector3(0.020f, 0.008f, 0.290f), "Lab_Navy");
            for (var i = 0; i < 7; i++)
                Box($"Rule {i + 1}", pad, new Vector3(0.012f, 0.006f, -0.100f + i * 0.032f), new Vector3(0.150f, 0.001f, 0.002f), "Lab_Line");
            Cyl("Pen", pad, new Vector3(0.130f, 0.006f, -0.020f), new Vector3(0.010f, 0.070f, 0.010f), "Lab_Accent", new Vector3(90f, 18f, 0f));

            // Headset resting on the bench, a research-lab signature object.
            var headset = Group("Headset", parent, new Vector3(0.95f, k_BenchTop + 0.050f, 0.95f), new Vector3(0f, -22f, 0f));
            Box("Visor", headset, Vector3.zero, new Vector3(0.190f, 0.095f, 0.110f), "Lab_PlasticDark");
            Box("Lens Face", headset, new Vector3(0f, 0f, -0.058f), new Vector3(0.170f, 0.075f, 0.006f), "Lab_GlassPanel");
            Box("Strap Top", headset, new Vector3(0f, 0.062f, 0.030f), new Vector3(0.150f, 0.028f, 0.130f), "Lab_Trim");
            Box("Strap Rear", headset, new Vector3(0f, 0.010f, 0.096f), new Vector3(0.130f, 0.070f, 0.020f), "Lab_Trim");

            // Two controllers alongside it.
            for (var i = 0; i < 2; i++)
            {
                var c = Group($"Controller {i + 1}", parent, new Vector3(1.32f + i * 0.20f, k_BenchTop + 0.034f, 0.92f), new Vector3(0f, -18f + i * 12f, 0f));
                Box("Grip", c, Vector3.zero, new Vector3(0.048f, 0.058f, 0.130f), "Lab_PlasticDark");
                Cyl("Ring", c, new Vector3(0f, 0.042f, -0.030f), new Vector3(0.090f, 0.006f, 0.090f), "Lab_Trim");
                Cyl("Stick", c, new Vector3(0f, 0.034f, 0.022f), new Vector3(0.018f, 0.008f, 0.018f), "Lab_Accent");
            }

            BenchDressing.Zone(parent, new Vector3(0.95f, k_BenchTop + 0.004f, 0.66f), "HEADSET");
            BenchDressing.Zone(parent, new Vector3(0f, k_BenchTop + 0.004f, 1.30f), "RESEARCHER CONSOLE");
        }
    }
}
