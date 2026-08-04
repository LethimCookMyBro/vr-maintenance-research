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

            // Names the lower shelf, so the panel lying on it reads as a part taken off
            // this machine rather than one more component waiting to go in. Has to come
            // after BenchDressing.Build, which rebuilds the root this hangs from.
            var dressing = GameObject.Find("Workstation Dressing");
            if (dressing != null)
                BenchDressing.Zone(dressing.transform, new Vector3(-1.25f, 0.372f, 0.664f), "REMOVED PARTS");

            // Stowed last, so a rebuild still refreshes the part before putting it away.
            //
            // A sealed anonymous module in the spares tray is the wrong prompt for a
            // diagnostic task: it is neither the fault, nor a tool, nor anything the
            // work order asks about, and beside an open case it reads as one more thing
            // to install. It keeps its StableObjectId and its collider — reactivating
            // the object restores it exactly.
            Stow("Computer Non Target Module");

            // The in-case parts are children of Desktop Case, so the case's interactable
            // was claiming their colliders and answering rays meant for them.
            BindOwnColliders();

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

        /// <summary>
        /// The chassis.
        ///
        /// The previous shell was a five-sided grey box with a 5.25"-bay bezel: a
        /// beige-era tower drawn around a licensed motherboard, a Wraith cooler and a
        /// modular supply. Between the pale steel and the two 5.25" bays it read as
        /// the oldest object in the room, and it made the machine inside look like
        /// parts in a carton rather than a built PC with one panel off.
        ///
        /// It is now the mid-tower those components come out of: matte graphite
        /// interior, a full-height mesh intake, a glass panel on the closed side, a
        /// vented roof and a basement divider. Nothing structural changed — the same
        /// half-extents, the same open side, the same interior anchor points — so
        /// every part placement below still lands where it did.
        /// </summary>
        static void BuildCaseShell(Transform root)
        {
            if (root == null)
                return;

            const string steel = "Lab_CaseSteel";
            const string panel = "Lab_CasePanel";
            const string inner = "Lab_CaseInterior";

            // --- shell: left side panel is absent, that is the service opening ---
            // The closed side is a glass panel in a steel frame — the twin of the one
            // stowed on the bench shelf, so the removed panel is recognisably this
            // machine's.
            Box("Shell Right Frame", root, new Vector3(k_W - 0.003f, 0f, 0f), new Vector3(0.006f, k_H * 2f, k_D * 2f), steel);
            Box("Shell Right Glass", root, new Vector3(k_W - 0.008f, 0.010f, 0.008f), new Vector3(0.004f, k_H * 2f - 0.034f, k_D * 2f - 0.038f), "Lab_GlassPanel");

            Box("Shell Top", root, new Vector3(0f, k_H - 0.004f, 0f), new Vector3(k_W * 2f, 0.008f, k_D * 2f), panel);
            Box("Shell Bottom", root, new Vector3(0f, -k_H + 0.004f, 0f), new Vector3(k_W * 2f, 0.008f, k_D * 2f), panel);
            Box("Shell Rear", root, new Vector3(0f, 0f, k_D - 0.005f), new Vector3(k_W * 2f, k_H * 2f, 0.010f), steel);

            // Matte graphite liners. These used to be pale, which is what turned the
            // cavity into a lit white box; the components now read against them.
            Box("Liner Top", root, new Vector3(0f, k_H - 0.010f, 0f), new Vector3(k_W * 2f - 0.006f, 0.004f, k_D * 2f - 0.014f), inner);
            Box("Liner Bottom", root, new Vector3(0f, -k_H + 0.010f, 0f), new Vector3(k_W * 2f - 0.006f, 0.004f, k_D * 2f - 0.014f), inner);
            Box("Liner Rear", root, new Vector3(0f, 0f, k_D - 0.012f), new Vector3(k_W * 2f - 0.006f, k_H * 2f - 0.014f, 0.004f), inner);

            // Roof vent: slots over the rear two thirds, where the exhaust is.
            for (var i = 0; i < 9; i++)
                Box($"Roof Slot {i + 1}", root, new Vector3(0f, k_H - 0.007f, -0.040f + i * 0.028f), new Vector3(k_W * 2f - 0.048f, 0.004f, 0.014f), "Lab_CaseMesh");

            // Open-edge lip: without it the missing panel looks like a modelling error.
            Box("Open Lip Front", root, new Vector3(-k_W + 0.006f, 0f, -k_D + 0.012f), new Vector3(0.012f, k_H * 2f, 0.014f), steel);
            Box("Open Lip Rear", root, new Vector3(-k_W + 0.006f, 0f, k_D - 0.012f), new Vector3(0.012f, k_H * 2f, 0.014f), steel);
            Box("Open Lip Top", root, new Vector3(-k_W + 0.006f, k_H - 0.012f, 0f), new Vector3(0.012f, 0.014f, k_D * 2f), steel);
            Box("Open Lip Bottom", root, new Vector3(-k_W + 0.006f, -k_H + 0.012f, 0f), new Vector3(0.012f, 0.014f, k_D * 2f), steel);

            // Thumb screws on the rear lip: they say the panel was unscrewed, not lost.
            foreach (var y in new[] { 0.150f, -0.150f })
                Cyl($"Panel Screw {(y > 0 ? "A" : "B")}", root, new Vector3(-k_W + 0.006f, y, k_D - 0.004f), new Vector3(0.016f, 0.004f, 0.016f), "Lab_ToolSteel", new Vector3(90f, 0f, 0f));

            BuildFrontBezel(root);
            BuildRearIO(root);
            BuildMotherboard(root);
            BuildFeet(root);
        }

        /// <summary>
        /// Front: a full-height mesh intake behind a chamfered graphite bezel, with the
        /// I/O cluster a current case actually carries.
        ///
        /// The two 5.25" optical bays are gone. They were the single strongest "old
        /// computer" cue on the machine and no participant is asked to do anything
        /// with them.
        /// </summary>
        static void BuildFrontBezel(Transform root)
        {
            var bezel = Group("Front Bezel", root, new Vector3(0f, 0f, -k_D + 0.006f));

            Box("Face", bezel, Vector3.zero, new Vector3(k_W * 2f + 0.006f, k_H * 2f + 0.006f, 0.014f), "Lab_PlasticDark");

            // Recessed intake behind the bezel, so the slats read as a way in rather
            // than as stripes painted on a slab.
            Box("Intake Well", bezel, new Vector3(-0.008f, -0.020f, 0.002f), new Vector3(0.150f, 0.330f, 0.008f), "Lab_CaseMesh");
            for (var i = 0; i < 16; i++)
                Box($"Intake Slat {i + 1}", bezel, new Vector3(-0.008f, -0.176f + i * 0.0208f, -0.008f), new Vector3(0.150f, 0.011f, 0.006f), "Lab_PlasticDark");

            // Brushed strip down the open-side edge, and a chamfer along the other.
            Box("Edge Strip", bezel, new Vector3(0.074f, 0f, -0.008f), new Vector3(0.030f, k_H * 2f - 0.012f, 0.006f), "Lab_MetalDark");
            Box("Chamfer", bezel, new Vector3(-k_W + 0.004f, 0f, -0.006f), new Vector3(0.014f, k_H * 2f - 0.006f, 0.010f), "Lab_CasePanel", new Vector3(0f, 0f, 0f));

            // Top I/O cluster, on the brushed strip.
            Cyl("Power Button", bezel, new Vector3(0.074f, 0.186f, -0.012f), new Vector3(0.020f, 0.004f, 0.020f), "Lab_PlasticLight", new Vector3(90f, 0f, 0f));
            Cyl("Power Ring", bezel, new Vector3(0.074f, 0.186f, -0.014f), new Vector3(0.012f, 0.003f, 0.012f), "Lab_MetalDark", new Vector3(90f, 0f, 0f));
            for (var i = 0; i < 2; i++)
                Box($"USB A {i + 1}", bezel, new Vector3(0.074f, 0.146f - i * 0.024f, -0.012f), new Vector3(0.017f, 0.007f, 0.005f), "Lab_Metal");
            Box("USB C", bezel, new Vector3(0.074f, 0.098f, -0.012f), new Vector3(0.012f, 0.005f, 0.005f), "Lab_Metal");
            Cyl("Audio Jack", bezel, new Vector3(0.074f, 0.072f, -0.012f), new Vector3(0.010f, 0.003f, 0.010f), "Lab_PlasticDark", new Vector3(90f, 0f, 0f));

            // Badge on the bezel's foot: a case with a name reads as a product.
            Box("Badge", bezel, new Vector3(-0.008f, -0.202f, -0.010f), new Vector3(0.052f, 0.012f, 0.004f), "Lab_MetalDark");
        }

        static void BuildRearIO(Transform root)
        {
            var rear = Group("Rear IO", root, new Vector3(0f, 0f, k_D - 0.012f));

            // No handmade I/O shield or ports here: the board model carries its own
            // cluster and it now meets this panel, so building a second set would put
            // two stacks of ports a few millimetres apart.

            // Exhaust grille around the rear fan, which sits at y = +0.130.
            for (var i = 0; i < 9; i++)
                Box($"Fan Grille {i + 1}", rear, new Vector3(-0.006f, 0.074f + i * 0.014f, -0.004f), new Vector3(0.116f, 0.005f, 0.004f), "Lab_CaseMesh");

            // Expansion slot covers, seven as a mid-tower has, each with its screw.
            for (var i = 0; i < 7; i++)
            {
                Box($"Slot Cover {i + 1}", rear, new Vector3(0.030f, 0.036f - i * 0.021f, 0f), new Vector3(0.110f, 0.016f, 0.005f), "Lab_MetalDark");
                Cyl($"Slot Screw {i + 1}", rear, new Vector3(0.078f, 0.036f - i * 0.021f, -0.004f), new Vector3(0.009f, 0.003f, 0.009f), "Lab_ToolSteel", new Vector3(90f, 0f, 0f));
            }

            Box("PSU Cutout", rear, new Vector3(0f, -0.163f, 0f), new Vector3(0.16f, 0.086f, 0.008f), "Lab_CaseSteel");
        }

        // No basement divider. One was built here to give the supply a bay and fill the
        // empty lower cavity, and from the participant's approach — which looks straight
        // into the open side — a full-width shelf at that height reads as a drawer
        // pulled out of the machine. The cavity below the board is meant to look like
        // an empty basement, because that is what it is.

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
            BuildStorage(board);
            BuildAtxHeader(board);
            BuildGraphicsCard(board);
        }

        /// <summary>
        /// The solid-state drive, on the board.
        ///
        /// It used to sit in a handmade drive cage: a 120 mm box standing on the case
        /// floor in the middle of the cavity, which from every angle read as a filing
        /// cabinet someone had left inside the computer. The cage is gone.
        ///
        /// The licensed part is an M.2 stick and an M.2 stick mounts flat on the
        /// board, so that is where it goes. It was also being fitted wrong: the fit is
        /// uniform on the tightest axis, and the box it was given was not proportional
        /// to the mesh, so the 80 mm drive came out 36 mm long and under a millimetre
        /// thick. The box below is the mesh's own 1.190 x 0.104 x 4.289 ratio taken to
        /// a true 80 x 22 x 2 mm.
        /// </summary>
        static void BuildStorage(Transform board)
        {
            // Rotated so the mesh's thin axis points out of the board: the drive lies
            // on the board face rather than standing off it on edge. -90 rather than
            // +90 because the kit's printed face is the mesh's -Y side: turned the
            // other way the Samsung artwork was legible but mirrored, which is what a
            // double-sided label quad looks like from behind.
            ImportedVisual("Solid State Drive", board, k_Item3D + "Storage/source/ssd-kit.glb",
                OnBoard(-0.010f, 0.012f, 0.0015f), new Vector3(0.0020f, 0.0222f, 0.0800f), new Vector3(0f, 0f, -90f),
                new[] { "Circle.002" });   // the kit is two drives; the bench is allowed one

            Box("M2 Standoff", board, OnBoard(0.028f, 0.012f, 0.0010f), new Vector3(0.003f, 0.008f, 0.008f), "Lab_MetalDark");
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

        // Memory slot centres, measured off the board map. The second slot is built
        // here as dressing; the fourth is where the computer.ram interactable is
        // seated, so the pair a technician would populate is the pair on the board.
        static readonly Vector3 k_MemorySlotA = new Vector3(0.0711f, 0.0726f, 0.018f);
        static readonly Vector3 k_MemorySlotB = new Vector3(0.0862f, 0.0726f, 0.018f);
        static readonly Vector3 k_MemoryFit = new Vector3(0.031f, 0.135f, 0.006f);
        static readonly Vector3 k_MemoryEuler = new Vector3(0f, 0f, 90f);
        const string k_MemoryModel = k_Item3D + "Optimized/RAM/random_access_memory_ram_ddr4_quest.glb";

        /// <summary>
        /// One dressing DIMM in the second slot. The fourth slot is filled by the
        /// computer.ram interactable, which PlaceInteriorParts seats there.
        ///
        /// Slot centres were measured off the board map, so the modules land in the
        /// slots the board draws rather than beside them. They stand vertically, which
        /// is how DIMMs sit in a tower.
        /// </summary>
        static void BuildMemory(Transform board)
        {
            ImportedVisual("RAM Module 1", board, k_MemoryModel, OnBoard(k_MemorySlotA.x, k_MemorySlotA.y, k_MemorySlotA.z),
                k_MemoryFit, k_MemoryEuler);
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
            // Etched, not printed. In Lab_LabelPlate this was a near-white 52 x 48 mm
            // rectangle on the brightest face of the largest part in the cavity, and it
            // was the first thing the eye landed on inside the machine.
            Box("Backplate Label", gpu, new Vector3(-0.010f, 0.008f, -0.070f), new Vector3(0.044f, 0.001f, 0.026f), "Lab_CasePanel");
            Box("Shroud", gpu, new Vector3(0f, -0.020f, 0f), new Vector3(0.098f, 0.038f, 0.222f), "Lab_PlasticDark");
            // The card's outer edge, the one face that is square-on to the open panel.
            // In Lab_MetalDark this was a 222 mm strip of light metallic grey across the
            // middle of the cavity, and it — not the backplate — was what made the card
            // read as a bright shelf rather than as a component.
            Box("Shroud Edge", gpu, new Vector3(-0.048f, -0.020f, 0f), new Vector3(0.004f, 0.038f, 0.222f), "Lab_PlasticDark");

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
        /// Feet, raised enough that the bottom intake filter under the supply reads as
        /// an air path rather than a painted stripe.
        /// </summary>
        static void BuildFeet(Transform root)
        {
            foreach (var (x, z) in new[] { (-0.078f, -0.185f), (0.078f, -0.185f), (-0.078f, 0.185f), (0.078f, 0.185f) })
            {
                Box($"Foot {x}_{z}", root, new Vector3(x, -k_H - 0.011f, z), new Vector3(0.034f, 0.022f, 0.038f), "Lab_PlasticDark");
                Box($"Foot Pad {x}_{z}", root, new Vector3(x, -k_H - 0.021f, z), new Vector3(0.030f, 0.004f, 0.034f), "Lab_Rubber");
            }

            Box("Intake Filter", root, new Vector3(0f, -k_H - 0.002f, 0.110f), new Vector3(k_W * 2f - 0.030f, 0.004f, 0.170f), "Lab_CaseMesh");
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

            // Memory is an installed component, not a spare.
            //
            // computer.ram used to lie on an antistatic pad in the spares tray, which
            // is the single clearest "build this computer" cue a bench can carry: a
            // bare DIMM on a pad is what you see when a machine is being assembled, not
            // when one is being diagnosed. The interactable is the same object with the
            // same id and the same collider — it is now seated in the fourth slot, so
            // the board is populated and the tray is not.
            var memorySeat = k_BoardCentre + OnBoard(k_MemorySlotB.x, k_MemorySlotB.y, k_MemorySlotB.z);
            Move("RAM Placeholder", case_.transform, memorySeat, Vector3.one);
            var seatedRam = ResetVisual("RAM Placeholder", out var seatedRamGo);
            if (seatedRam != null)
            {
                ImportedVisual("RAM Model", seatedRam, k_MemoryModel, Vector3.zero, k_MemoryFit, k_MemoryEuler);
                SetCollider(seatedRamGo, new Vector3(0.030f, 0.140f, 0.014f));
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
        /// This is a diagnostic bench, so it carries what a technician brings to a
        /// machine that will not power on and nothing else: one replacement lead in
        /// the spares tray, one screwdriver in the tool tray, the mains lead, and the
        /// side panel that came off the machine, stowed on the lower shelf.
        ///
        /// What used to be here as well — a bare DIMM on an antistatic pad and a
        /// sealed module — described a different task. A tray of assorted components
        /// beside an open case is an assembly kit, and a participant who reads the
        /// bench that way starts looking for what to fit rather than for what is
        /// wrong. The DIMM is now installed on the board; the sealed module is stowed
        /// at the end of this method.
        ///
        /// Everything loose rests on a tray floor rather than hovering above the
        /// bench. Nothing is arranged to hint at the answer.
        /// </summary>
        static void PlaceBenchParts()
        {
            // Inside floor of the lab bench's own parts and tool trays.
            const float tray = BenchDressing.TrayFloor;

            // --- spares tray (left) ---
            // Replacement 24-pin lead: the same connector family as the plug hanging
            // in the case, so the two are recognisably a pair on inspection — but it
            // sits in the tray alongside the other spares, unmarked.
            // Centred in the tray now that it is the only thing in it; at the tray's
            // left end with two neighbours it was fine, alone it read as a part that
            // had been pushed aside. Not moved further left than this: the task volume
            // these parts define would reach the information dock and the validator
            // fails the scene for a reader that would open over the work area.
            Move("Main Power Connector", null, new Vector3(-1.10f, tray + 0.013f, 0.95f), Vector3.one, new Vector3(0f, 14f, 0f));
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
            var go = FindAny(name);
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
