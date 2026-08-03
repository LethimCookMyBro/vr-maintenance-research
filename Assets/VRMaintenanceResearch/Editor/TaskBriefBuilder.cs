using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// The bench-mounted work order: the short neutral brief a participant reads
    /// before touching anything.
    ///
    /// It states the reported symptom, says the goal is to find the cause and fix
    /// it, points at the information dock and at the INSPECT control, and stops
    /// there. It deliberately does not number the steps, does not say which source
    /// to open, and does not name a component — the previous PROCEDURE notice did
    /// all three, which turned an open diagnostic task into a checklist.
    ///
    /// It is built as scene geometry rather than a runtime canvas so it is present
    /// in edit mode, in the audit captures, and from the first frame of the task.
    /// </summary>
    public static class TaskBriefBuilder
    {
        // Left of the device and above the bench: the empty wall band between the
        // information dock and the workstation, blocking nothing.
        //
        // Raised and pulled left once the panel was enlarged for legibility — at the
        // old anchor its right edge finished level with the device and the two read
        // as one crowded mass.
        static readonly Vector3 k_Anchor = new Vector3(-0.68f, 1.58f, 1.30f);
        const float k_Yaw = -12f;

        const string k_Sources =
            "Manuals, a troubleshooting guide, a video and a visual guide are on the dock to your left. " +
            "Use any of them, or none, in any order.";

        const string k_Bench =
            "Tools and spare parts are in the marked trays on this bench.";

        const string k_Inspect =
            "Press INSPECT on the pedestal whenever you want the unit checked.";

        public static void BuildComputer() => Build(
            "COMPUTER  ·  WORK ORDER",
            "Reported fault:  the computer does not power on.\n\n" +
            "Find out why, and put it right.\n\n" +
            k_Sources + "\n\n" + k_Bench + "\n\n" + k_Inspect);

        public static void BuildFan() => Build(
            "DESK FAN  ·  WORK ORDER",
            "Reported fault:  the fan does not run.\n\n" +
            "Find out why, and put it right.\n\n" +
            k_Sources + "\n\n" + k_Bench + "\n\n" + k_Inspect);

        static void Build(string heading, string body)
        {
            var root = Root("Task Brief", k_Anchor, new Vector3(0f, k_Yaw, 0f));
            var t = root.transform;

            // Stand: a clamp post off the bench's back edge, so the brief reads as
            // bench equipment rather than floating signage.
            Box("Clamp", t, new Vector3(0.30f, -0.645f, 0.030f), new Vector3(0.070f, 0.050f, 0.090f), "Lab_MetalDark");
            Cyl("Post", t, new Vector3(0.30f, -0.440f, 0.030f), new Vector3(0.024f, 0.185f, 0.024f), "Lab_MetalDark");
            Box("Post Head", t, new Vector3(0.30f, -0.268f, 0.024f), new Vector3(0.048f, 0.038f, 0.048f), "Lab_MetalDark");

            // Sized for legibility from the start pose, 2.9 m back — not for the
            // screenshot. Fresh readers could see that a brief existed and could not
            // read a word of it, which meant they could not say what the task was:
            // the panel and its copy are both roughly 1.5x what they were.
            Box("Backing", t, Vector3.zero, new Vector3(0.980f, 0.560f, 0.016f), "Lab_Navy");
            Box("Face", t, new Vector3(0f, -0.038f, -0.010f), new Vector3(0.930f, 0.450f, 0.008f), "Lab_PanelSurface");
            Box("Header Band", t, new Vector3(0f, 0.232f, -0.010f), new Vector3(0.980f, 0.096f, 0.010f), "Lab_Navy");
            Box("Header Rule", t, new Vector3(0f, 0.178f, -0.014f), new Vector3(0.980f, 0.008f, 0.010f), "Lab_Accent");

            var title = Label("Heading", t, new Vector3(0f, 0.232f, -0.018f), heading, 0.30f, "#F2F5F8", Vector3.zero, 0.94f);
            title.fontStyle = TMPro.FontStyles.Bold;
            title.characterSpacing = 6f;

            var text = Label("Body", t, new Vector3(0f, -0.038f, -0.016f), body, 0.215f, "#1E2A3A", Vector3.zero, 0.88f);
            text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.rectTransform.sizeDelta = new Vector2(0.870f, 0.430f);
            text.lineSpacing = 6f;
        }
    }
}
