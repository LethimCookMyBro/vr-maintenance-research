using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Asks whether any two solid props occupy the same space.
    ///
    /// Nothing else in the check set can see this. The scene integrity tests read
    /// components, the visual validator reads appearance and sight lines, and the ray
    /// aim report asks what a controller ray selects — a shelf standing inside a wall
    /// answers all three correctly and still looks broken to anyone in the headset.
    /// The guardrail that used to run down each side aisle is what prompted this: it
    /// cleared every existing check while fencing off the storage it stood in front of.
    ///
    /// == why oriented boxes, not the AABBs Unity hands out ==
    ///
    /// <see cref="Renderer.bounds"/> is the world-axis-aligned box around a rotated
    /// mesh, so the hazard sign — a plate turned 45 degrees — reports a box a third
    /// larger than the plate, and every neighbour inside that slack reads as a clash.
    /// A report whose entries are mostly false is a report nobody re-runs, so each
    /// renderer is tested as the oriented box it actually is, by separating axis.
    ///
    /// == what counts as a clash ==
    ///
    /// Touching is not overlapping: a crate resting on a shelf shares a plane with it,
    /// and a mat lying on a bench shares one too. Only interpenetration deeper than
    /// <see cref="k_Tolerance"/> on every axis at once is reported, which is the
    /// difference between two surfaces meeting and one solid inside another.
    ///
    /// == intended joints ==
    ///
    /// Most solids that share space are supposed to. A room is a box whose walls meet
    /// at the corners, a notice is screwed to the board behind it, a clamp grips the
    /// bench it hangs off, and a skirting is bedded into the wall above it. Geometry
    /// cannot tell those from a shelf buried in a wall, so the joints this room is
    /// built from are listed in <see cref="k_IntendedJoints"/> and everything not on
    /// that list is reported. The list is the design written down: adding a line to it
    /// claims two things are fastened together, and that should be as hard to justify
    /// as fixing the clash would have been.
    /// </summary>
    public static class ResearchPropIntersectionAudit
    {
        const string k_Path = "Assets/VRMaintenanceResearch/Docs/Verification/Prop_Intersections.txt";

        /// <summary>
        /// How far one solid may reach into another before it is called a clash.
        /// Two millimetres is under the thinnest decal in the room (the 4 mm ESD mat)
        /// and well over the float noise in a transform chain four levels deep.
        /// </summary>
        const float k_Tolerance = 0.002f;

        /// <summary>
        /// Scene objects that are furniture or dressing rather than task apparatus.
        /// A prop is the top of one of these; everything under it is one solid.
        /// </summary>
        static readonly string[] k_PropRoots =
        {
            "Lab Environment/Shell",
            "Lab Environment/Furniture",
            "Lab Environment/Information Station",
            "Lab Environment/Industrial Dressing",
            "Workstation Dressing",
            "Lab Notice Board",
            "Task Brief",
            "Information Dock",
            "Inspect Station Sign",
            "Research Floor",
        };

        /// <summary>
        /// Pairs of props that are fastened to each other, and why. A pair matches when
        /// one side matches the first pattern and the other the second, in either
        /// order; "*" ends a prefix.
        ///
        /// Everything here was read off the first run of this audit and kept only where
        /// the two solids are genuinely one assembly. What that run also turned up —
        /// racking buried in the east wall, two bench mats sharing the same four
        /// millimetres — is not here, because those were defects and were fixed.
        /// </summary>
        static readonly (string A, string B, string Why)[] k_IntendedJoints =
        {
            // The room is a box: its walls meet at four corners, and the skirting is
            // bedded into the wall it runs along and mitred where two runs meet.
            ("Shell/Wall*", "Shell/Wall*", "walls meet at the room corners"),
            ("Shell/Skirting*", "Shell/Skirting*", "skirting mitred at the corners"),
            ("Shell/Skirting*", "Shell/Wall*", "skirting bedded into the wall"),
            ("Shell/Skirting*", "Industrial Dressing/Two Tone Walls", "dado runs down behind the skirting"),
            ("Shell/Wall*", "Industrial Dressing/Two Tone Walls", "dado is a facing on the wall"),

            // The bench is welded from a top, an apron, four legs and a shelf, each of
            // which the builder places as its own object.
            ("Furniture/Workbench*", "Furniture/Workbench*", "bench is one weldment"),
            ("Furniture/Work Zone Line*", "Furniture/Work Zone Line*", "painted lines meet at the box corners"),

            // Fabrications the builders spread over several sibling objects, so the
            // same-assembly rule cannot see them as one prop: a plate in a bezel, a
            // cap on a post, a card recessed into its housing.
            ("Information Dock/*", "Information Dock/*", "dock is one fabrication"),
            ("Task Brief/*", "Task Brief/*", "brief board is one fabrication"),
            ("Information Station/*", "Information Station/*", "station board is one fabrication"),
            ("Inspect Station Sign/*", "Inspect Station Sign/*", "sign is one fabrication"),

            // Things hung on, clamped to, or resting on something else.
            ("Information Dock/Clamp", "Furniture/Workbench*", "the dock clamps to the bench"),
            ("Lab Notice Board/*", "Information Station/Station Backing", "notices are mounted on the board"),
            ("Task Brief/Clamp", "Furniture/Workbench*", "the brief clamps to the bench"),
            ("Workstation Dressing/Service Mat", "Workstation Dressing/Mat Edge", "the edge is inset in the mat"),
            ("Workstation Dressing/Tool Set", "Furniture/Tool Tray", "the tool kit sits in the tray"),
            ("Workstation Dressing/Placard*", "Furniture/*", "placards stand on the tray rims"),
        };

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Report Prop Intersections", priority = 9)]
        public static void Run()
        {
            var report = new StringBuilder("=== prop intersections ===\n");
            report.AppendLine("Every solid the participant can see but not select, tested against every");
            report.AppendLine("other one as an oriented box. Touching is allowed; interpenetration deeper");
            report.AppendLine($"than {k_Tolerance * 1000f:0} mm on all three axes at once is a clash.");
            report.AppendLine();
            report.AppendLine("Joints the room is built from are not clashes and are listed separately");
            report.AppendLine("below, so that the list of what is deliberately fastened to what stays");
            report.AppendLine("visible rather than turning into a silent filter.");
            report.AppendLine();

            var total = 0;
            foreach (var scenePath in ResearchSceneSet.AllScenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var clashes = Evaluate();
                total += clashes.Count;

                report.AppendLine($"=== {scene.name}: {(clashes.Count == 0 ? "clear" : clashes.Count + " clash(es)")}");
                foreach (var clash in clashes)
                    report.AppendLine("  " + clash);
                report.AppendLine();
            }

            report.AppendLine(total == 0
                ? "NO PROP INTERSECTS ANOTHER"
                : $"{total} INTERSECTION(S) ACROSS {ResearchSceneSet.AllScenes.Length} SCENES");

            report.AppendLine();
            report.AppendLine("=== joints treated as intended ===");
            foreach (var (a, b, why) in k_IntendedJoints)
                report.AppendLine($"  {a} + {b} — {why}");

            System.IO.File.WriteAllText(k_Path, report.ToString());
            AssetDatabase.ImportAsset(k_Path);
            Debug.Log("[PropIntersections]\n" + report);
        }

        /// <summary>
        /// Every clash in the open scene, as printable lines. The scene integrity test
        /// asserts on this same list, so the report and the gate cannot disagree.
        /// </summary>
        public static List<string> Evaluate()
        {
            var solids = Collect();
            var clashes = new List<string>();

            for (var i = 0; i < solids.Count; i++)
            for (var j = i + 1; j < solids.Count; j++)
            {
                var a = solids[i];
                var b = solids[j];
                if (Intended(a, b))
                    continue;
                if (!Overlaps(a, b, out var depth))
                    continue;

                clashes.Add($"{a.Prop} '{a.Name}' X {b.Prop} '{b.Name}' — {depth * 1000f:0} mm deep");
            }

            return clashes.OrderBy(line => line).ToList();
        }

        /// <summary>
        /// Whether two solids are supposed to share space.
        ///
        /// One prop is one assembly: a tray's rims are let into its base, a rail runs
        /// through its posts, a cap sits on a column. Those are how the thing is made
        /// and are never reported. The exception is items placed into a prop one at a
        /// time — a crate onto a shelf, a case into the storage unit — where a crate
        /// inside another crate is the same defect as a crate inside a wall.
        /// </summary>
        static bool Intended(Solid a, Solid b)
        {
            if (a.Prop == b.Prop)
                return !(Placed(a.Name) && Placed(b.Name));

            foreach (var (patternA, patternB, _) in k_IntendedJoints)
            {
                if (Matches(a.Prop, patternA) && Matches(b.Prop, patternB))
                    return true;
                if (Matches(a.Prop, patternB) && Matches(b.Prop, patternA))
                    return true;
            }

            return false;
        }

        static bool Placed(string name) =>
            name.StartsWith("Crate") || name.StartsWith("Storage Case");

        static bool Matches(string prop, string pattern) =>
            pattern.EndsWith("*")
                ? prop.StartsWith(pattern.Substring(0, pattern.Length - 1))
                : prop == pattern;

        sealed class Solid
        {
            public string Prop;
            public string Name;
            public Bounds Local;
            public Matrix4x4 ToWorld;
        }

        static List<Solid> Collect()
        {
            var solids = new List<Solid>();
            foreach (var path in k_PropRoots)
            {
                var root = Find(path);
                if (root == null)
                    continue;

                foreach (var renderer in root.GetComponentsInChildren<Renderer>(false))
                {
                    // Text is drawn on a plate that is already tested; a TMP mesh is a
                    // sheet of glyph quads whose box is meant to sit inside its sign.
                    if (renderer is TMPro.TMP_SubMeshUI || renderer.GetComponent<TMPro.TMP_Text>() != null)
                        continue;
                    // Task apparatus is not dressing. Anything selectable is checked by
                    // the ray aim report instead, which asks the question that matters
                    // for a part: not "does it overlap" but "what does a ray select".
                    if (renderer.GetComponentInParent<ResearchInteractable>() != null)
                        continue;

                    solids.Add(new Solid
                    {
                        Prop = PropOf(renderer.transform, root.transform),
                        Name = renderer.name,
                        Local = renderer.localBounds,
                        ToWorld = renderer.localToWorldMatrix,
                    });
                }
            }

            return solids;
        }

        /// <summary>
        /// The prop a renderer belongs to: the highest ancestor below the prop root.
        /// "Industrial Dressing/Racking East/Shelf 2" is one piece of the prop
        /// "Industrial Dressing/Racking East".
        /// </summary>
        static string PropOf(Transform t, Transform root)
        {
            var node = t;
            while (node.parent != null && node.parent != root)
                node = node.parent;
            return root.name + "/" + node.name;
        }

        /// <summary>
        /// Separating axis test on two oriented boxes: they overlap only if no axis
        /// separates them, and the depth reported is the shallowest overlap found,
        /// which is how far one would have to move to come apart.
        /// </summary>
        static bool Overlaps(Solid a, Solid b, out float depth)
        {
            depth = float.MaxValue;

            var axes = new List<Vector3>(15);
            for (var i = 0; i < 3; i++)
            {
                axes.Add(Axis(a.ToWorld, i));
                axes.Add(Axis(b.ToWorld, i));
            }

            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
            {
                var cross = Vector3.Cross(Axis(a.ToWorld, i), Axis(b.ToWorld, j));
                // Parallel edges give a zero cross product and no usable axis; the six
                // face normals already cover that case.
                if (cross.sqrMagnitude > 1e-6f)
                    axes.Add(cross.normalized);
            }

            var cornersA = Corners(a);
            var cornersB = Corners(b);
            foreach (var axis in axes)
            {
                Project(cornersA, axis, out var minA, out var maxA);
                Project(cornersB, axis, out var minB, out var maxB);

                var overlap = Mathf.Min(maxA, maxB) - Mathf.Max(minA, minB);
                if (overlap <= k_Tolerance)
                {
                    depth = 0f;
                    return false;
                }

                depth = Mathf.Min(depth, overlap);
            }

            return true;
        }

        static Vector3 Axis(Matrix4x4 m, int index) => ((Vector3)m.GetColumn(index)).normalized;

        static Vector3[] Corners(Solid solid)
        {
            var c = solid.Local.center;
            var e = solid.Local.extents;
            var corners = new Vector3[8];
            var n = 0;
            for (var x = -1; x <= 1; x += 2)
            for (var y = -1; y <= 1; y += 2)
            for (var z = -1; z <= 1; z += 2)
                corners[n++] = solid.ToWorld.MultiplyPoint3x4(c + new Vector3(e.x * x, e.y * y, e.z * z));
            return corners;
        }

        static void Project(Vector3[] corners, Vector3 axis, out float min, out float max)
        {
            min = max = Vector3.Dot(corners[0], axis);
            for (var i = 1; i < corners.Length; i++)
            {
                var d = Vector3.Dot(corners[i], axis);
                if (d < min) min = d;
                if (d > max) max = d;
            }
        }

        static GameObject Find(string path)
        {
            var parts = path.Split('/');
            var current = GameObject.Find(parts[0]);
            for (var i = 1; i < parts.Length && current != null; i++)
            {
                var child = current.transform.Find(parts[i]);
                current = child == null ? null : child.gameObject;
            }

            return current;
        }
    }
}
