using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// The bench-mounted work order: the short neutral brief a participant reads
    /// before touching anything.
    ///
    /// It states the reported symptom, states the goal, points at the information
    /// dock and at the INSPECT control, and stops there. It deliberately does not
    /// number the steps, does not say which source to open, and does not name a
    /// component — the previous PROCEDURE notice did all three, which turned an open
    /// task into a checklist.
    ///
    /// **The two goals are deliberately different, and the difference is the study.**
    /// Proposal 9.3 defines Task A as a component-replacement task and Task B as a
    /// fault-diagnosis task, and requires Task B to carry more diagnostic decision
    /// making than Task A. So the computer brief tells the participant to follow the
    /// manual and fit the correct replacement part, and never uses the words "find
    /// the cause"; the fan brief says the cause could be one of several parts and
    /// asks them to find it. For a spell both briefs read "find the cause and repair
    /// it", which erased the contrast the whole comparison rests on.
    ///
    /// Everything else about the two is held identical — the same four headings, the
    /// same order, the same closing sentence, and goal lines within a few characters
    /// of each other — because 11.7 standardises the procedure across the two sites
    /// and 10.3.3 across the two tasks. What differs is what the participant is asked
    /// to do, not how much text they are given to read.
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
        static readonly Vector3 k_Anchor = new Vector3(-0.75f, 1.68f, 1.30f);
        const float k_Yaw = -12f;

        // "The unit is assembled and open for service" is the one line that tells a
        // participant which task this is. Without it the brief says only that
        // something is wrong, and a bench with an open machine on it can as easily be
        // read as a machine waiting to be built. It says nothing about where to look
        // or what to change, so neither task's answer is touched.

        /// <summary>
        /// Task A, proposal 9.3.1 — replace the correct component.
        ///
        /// "Follow the manual" is the instruction; which component is correct is in the
        /// sources and nowhere else. Naming no part keeps the identification step, which
        /// is the part of Task A that is measured.
        /// </summary>
        public static void BuildComputer() => Build(true);

        /// <summary>
        /// Task B, proposal 9.3.2 — find the fault among several possible causes.
        ///
        /// "Several parts could be responsible" is the whole of the extra decision load,
        /// and it names none of them. The bench backs it up: the spares tray holds two
        /// cartridges that look alike, and the service mat carries the motor and the
        /// sealed module as further candidates to rule out.
        /// </summary>
        public static void BuildFan() => Build(false);

        static void Build(bool computer)
        {
            // The English comes from the same method the runtime uses for the other two
            // languages, so the scene copy and the translated copy cannot drift apart.
            // Editing the wording in one place now changes both.
            var heading = LocalizedTaskBrief.Heading(computer, ResearchLanguage.English);
            var body = LocalizedTaskBrief.Body(computer, ResearchLanguage.English);

            var root = Root("Task Brief", k_Anchor, new Vector3(0f, k_Yaw, 0f));
            root.AddComponent<LocalizedTaskBrief>();
            var t = root.transform;

            // Stand: a clamp post off the bench's back edge, so the brief reads as
            // bench equipment rather than floating signage.
            Box("Clamp", t, new Vector3(0.30f, -0.645f, 0.030f), new Vector3(0.070f, 0.050f, 0.090f), "Lab_MetalDark");
            Cyl("Post", t, new Vector3(0.30f, -0.430f, 0.030f), new Vector3(0.024f, 0.200f, 0.024f), "Lab_MetalDark");
            Box("Post Head", t, new Vector3(0.30f, -0.244f, 0.024f), new Vector3(0.048f, 0.038f, 0.048f), "Lab_MetalDark");

            // Sized for legibility from the start pose, 2.9 m back — not for the
            // screenshot. Fresh readers could see that a brief existed and could not
            // read a word of it, which meant they could not say what the task was.
            //
            // The body copy is not scaled down to fit. The validator holds a 0.30
            // readability floor for the brief, so the copy stays at the floor and the
            // panel gives back the margin instead.
            //
            // Sized for the **longest of the three languages**, not for English. At
            // 0.790 x 0.420 the English body measured 0.400 and fitted; the same panel
            // measured 0.490 in Thai and 0.570 in Japanese, so the two languages the
            // study is actually about had their closing INSPECT line cut off by the
            // plate — the exact defect that was fixed for English and never checked for
            // the other two. The copy carries the same information in all three (9.5),
            // so shortening a translation to fit was not available.
            //
            // Two cheaper levers were measured first and neither works. Widening from
            // 0.748 to 1.100 takes Japanese from 0.570 to 0.470 and leaves Thai at
            // 0.490 — Thai has no spaces to wrap on, so extra width buys it nothing.
            // Dropping lineSpacing from 4 to 0 saves 10 mm across the whole panel and
            // costs readability. The line count is set by the four headings and the
            // blank lines between them, and Thai and Japanese need two more lines than
            // English for the same sentences. So the box has to hold 0.490, and it is
            // 0.510 with 20 mm of margin.
            //
            // The growth went sideways first and upward second: at 0.790 wide the same
            // capacity needed a much taller plate, and a tall plate at this anchor is
            // what got the 0.98 x 0.56 version cut back for being the largest thing in
            // the room. The anchor also moved 70 mm left so the extra width does not
            // close on the device. This panel is 23 per cent larger in area than the one
            // that was called too dominant, which is a real cost and is recorded in
            // PROTOCOL_CHANGE_LOG.md; the alternative was a work order that two thirds
            // of the participants could not read to the end. The validator now measures
            // all three languages, so it cannot regress silently again.
            Box("Backing", t, Vector3.zero, new Vector3(1.085f, 0.620f, 0.016f), "Lab_Navy");
            Box("Face", t, new Vector3(0f, -0.042f, -0.010f), new Vector3(1.042f, 0.520f, 0.008f), "Lab_PanelSurface");
            Box("Header Band", t, new Vector3(0f, 0.269f, -0.010f), new Vector3(1.085f, 0.082f, 0.010f), "Lab_Navy");
            Box("Header Rule", t, new Vector3(0f, 0.224f, -0.014f), new Vector3(1.085f, 0.008f, 0.010f), "Lab_Accent");

            var title = Label("Heading", t, new Vector3(0f, 0.269f, -0.018f), heading, 0.26f, "#F2F5F8", Vector3.zero, 1.02f);
            title.fontStyle = TMPro.FontStyles.Bold;
            title.characterSpacing = 6f;

            // Light copy, because the face is now dark glass rather than the pale
            // plate it was authored against. #DCE4EE over the composited surface is
            // about 11:1; the 0.30 readability floor the validator holds is unchanged.
            var text = Label("Body", t, new Vector3(0f, -0.042f, -0.016f), body, 0.30f, "#DCE4EE", Vector3.zero, 1.00f);
            text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TMPro.TextWrappingModes.Normal;
            text.rectTransform.sizeDelta = new Vector2(1.000f, 0.510f);
            text.lineSpacing = 4f;
        }
    }
}
