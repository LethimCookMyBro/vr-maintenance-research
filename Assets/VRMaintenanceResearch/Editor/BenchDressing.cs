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
        /// Top of the inside floor of the lab's own Parts and Tool trays, which stand on
        /// the bench at x = -1.10 and x = +1.10. Anything stored in a tray rests here.
        /// </summary>
        public const float TrayFloor = 0.940f;

        /// <summary>
        /// Mat + zone labels + tools. <paramref name="serviceCentreX"/> is where the
        /// device sits. Set <paramref name="withTools"/> false for training, whose
        /// right-hand tray is the placement target rather than a tool tray.
        /// </summary>
        public static void Build(float serviceCentreX, float matWidth = 0.90f, float matDepth = 0.66f, bool withTools = true,
            string serviceLabel = "SERVICE AREA")
        {
            var root = Root("Workstation Dressing", Vector3.zero);
            var t = root.transform;

            // Anti-static mat marks the service area and grounds the device visually.
            Box("Service Mat", t, new Vector3(serviceCentreX, BenchTop + 0.003f, 0.95f), new Vector3(matWidth, 0.006f, matDepth), "Lab_AntiStatic");
            Box("Mat Edge", t, new Vector3(serviceCentreX, BenchTop + 0.005f, 0.95f), new Vector3(matWidth - 0.04f, 0.004f, matDepth - 0.04f), "Lab_Navy");

            // The lab's own Parts Tray and Tool Tray already stand on the bench at
            // x = -1.10 and x = +1.10. Building a second pair here put a tray under a
            // tray and swallowed anything resting between the two floors, so the trays
            // looked half empty. Parts go in the furniture's trays, at TrayFloor.

            // The pale antistatic pad that used to sit here existed to make a bare
            // spare DIMM visible in the tray. Both maintenance benches are diagnostic
            // and neither hands out loose board parts any more, so the pad was a lit
            // white rectangle marking an empty spot.

            if (withTools)
                BuildToolSet(t);

            Zone(t, new Vector3(-1.10f, TrayFloor + 0.046f, 0.762f), withTools ? "SPARE PARTS" : "PARTS BIN");
            Zone(t, new Vector3(1.10f, TrayFloor + 0.046f, 0.762f), withTools ? "TOOLS" : "PLACE PART HERE");
            Zone(t, new Vector3(serviceCentreX, BenchTop + 0.030f, 0.95f - matDepth * 0.5f - 0.014f), serviceLabel);
            // No INSPECT caption here: PlaceInspectControl gives that station its own
            // sign, and two labels for one button read as two controls.
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
            // Sits on the tool tray's floor. The foam slab and its cut-out are gone:
            // the tray is now the recess, and a dark foam sheet filling it only hid the
            // one tool the participant is meant to find.
            var tray = Group("Tool Set", root, new Vector3(1.10f, TrayFloor, 0.95f));

            Cyl("Screw Cup", tray, new Vector3(0.16f, 0.008f, 0.09f), new Vector3(0.070f, 0.014f, 0.070f), "Lab_PlasticDark");
            for (var i = 0; i < 5; i++)
                Cyl($"Screw {i + 1}", tray, new Vector3(0.16f + Mathf.Cos(i * 1.3f) * 0.018f, 0.020f, 0.09f + Mathf.Sin(i * 1.3f) * 0.018f), new Vector3(0.008f, 0.003f, 0.008f), "Lab_ToolSteel");

            Cyl("Wrist Strap Coil", tray, new Vector3(0.17f, 0.004f, -0.09f), new Vector3(0.080f, 0.006f, 0.080f), "Lab_CableBlack");
            Box("Wrist Cuff", tray, new Vector3(0.17f, 0.011f, -0.09f), new Vector3(0.040f, 0.010f, 0.020f), "Lab_AntiStatic");
        }

        /// <summary>
        /// The device-test control on the pedestal — the action that ends the task.
        ///
        /// It shipped as an unlabelled disc with the interactable scaled to 0.11, so
        /// anything parented to it came out at a ninth of its intended size. The
        /// scale is normalised, the collider resized to match, and the pedestal now
        /// carries an INSPECT plate so the control names itself.
        ///
        /// Both benches then showed the same three stacked cylinders — a flat disc a
        /// pilot reader could take for an indicator lamp as easily as for a control. The
        /// XRI example kit already ships the two shapes that say "press" and "turn"
        /// without a caption, at the size these pedestals need, so the control now looks
        /// like what its stable id calls it: a push button on the computer bench, a
        /// rotary speed selector on the fan bench. Only the meshes are borrowed —
        /// XRPushButton and XRKnob keep their own interaction state and would fight
        /// <see cref="ResearchInteractable"/> for the same events.
        /// </summary>
        public enum InspectControlShape { PushButton, Dial }

        const string k_XriControls = "Assets/XRI_Examples/UI_3D/Models/";

        public static void PlaceInspectControl(string interactableName, InspectControlShape shape = InspectControlShape.PushButton)
        {
            var go = GameObject.Find(interactableName);
            if (go == null)
            {
                Debug.LogWarning($"[BenchDressing] missing {interactableName}");
                return;
            }

            go.transform.localScale = Vector3.one;
            var position = go.transform.position;

            // The same box the sphere already bounded, through the one rule every other
            // part goes through. The control keeps the reach it had; what it loses is a
            // second hand-rolled collider path for a future primitive to slip through.
            SetCollider(go, new Vector3(0.15f, 0.15f, 0.15f));

            var visual = ResetVisual(interactableName, out _);
            if (visual != null)
            {
                // Mounting collar under the borrowed plate, so the control sits on the
                // pedestal rather than floating a centimetre above it.
                Cyl("Mount Collar", visual, new Vector3(0f, -0.018f, 0f), new Vector3(0.132f, 0.008f, 0.132f), "Lab_MetalDark");

                // Fitted so the model's own height is the binding axis; the plate then
                // lands on the collar and the moving part stands proud of it.
                var push = shape == InspectControlShape.PushButton;
                var model = ImportedVisual(push ? "Push Button" : "Speed Dial", visual,
                    k_XriControls + (push ? "PushButton.fbx" : "Dial.fbx"),
                    new Vector3(0f, push ? 0.034f : 0.032f, 0f),
                    push ? new Vector3(0.140f, 0.096f, 0.140f) : new Vector3(0.140f, 0.088f, 0.140f));

                if (model != null)
                {
                    // Plate in the bench's own dark metal, moving part in the palette's
                    // structural slate.
                    //
                    // The moving part used to be Lab_Accent, which is emissive. That
                    // made the one control a participant has to press the brightest
                    // saturated object in the room, and the study's first outcome is
                    // where a participant goes first (proposal 10.2.2). A glowing pad
                    // does not reveal the fault, but it does say "start here", and an
                    // instrument may not answer its own first question. Everything the
                    // room uses Lab_Accent for is a rule a few millimetres wide — the
                    // notice board header, the station strip, this station's own sign
                    // rule — never a filled face, and the control now follows that.
                    PaintNamed(model, push ? "EmergencyStopPlate" : "Dial", "Lab_MetalDark");
                    PaintNamed(model, push ? "EmergencyStop" : "Knob", "Lab_Trim");
                }
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
        /// Names a bench zone with a small placard standing on the tray's near rim.
        ///
        /// This used to be text engraved flat into the bench top at 0.62 font across a
        /// 1.1 m box — captions the size of the trays they named, reading as signage
        /// painted on the furniture. A tray gets a placard on its edge, the way a real
        /// bench does; it names the zone without competing with what is in it.
        /// </summary>
        public static void Zone(Transform parent, Vector3 pos, string text)
        {
            var card = Group($"Placard {text}", parent, pos, new Vector3(22f, 0f, 0f));

            // Plate grows with the caption. A fixed plate fitted SPARE PARTS and let
            // longer captions like RESEARCHER CONSOLE run off both ends of it.
            var width = Mathf.Max(0.115f, text.Length * 0.0115f + 0.020f);

            Box("Plate", card, Vector3.zero, new Vector3(width, 0.026f, 0.003f), "Lab_LabelPlate");
            Box("Stand", card, new Vector3(0f, -0.014f, 0.004f), new Vector3(0.018f, 0.012f, 0.008f), "Lab_MetalDark");

            var label = Label("Caption", card, new Vector3(0f, 0f, -0.003f), text, 0.10f, "#243141", Vector3.zero, width - 0.004f);
            label.fontStyle = TMPro.FontStyles.Bold;
            label.characterSpacing = 6f;
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

            // Neutral Tool is a capsule primitive, so the box-only version of this stood
            // a 1 x 2 x 1 m grab volume on the tool tray of both benches.
            SetCollider(tool, new Vector3(0.24f, 0.06f, 0.06f));

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
