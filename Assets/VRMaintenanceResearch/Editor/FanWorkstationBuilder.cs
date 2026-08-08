using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Rebuilds the fan bench for participant comprehension.
    ///
    /// The previous head was four full-diameter bars crossing at the hub, which
    /// renders as a white plus sign on a stick — pilot readers called it a signpost,
    /// a lamp and a weather vane before they called it a fan. Three things carry the
    /// identity now: a five-paddle propeller whose paddles are offset from the hub so
    /// the disc reads solid, a rear guard cage that gives the circular silhouette
    /// even with the front guard removed, and a weighted base with a control pod.
    ///
    /// The fault is a blown fuse, so the scene must also show a service bay: cover
    /// swung open, fuse holder and wiring exposed, replacement fuses in the spares
    /// tray. Neither spare fuse advertises its condition from a distance — the
    /// broken element is only visible close up, which is the diagnostic act.
    ///
    /// Local frame of the fan: front is -Z, service bay is -X, both angled toward
    /// the participant by the body's -28 degree yaw.
    /// </summary>
    public static class FanWorkstationBuilder
    {
        const float k_BenchTop = BenchDressing.BenchTop;

        const float k_HeadY = 0.400f;    // motor axis height above the bench
        const float k_BladeTip = 0.159f; // propeller tip radius -> 0.32 m disc
        const float k_CageR = 0.166f;    // rear guard rim radius
        const float k_BarrelR = 0.098f;  // motor housing radius
        const float k_BayX = -0.112f;    // service blister centre, clear of the barrel

        const string k_XriControls = "Assets/XRI_Examples/UI_3D/Models/";

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Fan Workstation")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ResearchSceneSet.Fan, OpenSceneMode.Single);

            BuildFanDevice();
            PlaceFanParts();
            BenchDressing.PlaceScrewdriver(new Vector3(1.02f, k_BenchTop + 0.035f, 0.95f));
            // Same caption as the computer bench. "INSTALLED COMPONENT" sat on the mat
            // under the whole fan and named the machine a component, which is exactly
            // the wrong frame for a unit the participant is meant to diagnose.
            BenchDressing.Build(0f, 0.86f, 0.62f, serviceLabel: "SERVICE AREA");
            BuildRemovedPartsRack();
            BenchDressing.PlaceInspectControl("Fan Speed Selector", BenchDressing.InspectControlShape.Dial);
            TaskBriefBuilder.BuildFan();

            // A cartridge fuse is 30 mm of glass and 6 mm across. Alone on the floor of
            // a 0.8 m tray it is invisible from the participant's start pose, and a tray
            // captioned SPARE PARTS that looks empty says the wrong thing about the
            // task. The pad is the marker; the part is still the part. Sized to the real
            // fuse rather than the 84 mm one it used to hold, and in the bench's own
            // antistatic blue rather than white — a glass-and-nickel cartridge on a
            // white card is the same value as the card and disappears into it. Added
            // after BenchDressing.Build, which rebuilds the root it hangs from.
            var dressing = GameObject.Find("Workstation Dressing");
            if (dressing != null)
                Box("Fuse Pad", dressing.transform, new Vector3(-1.10f, BenchDressing.TrayFloor + 0.003f, 0.95f),
                    new Vector3(0.072f, 0.006f, 0.044f), "Lab_AntiStatic");

            // Stowed last, so a rebuild refreshes each part before putting it away and
            // a second run finds them again through FindAny.
            //
            // The bench is now one assembled fan, one open service bay with one fuse
            // fitted in it, one spare fuse, and one screwdriver. What is stowed:
            //
            //  - the second loose fuse. Two identical cartridges in the tray, one of
            //    them blown, was a second diagnosis stacked on top of the first, and
            //    from the bench it read as a box of fuses to fit rather than a fault to
            //    find.
            //  - the spare motor. It is the largest object on the bench, it is not the
            //    fault, no information source mentions it, and a motor sitting beside a
            //    part-stripped fan says "assemble this".
            //  - the sealed module, for the same reason as the computer bench.
            //
            // All three keep their StableObjectId, their ResearchInteractable and their
            // collider; reactivating the object restores it exactly.
            Stow("Faulty Fuse");
            Stow("Fan Motor Module");
            Stow("Fan Non Target Module");

            // The blade, fuse holder, wiring, fastener and switch are children of
            // Electric Fan Body, so the body's interactable was claiming their colliders
            // and answering rays meant for the fault site.
            BindOwnColliders();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[FanWorkstation] rebuilt");
        }

        static void BuildFanDevice()
        {
            var legacy = GameObject.Find("Visual Desk Fan Assembly");
            if (legacy != null)
                Object.DestroyImmediate(legacy);

            var body = GameObject.Find("Electric Fan Body");
            if (body == null)
            {
                Debug.LogWarning("[FanWorkstation] Electric Fan Body missing");
                return;
            }

            body.transform.SetPositionAndRotation(new Vector3(0f, k_BenchTop, 1.00f), Quaternion.Euler(0f, -28f, 0f));
            body.transform.localScale = Vector3.one;
            SetCollider(body, new Vector3(0.30f, 0.56f, 0.30f));
            var visual = ResetVisual("Electric Fan Body", out _);
            if (visual == null)
                return;

            BuildStand(visual);
            BuildHead(visual);
            BuildControlPod(visual);
            PlaceFanInteractables(body.transform);
        }

        /// <summary>Weighted base, telescoping column and the mains lead's strain relief.</summary>
        static void BuildStand(Transform visual)
        {
            Cyl("Base Pad", visual, new Vector3(0f, 0.005f, 0f), new Vector3(0.290f, 0.005f, 0.290f), "Lab_Rubber");
            Cyl("Base", visual, new Vector3(0f, 0.026f, 0f), new Vector3(0.280f, 0.018f, 0.280f), "Lab_PlasticLight");
            Cyl("Base Bevel", visual, new Vector3(0f, 0.048f, 0f), new Vector3(0.244f, 0.006f, 0.244f), "Lab_PlasticLight");
            Cyl("Base Trim", visual, new Vector3(0f, 0.055f, 0f), new Vector3(0.212f, 0.003f, 0.212f), "Lab_Navy");

            // Cord exit and strain relief at the back of the base, so the lead on the
            // bench reads as this fan's lead rather than a loose coil of wire.
            Box("Cord Gland", visual, new Vector3(0f, 0.030f, 0.138f), new Vector3(0.036f, 0.024f, 0.030f), "Lab_PlasticDark");
            Box("Cord Tail", visual, new Vector3(0.012f, 0.020f, 0.180f), new Vector3(0.012f, 0.012f, 0.080f), "Lab_CableBlack", new Vector3(-16f, -18f, 0f));

            Cyl("Column Lower", visual, new Vector3(0f, 0.140f, 0f), new Vector3(0.062f, 0.085f, 0.062f), "Lab_PlasticLight");
            Cyl("Column Collar", visual, new Vector3(0f, 0.226f, 0f), new Vector3(0.070f, 0.012f, 0.070f), "Lab_MetalDark");
            Cyl("Column Upper", visual, new Vector3(0f, 0.286f, 0f), new Vector3(0.048f, 0.050f, 0.048f), "Lab_PlasticLight");

            // Height-lock knob: reads as an adjustable stand, not a welded pole.
            Cyl("Height Knob", visual, new Vector3(-0.044f, 0.226f, 0f), new Vector3(0.030f, 0.014f, 0.030f), "Lab_Navy", new Vector3(0f, 0f, 90f));

            // Tilt yoke carrying the head.
            Box("Yoke Left", visual, new Vector3(-0.036f, 0.352f, 0f), new Vector3(0.012f, 0.070f, 0.044f), "Lab_PlasticLight");
            Box("Yoke Right", visual, new Vector3(0.036f, 0.352f, 0f), new Vector3(0.012f, 0.070f, 0.044f), "Lab_PlasticLight");
            Cyl("Tilt Bolt L", visual, new Vector3(-0.040f, k_HeadY, 0f), new Vector3(0.022f, 0.006f, 0.022f), "Lab_MetalDark", new Vector3(0f, 0f, 90f));
            Cyl("Tilt Bolt R", visual, new Vector3(0.040f, k_HeadY, 0f), new Vector3(0.022f, 0.006f, 0.022f), "Lab_MetalDark", new Vector3(0f, 0f, 90f));
        }

        static void BuildHead(Transform visual)
        {
            var head = Group("Head", visual, new Vector3(0f, k_HeadY, 0f));

            // --- motor barrel: the dark mass the light propeller reads against ---
            Cyl("Housing Front", head, new Vector3(0f, 0f, 0.012f), new Vector3(0.190f, 0.008f, 0.190f), "Lab_Navy", new Vector3(90f, 0f, 0f));
            Cyl("Housing", head, new Vector3(0f, 0f, 0.088f), new Vector3(0.196f, 0.070f, 0.196f), "Lab_Navy", new Vector3(90f, 0f, 0f));
            Cyl("Housing Rear", head, new Vector3(0f, 0f, 0.160f), new Vector3(0.178f, 0.008f, 0.178f), "Lab_Navy", new Vector3(90f, 0f, 0f));
            Cyl("Housing Rim", head, new Vector3(0f, 0f, 0.006f), new Vector3(0.206f, 0.005f, 0.206f), "Lab_MetalDark", new Vector3(90f, 0f, 0f));

            // Motor cooling fins, confined to a band behind the service bay.
            //
            // They used to run the barrel's whole length at every angle, which put a
            // fin squarely across the bay opening: the compartment a participant is
            // meant to look into was behind a grille.
            for (var i = 0; i < 12; i++)
                Box($"Fin {i + 1}", head, new Vector3(0f, 0f, 0.146f), new Vector3(0.206f, 0.006f, 0.026f), "Lab_Navy", new Vector3(0f, 0f, i * 15f));

            // Guard clips: three lugs on the front rim where the removed cage bolts on.
            for (var i = 0; i < 3; i++)
            {
                var a = (90f + i * 120f) * Mathf.Deg2Rad;
                Box($"Guard Lug {i + 1}", head, new Vector3(Mathf.Cos(a) * 0.100f, Mathf.Sin(a) * 0.100f, 0.006f), new Vector3(0.024f, 0.024f, 0.022f), "Lab_MetalDark");
            }

            BuildRearCage(head);
            BuildServiceBay(head);
        }

        /// <summary>
        /// The rear guard, not the front one, is what keeps the circular fan
        /// silhouette while the front cage is off the machine and on the shelf.
        /// </summary>
        static void BuildRearCage(Transform head)
        {
            var cage = Group("Rear Guard", head);

            // Rim: tangential segments, so it is a ring rather than a filled disc.
            for (var i = 0; i < 24; i++)
            {
                var seg = Group($"Rim {i + 1}", cage, Vector3.zero, new Vector3(0f, 0f, i * 15f));
                Box("Segment", seg, new Vector3(k_CageR, 0f, -0.050f), new Vector3(0.009f, 0.047f, 0.009f), "Lab_Metal");
            }

            // Wires bowing back from the rim to a rear hub.
            for (var i = 0; i < 12; i++)
            {
                var spoke = Group($"Wire {i + 1}", cage, Vector3.zero, new Vector3(0f, 0f, i * 30f));
                Box("Run", spoke, new Vector3(0.098f, 0f, 0.025f), new Vector3(0.202f, 0.006f, 0.006f), "Lab_Metal", new Vector3(0f, 48f, 0f));
            }

            Cyl("Rear Hub", cage, new Vector3(0f, 0f, 0.108f), new Vector3(0.062f, 0.006f, 0.062f), "Lab_Metal", new Vector3(90f, 0f, 0f));
        }

        /// <summary>
        /// Service bay on the motor housing's participant-facing side: cover swung
        /// open on its hinge, fuse board and wiring exposed inside. It shows *where*
        /// service happens without saying what is wrong.
        /// </summary>
        static void BuildServiceBay(Transform head)
        {
            // Opening resized around the part it exists to show. It was 100 x 128 mm,
            // built when the fuse in it was 84 mm long; against a real 30 mm cartridge
            // that compartment read as an empty locker with something small dropped in
            // it, and scale is most of what tells a viewer what they are looking at.
            // 72 x 92 mm is an appliance terminal box holding one 6 x 30 carrier.
            const float h = 0.036f;   // half height of the opening
            const float d = 0.046f;   // half depth

            var bay = Group("Service Bay", head, new Vector3(k_BayX, -0.006f, 0.074f));

            // A moulded service blister standing proud of the motor barrel, not a
            // recess sunk into it.
            //
            // The recess could never be seen: a cavity was modelled *inside* the
            // barrel's radius, and the barrel is a solid primitive cylinder, so from
            // outside there was nothing there but cylinder. Standing the compartment
            // off the housing puts its walls, its opening and its contents in front
            // of the barrel where a participant can actually look into them — and it
            // is what a real appliance's terminal box looks like anyway.
            Box("Bay Back", bay, new Vector3(0.020f, 0f, 0f), new Vector3(0.006f, h * 2f, d * 2f), "Lab_Navy");
            Box("Bay Wall Top", bay, new Vector3(0.002f, h, 0f), new Vector3(0.042f, 0.006f, d * 2f), "Lab_Navy");
            Box("Bay Wall Bottom", bay, new Vector3(0.002f, -h, 0f), new Vector3(0.042f, 0.006f, d * 2f), "Lab_Navy");
            Box("Bay Wall Front", bay, new Vector3(0.002f, 0f, -d), new Vector3(0.042f, h * 2f, 0.006f), "Lab_Navy");
            Box("Bay Wall Rear", bay, new Vector3(0.002f, 0f, d), new Vector3(0.042f, h * 2f, 0.006f), "Lab_Navy");

            // Frame around the opening, built as four edges. As one solid plate it
            // did exactly what a plate does: it covered the compartment.
            Box("Lip Top", bay, new Vector3(-0.019f, h + 0.003f, 0f), new Vector3(0.006f, 0.010f, d * 2f + 0.014f), "Lab_MetalDark");
            Box("Lip Bottom", bay, new Vector3(-0.019f, -h - 0.003f, 0f), new Vector3(0.006f, 0.010f, d * 2f + 0.014f), "Lab_MetalDark");
            Box("Lip Front", bay, new Vector3(-0.019f, 0f, -d - 0.003f), new Vector3(0.006f, h * 2f + 0.014f, 0.010f), "Lab_MetalDark");
            Box("Lip Rear", bay, new Vector3(-0.019f, 0f, d + 0.003f), new Vector3(0.006f, h * 2f + 0.014f, 0.010f), "Lab_MetalDark");

            // Fuse board on the back wall, with its two supply tracks.
            Box("Fuse Board", bay, new Vector3(0.013f, -0.004f, 0f), new Vector3(0.004f, 0.052f, 0.074f), "Lab_Pcb");
            Box("Board Track A", bay, new Vector3(0.010f, 0.011f, 0f), new Vector3(0.003f, 0.004f, 0.062f), "Lab_Copper");
            Box("Board Track B", bay, new Vector3(0.010f, -0.019f, 0f), new Vector3(0.003f, 0.004f, 0.062f), "Lab_Copper");
            // Terminal strip, with its two clamp screws so it reads as a terminal strip.
            // As a bare 22 mm cream box in the compartment's corner it was just another
            // pale block competing with the cartridge for attention.
            Box("Terminal Block", bay, new Vector3(0.008f, -0.021f, 0.030f), new Vector3(0.012f, 0.010f, 0.018f), "Lab_ConnectorWhite");
            foreach (var z in new[] { -0.005f, 0.005f })
                Cyl("Terminal Screw", bay, new Vector3(0.004f, -0.016f, 0.030f + z), new Vector3(0.005f, 0.0006f, 0.005f), "Lab_MetalDark");
            Box("Bay Label", bay, new Vector3(-0.021f, h + 0.010f, 0.022f), new Vector3(0.004f, 0.009f, 0.046f), "Lab_LabelPlate");

            // Cover hinged on its lower edge and dropped clear.
            //
            // Hinging it on the front edge and swinging it outward put the panel
            // directly across the opening from the side a participant approaches on,
            // so the compartment they are meant to inspect was behind its own door.
            // Hanging down, it still reads as "this cover was opened" and blocks
            // nothing.
            var cover = Group("Service Cover", bay, new Vector3(-0.018f, -h - 0.010f, 0f), new Vector3(0f, 0f, -14f));
            Box("Panel", cover, new Vector3(0f, -0.040f, 0f), new Vector3(0.005f, 0.080f, d * 2f + 0.006f), "Lab_Navy");
            Box("Panel Rib", cover, new Vector3(-0.004f, -0.040f, 0f), new Vector3(0.004f, 0.012f, 0.080f), "Lab_MetalDark");
            Cyl("Hinge", cover, Vector3.zero, new Vector3(0.008f, 0.048f, 0.008f), "Lab_MetalDark", new Vector3(90f, 0f, 0f));
            Box("Latch Tab", cover, new Vector3(0f, -0.082f, 0f), new Vector3(0.006f, 0.012f, 0.016f), "Lab_MetalDark");
        }

        /// <summary>Base-mounted control pod. Only the rocker is an interactable, so it is the only raised control.</summary>
        static void BuildControlPod(Transform visual)
        {
            var pod = Group("Control Pod", visual, new Vector3(0f, 0.068f, -0.108f), new Vector3(-16f, 0f, 0f));
            Box("Pod Body", pod, Vector3.zero, new Vector3(0.190f, 0.030f, 0.086f), "Lab_PlasticLight");
            Box("Pod Face", pod, new Vector3(0.026f, 0.016f, 0f), new Vector3(0.118f, 0.003f, 0.070f), "Lab_Navy");

            // Printed legend only: no decorative buttons next to the real switch. The
            // switch sits to the pod's left, so the legend is pushed right far enough
            // that the slider plate cannot cover its first character — with the plate
            // at its old width the legend read "FF 1 2 3".
            var legend = Label("Speed Legend", pod, new Vector3(0.044f, 0.019f, -0.012f), "O F F    1    2    3", 0.14f,
                "#DFE6EE", new Vector3(90f, 0f, 0f), 0.096f);
            legend.characterSpacing = 2f;

            Box("Legend Rule", pod, new Vector3(0.044f, 0.018f, 0.014f), new Vector3(0.090f, 0.002f, 0.002f), "Lab_Accent");
        }

        /// <summary>
        /// Reparents the task interactables onto the fan and rebuilds their visuals.
        /// StableObjectIds, colliders and components are untouched — only the child
        /// "Visual" group and the local transform change.
        /// </summary>
        static void PlaceFanInteractables(Transform body)
        {
            // --- propeller: the interactable IS the blades, so focusing it lights the blades ---
            var blade = GameObject.Find("Fan Blade");
            if (blade != null)
            {
                blade.transform.SetParent(body, true);
                blade.transform.localPosition = new Vector3(0f, k_HeadY, -0.062f);
                blade.transform.localRotation = Quaternion.identity;
                blade.transform.localScale = Vector3.one;

                var bv = ResetVisual("Fan Blade", out var bladeGo);
                if (bv != null)
                {
                    Cyl("Hub", bv, new Vector3(0f, 0f, 0.014f), new Vector3(0.080f, 0.026f, 0.080f), "Lab_MetalDark", new Vector3(90f, 0f, 0f));
                    Cyl("Hub Collar", bv, new Vector3(0f, 0f, -0.010f), new Vector3(0.062f, 0.008f, 0.062f), "Lab_PlasticLight", new Vector3(90f, 0f, 0f));
                    Cyl("Spinner", bv, new Vector3(0f, 0f, -0.026f), new Vector3(0.048f, 0.010f, 0.048f), "Lab_Accent", new Vector3(90f, 0f, 0f));

                    // Five rounded paddles leave clear gaps between blades. The prior
                    // overlapping boxes read as a cracked white disc — an accidental
                    // damage cue on a fan whose fault is elsewhere.
                    for (var i = 0; i < 5; i++)
                    {
                        var arm = Group($"Paddle {i + 1}", bv, Vector3.zero, new Vector3(0f, 0f, i * 72f));
                        var paddle = Sphere("Blade", arm, new Vector3(0.092f, 0f, 0.008f), 1f, "Lab_PlasticLight");
                        paddle.transform.localRotation = Quaternion.Euler(0f, 0f, 16f);
                        paddle.transform.localScale = new Vector3(0.142f, 0.066f, 0.010f);
                    }
                }
                SetCollider(bladeGo, new Vector3(k_BladeTip * 2f, k_BladeTip * 2f, 0.070f));
            }

            // --- fault site: holder on the bay's back wall, cartridge lying along the
            //     bay so its glass faces the opening; wiring below it ---
            // Re-seated for the resized bay: the carrier on the board's centre line, the
            // wiring below it, the cover screw on the lip's rear corner. Every collider
            // keeps the size and the owner it had.
            Reparent("Fan Fuse Holder", body, new Vector3(k_BayX + 0.006f, k_HeadY - 0.002f, 0.074f), new Vector3(0f, 90f, 0f));
            Reparent("Fan Internal Wire", body, new Vector3(k_BayX + 0.006f, k_HeadY - 0.028f, 0.096f), new Vector3(0f, 90f, 0f));
            Reparent("Fan Fastener", body, new Vector3(k_BayX - 0.021f, k_HeadY + 0.030f, 0.112f), new Vector3(0f, 90f, 0f));

            // --- power rocker on the control pod, left of the printed legend ---
            Reparent("Fan Power Switch", body, new Vector3(-0.056f, 0.086f, -0.140f), new Vector3(-16f, 0f, 0f));
            var sw = ResetVisual("Fan Power Switch", out var swGo);
            if (sw != null)
            {
                // The XRI example slider mesh: a plate with a handle standing in a
                // travel slot. Three flat boxes could not show travel, so the control
                // that selects OFF-1-2-3 looked like a moulding, and the printed legend
                // beside it had nothing to belong to. Mesh only — XRSlider brings its
                // own value state and would compete with ResearchInteractable.
                Box("Switch Plate", sw, new Vector3(0f, -0.004f, 0f), new Vector3(0.058f, 0.004f, 0.040f), "Lab_PlasticDark");
                var travel = ImportedVisual("Slide Switch", sw, k_XriControls + "Slider.fbx",
                    new Vector3(0f, 0.004f, 0f), new Vector3(0.050f, 0.018f, 0.050f));
                if (travel != null)
                {
                    PaintNamed(travel, "SliderPlate", "Lab_PlasticDark");
                    PaintNamed(travel, "Slider", "Lab_PlasticLight");
                }
                SetCollider(swGo, new Vector3(0.08f, 0.05f, 0.06f));
            }

            var fastener = ResetVisual("Fan Fastener", out var fGo);
            if (fastener != null)
            {
                // 9 mm pan head with a captive washer, not the 22 mm disc it was. On a
                // 72 mm compartment a 22 mm screw is the size of the fuse behind it,
                // which made the cover screw the biggest thing in the bay.
                Cyl("Washer", fastener, new Vector3(0f, 0f, 0.0016f), new Vector3(0.011f, 0.0006f, 0.011f), "Lab_MetalDark", new Vector3(90f, 0f, 0f));
                Cyl("Screw Head", fastener, Vector3.zero, new Vector3(0.009f, 0.0018f, 0.009f), "Lab_ToolSteel", new Vector3(90f, 0f, 0f));
                Box("Slot", fastener, new Vector3(0f, 0f, -0.0018f), new Vector3(0.0070f, 0.0012f, 0.0010f), "Lab_MetalDark");
                SetCollider(fGo, new Vector3(0.04f, 0.04f, 0.03f));
            }
        }

        static void PlaceFanParts()
        {
            // --- spares tray: the one replacement the repair needs ---
            // Centred in the tray now that it is the only thing in it; it used to sit
            // at the tray's left end with a twin beside it.
            Move("Working Replacement Fuse", new Vector3(-1.10f, BenchDressing.TrayFloor + 0.0092f, 0.95f), new Vector3(0f, 10f, 0f));
            var good = ResetVisual("Working Replacement Fuse", out var goodGo);
            if (good != null)
            {
                BuildFuse(good, intact: true);
                SetCollider(goodGo, new Vector3(0.11f, 0.05f, 0.05f));
            }

            Move("Faulty Fuse", new Vector3(-0.86f, k_BenchTop + 0.0062f, 0.95f), new Vector3(0f, -8f, 0f));
            var bad = ResetVisual("Faulty Fuse", out var badGo);
            if (bad != null)
            {
                BuildFuse(bad, intact: false);
                SetCollider(badGo, new Vector3(0.11f, 0.05f, 0.05f));
            }

            // --- front guard: stood upright on the lower shelf, so it reads as the
            //     cage that came off this fan rather than a white dinner plate ---
            // Out from behind the bench leg and turned to face the participant: tucked
            // in the corner it read as a leftover asset rather than this fan's cage.
            Move("Fan Front Cover", new Vector3(-0.72f, 0.560f, 0.98f), new Vector3(0f, 24f, 0f));
            var cover = ResetVisual("Fan Front Cover", out var coverGo);
            if (cover != null)
            {
                BuildGuardCage(cover);
                SetCollider(coverGo, new Vector3(0.38f, 0.38f, 0.06f));
            }

            // --- motor module in the spares zone ---
            Move("Fan Motor Module", new Vector3(-1.01f, k_BenchTop + 0.062f, 1.15f), new Vector3(0f, -16f, 0f));
            var motor = ResetVisual("Fan Motor Module", out var motorGo);
            if (motor != null)
            {
                Cyl("Can", motor, Vector3.zero, new Vector3(0.130f, 0.058f, 0.130f), "Lab_MetalDark", new Vector3(0f, 0f, 90f));
                for (var i = 0; i < 10; i++)
                    Box($"Lamination {i + 1}", motor, new Vector3(-0.050f + i * 0.011f, 0f, 0f), new Vector3(0.004f, 0.126f, 0.126f), "Lab_Metal");
                Cyl("Shaft", motor, new Vector3(0.085f, 0f, 0f), new Vector3(0.016f, 0.030f, 0.016f), "Lab_ToolSteel", new Vector3(0f, 0f, 90f));
                Box("Terminal Block", motor, new Vector3(0f, 0.062f, 0f), new Vector3(0.050f, 0.020f, 0.032f), "Lab_ConnectorWhite");
                Box("Lead A", motor, new Vector3(-0.010f, 0.078f, 0f), new Vector3(0.008f, 0.030f, 0.008f), "Lab_CableBlack", new Vector3(12f, 0f, 0f));
                Box("Lead B", motor, new Vector3(0.010f, 0.078f, 0f), new Vector3(0.008f, 0.030f, 0.008f), "Lab_CableRed", new Vector3(-12f, 0f, 0f));
                SetCollider(motorGo, new Vector3(0.18f, 0.15f, 0.15f));
            }

            // --- fuse holder: an open cartridge carrier with the fitted fuse seated in
            //     its spring clips.
            //
            //     The fitted fuse is the evidence. Its element is broken and its glass
            //     lightly stained, but the holder, the cartridge body and the printed
            //     rating are identical to the good spare in the tray, so nothing about
            //     it is legible until a participant leans in. That is the diagnosis
            //     the task is measuring; announcing it with a red part would remove
            //     the task. ---
            var holder = ResetVisual("Fan Fuse Holder", out var holderGo);
            if (holder != null)
            {
                // An open clip carrier sized round a 30 mm cartridge, and built to say
                // that both halves come apart: the fuse lifts out of two sprung clips
                // whose lips stand above it, and the carrier itself unbolts from the
                // board on two slotted screws with a moulded pull tab between them.
                // The old holder was 92 mm of base under a 84 mm fuse, with no screws
                // and no lips, so nothing about it suggested either could be removed.
                Box("Base", holder, new Vector3(0f, -0.0075f, 0.0035f), new Vector3(0.044f, 0.007f, 0.014f), "Lab_PlasticDark");
                Box("Base Rib", holder, new Vector3(0f, -0.0110f, 0.0035f), new Vector3(0.048f, 0.0025f, 0.018f), "Lab_PlasticDark");
                Box("Pull Tab", holder, new Vector3(0f, -0.0075f, -0.0075f), new Vector3(0.012f, 0.006f, 0.0080f), "Lab_PlasticDark");

                foreach (var side in new[] { -1f, 1f })
                {
                    var tag = side < 0 ? "A" : "B";
                    Box($"Clip {tag}", holder, new Vector3(side * 0.0125f, -0.0005f, 0f), new Vector3(0.0045f, 0.0110f, 0.0090f), "Lab_Metal");
                    // The lip that has to be sprung past to take the cartridge out.
                    Box($"Clip Lip {tag}", holder, new Vector3(side * 0.0125f, 0.0048f, 0f), new Vector3(0.0045f, 0.0030f, 0.0125f), "Lab_Metal", new Vector3(side * 16f, 0f, 0f));
                    Cyl($"Mount Screw {tag}", holder, new Vector3(side * 0.0185f, -0.0040f, 0.0035f), new Vector3(0.0050f, 0.0008f, 0.0050f), "Lab_ToolSteel");
                    Box($"Screw Slot {tag}", holder, new Vector3(side * 0.0185f, -0.0032f, 0.0035f), new Vector3(0.0038f, 0.0006f, 0.0009f), "Lab_MetalDark");
                }

                // The evidence. Same builder as both spares, so the fitted cartridge is
                // the identical part with the identical printed band; only its element
                // has parted, and only from inspection distance.
                BuildFuse(Group("Fitted Fuse", holder, new Vector3(0f, 0.0005f, 0f)), intact: false);

                // ponytail: no separate rating plate. It was a blank white card lying
                // under the base with nothing printed on it, and the compartment already
                // carries a label and the cartridge its own printed band.
                SetCollider(holderGo, new Vector3(0.11f, 0.05f, 0.05f));
            }

            // --- exposed internal wiring from the holder down to the motor ---
            var wire = ResetVisual("Fan Internal Wire", out var wireGo);
            if (wire != null)
            {
                // Shortened to the new bay: 60 mm runs stood taller than the compartment
                // they live in once it came down to appliance size.
                // Two leads running up from the sleeve to the carrier's terminals,
                // close together and near-vertical. Splayed at 18 degrees over 60 mm
                // they read as two loose sticks crossing the compartment; the white
                // cable tie that sat on top of them was one more bright block in a bay
                // that already had a label and a rating plate in it.
                Box("Run A", wire, new Vector3(-0.005f, 0.011f, 0f), new Vector3(0.0050f, 0.030f, 0.0050f), "Lab_CableBlack", new Vector3(0f, 0f, 7f));
                Box("Run B", wire, new Vector3(0.005f, 0.010f, 0.002f), new Vector3(0.0050f, 0.028f, 0.0050f), "Lab_CableRed", new Vector3(0f, 0f, -7f));
                Box("Sleeve", wire, new Vector3(0f, -0.008f, 0.001f), new Vector3(0.014f, 0.010f, 0.010f), "Lab_PlasticDark");
                SetCollider(wireGo, new Vector3(0.06f, 0.10f, 0.05f));
            }

            // --- mains lead: one object read as two.
            //
            //     The plug used to stand 0.32 m clear of its own coil, so the bench
            //     showed a loose plug and a separate loop of wire — two more parts to
            //     fit. It now sits at the coil's edge with a tail running into it, and
            //     the coil's other end runs to the fan's cord gland, so the whole thing
            //     reads as this fan's lead, unplugged. ---
            Move("Fan Power Plug", new Vector3(0.404f, k_BenchTop + 0.022f, 1.236f), new Vector3(0f, 20f, 0f));
            var plug = ResetVisual("Fan Power Plug", out var plugGo);
            if (plug != null)
            {
                // A moulded mains plug rather than a brick with three tabs: a chamfered
                // body, a finger grip, insulated pin shanks and a strain-relief boot
                // where the flex enters. The shanks are what say "mains plug" — bare
                // metal tabs on a black box read as a bracket.
                Box("Body", plug, Vector3.zero, new Vector3(0.050f, 0.030f, 0.032f), "Lab_PlasticDark");
                Box("Face", plug, new Vector3(0f, 0f, -0.017f), new Vector3(0.044f, 0.026f, 0.004f), "Lab_PlasticDark");
                Box("Grip", plug, new Vector3(0f, 0f, 0.016f), new Vector3(0.038f, 0.024f, 0.014f), "Lab_PlasticDark");
                for (var i = 0; i < 3; i++)
                {
                    var x = -0.013f + i * 0.013f;
                    var earth = i == 1;
                    Box($"Pin Shank {i + 1}", plug, new Vector3(x, earth ? -0.008f : 0.004f, -0.023f), new Vector3(0.0055f, 0.0075f, 0.008f), "Lab_PlasticDark");
                    Box($"Prong {i + 1}", plug, new Vector3(x, earth ? -0.008f : 0.004f, -0.031f), new Vector3(0.0042f, 0.0068f, 0.014f), "Lab_ToolSteel");
                }
                // Strain-relief boot, then the flex back into the coil, so the plug is
                // the end of one lead and not a second loose part.
                Box("Boot", plug, new Vector3(-0.030f, -0.004f, 0.006f), new Vector3(0.022f, 0.016f, 0.016f), "Lab_PlasticDark", new Vector3(0f, -14f, 0f));
                Box("Tail", plug, new Vector3(-0.052f, -0.006f, 0.011f), new Vector3(0.030f, 0.008f, 0.008f), "Lab_CableBlack", new Vector3(0f, -14f, 0f));
                SetCollider(plugGo, new Vector3(0.09f, 0.06f, 0.09f));
            }

            Move("Fan Power Cord", new Vector3(0.30f, k_BenchTop + 0.012f, 1.24f), Vector3.zero);
            var cord = ResetVisual("Fan Power Cord", out var cordGo);
            if (cord != null)
            {
                // Three flat discs at three radii, which is what this was, render as a
                // stack of plates. A cable coil has a hole in it: these are rings of
                // short tangential segments, so light passes between the turns and the
                // thing reads as flex laid in loops.
                Coil("Coil Outer", cord, 0.086f, 0.000f, 22, 0.0070f, 4f);
                Coil("Coil Mid", cord, 0.066f, 0.007f, 18, 0.0070f, -6f);
                Coil("Coil Inner", cord, 0.046f, 0.014f, 14, 0.0070f, 10f);
                // Run back toward the fan's cord gland so the lead belongs to the fan.
                Box("Run To Fan", cord, new Vector3(-0.130f, 0.001f, -0.020f), new Vector3(0.100f, 0.0070f, 0.0070f), "Lab_CableBlack", new Vector3(0f, 14f, 0f));
                SetCollider(cordGo, new Vector3(0.22f, 0.06f, 0.22f));
            }

            // --- sealed spare, deliberately not part of this repair ---
            Move("Fan Non Target Module", new Vector3(1.58f, k_BenchTop + 0.052f, 0.98f), new Vector3(0f, -12f, 0f));
            var nonTarget = ResetVisual("Fan Non Target Module", out var nonTargetGo);
            if (nonTarget != null)
            {
                Box("Body", nonTarget, Vector3.zero, new Vector3(0.150f, 0.090f, 0.110f), "Lab_MetalDark");
                Box("Label", nonTarget, new Vector3(0f, 0.046f, 0f), new Vector3(0.110f, 0.002f, 0.070f), "Lab_LabelPlate");
                Box("Seal", nonTarget, new Vector3(0f, 0.047f, 0.030f), new Vector3(0.060f, 0.002f, 0.016f), "Lab_Warning");
                SetCollider(nonTargetGo, new Vector3(0.16f, 0.10f, 0.12f));
            }

            // --- status lamp, same fixture as the computer bench ---
            Move("Fan Status Indicator", new Vector3(LabLayoutBuilder.TestStationX, k_BenchTop + 0.090f, 1.05f), Vector3.zero);
            var status = ResetVisual("Fan Status Indicator", out var statusGo);
            if (status != null)
            {
                Box("Base", status, new Vector3(0f, -0.082f, 0f), new Vector3(0.090f, 0.016f, 0.090f), "Lab_Navy");
                Cyl("Stem", status, new Vector3(0f, -0.045f, 0f), new Vector3(0.018f, 0.038f, 0.018f), "Lab_MetalDark");
                Cyl("Lamp Housing", status, Vector3.zero, new Vector3(0.070f, 0.045f, 0.070f), "Lab_PlasticDark");
                Cyl("Lamp Lens", status, new Vector3(0f, 0.038f, 0f), new Vector3(0.056f, 0.010f, 0.056f), "Lab_StatusRed");
                SetCollider(statusGo, new Vector3(0.10f, 0.20f, 0.10f));
            }
        }

        /// <summary>
        /// One turn of laid cable, as tangential segments round a circle. Flat
        /// primitives cannot draw a coil: a cylinder is a solid disc and reads as a
        /// disc, and it is the gap in the middle that makes a loop of flex legible.
        /// </summary>
        static void Coil(string name, Transform parent, float radius, float y, int segments, float thickness, float startAngle)
        {
            var ring = Group(name, parent, new Vector3(0f, y, 0f));
            var chord = 2f * Mathf.PI * radius / segments * 1.18f;
            for (var i = 0; i < segments; i++)
            {
                var seg = Group($"Turn {i + 1}", ring, Vector3.zero, new Vector3(0f, startAngle + i * (360f / segments), 0f));
                // Long axis X: after the group's yaw that is the tangent. Putting the
                // length on Z instead pointed every segment straight out of the circle,
                // which drew a cog rather than a coil.
                Box("Segment", seg, new Vector3(0f, 0f, radius), new Vector3(chord, thickness, thickness), "Lab_CableBlack");
            }
        }

        /// <summary>Inert lower-shelf rack that makes the removed guard look deliberately stored.</summary>
        static void BuildRemovedPartsRack()
        {
            var rack = Root("Fan Removed Parts Rack", new Vector3(-0.72f, 0.365f, 0.98f));
            var t = rack.transform;
            Box("Shelf Pad", t, Vector3.zero, new Vector3(0.48f, 0.018f, 0.30f), "Lab_Rubber");
            Box("Rear Rail", t, new Vector3(0f, 0.105f, 0.110f), new Vector3(0.44f, 0.20f, 0.018f), "Lab_MetalDark");
            Box("Left Cradle", t, new Vector3(-0.19f, 0.055f, 0f), new Vector3(0.025f, 0.11f, 0.22f), "Lab_MetalDark");
            Box("Right Cradle", t, new Vector3(0.19f, 0.055f, 0f), new Vector3(0.025f, 0.11f, 0.22f), "Lab_MetalDark");
            // Small placard on the rack's front edge, the same as the bench trays. This
            // was 0.24 font across a 0.44 box — signage painted on the shelf.
            BenchDressing.Zone(t, new Vector3(0f, 0.020f, -0.152f), "REMOVED PARTS");
        }

        /// <summary>The removed front cage: a wire guard, built as a ring rather than a disc.</summary>
        static void BuildGuardCage(Transform root)
        {
            const float rim = 0.178f;

            for (var i = 0; i < 26; i++)
            {
                var seg = Group($"Rim {i + 1}", root, Vector3.zero, new Vector3(0f, 0f, i * (360f / 26f)));
                Box("Segment", seg, new Vector3(rim, 0f, 0f), new Vector3(0.011f, 0.046f, 0.014f), "Lab_Metal");
            }

            for (var i = 0; i < 20; i++)
            {
                var seg = Group($"Inner Ring {i + 1}", root, Vector3.zero, new Vector3(0f, 0f, i * 18f));
                Box("Segment", seg, new Vector3(0.098f, 0f, 0.008f), new Vector3(0.006f, 0.032f, 0.006f), "Lab_Metal");
            }

            // Radial wires run right across, so twelve bars draw twenty-four spokes.
            for (var i = 0; i < 12; i++)
                Box($"Spoke {i + 1}", root, new Vector3(0f, 0f, 0.010f), new Vector3(rim * 2f, 0.005f, 0.005f), "Lab_Metal", new Vector3(0f, 0f, i * 15f));

            Cyl("Hub Cap", root, new Vector3(0f, 0f, 0.018f), new Vector3(0.072f, 0.008f, 0.072f), "Lab_PlasticLight", new Vector3(90f, 0f, 0f));
            Box("Clip A", root, new Vector3(0f, rim, -0.006f), new Vector3(0.026f, 0.024f, 0.020f), "Lab_MetalDark");
            Box("Clip B", root, new Vector3(-0.154f, -0.089f, -0.006f), new Vector3(0.026f, 0.024f, 0.020f), "Lab_MetalDark");
            Box("Clip C", root, new Vector3(0.154f, -0.089f, -0.006f), new Vector3(0.026f, 0.024f, 0.020f), "Lab_MetalDark");
        }

        // --- the cartridge fuse --------------------------------------------------
        //
        // A 6 x 30 mm glass cartridge: 30 mm end to end, 6.3 mm across the caps, about
        // 18 mm of glass between them. Every fuse in this scene is built from these
        // numbers - the spare in the tray, the stowed spare, and the one fitted in the
        // holder - so the participant is comparing three copies of one part.
        //
        // They used to be 84 mm long and 26 mm across: a cylinder the size of a torch
        // battery, which is what pilot readers called it. Nothing at that scale can be
        // matched against a 6 x 30 rating printed in a manual.

        const float k_FuseLength = 0.030f;
        const float k_FuseGlassR = 0.0030f;   // 6 mm glass tube
        const float k_FuseCapR = 0.0032f;     // 6.3 mm ferrule
        const float k_FuseCapLength = 0.006f;
        const float k_FuseElement = 0.0007f;  // element wire, drawn at the thickness it can be seen at

        /// <summary>
        /// Cartridge fuse. Both fuses carry the same glass, the same ferrules and the
        /// same printed rating band, so from across the bench they are one part seen
        /// twice — which is the point. The element is the only difference and it is
        /// only legible close up: an unbroken wire, or two stubs with a gap between
        /// them. No stain, no discolouration, no second cue; a participant who can name
        /// the blown one from the far side of the bench has not diagnosed anything.
        /// </summary>
        static void BuildFuse(Transform root, bool intact)
        {
            var glassLength = k_FuseLength - 2f * k_FuseCapLength + 0.004f;
            Cyl("Glass", root, Vector3.zero, new Vector3(k_FuseGlassR * 2f, glassLength * 0.5f, k_FuseGlassR * 2f), "Lab_FuseGlass", new Vector3(0f, 0f, 90f));

            // ponytail: two ferrules, no crimp rings. At 6 mm a crimp step is under a
            // pixel at any distance a participant can hold the part.
            foreach (var side in new[] { -1f, 1f })
                Cyl($"Cap {(side < 0 ? "A" : "B")}", root, new Vector3(side * (k_FuseLength - k_FuseCapLength) * 0.5f, 0f, 0f),
                    new Vector3(k_FuseCapR * 2f, k_FuseCapLength * 0.5f, k_FuseCapR * 2f), "Lab_Metal", new Vector3(0f, 0f, 90f));

            // Rating stamped on one ferrule, which is where a 6 x 30 actually carries it
            // — and, more to the point, not across the middle of the glass. Printed as a
            // band round the centre it lay exactly over the element, so the one feature
            // that distinguishes a good fuse from a blown one was behind the label on
            // both of them.
            Box("Rating Print", root, new Vector3(-(k_FuseLength - k_FuseCapLength) * 0.5f, k_FuseCapR * 0.94f, 0f),
                new Vector3(0.0050f, 0.0004f, 0.0038f), "Lab_LabelPlate");

            var reach = (k_FuseLength - 2f * k_FuseCapLength) * 0.5f + k_FuseCapLength * 0.5f;
            if (intact)
            {
                Box("Element", root, Vector3.zero, new Vector3(reach * 2f, k_FuseElement, k_FuseElement), "Lab_ToolSteel");
                return;
            }

            // Blown: the wire has parted near the middle. 3.4 mm of gap is what a
            // 6 x 30 actually leaves, and it is the only thing that distinguishes this
            // fuse from the good one.
            const float gap = 0.0034f;
            var stub = reach - gap * 0.5f;
            foreach (var side in new[] { -1f, 1f })
                Box($"Element Stub {(side < 0 ? "A" : "B")}", root, new Vector3(side * (gap * 0.5f + stub * 0.5f), 0f, 0f),
                    new Vector3(stub, k_FuseElement, k_FuseElement), "Lab_ToolSteel");
        }

        static void Reparent(string name, Transform parent, Vector3 localPosition, Vector3 euler)
        {
            var go = FindAny(name);
            if (go == null)
            {
                Debug.LogWarning($"[FanWorkstation] missing {name}");
                return;
            }
            go.transform.SetParent(parent, true);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = Vector3.one;
        }

        static void Move(string name, Vector3 position, Vector3 euler)
        {
            var go = FindAny(name);
            if (go == null)
            {
                Debug.LogWarning($"[FanWorkstation] missing {name}");
                return;
            }
            go.transform.SetParent(null, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            go.transform.localScale = Vector3.one;
        }

        static void SetCollider(GameObject go, Vector3 size)
        {
            if (go == null)
                return;
            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                return;
            box.center = Vector3.zero;
            box.size = size;
        }
    }
}
