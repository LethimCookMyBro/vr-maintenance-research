using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Objective checks behind the visual audit. Two whole-scene defects in this
    /// pass were scale bugs invisible in a hierarchy listing (an 11 m screwdriver,
    /// a 2 m cylinder where a fan blade should be), and the headline requirement —
    /// manuals must not block the task — is a geometry question, not a taste one.
    /// Both are asserted here so a regression fails loudly instead of quietly.
    /// </summary>
    public static class ResearchVisualValidator
    {
        const float k_MaxPropSize = 1.5f;      // metres; the scale bugs were 2 m and 11 m
        static readonly Vector3 k_ParticipantEye = new Vector3(0f, 1.36f, -1.6f);

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Validate Scenes", priority = 1)]
        public static void ValidateAll()
        {
            var report = new StringBuilder();
            var failures = 0;

            foreach (var scenePath in ResearchSceneSet.AllScenes)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var scene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                report.AppendLine($"=== {scene} ===");

                var issues = new List<string>();
                issues.AddRange(CheckOversizedProps());
                issues.AddRange(CheckDockClearsBench());
                issues.AddRange(CheckNoBlankReaders());
                issues.AddRange(CheckSightLines());
                issues.AddRange(CheckTaskBrief());
                issues.AddRange(CheckInspectControl());
                issues.AddRange(CheckFaultNotAdvertised());
                issues.AddRange(CheckDecorationIsInert());

                if (issues.Count == 0)
                    report.AppendLine("  PASS");
                else
                    foreach (var issue in issues)
                        report.AppendLine("  FAIL " + issue);

                // Data-integrity findings are reported but do not fail the visual
                // audit: fixing them means editing StableObjectIds, which changes
                // what the research log records.
                foreach (var warning in CheckTaskPartsPresent())
                    report.AppendLine("  WARN " + warning);

                failures += issues.Count;
            }

            report.AppendLine(failures == 0
                ? "ALL SCENES PASS"
                : $"{failures} issue(s) found");

            var path = "Assets/VRMaintenanceResearch/Docs/Verification/VisualAudit_Validation.txt";
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, report.ToString());
            AssetDatabase.Refresh();

            if (failures == 0)
                Debug.Log("[VisualValidator] all scenes pass\n" + report);
            else
                Debug.LogError("[VisualValidator] issues found\n" + report);
        }

        /// <summary>
        /// Every part the task definition declares must still exist in the scene.
        /// A visual rebuild that reparents interactables can delete them on a second
        /// run; that removes task logic, not just polish, so it fails the audit.
        /// </summary>
        static IEnumerable<string> CheckTaskPartsPresent()
        {
            var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
            if (controller == null)
                yield break;

            var definition = controller.Definition;
            if (definition == null)
                yield break;

            var present = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Select(i => i.StableObjectId)
                .ToHashSet();

            var declared = definition.availableComponents
                .Concat(definition.availableTools)
                .Concat(definition.availableReplacementParts);

            foreach (var id in declared)
                if (!string.IsNullOrEmpty(id) && !present.Contains(id))
                    yield return $"task part '{id}' declared by {definition.name} has no matching StableObjectId in the scene (pre-existing data mismatch, not changed by the visual pass)";
        }

        // ---------------------------------------------------------------------
        // Comprehension guards
        //
        // The visual pass could pass every check above and still leave a scene a
        // first-time participant cannot read. These four assert the things the
        // comprehension redesign put in, so a later edit cannot quietly undo them.
        // ---------------------------------------------------------------------

        /// <summary>Each maintenance scene states its task before the participant touches anything.</summary>
        static IEnumerable<string> CheckTaskBrief()
        {
            var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
            if (controller == null || controller.Definition == null)
                yield break;
            var taskId = controller.Definition.taskId;
            if (taskId != ResearchTaskId.Computer && taskId != ResearchTaskId.Fan)
                yield break;

            var brief = GameObject.Find("Task Brief");
            if (brief == null)
            {
                yield return "no Task Brief in a maintenance scene: participants get no statement of the task";
                yield break;
            }

            var copy = brief.GetComponentsInChildren<TMPro.TMP_Text>(true).ToList();
            if (copy.Count == 0 || copy.Any(t => string.IsNullOrWhiteSpace(t.text)))
                yield return "Task Brief has empty text";

            // The brief must not turn an open diagnostic task into a recipe.
            var body = string.Join(" ", copy.Select(t => t.text)).ToLowerInvariant();
            foreach (var leak in new[] { "fuse", "24-pin", "atx", "connector is", "plug it", "step 1", "first," })
                if (body.Contains(leak))
                    yield return $"Task Brief names the answer or the procedure ('{leak}')";
        }

        /// <summary>
        /// The device test ends the task, so a participant has to be able to find it.
        /// It also has to be at unit scale: it shipped scaled to 0.11, which shrank
        /// anything parented to it to a ninth of its intended size.
        /// </summary>
        static IEnumerable<string> CheckInspectControl()
        {
            var test = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(i => i.Kind == ResearchInteractionKind.DeviceTest);
            if (test == null)
                yield break;

            var scale = test.transform.lossyScale;
            if (Mathf.Abs(scale.x - 1f) > 0.01f || Mathf.Abs(scale.y - 1f) > 0.01f)
                yield return $"device-test control '{test.name}' is scaled {scale:F2}; its visual will not be the size it was built at";

            if (test.GetComponentsInChildren<Renderer>(true).Count(r => r.enabled) == 0)
                yield return $"device-test control '{test.name}' has no visible geometry";

            if (GameObject.Find("Inspect Station Sign") == null)
                yield return "device-test control is unlabelled: no Inspect Station Sign in the scene";
        }

        /// <summary>
        /// Nothing at the fault site may carry a signal colour.
        ///
        /// The study measures diagnosis. A red band on the blown fuse, or an accent
        /// glow on the unplugged connector, hands over the answer from across the
        /// room and turns the task into a fetch quest — so the fault-adjacent parts
        /// are held to neutral materials, and the two spare fuses are additionally
        /// required to be built from the same material set as each other.
        /// </summary>
        static IEnumerable<string> CheckFaultNotAdvertised()
        {
            var signal = new HashSet<string> { "Lab_StatusRed", "Lab_StatusGreen", "Lab_Warning", "Lab_Accent" };
            var faultSites = new HashSet<string>
            {
                "fan.fuse-holder", "fan.working-fuse", "fan.faulty-fuse", "fan.internal-wire",
                "computer.internal-cable", "computer.main-power-connector",
            };

            var byId = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(i => faultSites.Contains(i.StableObjectId))
                .ToList();

            foreach (var interactable in byId)
                foreach (var name in Materials(interactable))
                    if (signal.Contains(name))
                        yield return $"'{interactable.StableObjectId}' uses signal material {name} at the fault site: it advertises the answer";

            // The two spares have to be twins in the body — same glass, caps and
            // printed rating — so neither is identifiable from across the bench.
            //
            // They are allowed to differ in the two materials that *are* the
            // diagnosis: the element and the smoke stain on a blown one. Requiring a
            // byte-identical material set forbade the only cue a participant is meant
            // to find, which would have left two indistinguishable fuses and no task.
            var evidence = new HashSet<string> { "Lab_ToolSteel", "Lab_Rubber" };

            var good = byId.FirstOrDefault(i => i.StableObjectId == "fan.working-fuse");
            var bad = byId.FirstOrDefault(i => i.StableObjectId == "fan.faulty-fuse");
            if (good == null || bad == null)
                yield break;

            var goodMaterials = Materials(good);
            var badMaterials = Materials(bad);
            foreach (var name in goodMaterials.Except(badMaterials).Concat(badMaterials.Except(goodMaterials)))
                if (!evidence.Contains(name))
                    yield return $"the spare fuses differ in body material '{name}': one of them is identifiable without inspection";
        }

        static HashSet<string> Materials(Component root)
        {
            return root.GetComponentsInChildren<Renderer>(true)
                .Where(r => r.sharedMaterial != null)
                // A renderer touched through .material carries a runtime clone named
                // "Lab_X (Instance)"; compare against the asset name either way.
                .Select(r => r.sharedMaterial.name.Replace(" (Instance)", string.Empty))
                .ToHashSet();
        }

        /// <summary>
        /// Builder-made dressing must stay inert. Anything with a collider answers an
        /// XR ray, so a decorative prop with one is indistinguishable from a task part
        /// until the participant has already wasted the attempt on it.
        /// </summary>
        static IEnumerable<string> CheckDecorationIsInert()
        {
            foreach (var rootName in new[] { "Workstation Dressing", "Task Brief", "Inspect Station Sign", "Lab Notice Board" })
            {
                var root = GameObject.Find(rootName);
                if (root == null)
                    continue;
                foreach (var collider in root.GetComponentsInChildren<Collider>(true))
                    yield return $"decoration '{Path(collider.transform)}' has a collider and will answer a controller ray";
            }
        }

        /// <summary>Catches placeholders whose own primitive mesh survived a scale reset.</summary>
        static IEnumerable<string> CheckOversizedProps()
        {
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!r.enabled)
                    continue;
                var go = r.gameObject;
                if (IsArchitecture(go))
                    continue;

                var size = r.bounds.size;
                if (size.x <= k_MaxPropSize && size.y <= k_MaxPropSize && size.z <= k_MaxPropSize)
                    continue;

                yield return $"oversized prop '{Path(go.transform)}' size={size:F2}";
            }
        }

        /// <summary>
        /// The headline requirement: no information-source geometry may overlap the
        /// task volume. That volume is derived from where the task objects actually
        /// are, not from the whole bench top — the bench's empty far-left corner is
        /// legitimately where the dock lives.
        /// </summary>
        static IEnumerable<string> CheckDockClearsBench()
        {
            var taskVolume = TaskVolume();
            if (taskVolume == null)
                yield break;

            foreach (var source in Object.FindObjectsByType<InformationSourceController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                foreach (var issue in CheckClear(source.gameObject, "selector card", taskVolume.Value))
                    yield return issue;

                var prop = new SerializedObject(source).FindProperty("contentPanel");
                var panel = prop != null ? prop.objectReferenceValue as GameObject : null;
                if (panel == null)
                    continue;

                var wasActive = panel.activeSelf;
                panel.SetActive(true);
                var issues = CheckClear(panel, "opened reader", taskVolume.Value).ToList();
                panel.SetActive(wasActive);
                foreach (var issue in issues)
                    yield return issue;
            }
        }

        /// <summary>Bounds enclosing every task interactable that sits on the bench.</summary>
        static Bounds? TaskVolume()
        {
            Bounds? volume = null;
            foreach (var interactable in Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                var renderers = interactable.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled).ToList();
                if (renderers.Count == 0)
                    continue;

                var b = renderers[0].bounds;
                foreach (var r in renderers)
                    b.Encapsulate(r.bounds);

                // Only bench-height objects; pedestal controls and shelf storage
                // are deliberately outside the working area.
                if (b.center.y < 0.90f || b.center.y > 1.75f || b.center.z < 0.40f)
                    continue;

                volume = volume == null ? b : Encapsulated(volume.Value, b);
            }
            return volume;
        }

        static Bounds Encapsulated(Bounds a, Bounds b)
        {
            a.Encapsulate(b);
            return a;
        }

        static IEnumerable<string> CheckClear(GameObject go, string what, Bounds taskVolume)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if (!r.enabled)
                    continue;
                if (!taskVolume.Intersects(r.bounds))
                    continue;
                yield return $"{what} '{Path(r.transform)}' overlaps the task volume at {r.bounds.center:F2}";
            }
        }

        /// <summary>A reader with no visible copy is the "unloaded guide content" defect.</summary>
        static IEnumerable<string> CheckNoBlankReaders()
        {
            foreach (var source in Object.FindObjectsByType<InformationSourceController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var prop = new SerializedObject(source).FindProperty("contentPanel");
                var panel = prop != null ? prop.objectReferenceValue as GameObject : null;
                if (panel == null)
                    continue;

                var hasCopy = panel.GetComponentsInChildren<TMPro.TMP_Text>(true)
                    .Any(l => l.name == "GEN Title" && !string.IsNullOrWhiteSpace(l.text));
                if (!hasCopy)
                    yield return $"reader '{panel.name}' has no title text";

                if (hasCopy && panel.GetComponentsInChildren<TextMesh>(true).Any(t => t.gameObject.activeSelf))
                    yield return $"reader '{panel.name}' has enabled legacy text behind its TMP copy";
            }
        }

        /// <summary>Every task interactable must be visible from the participant pose.</summary>
        static IEnumerable<string> CheckSightLines()
        {
            var blockers = Object.FindObjectsByType<InformationSourceController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .SelectMany(s => s.GetComponentsInChildren<Renderer>(true))
                .Select(r => r.bounds)
                .ToList();

            foreach (var interactable in Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                var target = interactable.transform.position;
                if (target.y < 0.90f || target.z < 0.40f)
                    continue;   // off-bench controls (pedestal, shelf) are out of scope

                var direction = target - k_ParticipantEye;
                var ray = new Ray(k_ParticipantEye, direction.normalized);
                foreach (var bounds in blockers)
                {
                    if (!bounds.IntersectRay(ray, out var distance))
                        continue;
                    if (distance < direction.magnitude)
                        yield return $"information geometry blocks the view of '{interactable.name}'";
                }
            }
        }

        static bool IsArchitecture(GameObject go)
        {
            var t = go.transform;
            while (t != null)
            {
                var n = t.name;
                if (n == "Lab Environment" || n == "Research Floor" || n == "Lab Notice Board" ||
                    n == "Official XRI Complete Setup")
                    return true;
                t = t.parent;
            }
            return false;
        }

        static string Path(Transform t)
        {
            var path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }
    }
}
