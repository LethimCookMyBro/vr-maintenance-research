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
    /// identity now: a six-paddle propeller whose paddles are offset from the hub so
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

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Fan Workstation")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ResearchSceneSet.Fan, OpenSceneMode.Single);

            BuildFanDevice();
            PlaceFanParts();
            BenchDressing.PlaceScrewdriver(new Vector3(1.02f, k_BenchTop + 0.035f, 0.95f));
            BenchDressing.Build(0f, 0.86f, 0.62f);
            BenchDressing.PlaceInspectControl("Fan Speed Selector");
            TaskBriefBuilder.BuildFan();

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
            var bay = Group("Service Bay", head, new Vector3(k_BayX, -0.010f, 0.074f));

            // A moulded service blister standing proud of the motor barrel, not a
            // recess sunk into it.
            //
            // The recess could never be seen: a cavity was modelled *inside* the
            // barrel's radius, and the barrel is a solid primitive cylinder, so from
            // outside there was nothing there but cylinder. Standing the compartment
            // off the housing puts its walls, its opening and its contents in front
            // of the barrel where a participant can actually look into them — and it
            // is what a real appliance's terminal box looks like anyway.
            Box("Bay Back", bay, new Vector3(0.020f, 0f, 0f), new Vector3(0.006f, 0.100f, 0.128f), "Lab_Navy");
            Box("Bay Wall Top", bay, new Vector3(0.002f, 0.050f, 0f), new Vector3(0.042f, 0.006f, 0.128f), "Lab_Navy");
            Box("Bay Wall Bottom", bay, new Vector3(0.002f, -0.050f, 0f), new Vector3(0.042f, 0.006f, 0.128f), "Lab_Navy");
            Box("Bay Wall Front", bay, new Vector3(0.002f, 0f, -0.064f), new Vector3(0.042f, 0.100f, 0.006f), "Lab_Navy");
            Box("Bay Wall Rear", bay, new Vector3(0.002f, 0f, 0.064f), new Vector3(0.042f, 0.100f, 0.006f), "Lab_Navy");

            // Frame around the opening, built as four edges. As one solid plate it
            // did exactly what a plate does: it covered the compartment.
            Box("Lip Top", bay, new Vector3(-0.019f, 0.053f, 0f), new Vector3(0.006f, 0.012f, 0.142f), "Lab_MetalDark");
            Box("Lip Bottom", bay, new Vector3(-0.019f, -0.053f, 0f), new Vector3(0.006f, 0.012f, 0.142f), "Lab_MetalDark");
            Box("Lip Front", bay, new Vector3(-0.019f, 0f, -0.067f), new Vector3(0.006f, 0.118f, 0.012f), "Lab_MetalDark");
            Box("Lip Rear", bay, new Vector3(-0.019f, 0f, 0.067f), new Vector3(0.006f, 0.118f, 0.012f), "Lab_MetalDark");

            // Fuse board on the back wall, with its two supply tracks.
            Box("Fuse Board", bay, new Vector3(0.013f, -0.006f, 0f), new Vector3(0.004f, 0.070f, 0.100f), "Lab_Pcb");
            Box("Board Track A", bay, new Vector3(0.010f, 0.014f, 0f), new Vector3(0.003f, 0.005f, 0.084f), "Lab_Copper");
            Box("Board Track B", bay, new Vector3(0.010f, -0.026f, 0f), new Vector3(0.003f, 0.005f, 0.084f), "Lab_Copper");
            Box("Terminal Block", bay, new Vector3(0.008f, -0.032f, 0.042f), new Vector3(0.014f, 0.018f, 0.026f), "Lab_ConnectorWhite");
            Box("Bay Label", bay, new Vector3(-0.021f, 0.060f, 0.030f), new Vector3(0.004f, 0.012f, 0.062f), "Lab_LabelPlate");

            // Cover hinged on its lower edge and dropped clear.
            //
            // Hinging it on the front edge and swinging it outward put the panel
            // directly across the opening from the side a participant approaches on,
            // so the compartment they are meant to inspect was behind its own door.
            // Hanging down, it still reads as "this cover was opened" and blocks
            // nothing.
            var cover = Group("Service Cover", bay, new Vector3(-0.018f, -0.058f, 0f), new Vector3(0f, 0f, -14f));
            Box("Panel", cover, new Vector3(0f, -0.052f, 0f), new Vector3(0.005f, 0.104f, 0.128f), "Lab_Navy");
            Box("Panel Rib", cover, new Vector3(-0.004f, -0.052f, 0f), new Vector3(0.004f, 0.014f, 0.110f), "Lab_MetalDark");
            Cyl("Hinge", cover, Vector3.zero, new Vector3(0.010f, 0.064f, 0.010f), "Lab_MetalDark", new Vector3(90f, 0f, 0f));
            Box("Latch Tab", cover, new Vector3(0f, -0.106f, 0f), new Vector3(0.006f, 0.014f, 0.018f), "Lab_MetalDark");
        }

        /// <summary>Base-mounted control pod. Only the rocker is an interactable, so it is the only raised control.</summary>
        static void BuildControlPod(Transform visual)
        {
            var pod = Group("Control Pod", visual, new Vector3(0f, 0.068f, -0.108f), new Vector3(-16f, 0f, 0f));
            Box("Pod Body", pod, Vector3.zero, new Vector3(0.190f, 0.030f, 0.086f), "Lab_PlasticLight");
            Box("Pod Face", pod, new Vector3(0.026f, 0.016f, 0f), new Vector3(0.118f, 0.003f, 0.070f), "Lab_Navy");

            // Printed legend only: no decorative buttons next to the real switch. The
            // rocker sits to the pod's left so it cannot cover its own legend.
            var legend = Label("Speed Legend", pod, new Vector3(0.030f, 0.019f, -0.012f), "O F F    1    2    3", 0.15f,
                "#DFE6EE", new Vector3(90f, 0f, 0f), 0.11f);
            legend.characterSpacing = 2f;

            Box("Legend Rule", pod, new Vector3(0.026f, 0.018f, 0.014f), new Vector3(0.100f, 0.002f, 0.002f), "Lab_Accent");
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
            Reparent("Fan Fuse Holder", body, new Vector3(k_BayX + 0.002f, k_HeadY + 0.008f, 0.074f), new Vector3(0f, 90f, 0f));
            Reparent("Fan Internal Wire", body, new Vector3(k_BayX + 0.004f, k_HeadY - 0.040f, 0.100f), new Vector3(0f, 90f, 0f));
            Reparent("Fan Fastener", body, new Vector3(k_BayX - 0.021f, k_HeadY + 0.042f, 0.130f), new Vector3(0f, 90f, 0f));

            // --- power rocker on the control pod, left of the printed legend ---
            Reparent("Fan Power Switch", body, new Vector3(-0.056f, 0.086f, -0.140f), new Vector3(-16f, 0f, 0f));
            var sw = ResetVisual("Fan Power Switch", out var swGo);
            if (sw != null)
            {
                Box("Bezel", sw, Vector3.zero, new Vector3(0.056f, 0.014f, 0.034f), "Lab_PlasticDark");
                Box("Rocker", sw, new Vector3(0f, 0.010f, 0f), new Vector3(0.044f, 0.010f, 0.026f), "Lab_PlasticLight", new Vector3(9f, 0f, 0f));
                Box("Rocker Mark", sw, new Vector3(0f, 0.016f, -0.008f), new Vector3(0.014f, 0.002f, 0.004f), "Lab_Navy", new Vector3(9f, 0f, 0f));
                SetCollider(swGo, new Vector3(0.08f, 0.05f, 0.06f));
            }

            var fastener = ResetVisual("Fan Fastener", out var fGo);
            if (fastener != null)
            {
                Cyl("Screw Head", fastener, Vector3.zero, new Vector3(0.022f, 0.004f, 0.022f), "Lab_ToolSteel", new Vector3(90f, 0f, 0f));
                Box("Slot", fastener, new Vector3(0f, 0f, -0.004f), new Vector3(0.017f, 0.003f, 0.002f), "Lab_MetalDark");
                SetCollider(fGo, new Vector3(0.04f, 0.04f, 0.03f));
            }
        }

        static void PlaceFanParts()
        {
            // --- spares tray: two fuses that look identical until inspected ---
            Move("Working Replacement Fuse", new Vector3(-1.16f, k_BenchTop + 0.042f, 0.95f), new Vector3(0f, 10f, 0f));
            var good = ResetVisual("Working Replacement Fuse", out var goodGo);
            if (good != null)
            {
                BuildFuse(good, intact: true);
                SetCollider(goodGo, new Vector3(0.11f, 0.05f, 0.05f));
            }

            Move("Faulty Fuse", new Vector3(-0.86f, k_BenchTop + 0.042f, 0.95f), new Vector3(0f, -8f, 0f));
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
            Move("Fan Front Cover", new Vector3(-0.72f, 0.522f, 0.98f), new Vector3(-10f, 24f, 0f));
            var cover = ResetVisual("Fan Front Cover", out var coverGo);
            if (cover != null)
            {
                BuildGuardCage(cover);
                SetCollider(coverGo, new Vector3(0.38f, 0.38f, 0.06f));
            }

            // --- motor module in the spares zone ---
            Move("Fan Motor Module", new Vector3(0.60f, k_BenchTop + 0.062f, 1.02f), new Vector3(0f, -16f, 0f));
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
                Box("Base", holder, new Vector3(0f, -0.002f, 0.010f), new Vector3(0.092f, 0.034f, 0.008f), "Lab_PlasticDark");
                Box("Clip Left", holder, new Vector3(-0.033f, 0.004f, -0.002f), new Vector3(0.011f, 0.028f, 0.016f), "Lab_Metal");
                Box("Clip Right", holder, new Vector3(0.033f, 0.004f, -0.002f), new Vector3(0.011f, 0.028f, 0.016f), "Lab_Metal");

                Cyl("Fitted Glass", holder, new Vector3(0f, 0.004f, -0.005f), new Vector3(0.020f, 0.032f, 0.020f), "Lab_FuseGlass", new Vector3(0f, 0f, 90f));
                Cyl("Fitted Cap A", holder, new Vector3(-0.035f, 0.004f, -0.005f), new Vector3(0.022f, 0.009f, 0.022f), "Lab_Metal", new Vector3(0f, 0f, 90f));
                Cyl("Fitted Cap B", holder, new Vector3(0.035f, 0.004f, -0.005f), new Vector3(0.022f, 0.009f, 0.022f), "Lab_Metal", new Vector3(0f, 0f, 90f));
                Box("Fitted Rating", holder, new Vector3(0f, 0.015f, -0.005f), new Vector3(0.028f, 0.002f, 0.010f), "Lab_LabelPlate");
                Box("Element Stub A", holder, new Vector3(-0.017f, 0.004f, -0.005f), new Vector3(0.022f, 0.003f, 0.003f), "Lab_ToolSteel");
                Box("Element Stub B", holder, new Vector3(0.017f, 0.004f, -0.005f), new Vector3(0.022f, 0.003f, 0.003f), "Lab_ToolSteel");
                Box("Stain", holder, new Vector3(0.002f, 0.004f, -0.005f), new Vector3(0.017f, 0.016f, 0.016f), "Lab_Rubber");

                Box("Rating Plate", holder, new Vector3(0f, -0.019f, 0.004f), new Vector3(0.056f, 0.002f, 0.016f), "Lab_LabelPlate");
                SetCollider(holderGo, new Vector3(0.11f, 0.05f, 0.05f));
            }

            // --- exposed internal wiring from the holder down to the motor ---
            var wire = ResetVisual("Fan Internal Wire", out var wireGo);
            if (wire != null)
            {
                Box("Run A", wire, new Vector3(-0.014f, 0.020f, 0f), new Vector3(0.007f, 0.060f, 0.007f), "Lab_CableBlack", new Vector3(0f, 0f, 18f));
                Box("Run B", wire, new Vector3(0.014f, 0.018f, 0.004f), new Vector3(0.007f, 0.056f, 0.007f), "Lab_CableRed", new Vector3(0f, 0f, -16f));
                Box("Sleeve", wire, new Vector3(0f, -0.014f, 0.002f), new Vector3(0.020f, 0.016f, 0.016f), "Lab_PlasticDark");
                Box("Tie", wire, new Vector3(0f, 0.036f, 0f), new Vector3(0.022f, 0.006f, 0.014f), "Lab_ConnectorWhite");
                SetCollider(wireGo, new Vector3(0.06f, 0.10f, 0.05f));
            }

            // --- mains lead: plug in the spares zone, cord coiled behind the fan and
            //     running to the fan's own cord gland ---
            Move("Fan Power Plug", new Vector3(0.62f, k_BenchTop + 0.030f, 1.24f), new Vector3(0f, 20f, 0f));
            var plug = ResetVisual("Fan Power Plug", out var plugGo);
            if (plug != null)
            {
                Box("Body", plug, Vector3.zero, new Vector3(0.056f, 0.038f, 0.034f), "Lab_PlasticDark");
                Box("Grip", plug, new Vector3(0f, 0f, 0.022f), new Vector3(0.040f, 0.028f, 0.018f), "Lab_PlasticDark");
                for (var i = 0; i < 3; i++)
                    Box($"Prong {i + 1}", plug, new Vector3(-0.014f + i * 0.014f, 0.002f, -0.026f), new Vector3(0.005f, 0.014f, 0.020f), "Lab_ToolSteel");
                SetCollider(plugGo, new Vector3(0.09f, 0.06f, 0.09f));
            }

            Move("Fan Power Cord", new Vector3(0.30f, k_BenchTop + 0.016f, 1.24f), Vector3.zero);
            var cord = ResetVisual("Fan Power Cord", out var cordGo);
            if (cord != null)
            {
                Cyl("Coil Outer", cord, Vector3.zero, new Vector3(0.180f, 0.008f, 0.180f), "Lab_CableBlack");
                Cyl("Coil Mid", cord, new Vector3(0.004f, 0.010f, 0.004f), new Vector3(0.135f, 0.008f, 0.135f), "Lab_CableBlack");
                Cyl("Coil Inner", cord, new Vector3(-0.004f, 0.020f, 0.002f), new Vector3(0.095f, 0.008f, 0.095f), "Lab_CableBlack");
                // Run back toward the fan's cord gland so the lead belongs to the fan.
                Box("Run To Fan", cord, new Vector3(-0.135f, 0.004f, -0.020f), new Vector3(0.180f, 0.010f, 0.010f), "Lab_CableBlack", new Vector3(0f, 14f, 0f));
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

        /// <summary>
        /// Cartridge fuse. Both fuses carry the same body, caps and printed rating —
        /// from across the bench they are indistinguishable, which is the point. Only
        /// the element differs, and only close up: intact wire versus two stubs with a
        /// gap and a smoke stain on the inside of the glass.
        /// </summary>
        static void BuildFuse(Transform root, bool intact)
        {
            Cyl("Glass", root, Vector3.zero, new Vector3(0.024f, 0.040f, 0.024f), "Lab_FuseGlass", new Vector3(0f, 0f, 90f));
            Cyl("Cap A", root, new Vector3(-0.042f, 0f, 0f), new Vector3(0.026f, 0.012f, 0.026f), "Lab_Metal", new Vector3(0f, 0f, 90f));
            Cyl("Cap B", root, new Vector3(0.042f, 0f, 0f), new Vector3(0.026f, 0.012f, 0.026f), "Lab_Metal", new Vector3(0f, 0f, 90f));
            Box("Rating Print", root, new Vector3(0f, 0.013f, 0f), new Vector3(0.034f, 0.002f, 0.012f), "Lab_LabelPlate");

            if (intact)
            {
                Box("Element", root, Vector3.zero, new Vector3(0.066f, 0.003f, 0.003f), "Lab_ToolSteel");
                return;
            }

            Box("Element Stub A", root, new Vector3(-0.020f, 0f, 0f), new Vector3(0.026f, 0.003f, 0.003f), "Lab_ToolSteel");
            Box("Element Stub B", root, new Vector3(0.020f, 0f, 0f), new Vector3(0.026f, 0.003f, 0.003f), "Lab_ToolSteel");
            Box("Smoke Stain", root, new Vector3(0.002f, 0f, 0f), new Vector3(0.020f, 0.019f, 0.019f), "Lab_Rubber");
        }

        static void Reparent(string name, Transform parent, Vector3 localPosition, Vector3 euler)
        {
            var go = GameObject.Find(name);
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
            var go = GameObject.Find(name);
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
