using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static TMUVR.MaintenanceResearch.EditorTools.ResearchBuildKit;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// The training bench was three untextured primitives — a blue cube, a white
    /// cube and a grey cylinder — on an otherwise empty 4.6 m bench, which told the
    /// participant nothing about what to do. Each practice object now looks like the
    /// interaction it teaches, and the socket reads as a target rather than a slab.
    /// </summary>
    public static class TrainingSceneBuilder
    {
        const float k_BenchTop = BenchDressing.BenchTop;

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Rebuild Training Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ResearchSceneSet.Training, OpenSceneMode.Single);

            BuildPracticeParts();
            BuildSocket();
            BuildStationSigns();
            BenchDressing.Build(0f, 1.30f, 0.62f, withTools: false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[TrainingScene] rebuilt");
        }

        static void BuildPracticeParts()
        {
            // Practice part 1: a module with a grip, teaching "pick up".
            Move("Training Cube A", new Vector3(-0.42f, k_BenchTop + 0.052f, 0.95f), new Vector3(0f, 12f, 0f));
            var a = ResetVisual("Training Cube A", out var aGo);
            if (a != null)
            {
                Box("Body", a, Vector3.zero, new Vector3(0.110f, 0.090f, 0.110f), "Lab_Navy");
                Box("Face", a, new Vector3(0f, 0f, -0.056f), new Vector3(0.086f, 0.066f, 0.004f), "Lab_Accent");
                Box("Grip", a, new Vector3(0f, 0.055f, 0f), new Vector3(0.030f, 0.028f, 0.070f), "Lab_PlasticDark");
                Box("Label", a, new Vector3(0f, 0.047f, 0.040f), new Vector3(0.060f, 0.002f, 0.026f), "Lab_LabelPlate");
                SetCollider(aGo, new Vector3(0.14f, 0.14f, 0.14f));
                PartLabel(a, "PART A");
            }

            // Practice part 2: same family, neutral reference for visual comparison.
            Move("Training Cube B", new Vector3(0f, k_BenchTop + 0.052f, 0.95f), new Vector3(0f, -8f, 0f));
            var b = ResetVisual("Training Cube B", out var bGo);
            if (b != null)
            {
                Box("Body", b, Vector3.zero, new Vector3(0.110f, 0.090f, 0.110f), "Lab_Trim");
                Box("Face", b, new Vector3(0f, 0f, -0.056f), new Vector3(0.086f, 0.066f, 0.004f), "Lab_Accent");
                Box("Grip", b, new Vector3(0f, 0.055f, 0f), new Vector3(0.030f, 0.028f, 0.070f), "Lab_PlasticDark");
                Box("Label", b, new Vector3(0f, 0.047f, 0.040f), new Vector3(0.060f, 0.002f, 0.026f), "Lab_LabelPlate");
                SetCollider(bGo, new Vector3(0.14f, 0.14f, 0.14f));
                PartLabel(b, "REFERENCE");
            }

            // Practice part 3: a knob, teaching "turn / activate".
            Move("Training Cylinder", new Vector3(0.42f, k_BenchTop + 0.046f, 0.95f), Vector3.zero);
            var c = ResetVisual("Training Cylinder", out var cGo);
            if (c != null)
            {
                Cyl("Base", c, new Vector3(0f, -0.030f, 0f), new Vector3(0.110f, 0.010f, 0.110f), "Lab_Navy");
                Cyl("Knob", c, Vector3.zero, new Vector3(0.090f, 0.030f, 0.090f), "Lab_PlasticDark");
                Cyl("Cap", c, new Vector3(0f, 0.032f, 0f), new Vector3(0.070f, 0.004f, 0.070f), "Lab_Metal");
                Box("Pointer", c, new Vector3(0f, 0.036f, -0.026f), new Vector3(0.008f, 0.003f, 0.036f), "Lab_Accent");
                for (var i = 0; i < 8; i++)
                    Box($"Grip Ridge {i + 1}", c, Vector3.zero, new Vector3(0.094f, 0.026f, 0.010f), "Lab_PlasticDark", new Vector3(0f, i * 22.5f, 0f));
                SetCollider(cGo, new Vector3(0.13f, 0.10f, 0.13f));
                PartLabel(c, "DIAL");
            }
        }

        /// <summary>The place-here target: a recessed cradle with an outline, not a flat slab.</summary>
        static void BuildSocket()
        {
            Move("Training Socket", new Vector3(1.10f, k_BenchTop + 0.020f, 0.95f), Vector3.zero);
            var socket = ResetVisual("Training Socket", out var socketGo);
            if (socket == null)
                return;

            Box("Plinth", socket, new Vector3(0f, -0.012f, 0f), new Vector3(0.300f, 0.024f, 0.260f), "Lab_Navy");
            Box("Recess", socket, new Vector3(0f, 0.002f, 0f), new Vector3(0.150f, 0.014f, 0.150f), "Lab_PlasticDark");

            // Dashed outline: reads as "put it here" at a glance.
            for (var i = 0; i < 4; i++)
            {
                var horizontal = i < 2;
                var offset = i % 2 == 0 ? -0.078f : 0.078f;
                Box($"Outline {i + 1}", socket,
                    new Vector3(horizontal ? 0f : offset, 0.010f, horizontal ? offset : 0f),
                    horizontal ? new Vector3(0.170f, 0.004f, 0.008f) : new Vector3(0.008f, 0.004f, 0.170f),
                    "Lab_Accent");
            }

            for (var i = 0; i < 4; i++)
            {
                var x = i % 2 == 0 ? -0.128f : 0.128f;
                var z = i < 2 ? -0.108f : 0.108f;
                Cyl($"Corner Stud {i + 1}", socket, new Vector3(x, 0.004f, z), new Vector3(0.020f, 0.008f, 0.020f), "Lab_MetalDark");
            }

            var label = Label("Socket Label", socket, new Vector3(0f, 0.014f, 0.104f), "PLACE TO COMPARE", 0.27f, "#CFE0FF", new Vector3(90f, 0f, 0f), 0.30f);
            label.fontStyle = TMPro.FontStyles.Bold;
            label.characterSpacing = 6f;

            SetCollider(socketGo, new Vector3(0.32f, 0.10f, 0.28f));
        }

        /// <summary>Numbered step markers, so the bench itself explains the training order.</summary>
        static void BuildStationSigns()
        {
            var root = Root("Training Signage", Vector3.zero);
            var t = root.transform;

            Sign(t, new Vector3(-0.42f, k_BenchTop + 0.135f, 0.84f), "1  PICK UP");
            Sign(t, new Vector3(0f, k_BenchTop + 0.135f, 0.84f), "2  COMPARE");
            Sign(t, new Vector3(0.42f, k_BenchTop + 0.135f, 0.84f), "3  TURN");

            // Reset control gets a caption; it was an unlabelled puck on a pedestal.
            var reset = Label("Reset Caption", t, new Vector3(-1.95f, 1.00f, 0.14f), "RESET", 0.30f, "#DFE6EE", new Vector3(30f, 0f, 0f), 0.4f);
            reset.fontStyle = TMPro.FontStyles.Bold;
            reset.characterSpacing = 8f;
        }

        static void Sign(Transform parent, Vector3 pos, string text)
        {
            var plaque = Group($"Step {text}", parent, pos);
            Box("Backing", plaque, Vector3.zero, new Vector3(0.30f, 0.070f, 0.012f), "Lab_Navy");
            Box("Rule", plaque, new Vector3(0f, -0.042f, -0.008f), new Vector3(0.30f, 0.008f, 0.010f), "Lab_Accent");
            var label = Label("Text", plaque, new Vector3(0f, 0f, -0.008f), text, 0.34f, "#DFE6EE", Vector3.zero, 0.28f);
            label.fontStyle = TMPro.FontStyles.Bold;
            label.characterSpacing = 8f;
        }

        static void PartLabel(Transform parent, string text)
        {
            var label = Label("Part Label", parent, new Vector3(0f, 0.049f, 0.040f), text, 0.14f, "#1E2A3A", new Vector3(90f, 0f, 0f), 0.12f);
            label.fontStyle = TMPro.FontStyles.Bold;
        }

        static void Move(string name, Vector3 position, Vector3 euler)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"[TrainingScene] missing {name}");
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
            if (box != null)
            {
                box.center = Vector3.zero;
                box.size = size;
                return;
            }
            // Training Cylinder ships with a CapsuleCollider.
            var capsule = go.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                capsule.center = Vector3.zero;
                capsule.radius = size.x * 0.5f;
                capsule.height = size.y;
            }
        }
    }
}
