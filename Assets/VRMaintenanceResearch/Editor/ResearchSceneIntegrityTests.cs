using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Scene-level regressions that only show up once a builder has run. The visual
    /// validator checks how a scene looks and the play-mode runtime checks drive the
    /// repair loop; neither can see a duplicated stable id, an interactable that
    /// answers for its children's colliders, or a task whose required repair object
    /// left the scene during a rebuild.
    ///
    /// These open scenes, so they replace whatever is currently open — the same as
    /// the visual validator.
    /// </summary>
    public sealed class ResearchSceneIntegrityTests
    {
        const string k_SceneFolder = "Assets/VRMaintenanceResearch/Scenes/";
        static readonly string[] TaskScenes = { "ComputerRepairTask", "FanRepairTask" };
        static readonly string[] AllScenes = { "ResearcherSetup", "VRTraining", "ComputerRepairTask", "FanRepairTask" };

        static void Open(string scene) => EditorSceneManager.OpenScene(k_SceneFolder + scene + ".unity", OpenSceneMode.Single);

        static ResearchInteractable[] Interactables() =>
            Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        [Test]
        public void StableObjectIdsAreUniqueAndDeliberateInEveryScene()
        {
            foreach (var scene in AllScenes)
            {
                Open(scene);
                var seen = new Dictionary<string, string>();
                foreach (var interactable in Interactables())
                {
                    var id = interactable.StableObjectId;
                    Assert.That(id, Is.Not.Null.And.Not.Empty, scene + ": " + interactable.name + " has no stable id");
                    Assert.That(id, Is.Not.EqualTo("object.development"),
                        scene + ": " + interactable.name + " still carries the default placeholder id");
                    Assert.That(seen.ContainsKey(id), Is.False,
                        scene + ": '" + id + "' is on both " + interactable.name + " and " + (seen.TryGetValue(id, out var first) ? first : ""));
                    seen[id] = interactable.name;
                }
            }
        }

        /// <summary>
        /// The 743b1c3 regression. XRBaseInteractable auto-collects
        /// GetComponentsInChildren&lt;Collider&gt;() when its own list is empty, and both
        /// builders reparent the in-machine parts under their device so local
        /// coordinates stay readable. That made the device claim its children's
        /// colliders, and since the parent registers first, a ray aimed at any part
        /// inside the machine resolved to the device — every hover and grab on the ATX
        /// connector, the fuse holder, the board or the supply was recorded against
        /// computer.case or fan.body. Nothing about the scene looked wrong.
        /// </summary>
        [Test]
        public void EveryInteractableAnswersOnlyForItsOwnColliders()
        {
            foreach (var scene in AllScenes)
            {
                Open(scene);
                foreach (var interactable in Interactables())
                {
                    var xr = interactable.GetComponent<XRBaseInteractable>();
                    if (xr == null)
                        continue;

                    var list = new SerializedObject(xr).FindProperty("m_Colliders");
                    Assert.That(list, Is.Not.Null, scene + ": " + interactable.StableObjectId + " has no m_Colliders property");
                    Assert.That(list.arraySize, Is.GreaterThan(0),
                        scene + ": " + interactable.StableObjectId + " has an empty collider list, so XRI will auto-collect its children's");

                    for (var i = 0; i < list.arraySize; i++)
                    {
                        var collider = list.GetArrayElementAtIndex(i).objectReferenceValue as Collider;
                        Assert.That(collider, Is.Not.Null,
                            scene + ": " + interactable.StableObjectId + " has a missing collider reference at " + i);
                        Assert.That(collider.gameObject, Is.EqualTo(interactable.gameObject),
                            scene + ": " + interactable.StableObjectId + " claims '" + collider.name + "', which belongs to another object");
                    }
                }
            }
        }

        [Test]
        public void EveryTaskSceneKeepsTheRepairObjectItsDefinitionRequires()
        {
            foreach (var scene in TaskScenes)
            {
                Open(scene);
                var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
                Assert.That(controller, Is.Not.Null, scene + ": no MaintenanceTaskController");

                var required = new SerializedObject(controller).FindProperty("requiredRepairObjectId").stringValue;
                Assert.That(required, Is.Not.Null.And.Not.Empty, scene + ": no required repair id");

                var repair = Interactables().FirstOrDefault(i => i.StableObjectId == required);
                Assert.That(repair, Is.Not.Null, scene + ": nothing in the scene carries the required repair id '" + required + "'");
                Assert.That(repair.Kind, Is.EqualTo(ResearchInteractionKind.RepairAction),
                    scene + ": '" + required + "' is a " + repair.Kind + ", so RecordInteraction will never complete the task");
                Assert.That(repair.gameObject.activeInHierarchy, Is.True,
                    scene + ": '" + required + "' is inactive, so the task cannot be completed");

                Assert.That(Interactables().Any(i => i.Kind == ResearchInteractionKind.DeviceTest && i.gameObject.activeInHierarchy),
                    Is.True, scene + ": no active DeviceTest object, so the participant cannot test the device");
            }
        }

        /// <summary>
        /// Both task orders load real scenes. A typo in a scene name only surfaces once
        /// a participant finishes the first task and the session tries to load the next.
        ///
        /// The manager is not placed in any scene — ResearcherSetupController and
        /// MaintenanceTaskController both create one at runtime with AddComponent — so
        /// the scene names that ship are this component's field initialisers. A fresh
        /// instance is therefore the thing to read, not a scene object.
        /// </summary>
        [Test]
        public void BothTaskOrdersResolveToEnabledBuildScenes()
        {
            var host = new GameObject("session name probe");
            try
            {
                var serialized = new SerializedObject(host.AddComponent<ResearchSessionManager>());
                var enabled = EditorBuildSettings.scenes
                    .Where(scene => scene.enabled)
                    .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
                    .ToList();

                foreach (var field in new[] { "researcherSetupScene", "trainingScene", "computerScene", "fanScene" })
                {
                    var name = serialized.FindProperty(field).stringValue;
                    Assert.That(enabled, Does.Contain(name),
                        field + " points at '" + name + "', which is not an enabled scene in Build Settings");
                }
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }

        [Test]
        public void EveryInformationSourceCarriesAllThreeLanguages()
        {
            foreach (var source in ReferencedSources())
            {
                var where = source.name + " (" + source.sourceId + ")";
                Assert.That(source.englishTitle, Is.Not.Null.And.Not.Empty, where + ": no English title");
                Assert.That(source.englishContent, Is.Not.Null.And.Not.Empty, where + ": no English content");
                Assert.That(source.thaiTitle, Is.Not.Null.And.Not.Empty, where + ": no Thai title");
                Assert.That(source.thaiContent, Is.Not.Null.And.Not.Empty, where + ": no Thai content");
                Assert.That(source.japaneseTitle, Is.Not.Null.And.Not.Empty, where + ": no Japanese title");
                Assert.That(source.japaneseContent, Is.Not.Null.And.Not.Empty, where + ": no Japanese content");
            }
        }

        /// <summary>
        /// The work order is the only text that states the task, and its Thai and
        /// Japanese wording lives in LocalizedTaskBrief, which reaches it with
        /// transform.Find("Heading") and transform.Find("Body"). Both lookups return
        /// null silently, and Refresh then returns without touching anything — so a
        /// rebuild that renames either child leaves every Thai and Japanese participant
        /// reading the English brief with nothing logged and nothing on the console.
        /// The task definition's own thai/japanese fields are not checked here because
        /// no code reads them; see KNOWN_LIMITATIONS.
        /// </summary>
        [Test]
        public void BothTaskScenesCanLocalizeTheWorkOrder()
        {
            foreach (var scene in TaskScenes)
            {
                Open(scene);
                var briefs = Object.FindObjectsByType<LocalizedTaskBrief>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Assert.That(briefs.Length, Is.EqualTo(1),
                    scene + ": expected exactly one LocalizedTaskBrief, found " + briefs.Length);

                foreach (var child in new[] { "Heading", "Body" })
                {
                    var found = briefs[0].transform.Find(child);
                    Assert.That(found, Is.Not.Null,
                        scene + ": LocalizedTaskBrief has no child named '" + child + "', so it will silently leave the brief in English");
                    Assert.That(found.GetComponent<TMPro.TMP_Text>(), Is.Not.Null,
                        scene + ": '" + child + "' carries no TMP_Text");
                }
            }
        }

        [Test]
        public void EveryTaskDefinitionCarriesItsEnglishFallback()
        {
            foreach (var task in TaskDefinitions().Where(definition => definition.taskId != ResearchTaskId.Session))
            {
                var where = task.name + " (" + task.taskId + ")";
                Assert.That(task.englishTitle, Is.Not.Null.And.Not.Empty, where + ": no English title");
                Assert.That(task.englishParticipantInstructions, Is.Not.Null.And.Not.Empty, where + ": no English instructions");
            }
        }

        /// <summary>
        /// Runs every test above and reports all failures rather than stopping at the
        /// first, which matters here because one rebuild tends to break several scenes
        /// the same way. Mirrors ResearchFoundationDirectTestRunner: the project drives
        /// its edit-mode tests from a menu item rather than the Test Runner window.
        /// </summary>
        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Run Scene Integrity Tests", priority = 5)]
        public static void RunAll()
        {
            var tests = new ResearchSceneIntegrityTests();
            var cases = new (string Name, System.Action Run)[]
            {
                (nameof(StableObjectIdsAreUniqueAndDeliberateInEveryScene), tests.StableObjectIdsAreUniqueAndDeliberateInEveryScene),
                (nameof(EveryInteractableAnswersOnlyForItsOwnColliders), tests.EveryInteractableAnswersOnlyForItsOwnColliders),
                (nameof(EveryTaskSceneKeepsTheRepairObjectItsDefinitionRequires), tests.EveryTaskSceneKeepsTheRepairObjectItsDefinitionRequires),
                (nameof(BothTaskOrdersResolveToEnabledBuildScenes), tests.BothTaskOrdersResolveToEnabledBuildScenes),
                (nameof(EveryInformationSourceCarriesAllThreeLanguages), tests.EveryInformationSourceCarriesAllThreeLanguages),
                (nameof(BothTaskScenesCanLocalizeTheWorkOrder), tests.BothTaskScenesCanLocalizeTheWorkOrder),
                (nameof(EveryTaskDefinitionCarriesItsEnglishFallback), tests.EveryTaskDefinitionCarriesItsEnglishFallback),
            };

            var report = new System.Text.StringBuilder("=== scene integrity tests ===\n");
            var failed = 0;
            foreach (var test in cases)
            {
                try
                {
                    test.Run();
                    report.AppendLine("PASS " + test.Name);
                }
                catch (System.Exception exception)
                {
                    failed++;
                    report.AppendLine("FAIL " + test.Name + "\n      " + exception.Message.Replace("\n", "\n      "));
                }
            }

            report.AppendLine(failed == 0 ? "ALL " + cases.Length + " PASS" : failed + " of " + cases.Length + " FAILED");
            var path = "Assets/VRMaintenanceResearch/Docs/Verification/Scene_Integrity_Tests.txt";
            System.IO.File.WriteAllText(path, report.ToString());
            Debug.Log("[SceneIntegrityTests]\n" + report);
        }

        static IEnumerable<ResearchTaskDefinition> TaskDefinitions() =>
            AssetDatabase.FindAssets("t:ResearchTaskDefinition", new[] { "Assets/VRMaintenanceResearch" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ResearchTaskDefinition>)
                .Where(asset => asset != null);

        /// <summary>
        /// Reached through the task definitions rather than by asset search, because the
        /// eight invalid v1 source assets are still on disk with missing script
        /// references and no task definition points at them.
        /// </summary>
        static IEnumerable<InformationSourceDefinition> ReferencedSources() =>
            TaskDefinitions()
                .SelectMany(task => task.informationSources)
                .Where(source => source != null)
                .Distinct();
    }
}
