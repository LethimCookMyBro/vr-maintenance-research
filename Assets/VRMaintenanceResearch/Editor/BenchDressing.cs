using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Bench furniture shared by every task scene, so the three benches read as
    /// the same lab rather than three unrelated greybox rooms. Carries no task
    /// logic: nothing here is interactable.
    /// </summary>
    public static class BenchDressing
    {
        public const float BenchTop = 0.92f;

        /// <summary>
        /// Mat + zone labels + tools. <paramref name="serviceCentreX"/> is where the
        /// device sits. Set <paramref name="withTools"/> false for training, whose
        /// right-hand tray is the placement target rather than a tool tray.
        /// </summary>
        public static void Build(float serviceCentreX, float matWidth = 0.90f, float matDepth = 0.66f, bool withTools = true)
        {
            var root = Root("Workstation Dressing", Vector3.zero);
            var t = root.transform;

            // Anti-static mat marks the service area and grounds the device visually.
            Box("Service Mat", t, new Vector3(serviceCentreX, BenchTop + 0.003f, 0.95f), new Vector3(matWidth, 0.006f, matDepth), "Lab_AntiStatic");
            Box("Mat Edge", t, new Vector3(serviceCentreX, BenchTop + 0.005f, 0.95f), new Vector3(matWidth - 0.04f, 0.004f, matDepth - 0.04f), "Lab_Navy");

            for (var i = 0; i < 2; i++)
                Box($"Spares Divider {i + 1}", t, new Vector3(-1.01f - i * 0.26f, BenchTop + 0.028f, 0.95f), new Vector3(0.008f, 0.030f, 0.340f), "Lab_PlasticDark");

            if (withTools)
                BuildToolSet(t);

            Zone(t, new Vector3(-1.10f, BenchTop + 0.004f, 0.66f), withTools ? "SPARE PARTS" : "PARTS BIN");
            Zone(t, new Vector3(1.10f, BenchTop + 0.004f, 0.66f), withTools ? "TOOLS" : "PLACE PART HERE");
            Zone(t, new Vector3(serviceCentreX, BenchTop + 0.010f, 0.95f - matDepth * 0.5f + 0.035f), "SERVICE AREA");
            Zone(t, new Vector3(LabLayoutBuilder.TestStationX, BenchTop + 0.004f, 0.86f), "INSPECT");
        }

        /// <summary>
        /// Bench kit around the tool tray.
        ///
        /// The decorative screwdriver and pliers that used to lie here were the same
        /// shape as the one screwdriver the participant can actually pick up, so the
        /// tray offered three tools and answered to one. They are gone: the tray now
        /// holds a foam cut-out for the real tool and nothing else tool-shaped.
        /// Consumables and PPE stay, because nobody mistakes a screw cup for a tool.
        /// </summary>
        static void BuildToolSet(Transform root)
        {
            var tray = Group("Tool Set", root, new Vector3(1.22f, BenchTop + 0.020f, 0.95f));

            // Foam insert: the recess the interactable screwdriver lies in.
            Box("Foam Insert", tray, new Vector3(-0.16f, 0.004f, 0.00f), new Vector3(0.320f, 0.014f, 0.300f), "Lab_Rubber");
            Box("Driver Recess", tray, new Vector3(-0.20f, 0.012f, 0.00f), new Vector3(0.260f, 0.010f, 0.070f), "Lab_PlasticDark", new Vector3(0f, 8f, 0f));

            Cyl("Screw Cup", tray, new Vector3(-0.13f, 0.014f, 0.11f), new Vector3(0.070f, 0.014f, 0.070f), "Lab_PlasticDark");
            for (var i = 0; i < 5; i++)
                Cyl($"Screw {i + 1}", tray, new Vector3(-0.13f + Mathf.Cos(i * 1.3f) * 0.018f, 0.026f, 0.11f + Mathf.Sin(i * 1.3f) * 0.018f), new Vector3(0.008f, 0.003f, 0.008f), "Lab_ToolSteel");

            Cyl("Wrist Strap Coil", tray, new Vector3(0.10f, 0.008f, -0.10f), new Vector3(0.080f, 0.006f, 0.080f), "Lab_CableBlack");
            Box("Wrist Cuff", tray, new Vector3(0.10f, 0.016f, -0.10f), new Vector3(0.040f, 0.010f, 0.020f), "Lab_AntiStatic");
        }

        /// <summary>
        /// The device-test control on the pedestal — the action that ends the task.
        ///
        /// It shipped as an unlabelled disc with the interactable scaled to 0.11, so
        /// anything parented to it came out at a ninth of its intended size. The
        /// scale is normalised, the collider resized to match, and the pedestal now
        /// carries an INSPECT plate so the control names itself.
        /// </summary>
        public static void PlaceInspectControl(string interactableName)
        {
            var go = GameObject.Find(interactableName);
            if (go == null)
            {
                Debug.LogWarning($"[BenchDressing] missing {interactableName}");
                return;
            }

            go.transform.localScale = Vector3.one;
            var position = go.transform.position;

            var sphere = go.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                sphere.center = Vector3.zero;
                sphere.radius = 0.075f;
            }

            var visual = ResetVisual(interactableName, out _);
            if (visual != null)
            {
                Cyl("Button Bezel", visual, new Vector3(0f, -0.008f, 0f), new Vector3(0.120f, 0.012f, 0.120f), "Lab_MetalDark");
                Cyl("Button Cap", visual, new Vector3(0f, 0.010f, 0f), new Vector3(0.096f, 0.014f, 0.096f), "Lab_Accent");
                Cyl("Button Inlay", visual, new Vector3(0f, 0.021f, 0f), new Vector3(0.058f, 0.004f, 0.058f), "Lab_PanelSurface");
            }

            // Sign sits directly over its own button and says what pressing it does.
            //
            // Standing it off to one side left readers unsure whether the pedestal was
            // a control or just a label for the whole station, and "INSPECT" alone did
            // not say that the button is the thing you press. The second line names
            // the action without naming what to fix.
            var sign = Root("Inspect Station Sign", position + new Vector3(0f, 0.145f, 0.055f), new Vector3(20f, 0f, 0f));
            var s = sign.transform;
            Box("Plate", s, Vector3.zero, new Vector3(0.290f, 0.120f, 0.010f), "Lab_Navy");
            Box("Plate Rule", s, new Vector3(0f, -0.052f, -0.004f), new Vector3(0.290f, 0.008f, 0.011f), "Lab_Accent");
            Cyl("Sign Post", s, new Vector3(0f, -0.098f, 0.014f), new Vector3(0.016f, 0.040f, 0.016f), "Lab_MetalDark");

            var caption = Label("Caption", s, new Vector3(0f, 0.026f, -0.008f), "INSPECT", 0.32f, "#F2F5F8", Vector3.zero, 0.28f);
            caption.fontStyle = TMPro.FontStyles.Bold;
            caption.characterSpacing = 10f;

            var sub = Label("Sub Caption", s, new Vector3(0f, -0.022f, -0.008f), "PRESS TO CHECK THE UNIT", 0.14f, "#C6D2E0", Vector3.zero, 0.28f);
            sub.characterSpacing = 2f;
        }

        /// <summary>
        /// Bench zone caption.
        ///
        /// These were near-white on a near-white bench top at a size that vanished
        /// past a metre. Fresh readers could see a tray but could not tell a tool
        /// tray from a parts tray, which is the whole job of the caption. Dark ink,
        /// half again as large.
        /// </summary>
        public static void Zone(Transform parent, Vector3 pos, string text)
        {
            var label = Label($"Label {text}", parent, pos, text, 0.62f, "#243141", new Vector3(90f, 0f, 0f), 1.1f);
            label.fontStyle = TMPro.FontStyles.Bold;
            label.characterSpacing = 14f;
        }

        /// <summary>
        /// The Poly Haven screwdriver mesh is ~2 mm long with a 5182x compensation
        /// scale on its child, so any change to the root scale distorts it wildly.
        /// This sets a uniform child scale that yields a ~0.20 m tool.
        /// </summary>
        public static void PlaceScrewdriver(Vector3 position, float yaw = 8f)
        {
            var tool = GameObject.Find("Neutral Tool");
            if (tool == null)
                return;

            tool.transform.SetParent(null, false);
            tool.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            tool.transform.localScale = Vector3.one;

            var box = tool.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = Vector3.zero;
                box.size = new Vector3(0.24f, 0.06f, 0.06f);
            }

            var model = tool.transform.Find("CC0 Screwdriver");
            if (model != null)
            {
                model.localScale = Vector3.one * 95f;
                model.localRotation = Quaternion.Euler(0f, 90f, 0f);
                model.localPosition = new Vector3(0f, -0.012f, 0f);
            }
        }
    }
}
