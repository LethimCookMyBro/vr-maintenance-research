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

        // --- board frame ---------------------------------------------------
        // The licensed B450 board is 0.244 x 0.022 x 0.305 m in its own axes, and its
        // rear-panel edge — the one carrying both the I/O cluster and the PCIe bracket
        // ends — is its -X edge, running the full 0.305 m. In a tower that edge stands
        // vertical against the rear panel, so the board is 305 mm tall and 244 mm deep,
        // not the other way round. This rotation is what puts it that way:
        //   model +X -> case -Z,  model +Y -> case +X,  model +Z -> case -Y
        // which also turns the component side (model -Y) toward the open panel.
        static readonly Vector3 k_BoardEuler = new Vector3(90f, 90f, 0f);

        // Sits high enough to clear the PSU basement and far enough back that the I/O
        // cluster meets the rear panel.
        static readonly Vector3 k_BoardCentre = new Vector3(0.081f, 0.050f, 0.083f);

        /// <summary>
        /// Places a part on the board, from coordinates read off the orthographic board
        /// map (Docs/Screenshots/Staging/Probe_Board_Ortho.png): <paramref name="dx"/>
        /// and <paramref name="dz"/> are that map's axes measured from the board centre,
        /// and <paramref name="lift"/> is how far the part stands off the board face.
        ///
        /// Returns a position local to Motherboard Assembly, which sits at the board
        /// centre — so socket, DIMM and slot positions are measured from the real model
        /// rather than guessed, which is how the first integration put a cooler in mid
        /// air and a card through the drive cage.
        /// </summary>
        static Vector3 OnBoard(float dx, float dz, float lift)
        {
            return new Vector3(-0.011f - lift, dz, -dx);
        }

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

            // No handmade I/O shield or ports here: the board model carries its own
            // cluster and it now meets this panel, so building a second set would put
            // two stacks of ports a few millimetres apart.

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
        /// <summary>
        /// Populates the board.
        ///
        /// The board is the licensed MSI B450 model, which already carries its own VRM
        /// and chipset heatsinks, capacitors, SATA ports, DIMM slots, front-panel header
        /// and rear I/O cluster. The handmade versions of all of those are gone rather
        /// than layered underneath: the first integration kept both and the interior
        /// filled up with two of everything, which is most of what read as clutter.
        ///
        /// The model measures 0.248 x 0.022 x 0.305 m on import — already true ATX size
        /// — so it is fitted at its native scale and only rotated into the board frame
        /// (-X out toward the open side, +Y up, +Z toward the case rear) and pushed back
        /// until its I/O cluster meets the rear panel.
        ///
        /// The ATX header stays handmade. It is the fault site, and the participant has
        /// to be able to read it as the mate of the plug hanging in the case.
        /// </summary>
        static void BuildMotherboard(Transform root)
        {
            var board = Group("Motherboard Assembly", root, k_BoardCentre);

            Box("Standoff Tray", board, new Vector3(0.015f, 0f, 0f), new Vector3(0.006f, 0.320f, 0.262f), "Lab_CaseInterior");

            // The board mesh itself hangs off the Motherboard Placeholder interactable
            // in PlaceInteriorParts, not here: that object is what the task calls the
            // motherboard, and giving it the real board means the scene has one board
            // instead of a model here and a green stand-in plate 2 mm away.
            BuildCpuCooler(board);
            BuildMemory(board);
            BuildAtxHeader(board);
            BuildGraphicsCard(board);
        }

        /// <summary>
        /// Processor and its stock cooler, seated on the socket the board model actually
        /// draws — socket centre measured at dx +0.012, dz +0.069 on the board map.
        ///
        /// The cooler is the licensed AMD Wraith Stealth, a top-down design, so it is
        /// laid with its height along the case's X axis: it stands off the board toward
        /// the open panel and covers the processor, instead of standing beside it.
        /// </summary>
        static void BuildCpuCooler(Transform board)
        {
            const float socketX = 0.0117f;
            const float socketZ = 0.0692f;

            // 40 x 40 x 7 mm, flat on the socket: model Z is its thin axis.
            ImportedVisual("CPU Package", board, k_Item3D + "CPU/ryzen_5_5600.glb",
                OnBoard(socketX, socketZ, 0.0035f), new Vector3(0.008f, 0.042f, 0.042f), new Vector3(0f, 90f, 0f));

            // Sits on top of the processor: 7 mm of CPU plus half the cooler's height.
            ImportedVisual("CPU Cooler", board, k_Item3D + "Cooler/source/amdwraithstealthnocable.glb",
                OnBoard(socketX, socketZ, 0.0315f), new Vector3(0.052f, 0.098f, 0.092f), new Vector3(0f, 0f, 90f));

            // No handmade fan, heatpipes or fan header here. The Wraith model carries
            // its own fan, and the board model draws its own headers.
        }

        /// <summary>
        /// Two DIMMs in the second and fourth slots — the dual-channel pair a technician
        /// would actually populate on this board.
        ///
        /// Slot centres were measured off the board map at dx +0.071 and +0.086, so the
        /// modules land in the slots the board draws rather than beside them. They stand
        /// vertically, which is how DIMMs sit in a tower.
        /// </summary>
        static void BuildMemory(Transform board)
        {
            const string path = k_Item3D + "Optimized/RAM/random_access_memory_ram_ddr4_quest.glb";
            var euler = new Vector3(0f, 0f, 90f);

            ImportedVisual("RAM Module 1", board, path, OnBoard(0.0711f, 0.0726f, 0.018f),
                new Vector3(0.031f, 0.135f, 0.006f), euler);
            ImportedVisual("RAM Module 2", board, path, OnBoard(0.0862f, 0.0726f, 0.018f),
                new Vector3(0.031f, 0.135f, 0.006f), euler);
        }

        /// <summary>
        /// The 24-pin ATX header on the board's front edge — the fault site. Two rows
        /// of twelve gold pins in a keyed white shroud, at the size and place a real
        /// one sits, so the loose plug on the bench is recognisably its mate.
        /// </summary>
        static void BuildAtxHeader(Transform board)
        {
            // Front-right edge of the board, where a real 24-pin sits, standing vertical
            // as it does in a tower.
            var header = Group("ATX Header", board, OnBoard(0.110f, -0.030f, 0.009f));

            Box("Shroud", header, Vector3.zero, new Vector3(0.018f, 0.062f, 0.014f), "Lab_ConnectorWhite");
            Box("Shroud Wall", header, new Vector3(-0.008f, 0f, 0f), new Vector3(0.003f, 0.062f, 0.014f), "Lab_ConnectorWhite");
            Box("Key Tab", header, new Vector3(-0.002f, 0.033f, 0f), new Vector3(0.014f, 0.006f, 0.010f), "Lab_ConnectorWhite");

            for (var i = 0; i < 24; i++)
                Box($"Pin {i + 1}", header, new Vector3(-0.002f, -0.027f + (i / 2) * 0.0049f, (i % 2 == 0 ? -0.0032f : 0.0032f)),
                    new Vector3(0.010f, 0.0022f, 0.0022f), "Lab_Gold");
        }

        /// <summary>
        /// Graphics card in the primary PCIe slot, which the board map puts at dx -0.034,
        /// dz -0.028. In a tower the card lies horizontally, so it is a slab that grows
        /// out of the board in -X and runs front-to-back in Z.
        ///
        /// This one stays handmade. Both licensed cards were measured and neither can be
        /// used: the black dual-fan card is correctly shaped but 113,728 triangles, and
        /// the RTX 4060 Ti — the cheaper of the two — is modelled far too flat, its
        /// backplate 26.5 x 2.49 where a real card is nearer 2.2:1. Fitted to a true
        /// 245 mm length it would read as a 22 mm blade in the slot. The handmade card
        /// is 632 triangles, correctly proportioned, and matches the case's style.
        /// No PCIe slot is drawn here either: the board model has its own.
        /// </summary>
        static void BuildGraphicsCard(Transform board)
        {
            var gpu = Group("Graphics Card", board, new Vector3(-0.065f, -0.028f, 0.009f));

            Box("PCB", gpu, Vector3.zero, new Vector3(0.100f, 0.003f, 0.226f), "Lab_PcbDark");
            // Dark backplate. In steel it caught the key light and read as a bright shelf
            // spanning the case rather than as the top of a card — it was the first thing
            // the eye landed on inside the machine.
            Box("Backplate", gpu, new Vector3(0f, 0.006f, 0f), new Vector3(0.096f, 0.003f, 0.220f), "Lab_PlasticDark");
            Box("Backplate Label", gpu, new Vector3(-0.010f, 0.008f, -0.070f), new Vector3(0.052f, 0.001f, 0.048f), "Lab_LabelPlate");
            Box("Shroud", gpu, new Vector3(0f, -0.020f, 0f), new Vector3(0.098f, 0.038f, 0.222f), "Lab_PlasticDark");
            Box("Shroud Edge", gpu, new Vector3(-0.048f, -0.020f, 0f), new Vector3(0.004f, 0.038f, 0.222f), "Lab_MetalDark");

            // Fan bays cut into the shroud's outer edge: from the side panel this is
            // the only part of the cooler you see, and without it the card is a slab.
            foreach (var z in new[] { -0.058f, 0.058f })
            {
                Box($"Bay Mouth {(z < 0 ? "A" : "B")}", gpu, new Vector3(-0.046f, -0.026f, z), new Vector3(0.010f, 0.024f, 0.074f), "Lab_PlasticDark");
                Box($"Bay Rim {(z < 0 ? "A" : "B")}", gpu, new Vector3(-0.044f, -0.010f, z), new Vector3(0.012f, 0.006f, 0.078f), "Lab_MetalDark");
            }
            Box("Shroud Spine", gpu, new Vector3(-0.020f, -0.002f, 0f), new Vector3(0.030f, 0.012f, 0.216f), "Lab_MetalDark");

            // Fans on the underside, the face a bystander actually sees in an open case.
            foreach (var z in new[] { -0.058f, 0.058f })
            {
                Cyl($"Fan {(z < 0 ? "A" : "B")}", gpu, new Vector3(-0.004f, -0.038f, z), new Vector3(0.076f, 0.003f, 0.076f), "Lab_CasePanel");
                Cyl($"Fan Hub {(z < 0 ? "A" : "B")}", gpu, new Vector3(-0.004f, -0.040f, z), new Vector3(0.026f, 0.003f, 0.026f), "Lab_MetalDark");
                for (var i = 0; i < 6; i++)
                    Box($"Fan Blade {(z < 0 ? "A" : "B")}{i + 1}", gpu, new Vector3(-0.004f, -0.039f, z), new Vector3(0.070f, 0.002f, 0.016f), "Lab_PlasticDark", new Vector3(0f, i * 30f, 0f));
            }

            // 8-pin PCIe power sockets on the top edge, wired back to the PSU loom.
            Box("Power Socket", gpu, new Vector3(-0.030f, 0.012f, 0.070f), new Vector3(0.030f, 0.014f, 0.020f), "Lab_PlasticDark");
            Box("Power Lead", gpu, new Vector3(-0.030f, 0.026f, 0.050f), new Vector3(0.014f, 0.014f, 0.060f), "Lab_CableBlack", new Vector3(28f, 0f, 0f));

            // Rear bracket in the expansion slot.
            Box("Bracket", gpu, new Vector3(0.006f, -0.016f, 0.118f), new Vector3(0.088f, 0.062f, 0.005f), "Lab_MetalDark");
            Box("Display Port", gpu, new Vector3(-0.012f, -0.020f, 0.122f), new Vector3(0.030f, 0.012f, 0.006f), "Lab_PlasticDark");
        }

        /// <summary>
        /// Drive tray carrying the one solid-state drive the bench is allowed.
        ///
        /// The 3.5" hard disk that used to share this cage is gone. It was never part of
        /// the repair and the interior is meant to hold one drive, so it was two hundred
        /// grams of clutter in front of the components that matter.
        ///
        /// The licensed drive is an M.2 stick — 80 x 22 x 2.4 mm, which is what the
        /// kit's Samsung 990 artwork describes — so it lies flat on the tray rather than
        /// standing in a 2.5" caddy it would rattle around in.
        /// </summary>
        static void BuildDriveCage(Transform root)
        {
            var cage = Group("Drive Cage", root, new Vector3(-0.026f, -0.108f, -0.130f));

            Box("Cage Frame", cage, Vector3.zero, new Vector3(0.120f, 0.120f, 0.108f), "Lab_CaseInterior");
            Box("Cage Rail Top", cage, new Vector3(-0.062f, 0.058f, 0f), new Vector3(0.010f, 0.010f, 0.108f), "Lab_CaseSteel");
            Box("Cage Rail Bottom", cage, new Vector3(-0.062f, -0.058f, 0f), new Vector3(0.010f, 0.010f, 0.108f), "Lab_CaseSteel");

            // Tray shelf the drive actually rests on, so it is supported rather than
            // hanging in the bay.
            Box("Drive Tray", cage, new Vector3(-0.062f, 0.004f, 0f), new Vector3(0.028f, 0.004f, 0.092f), "Lab_CaseSteel");

            // Circle.002 is the kit's second drive; the bench is allowed one.
            // Model axes already read width / thickness / length, so it needs no turning.
            ImportedVisual("Solid State Drive", cage, k_Item3D + "Storage/source/ssd-kit.glb",
                new Vector3(-0.062f, 0.008f, 0f), new Vector3(0.022f, 0.004f, 0.080f), Vector3.zero,
                new[] { "Circle.002" });
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

            // The motherboard interactable carries the real board. It used to carry a
            // flat green "target plate" standing in for one, which sat 2 mm off the
            // board mesh and hid it almost completely.
            //
            // Sat at the board's own centre so the mesh needs no offset of its own, and
            // so the grab collider wraps the thing the participant is looking at.
            Move("Motherboard Placeholder", case_.transform, k_BoardCentre, Vector3.one);
            var mb = ResetVisual("Motherboard Placeholder", out var mbGo);
            if (mb != null)
            {
                ImportedVisual("Motherboard Model", mb, k_Item3D + "Optimized/Motherboard/anakart_quest.glb",
                    Vector3.zero, new Vector3(0.024f, 0.312f, 0.250f), k_BoardEuler);
                SetCollider(mbGo, new Vector3(0.024f, 0.312f, 0.250f));
            }

            // PSU in the basement, fan facing up into the case, cables leaving the
            // front face where the fixed loom picks them up.
            Move("Power Supply Placeholder", case_.transform, new Vector3(0f, -0.163f, 0.130f), Vector3.one);
            var psu = ResetVisual("Power Supply Placeholder", out var psuGo);
            if (psu != null)
            {
                // Model axes are width / depth / height. This lays the unit in the
                // basement with its switched rear panel at the case rear and its printed
                // side the right way up — a straight 90 degrees put the label upside
                // down. Object_78 is a loose screw floating 16 units out from a body
                // only 10 deep, which would sit outside the case and shrink the unit by
                // inflating the bounds it is fitted to.
                ImportedVisual("PSU Model", psu, k_Item3D + "PSU/psu_power_supply_unit.glb",
                    Vector3.zero, new Vector3(0.150f, 0.086f, 0.140f), new Vector3(-90f, 180f, 0f),
                    new[] { "Object_78" });

                // Cable gland on the front face: the loom has to start somewhere.
                Box("Cable Gland", psu, new Vector3(0f, 0.010f, -0.072f), new Vector3(0.058f, 0.030f, 0.008f), "Lab_PlasticDark");
                SetCollider(psuGo, new Vector3(0.15f, 0.09f, 0.14f));
            }

            BuildCableLoom(case_.transform);

            // Rear exhaust fan.
            Move("Cooling Fan Placeholder", case_.transform, new Vector3(0f, 0.130f, 0.196f), Vector3.one);
            var fan = ResetVisual("Cooling Fan Placeholder", out var fanGo);
            if (fan != null)
            {
                // The licensed 120 mm fan, laid flat against the rear panel: its thin
                // axis is the model's Y, which this rotation turns into the case's Z.
                ImportedVisual("Case Fan Model", fan, k_Item3D + "Fans/120mm_computer_fans.glb",
                    Vector3.zero, new Vector3(0.120f, 0.120f, 0.026f), new Vector3(90f, 0f, 0f));
                SetCollider(fanGo, new Vector3(0.12f, 0.12f, 0.03f));
            }

            // PSU rocker switch on the rear face.
            Move("PSU Switch", case_.transform, new Vector3(0.052f, -0.163f, 0.222f), Vector3.one);
            var sw = ResetVisual("PSU Switch", out var swGo);
            if (sw != null)
            {
                Box("Bezel", sw, Vector3.zero, new Vector3(0.030f, 0.020f, 0.008f), "Lab_PlasticDark");
                Box("Rocker", sw, new Vector3(0f, 0f, 0.004f), new Vector3(0.022f, 0.013f, 0.005f), "Lab_PlasticLight", new Vector3(8f, 0f, 0f));
                SetCollider(swGo, new Vector3(0.04f, 0.03f, 0.02f));
            }

            // THE FAULT: the 24-pin plug hanging free on the end of the loom, about a
            // hand's width below the header it belongs in. Nothing marks it — no glow,
            // no colour, no label, and no exaggerated gap. It is simply a connector that
            // is not in its socket.
            //
            // The board's front edge faces away to the participant's right from the
            // standing pose, so the plug is not the first thing seen; it is found by
            // stepping in and looking along the board, which is the point of the task.
            Move("Internal Cable Connector", case_.transform, new Vector3(0.048f, -0.048f, -0.034f), Vector3.one, new Vector3(0f, 0f, -18f));
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
                SetCollider(cableGo, new Vector3(0.06f, 0.09f, 0.05f));
            }
        }

        /// <summary>
        /// The one cable run the bench is allowed: the 24-pin lead out of the power
        /// supply's gland, along the floor and up the case front to the plug hanging
        /// short of its header.
        ///
        /// Swept along a curve rather than assembled from four angled boxes, so it reads
        /// as a cable that was routed rather than a row of blocks. The drive chain,
        /// card lead and front-panel ribbon are gone: there is no longer a SATA drive to
        /// feed, and they were three more dark sticks crossing the board.
        /// </summary>
        static void BuildCableLoom(Transform case_)
        {
            var loom = Group("Cable Loom", case_);

            CableRun("ATX Run", loom, 0.014f, "Lab_CableBlack",
                new Vector3(0.000f, -0.153f, 0.058f),   // out of the supply's gland
                new Vector3(0.030f, -0.190f, -0.010f),  // down onto the floor
                new Vector3(0.072f, -0.150f, -0.078f),  // forward along the front corner
                new Vector3(0.048f, -0.062f, -0.040f)); // up to where the plug hangs
        }

        /// <summary>
        /// Lays a cable along a cubic Bezier as a chain of short segments. Segments
        /// overlap slightly so the run has no gaps at the bends.
        /// </summary>
        static void CableRun(string name, Transform parent, float thickness, string material,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segments = 16)
        {
            var run = Group(name, parent);
            var previous = p0;
            for (var i = 1; i <= segments; i++)
            {
                var t = i / (float)segments;
                var point = Bezier(p0, p1, p2, p3, t);
                var delta = point - previous;
                var length = delta.magnitude;
                if (length > 0.0001f)
                {
                    var seg = Box($"Segment {i}", run, (previous + point) * 0.5f,
                        new Vector3(thickness, thickness, length * 1.25f), material);
                    seg.transform.localRotation = Quaternion.LookRotation(delta / length, Vector3.up);
                }
                previous = point;
            }
        }

        static Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }

        /// <summary>
        /// Bench contents.
        ///
        /// Everything loose now rests on a tray floor rather than hovering above the
        /// bench: the tray's floor sits 8 mm proud of the top, so each part is placed at
        /// that height plus its own half-depth. Nothing is arranged to hint at the
        /// answer — the replacement lead sits among the other spares, unmarked.
        /// </summary>
        static void PlaceBenchParts()
        {
            // Inside floor of the lab bench's own parts and tool trays.
            const float tray = BenchDressing.TrayFloor;

            // --- spares tray (left) ---
            // Replacement 24-pin lead: the same connector family as the plug hanging
            // in the case, so the two are recognisably a pair on inspection — but it
            // sits in the tray alongside the other spares, unmarked.
            Move("Main Power Connector", null, new Vector3(-1.28f, tray + 0.013f, 0.95f), Vector3.one, new Vector3(0f, 14f, 0f));
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
                SetCollider(mpcGo, new Vector3(0.10f, 0.05f, 0.13f));
            }

            // Spare DIMM, lying flat in the tray. Rotated so its 3 mm thickness is the
            // vertical axis: standing on edge it read as a blade rather than a part.
            // Resting on the tray's antistatic pad, which is what makes a bare dark
            // board part visible at all from the bench.
            Move("RAM Placeholder", null, new Vector3(-1.02f, tray + 0.007f, 0.95f), Vector3.one, new Vector3(0f, -8f, 0f));
            var ram = ResetVisual("RAM Placeholder", out var ramGo);
            if (ram != null)
            {
                ImportedVisual("RAM Model", ram, k_Item3D + "Optimized/RAM/random_access_memory_ram_ddr4_quest.glb",
                    Vector3.zero, new Vector3(0.135f, 0.004f, 0.032f), new Vector3(-90f, 0f, 0f));
                SetCollider(ramGo, new Vector3(0.15f, 0.03f, 0.05f));
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
                SetCollider(sideGo, new Vector3(0.45f, 0.05f, 0.47f));
            }

            // --- tool tray (right): the one tool the task needs ---
            BenchDressing.PlaceScrewdriver(new Vector3(1.02f, tray + 0.025f, 0.95f));

            // --- distractor: a sealed spare module, clearly not part of this repair.
            //     Moved in among the spares, where a spare belongs; out on the bench's
            //     right end it read as a second piece of equipment. ---
            Move("Computer Non Target Module", null, new Vector3(-0.78f, tray + 0.045f, 0.95f), Vector3.one, new Vector3(0f, -12f, 0f));
            var nonTarget = ResetVisual("Computer Non Target Module", out var nonTargetGo);
            if (nonTarget != null)
            {
                Box("Body", nonTarget, Vector3.zero, new Vector3(0.150f, 0.090f, 0.110f), "Lab_MetalDark");
                Box("Label", nonTarget, new Vector3(0f, 0.046f, 0f), new Vector3(0.110f, 0.002f, 0.070f), "Lab_LabelPlate");
                Box("Seal", nonTarget, new Vector3(0f, 0.047f, 0.030f), new Vector3(0.060f, 0.002f, 0.016f), "Lab_Warning");
                SetCollider(nonTargetGo, new Vector3(0.16f, 0.10f, 0.12f));
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
                SetCollider(extGo, new Vector3(0.08f, 0.06f, 0.06f));
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
                SetCollider(statusGo, new Vector3(0.10f, 0.20f, 0.10f));
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

        /// <summary>Grab colliders must be resized once the root scale is normalised to 1.</summary>
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
