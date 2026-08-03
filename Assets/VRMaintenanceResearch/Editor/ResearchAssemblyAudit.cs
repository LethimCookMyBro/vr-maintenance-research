using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Geometry checks for the rebuilt computer assembly.
    ///
    /// The rejected model integration passed the visual validator and still shipped a
    /// graphics card floating through the drive cage, a cooler stood off against the
    /// rear wall, a power supply hanging under the case and two drives outside the
    /// shell entirely. None of that is a taste question, so none of it should need an
    /// eye to catch: a part that is not inside the case is a containment failure and a
    /// part that shares volume with another is an intersection, and both are numbers.
    ///
    /// Reports rather than fails. Coolers are supposed to touch processors and cards
    /// are supposed to touch slots, so a pass/fail oracle here would only teach us to
    /// ignore it; the report is read at every rebuild stage.
    /// </summary>
    public static class ResearchAssemblyAudit
    {
        const string k_ReportPath = "Assets/VRMaintenanceResearch/Docs/Verification/Assembly_Audit.txt";

        /// <summary>
        /// Interior parts and the real-world size each should read as (metres), taken
        /// from the actual part: ATX board 305 x 244, AM4 processor 40 mm square, Wraith
        /// Stealth 95 mm across and 49 mm tall, DDR4 DIMM 133 x 31, RTX 4060 Ti roughly
        /// 245 mm long, SFX-ish supply 150 x 86 x 140, 2.5" drive 100 x 70 x 7, 120 mm
        /// case fan.
        /// </summary>
        static readonly (string Name, Vector3 Real)[] k_Parts =
        {
            ("Motherboard Model",  new Vector3(0.022f, 0.305f, 0.244f)),
            ("CPU Package",        new Vector3(0.008f, 0.040f, 0.040f)),
            ("CPU Cooler",         new Vector3(0.049f, 0.095f, 0.092f)),
            ("RAM Module 1",       new Vector3(0.031f, 0.133f, 0.003f)),
            ("RAM Module 2",       new Vector3(0.031f, 0.133f, 0.003f)),
            ("Graphics Card",      new Vector3(0.040f, 0.120f, 0.245f)),
            ("PSU Model",          new Vector3(0.150f, 0.086f, 0.140f)),
            ("Solid State Drive",  new Vector3(0.007f, 0.070f, 0.100f)),
            ("Case Fan Model",     new Vector3(0.120f, 0.120f, 0.025f)),
        };

        /// <summary>Pairs that share volume because the real parts do.</summary>
        static readonly HashSet<string> k_Expected = new HashSet<string>
        {
            "CPU Cooler|CPU Package",
            "CPU Package|Motherboard Model",
            "CPU Cooler|Motherboard Model",
            "Motherboard Model|RAM Module 1",
            "Motherboard Model|RAM Module 2",
            "Graphics Card|Motherboard Model",
        };

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Audit Computer Assembly", priority = 2)]
        public static void Audit()
        {
            EditorSceneManager.OpenScene(ResearchSceneSet.Computer, OpenSceneMode.Single);

            var report = new StringBuilder();
            report.AppendLine("=== Computer assembly audit ===");

            var case_ = GameObject.Find("Desktop Case");
            if (case_ == null)
            {
                Write(report.AppendLine("FAIL Desktop Case missing").ToString());
                return;
            }

            s_Basis = case_.transform;
            var interior = WorldBounds(case_.transform);
            report.AppendLine($"Case shell bounds (case-local): size={F(interior.size)} center={F(interior.center)}");
            report.AppendLine();

            // --- per part: presence, duplicates, scale, containment, collider fit ---
            var found = new List<(string Name, Bounds B)>();
            report.AppendLine("-- parts --");
            foreach (var (name, real) in k_Parts)
            {
                var matches = FindAll(case_.transform, name);
                if (matches.Count == 0)
                {
                    report.AppendLine($"MISSING {name}");
                    continue;
                }
                if (matches.Count > 1)
                    report.AppendLine($"DUPLICATE {name} x{matches.Count}");

                var t = matches[0];
                var b = WorldBounds(t);
                found.Add((name, b));

                var worst = WorstAxisRatio(b.size, real);
                var flag = worst > 1.6f ? "  <-- SCALE" : "";
                report.AppendLine($"{name}: size={F(b.size)} expect~{F(real)} worstAxisRatio={worst:F2}{flag}");

                var outside = OutsideBy(b, interior);
                if (outside > 0.004f)
                    report.AppendLine($"   <-- OUTSIDE CASE by {outside * 1000f:F0} mm");

                report.AppendLine($"   tris={Tris(t)} mats={Mats(t)} depth={Depth(t, case_.transform)} path={Path(t, case_.transform)}");
            }

            // --- intersections ---
            report.AppendLine();
            report.AppendLine("-- intersections (unexpected volume sharing) --");
            var flagged = 0;
            for (var i = 0; i < found.Count; i++)
            for (var j = i + 1; j < found.Count; j++)
            {
                var a = found[i];
                var b = found[j];
                var pen = Penetration(a.B, b.B);
                if (pen <= 0.003f)
                    continue;

                var key = string.CompareOrdinal(a.Name, b.Name) < 0 ? $"{a.Name}|{b.Name}" : $"{b.Name}|{a.Name}";
                if (k_Expected.Contains(key))
                    continue;

                report.AppendLine($"OVERLAP {a.Name} <-> {b.Name} by {pen * 1000f:F0} mm");
                flagged++;
            }
            if (flagged == 0)
                report.AppendLine("none");

            // --- budget ---
            report.AppendLine();
            report.AppendLine("-- budget --");
            report.AppendLine($"Desktop Case subtree: tris={Tris(case_.transform)} mats={Mats(case_.transform)} renderers={case_.GetComponentsInChildren<Renderer>(true).Length}");

            // --- imported extras that must never reach the scene ---
            report.AppendLine();
            report.AppendLine("-- imported extras under Desktop Case --");
            report.AppendLine($"cameras={case_.GetComponentsInChildren<Camera>(true).Length} lights={case_.GetComponentsInChildren<Light>(true).Length} " +
                              $"animators={case_.GetComponentsInChildren<Animator>(true).Length} audio={case_.GetComponentsInChildren<AudioSource>(true).Length}");

            Write(report.ToString());
        }

        static void Write(string text)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(k_ReportPath));
            System.IO.File.WriteAllText(k_ReportPath, text);
            AssetDatabase.Refresh();
            Debug.Log("[AssemblyAudit]\n" + text);
        }

        /// <summary>Largest factor between measured and expected on any axis, order-independent.</summary>
        static float WorstAxisRatio(Vector3 got, Vector3 want)
        {
            var g = Sorted(got);
            var w = Sorted(want);
            var worst = 0f;
            for (var i = 0; i < 3; i++)
            {
                if (w[i] <= 0.0001f)
                    continue;
                var r = g[i] / w[i];
                worst = Mathf.Max(worst, r < 1f ? 1f / r : r);
            }
            return worst;
        }

        static float[] Sorted(Vector3 v)
        {
            var a = new[] { v.x, v.y, v.z };
            System.Array.Sort(a);
            return a;
        }

        /// <summary>How far <paramref name="b"/> pokes out of <paramref name="hull"/>, on its worst axis.</summary>
        static float OutsideBy(Bounds b, Bounds hull)
        {
            var over = Vector3.Max(hull.min - b.min, b.max - hull.max);
            return Mathf.Max(over.x, Mathf.Max(over.y, over.z));
        }

        /// <summary>Smallest axis overlap of two boxes; 0 or less when they are apart.</summary>
        static float Penetration(Bounds a, Bounds b)
        {
            var o = Vector3.Min(a.max, b.max) - Vector3.Max(a.min, b.min);
            return Mathf.Min(o.x, Mathf.Min(o.y, o.z));
        }

        static List<Transform> FindAll(Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).ToList();
        }

        /// <summary>
        /// Renderer bounds in the case's own frame.
        ///
        /// Measuring in world space is useless here: the case is yawed -70 degrees, so
        /// the world AABB of a correctly mounted 0.020 x 0.250 x 0.305 m board comes out
        /// as 0.296 x 0.250 x 0.126 and every part looks like a scale error. Sizes,
        /// containment and overlaps are only meaningful along the case's axes.
        /// </summary>
        static Bounds WorldBounds(Transform t)
        {
            var basis = s_Basis;
            var rends = t.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled).ToArray();
            if (rends.Length == 0)
                return new Bounds(basis == null ? t.position : basis.InverseTransformPoint(t.position), Vector3.zero);

            var found = false;
            var bounds = default(Bounds);
            foreach (var r in rends)
            {
                if (r is MeshRenderer == false && r is SkinnedMeshRenderer == false)
                    continue;
                var filter = r.GetComponent<MeshFilter>();
                var mesh = filter != null ? filter.sharedMesh : null;
                var local = mesh != null ? mesh.bounds : r.localBounds;
                for (var x = -1; x <= 1; x += 2)
                for (var y = -1; y <= 1; y += 2)
                for (var z = -1; z <= 1; z += 2)
                {
                    var corner = local.center + Vector3.Scale(local.extents, new Vector3(x, y, z));
                    var world = r.transform.TransformPoint(corner);
                    var point = basis == null ? world : basis.InverseTransformPoint(world);
                    if (found)
                        bounds.Encapsulate(point);
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                        found = true;
                    }
                }
            }
            return found ? bounds : new Bounds(Vector3.zero, Vector3.zero);
        }

        /// <summary>Frame every measurement is expressed in; set to the case for the run.</summary>
        static Transform s_Basis;

        static int Tris(Transform t)
        {
            return t.GetComponentsInChildren<MeshFilter>(true)
                .Where(m => m.sharedMesh != null)
                .Sum(m => m.sharedMesh.triangles.Length / 3);
        }

        static int Mats(Transform t)
        {
            return t.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials)
                .Where(m => m != null)
                .Distinct()
                .Count();
        }

        static int Depth(Transform t, Transform root)
        {
            var d = 0;
            for (var c = t; c != null && c != root; c = c.parent)
                d++;
            return d;
        }

        static string Path(Transform t, Transform root)
        {
            var parts = new List<string>();
            for (var c = t; c != null && c != root; c = c.parent)
                parts.Add(c.name);
            parts.Reverse();
            return string.Join("/", parts);
        }

        static string F(Vector3 v) => $"({v.x:F3},{v.y:F3},{v.z:F3})";
    }
}
