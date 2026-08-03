using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Rebuilds the computer-maintenance workstation so it reads as a real PC
    /// repair bench: an open mid-tower angled toward the participant, a populated
    /// motherboard, and the fault (an unplugged 24-pin ATX lead) visible from the
    /// participant's standing pose.
    ///
    /// Only visuals and transforms are touched. Every ResearchInteractable keeps
    /// its component, its StableObjectId and its collider, so task logic and the
    /// research log are unaffected.
    /// </summary>
    public static class ComputerWorkstationBuilder
    {
        const float k_BenchTop = 0.92f;

        // The case is angled so the open side faces the participant while the
        // front bezel stays partly visible — a 3/4 view reads as "computer"
        // faster than either a flat front or a bare open frame.
        // Pulled 0.18 m toward the participant instead of scaling the task hierarchy:
        // component recognition improves while every collider and XR reference keeps
        // its authored dimensions.
        static readonly Vector3 k_CasePos = new Vector3(-0.15f, k_BenchTop + 0.225f, 0.78f);
        const float k_CaseYaw = -70f;
        const float k_W = 0.105f;   // half width  (local x, side panels)
        const float k_H = 0.225f;   // half height (local y)
        const float k_D = 0.225f;   // half depth  (local z, front bezel at -z)
        const string k_Item3D = "Assets/VRMaintenanceResearch/ThirdParty/ITEM_3D/";

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Computer Workstation")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ResearchSceneSet.Computer, OpenSceneMode.Single);

            // The old decorative shell sat behind the real case as a dark arch with
            // a floating green board. The rebuilt case replaces it outright.
            var legacy = GameObject.Find("Visual Desktop PC Assembly");
            if (legacy != null)
                Object.DestroyImmediate(legacy);

            var case_ = PlaceCase();
            BuildCaseShell(case_);
            PlaceInteriorParts(case_);
            PlaceBenchParts();
            BenchDressing.Build(-0.15f);
            BenchDressing.PlaceInspectControl("Computer Power Button");
            TaskBriefBuilder.BuildComputer();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ComputerWorkstation] rebuilt");
        }

        static Transform PlaceCase()
        {
            var go = GameObject.Find("Desktop Case");
            if (go == null)
            {
                Debug.LogError("[ComputerWorkstation] Desktop Case missing");
                return null;
            }

            go.transform.SetPositionAndRotation(k_CasePos, Quaternion.Euler(0f, k_CaseYaw, 0f));
            go.transform.localScale = Vector3.one;

            // Root scale is now 1, so the grab collider must be sized explicitly
            // or it would become a 1 m cube swallowing the whole bench.
            var box = go.GetComponent<BoxCollider>();
            if (box != null)
            {
                box.center = Vector3.zero;
                box.size = new Vector3(k_W * 2f, k_H * 2f, k_D * 2f);
            }

            var visual = ResetVisual("Desktop Case", out _);
            return visual;
        }

        static void BuildCaseShell(Transform root)
        {
            if (root == null)
                return;

            const string steel = "Lab_CaseSteel";
            const string panel = "Lab_CasePanel";

            // --- shell: left side panel is absent, that is the service opening ---
            Box("Shell Right", root, new Vector3(k_W - 0.004f, 0f, 0f), new Vector3(0.008f, k_H * 2f, k_D * 2f), steel);
            Box("Shell Top", root, new Vector3(0f, k_H - 0.004f, 0f), new Vector3(k_W * 2f, 0.008f, k_D * 2f), panel);
            Box("Shell Bottom", root, new Vector3(0f, -k_H + 0.004f, 0f), new Vector3(k_W * 2f, 0.008f, k_D * 2f), panel);
            Box("Shell Rear", root, new Vector3(0f, 0f, k_D - 0.005f), new Vector3(k_W * 2f, k_H * 2f, 0.010f), steel);

            // Light liners on the cavity's floor and ceiling. The interior used to be
            // the same near-black as the shell, so nothing inside had anything to
            // silhouette against.
            Box("Liner Top", root, new Vector3(0f, k_H - 0.010f, 0f), new Vector3(k_W * 2f - 0.006f, 0.004f, k_D * 2f - 0.014f), "Lab_CaseInterior");
            Box("Liner Bottom", root, new Vector3(0f, -k_H + 0.010f, 0f), new Vector3(k_W * 2f - 0.006f, 0.004f, k_D * 2f - 0.014f), "Lab_CaseInterior");

            // Open-edge lip: without it the missing panel looks like a modelling error.
            Box("Open Lip Front", root, new Vector3(-k_W + 0.006f, 0f, -k_D + 0.012f), new Vector3(0.012f, k_H * 2f, 0.014f), steel);
            Box("Open Lip Rear", root, new Vector3(-k_W + 0.006f, 0f, k_D - 0.012f), new Vector3(0.012f, k_H * 2f, 0.014f), steel);
            Box("Open Lip Top", root, new Vector3(-k_W + 0.006f, k_H - 0.012f, 0f), new Vector3(0.012f, 0.014f, k_D * 2f), steel);
            Box("Open Lip Bottom", root, new Vector3(-k_W + 0.006f, -k_H + 0.012f, 0f), new Vector3(0.012f, 0.014f, k_D * 2f), steel);

            BuildFrontBezel(root);
            BuildRearIO(root);
            BuildMotherboard(root);
            BuildDriveCage(root);
            BuildFeet(root);
        }

        static void BuildFrontBezel(Transform root)
        {
            var bezel = Group("Front Bezel", root, new Vector3(0f, 0f, -k_D + 0.006f));

            Box("Face", bezel, Vector3.zero, new Vector3(k_W * 2f + 0.004f, k_H * 2f + 0.004f, 0.012f), "Lab_PlasticDark");
            Box("Mesh Inset", bezel, new Vector3(0f, -0.06f, -0.007f), new Vector3(0.16f, 0.20f, 0.004f), "Lab_MetalDark");

            // 5.25" bays read instantly as "desktop computer".
            for (var i = 0; i < 2; i++)
                Box($"Bay {i + 1}", bezel, new Vector3(0f, 0.175f - i * 0.048f, -0.008f), new Vector3(0.17f, 0.040f, 0.006f), "Lab_CasePanel");

            Box("Power Button", bezel, new Vector3(0.055f, 0.072f, -0.010f), new Vector3(0.022f, 0.022f, 0.008f), "Lab_PlasticLight");
            Box("Power Ring", bezel, new Vector3(0.055f, 0.072f, -0.013f), new Vector3(0.012f, 0.012f, 0.004f), "Lab_Accent");
            Box("Reset Button", bezel, new Vector3(-0.048f, 0.072f, -0.010f), new Vector3(0.010f, 0.010f, 0.008f), "Lab_MetalDark");

            for (var i = 0; i < 2; i++)
                Box($"USB {i + 1}", bezel, new Vector3(-0.012f + i * 0.024f, 0.072f, -0.010f), new Vector3(0.016f, 0.008f, 0.008f), "Lab_Metal");

            // Intake vent slots
            for (var i = 0; i < 7; i++)
                Box($"Vent Slot {i + 1}", bezel, new Vector3(0f, -0.14f + i * 0.026f, -0.010f), new Vector3(0.14f, 0.010f, 0.004f), "Lab_PlasticDark");
        }

        static void BuildRearIO(Transform root)
        {
            var rear = Group("Rear IO", root, new Vector3(0f, 0f, k_D - 0.012f));

            Box("IO Shield", rear, new Vector3(0.028f, 0.145f, 0f), new Vector3(0.11f, 0.05f, 0.006f), "Lab_Metal");
            for (var i = 0; i < 4; i++)
                Box($"IO Port {i + 1}", rear, new Vector3(0.0f + (i % 2) * 0.03f, 0.155f - (i / 2) * 0.022f, -0.004f), new Vector3(0.022f, 0.014f, 0.004f), "Lab_PlasticDark");

            // Expansion slot covers
            for (var i = 0; i < 5; i++)
                Box($"Slot Cover {i + 1}", rear, new Vector3(0.03f, 0.055f - i * 0.026f, 0f), new Vector3(0.11f, 0.020f, 0.005f), "Lab_MetalDark");

            Box("PSU Cutout", rear, new Vector3(0f, -0.163f, 0f), new Vector3(0.16f, 0.086f, 0.008f), "Lab_CaseSteel");
        }

        /// <summary>
        /// Populates the board.
        ///
        /// Everything here was previously modelled as a thin plate lying against the
        /// board, because each part's size was written in the case's axes rather than
        /// the board's. A DIMM came out as a 5 mm sliver, the graphics card stood on
        /// edge like a second motherboard, and from the participant's pose the whole
        /// interior read as coloured stripes on a green rectangle.
        ///
        /// The board's local frame is: -X out of the board toward the open side (this
        /// is the direction every component grows in), +Y up, +Z toward the case rear.
        /// Parts are now sized in that frame and at the proportions of the real thing,
        /// so a socket-and-cooler stack looks like a cooler and a card looks like a
        /// card.
        /// </summary>
        static void BuildMotherboard(Transform root)
        {
            var board = Group("Motherboard Assembly", root, new Vector3(k_W - 0.020f, 0.015f, 0.02f));

            Box("Standoff Tray", board, new Vector3(0.010f, 0f, 0f), new Vector3(0.006f, 0.278f, 0.298f), "Lab_CaseInterior");
            ImportedVisual("Motherboard Model", board, k_Item3D + "Optimized/Motherboard/anakart_quest.glb",
                new Vector3(-0.040f, 0f, 0f), new Vector3(0.085f, 0.255f, 0.265f), new Vector3(0f, 90f, 0f));

            BuildCpuCooler(board);
            BuildMemory(board);
            BuildAtxHeader(board);
            BuildGraphicsCard(board);
        }

        /// <summary>
        /// Tower cooler: base block, heatpipes, a stacked fin block and a fan clipped
        /// to its front face.
        ///
        /// The first attempt was a top-down cooler whose fins radiated in the board's
        /// plane. Seen through the open side it drew a black disc with a hub — several
        /// readers called it a record player. A tower has an unmistakable silhouette
        /// from exactly this angle, and being 90 mm tall it also gives the cavity the
        /// depth it was missing: the interior read flat because nothing stood up off
        /// the board.
        /// </summary>
        static void BuildCpuCooler(Transform board)
        {
            var euler = new Vector3(-90f, -90f, 0f);
            ImportedVisual("CPU Package", board, k_Item3D + "CPU/ryzen_5_5600.glb",
                new Vector3(-0.088f, 0.040f, -0.030f), new Vector3(0.008f, 0.042f, 0.042f), euler);
            ImportedVisual("CPU Cooler", board, k_Item3D + "Cooler/source/amdwraithstealthnocable.glb",
                new Vector3(-0.128f, 0.040f, -0.030f), new Vector3(0.070f, 0.095f, 0.095f), euler);
        }

        /// <summary>Four DIMM slots, two populated. Modules stand out of the board, not flat on it.</summary>
        static void BuildMemory(Transform board)
        {
            var path = k_Item3D + "Optimized/RAM/random_access_memory_ram_ddr4_quest.glb";
            var euler = new Vector3(0f, 90f, 90f);
            ImportedVisual("RAM Module 1", board, path, new Vector3(-0.105f, 0.050f, 0.070f), new Vector3(0.035f, 0.124f, 0.006f), euler);
            ImportedVisual("RAM Module 2", board, path, new Vector3(-0.105f, 0.050f, 0.087f), new Vector3(0.035f, 0.124f, 0.006f), euler);
        }

        /// <summary>
        /// The 24-pin ATX header on the board's front edge — the fault site. Two rows
        /// of twelve gold pins in a keyed white shroud, at the size and place a real
        /// one sits, so the loose plug on the bench is recognisably its mate.
        /// </summary>
        static void BuildAtxHeader(Transform board)
        {
            var header = Group("ATX Header", board, new Vector3(-0.095f, 0.040f, -0.122f));

            Box("Shroud", header, Vector3.zero, new Vector3(0.018f, 0.062f, 0.014f), "Lab_ConnectorWhite");
            Box("Shroud Wall", header, new Vector3(-0.008f, 0f, 0f), new Vector3(0.003f, 0.062f, 0.014f), "Lab_ConnectorWhite");
            Box("Key Tab", header, new Vector3(-0.002f, 0.033f, 0f), new Vector3(0.014f, 0.006f, 0.010f), "Lab_ConnectorWhite");

            for (var i = 0; i < 24; i++)
                Box($"Pin {i + 1}", header, new Vector3(-0.002f, -0.027f + (i / 2) * 0.0049f, (i % 2 == 0 ? -0.0032f : 0.0032f)),
                    new Vector3(0.010f, 0.0022f, 0.0022f), "Lab_Gold");
        }

        /// <summary>
        /// Graphics card in the top PCIe slot. In a tower the card lies horizontally,
        /// so it is a slab that grows in -X and runs front-to-back in Z — not the
        /// upright plate the old build produced.
        /// </summary>
        static void BuildGraphicsCard(Transform board)
        {
            Box("PCIe Slot", board, new Vector3(-0.005f, -0.036f, 0.004f), new Vector3(0.010f, 0.008f, 0.190f), "Lab_PlasticDark");
            ImportedVisual("Graphics Card", board, k_Item3D + "Optimized/GPU/gpu_quest.glb",
                new Vector3(-0.105f, -0.045f, 0.004f), new Vector3(0.095f, 0.060f, 0.225f), new Vector3(-90f, -90f, 0f));
        }

        /// <summary>
        /// Drive bay carrying a 3.5" hard disk and a 2.5" SSD in a caddy. Both face
        /// the open side, because a drive seen edge-on is just a metal bar.
        /// </summary>
        static void BuildDriveCage(Transform root)
        {
            var cage = Group("Drive Cage", root, new Vector3(-0.026f, -0.108f, -0.130f));

            Box("Cage Frame", cage, Vector3.zero, new Vector3(0.120f, 0.120f, 0.108f), "Lab_CaseInterior");
            Box("Cage Rail Top", cage, new Vector3(-0.062f, 0.058f, 0f), new Vector3(0.010f, 0.010f, 0.108f), "Lab_CaseSteel");
            Box("Cage Rail Bottom", cage, new Vector3(-0.062f, -0.058f, 0f), new Vector3(0.010f, 0.010f, 0.108f), "Lab_CaseSteel");

            // 3.5" disk, label out.
            var hdd = Group("Hard Disk", cage, new Vector3(-0.064f, 0.030f, 0f));
            Box("Body", hdd, Vector3.zero, new Vector3(0.026f, 0.048f, 0.101f), "Lab_MetalDark");
            Box("Label", hdd, new Vector3(-0.014f, 0f, 0f), new Vector3(0.002f, 0.040f, 0.090f), "Lab_LabelPlate");
            Box("SATA Data", hdd, new Vector3(-0.004f, -0.020f, 0.052f), new Vector3(0.014f, 0.008f, 0.010f), "Lab_Accent");
            Box("SATA Power", hdd, new Vector3(-0.004f, -0.020f, 0.036f), new Vector3(0.014f, 0.008f, 0.020f), "Lab_ConnectorWhite");

            ImportedVisual("Solid State Drive", cage, k_Item3D + "Storage/source/ssd-kit.glb",
                new Vector3(-0.064f, -0.030f, 0f), new Vector3(0.014f, 0.075f, 0.045f), new Vector3(90f, 90f, 0f));

            // Data leads running forward to the board's SATA ports.
            Box("Data Lead A", cage, new Vector3(-0.058f, 0.010f, 0.070f), new Vector3(0.010f, 0.004f, 0.060f), "Lab_Accent", new Vector3(-26f, 0f, 0f));
            Box("Data Lead B", cage, new Vector3(-0.058f, -0.042f, 0.070f), new Vector3(0.010f, 0.004f, 0.060f), "Lab_Accent", new Vector3(22f, 0f, 0f));
        }

        static void BuildFeet(Transform root)
        {
            foreach (var (x, z) in new[] { (-0.08f, -0.19f), (0.08f, -0.19f), (-0.08f, 0.19f), (0.08f, 0.19f) })
                Box($"Foot {x}_{z}", root, new Vector3(x, -k_H - 0.008f, z), new Vector3(0.030f, 0.016f, 0.030f), "Lab_Rubber");
        }

        /// <summary>Reparents the in-case interactables so local coordinates stay readable.</summary>
        static void PlaceInteriorParts(Transform caseVisual)
        {
            var case_ = GameObject.Find("Desktop Case");
            if (case_ == null)
                return;

            // Motherboard target: an outline plate over the board, not a second board.
            Move("Motherboard Placeholder", case_.transform, new Vector3(k_W - 0.028f, 0.015f, 0.02f), Vector3.one);
            var mb = ResetVisual("Motherboard Placeholder", out var mbGo);
            if (mb != null)
            {
                // The project-owned logical wrapper supplies inspection and identity;
                // the source-backed board under Desktop Case/Visual supplies the mesh.
                FitBoxCollider(mbGo, new Vector3(0.02f, 0.26f, 0.28f));
            }

            // PSU in the basement, fan facing up into the case, cables leaving the
            // front face where the fixed loom picks them up.
            Move("Power Supply Placeholder", case_.transform, new Vector3(0f, -0.163f, 0.130f), Vector3.one);
            var psu = ResetVisual("Power Supply Placeholder", out var psuGo);
            if (psu != null)
            {
                ImportedVisual("PSU Model", psu, k_Item3D + "PSU/psu_power_supply_unit.glb",
                    Vector3.zero, new Vector3(0.150f, 0.086f, 0.140f));
                FitBoxCollider(psuGo, new Vector3(0.15f, 0.09f, 0.14f));
            }

            BuildCableLoom(case_.transform);

            // Rear exhaust fan.
            Move("Cooling Fan Placeholder", case_.transform, new Vector3(0f, 0.130f, 0.196f), Vector3.one);
            var fan = ResetVisual("Cooling Fan Placeholder", out var fanGo);
            if (fan != null)
            {
                ImportedVisual("Case Fan Model", fan, k_Item3D + "Fans/120mm_computer_fans.glb",
                    Vector3.zero, new Vector3(0.118f, 0.118f, 0.025f));
                FitBoxCollider(fanGo, new Vector3(0.12f, 0.12f, 0.03f));
            }

            // PSU rocker switch on the rear face.
            Move("PSU Switch", case_.transform, new Vector3(0.052f, -0.163f, 0.222f), Vector3.one);
            var sw = ResetVisual("PSU Switch", out var swGo);
            if (sw != null)
            {
                Box("Bezel", sw, Vector3.zero, new Vector3(0.030f, 0.020f, 0.008f), "Lab_PlasticDark");
                Box("Rocker", sw, new Vector3(0f, 0f, 0.004f), new Vector3(0.022f, 0.013f, 0.005f), "Lab_PlasticLight", new Vector3(8f, 0f, 0f));
                FitBoxCollider(swGo, new Vector3(0.04f, 0.03f, 0.02f));
            }

            // THE FAULT: the 24-pin plug hanging free, a hand's width below the header
            // it belongs in. The loom behind it is fixed geometry, so the plug reads
            // as "this came out of there" rather than "a white brick fell in the case".
            // Hung in the clear space between the board's front edge and the bezel, at
            // the same height band as the header it belongs in, so the two are in one
            // glance of each other without anything pointing at either.
            Move("Internal Cable Connector", case_.transform, new Vector3(0.032f, 0.004f, -0.152f), Vector3.one, new Vector3(0f, 0f, -28f));
            var cable = ResetVisual("Internal Cable Connector", out var cableGo);
            if (cable != null)
            {
                // 24-pin plug: two rows of twelve sockets, keyed shroud, latch on the
                // spine — the mating half of the header on the board edge.
                Box("Plug Body", cable, Vector3.zero, new Vector3(0.020f, 0.064f, 0.016f), "Lab_ConnectorWhite");
                Box("Plug Mouth", cable, new Vector3(-0.011f, 0f, 0f), new Vector3(0.004f, 0.058f, 0.011f), "Lab_PlasticDark");
                Box("Plug Latch", cable, new Vector3(0.006f, 0.036f, 0f), new Vector3(0.012f, 0.016f, 0.010f), "Lab_ConnectorWhite");
                for (var i = 0; i < 12; i++)
                    Box($"Socket {i + 1}", cable, new Vector3(-0.009f, -0.026f + (i / 2) * 0.0095f, (i % 2 == 0 ? -0.003f : 0.003f)), new Vector3(0.004f, 0.004f, 0.004f), "Lab_PlasticDark");

                // Short tail: the rest of the run is the fixed loom.
                Box("Tail", cable, new Vector3(0.020f, -0.030f, 0.004f), new Vector3(0.026f, 0.044f, 0.022f), "Lab_CableBlack", new Vector3(0f, 0f, 34f));
                FitBoxCollider(cableGo, new Vector3(0.06f, 0.09f, 0.05f));
            }
        }

        /// <summary>
        /// Fixed cabling: the 24-pin run climbing the case front from the PSU, plus
        /// the drive and card leads. Without it the interior looked assembled by
        /// magic, and the loose plug had nothing to be loose from.
        /// </summary>
        static void BuildCableLoom(Transform case_)
        {
            var loom = Group("Cable Loom", case_, new Vector3(0.012f, -0.150f, 0.040f));

            // Main 24-pin run: out of the PSU gland, along the floor, up the front
            // corner and into the space beside the unplugged connector.
            Box("Run Floor", loom, new Vector3(0.010f, 0.006f, -0.048f), new Vector3(0.026f, 0.022f, 0.090f), "Lab_CableBlack", new Vector3(10f, 0f, 0f));
            Box("Run Bend", loom, new Vector3(0.014f, 0.038f, -0.098f), new Vector3(0.024f, 0.062f, 0.024f), "Lab_CableBlack", new Vector3(38f, 0f, -6f));
            Box("Run Rise", loom, new Vector3(0.020f, 0.090f, -0.126f), new Vector3(0.024f, 0.060f, 0.022f), "Lab_CableBlack", new Vector3(6f, 0f, -12f));
            Box("Loom Tie", loom, new Vector3(0.018f, 0.062f, -0.114f), new Vector3(0.030f, 0.008f, 0.028f), "Lab_ConnectorWhite", new Vector3(24f, 0f, -8f));

            // Drive power chain along the cage, and the card's supply climbing behind it.
            Box("Drive Lead", loom, new Vector3(-0.030f, 0.012f, -0.120f), new Vector3(0.014f, 0.014f, 0.110f), "Lab_CableBlack", new Vector3(-8f, 0f, 0f));
            Box("Drive Lead Tail", loom, new Vector3(-0.036f, 0.048f, -0.166f), new Vector3(0.012f, 0.062f, 0.012f), "Lab_CableBlack", new Vector3(28f, 0f, 0f));
            Box("Card Lead", loom, new Vector3(0.040f, 0.070f, 0.028f), new Vector3(0.016f, 0.120f, 0.016f), "Lab_CableBlack", new Vector3(-14f, 0f, -10f));

            // Front-panel ribbon from the bezel back to its header. Yellow made it the
            // brightest thing in the case, so a bystander's eye landed on a bundle of
            // wire instead of on the components.
            Box("Front Panel Ribbon", loom, new Vector3(-0.026f, 0.036f, -0.152f), new Vector3(0.014f, 0.072f, 0.003f), "Lab_CableBlack", new Vector3(-32f, 0f, 14f));
        }

        static void PlaceBenchParts()
        {
            // --- spares tray (left) ---
            // Replacement 24-pin lead: the same connector family as the plug hanging
            // in the case, so the two are recognisably a pair on inspection — but it
            // sits in the tray alongside the other spares, unmarked.
            Move("Main Power Connector", null, new Vector3(-1.16f, k_BenchTop + 0.042f, 0.95f), Vector3.one, new Vector3(0f, 14f, 0f));
            var mpc = ResetVisual("Main Power Connector", out var mpcGo);
            if (mpc != null)
            {
                Box("Shroud", mpc, Vector3.zero, new Vector3(0.070f, 0.024f, 0.022f), "Lab_ConnectorWhite");
                Box("Mouth", mpc, new Vector3(0f, -0.011f, 0f), new Vector3(0.064f, 0.004f, 0.017f), "Lab_PlasticDark");
                Box("Latch", mpc, new Vector3(0f, 0.006f, -0.013f), new Vector3(0.018f, 0.014f, 0.005f), "Lab_ConnectorWhite");
                for (var i = 0; i < 24; i++)
                    Box($"Pin {i + 1}", mpc, new Vector3(-0.031f + (i / 2) * 0.0055f, 0.010f, (i % 2 == 0 ? -0.005f : 0.005f)), new Vector3(0.0026f, 0.005f, 0.0026f), "Lab_Gold");
                Box("Tail A", mpc, new Vector3(0f, 0.006f, 0.032f), new Vector3(0.058f, 0.020f, 0.044f), "Lab_CableBlack");
                Box("Tail B", mpc, new Vector3(0.020f, 0.006f, 0.070f), new Vector3(0.048f, 0.018f, 0.046f), "Lab_CableBlack", new Vector3(0f, 26f, 0f));
                FitBoxCollider(mpcGo, new Vector3(0.10f, 0.05f, 0.13f));
            }

            Move("RAM Placeholder", null, new Vector3(-0.86f, k_BenchTop + 0.044f, 0.95f), Vector3.one, new Vector3(0f, -8f, 0f));
            var ram = ResetVisual("RAM Placeholder", out var ramGo);
            if (ram != null)
            {
                ImportedVisual("RAM Model", ram, k_Item3D + "Optimized/RAM/random_access_memory_ram_ddr4_quest.glb",
                    Vector3.zero, new Vector3(0.145f, 0.045f, 0.012f));
                FitBoxCollider(ramGo, new Vector3(0.15f, 0.06f, 0.04f));
            }

            // --- removed side panel, stowed on the bench's lower shelf: keeps the
            //     bench's left end clear for the information dock's sight line ---
            Move("Computer Side Panel", null, new Vector3(-1.25f, 0.345f, 0.90f), Vector3.one, new Vector3(0f, 8f, 0f));
            var side = ResetVisual("Computer Side Panel", out var sideGo);
            if (side != null)
            {
                Box("Panel", side, Vector3.zero, new Vector3(0.430f, 0.012f, 0.450f), "Lab_CasePanel");
                Box("Lip Front", side, new Vector3(0f, 0.010f, -0.220f), new Vector3(0.430f, 0.016f, 0.012f), "Lab_CaseSteel");
                Box("Lip Rear", side, new Vector3(0f, 0.010f, 0.220f), new Vector3(0.430f, 0.016f, 0.012f), "Lab_CaseSteel");
                Box("Window", side, new Vector3(0.02f, 0.008f, 0f), new Vector3(0.300f, 0.003f, 0.320f), "Lab_GlassPanel");
                Box("Thumb Screw A", side, new Vector3(0.200f, 0.012f, -0.170f), new Vector3(0.018f, 0.012f, 0.018f), "Lab_ToolSteel");
                Box("Thumb Screw B", side, new Vector3(0.200f, 0.012f, 0.170f), new Vector3(0.018f, 0.012f, 0.018f), "Lab_ToolSteel");
                FitBoxCollider(sideGo, new Vector3(0.45f, 0.05f, 0.47f));
            }

            // --- tool tray (right) ---
            BenchDressing.PlaceScrewdriver(new Vector3(1.02f, k_BenchTop + 0.035f, 0.95f));

            // --- distractor: a sealed spare module, clearly not part of this repair ---
            Move("Computer Non Target Module", null, new Vector3(1.58f, k_BenchTop + 0.052f, 0.98f), Vector3.one, new Vector3(0f, -12f, 0f));
            var nonTarget = ResetVisual("Computer Non Target Module", out var nonTargetGo);
            if (nonTarget != null)
            {
                Box("Body", nonTarget, Vector3.zero, new Vector3(0.150f, 0.090f, 0.110f), "Lab_MetalDark");
                Box("Label", nonTarget, new Vector3(0f, 0.046f, 0f), new Vector3(0.110f, 0.002f, 0.070f), "Lab_LabelPlate");
                Box("Seal", nonTarget, new Vector3(0f, 0.047f, 0.030f), new Vector3(0.060f, 0.002f, 0.016f), "Lab_Warning");
                FitBoxCollider(nonTargetGo, new Vector3(0.16f, 0.10f, 0.12f));
            }

            // --- external mains lead, coiled at the back of the bench ---
            Move("External Power Cable", null, new Vector3(0.52f, k_BenchTop + 0.030f, 1.24f), Vector3.one, new Vector3(0f, 0f, 0f));
            var ext = ResetVisual("External Power Cable", out var extGo);
            if (ext != null)
            {
                Box("Plug", ext, Vector3.zero, new Vector3(0.052f, 0.036f, 0.032f), "Lab_PlasticDark");
                for (var i = 0; i < 3; i++)
                    Box($"Prong {i + 1}", ext, new Vector3(-0.014f + i * 0.014f, 0.002f, -0.024f), new Vector3(0.005f, 0.014f, 0.018f), "Lab_ToolSteel");
                Cyl("Coil Outer", ext, new Vector3(0.10f, -0.014f, 0.02f), new Vector3(0.170f, 0.008f, 0.170f), "Lab_CableBlack");
                Cyl("Coil Inner", ext, new Vector3(0.10f, -0.004f, 0.02f), new Vector3(0.120f, 0.008f, 0.120f), "Lab_CableBlack");
                FitBoxCollider(extGo, new Vector3(0.08f, 0.06f, 0.06f));
            }

            // --- status lamp at the test end of the bench ---
            Move("Computer Status Indicator", null, new Vector3(LabLayoutBuilder.TestStationX, k_BenchTop + 0.090f, 1.05f), Vector3.one);
            var status = ResetVisual("Computer Status Indicator", out var statusGo);
            if (status != null)
            {
                Box("Base", status, new Vector3(0f, -0.082f, 0f), new Vector3(0.090f, 0.016f, 0.090f), "Lab_Navy");
                Cyl("Stem", status, new Vector3(0f, -0.045f, 0f), new Vector3(0.018f, 0.038f, 0.018f), "Lab_MetalDark");
                Cyl("Lamp Housing", status, new Vector3(0f, 0f, 0f), new Vector3(0.070f, 0.045f, 0.070f), "Lab_PlasticDark");
                Cyl("Lamp Lens", status, new Vector3(0f, 0.038f, 0f), new Vector3(0.056f, 0.010f, 0.056f), "Lab_StatusRed");
                FitBoxCollider(statusGo, new Vector3(0.10f, 0.20f, 0.10f));
            }
        }

        static void Move(string name, Transform parent, Vector3 position, Vector3 scale, Vector3 euler = default)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"[ComputerWorkstation] missing {name}");
                return;
            }

            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localPosition = position;
                go.transform.localRotation = Quaternion.Euler(euler);
            }
            else
            {
                go.transform.SetParent(null, false);
                go.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
            }

            go.transform.localScale = scale;
        }

    }
}
