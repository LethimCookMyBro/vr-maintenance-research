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

        /// <summary>
        /// Every material the scenes depend on being see-through. This is the list —
        /// add a name here when a material is meant to be transparent, and nowhere else.
        ///
        /// Lab_FuseGlass is the one that carries a measure: Task B's fault is diagnosed
        /// by looking at the fuse element through the glass, so if this material goes
        /// opaque the two fuses become identical cylinders and the diagnosis silently
        /// stops existing. The UI_* materials are the panel glass the participant reads
        /// the work order through. Lab_GlassPanel is deliberately NOT here; see the note
        /// on it in ResearchMaterialPalette.
        /// </summary>
        static readonly string[] MustBeTransparent =
        {
            "Lab_FuseGlass",
            "UI_PanelSurface",
            "UI_CardFace",
            "UI_PanelBorder",
            "UI_ButtonPrimary",
            "UI_ButtonSecondary",
            "UI_Divider",
        };

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

        /// <summary>
        /// The other half of the 743b1c3 regression, and the check that would have
        /// caught it from the outside.
        ///
        /// EveryInteractableAnswersOnlyForItsOwnColliders proves each part owns the
        /// right collider. It says nothing about how big that collider is, and the
        /// builders' SetCollider used to give up in silence on anything that was not a
        /// BoxCollider — so eleven parts built from capsule primitives kept a
        /// 1000 x 2000 x 1000 mm grab volume on objects 11 to 571 mm across, and two
        /// status lamps kept a 1 m sphere. Every other check in this file, in the visual
        /// validator and in the play-mode runtime checks reaches its interactable by
        /// name, so all of them passed while 31 of 54 aims resolved to a different part.
        ///
        /// This is the only check that goes through the physics scene. It spreads aim
        /// points over the faces of the part that turn toward the eye, from the poses a
        /// participant actually holds, and resolves each hit the way XRRayInteractor
        /// resolves one.
        ///
        /// It fails on three things, and the second is the one that took longest to get
        /// right.
        ///
        /// * A part that answers none of its aims from any pose, because that part can
        ///   never appear in the event stream at all.
        /// * A part that has fallen more than a quarter below the aim count committed in
        ///   Docs/Verification/Ray_Aim_Baseline.tsv. Until that file existed this check
        ///   only had the first tier, and a part could lose most of its pointable
        ///   surface while staying green: putting the fan's motor module in the wrong
        ///   place took `fan.power-plug` from 94 of 135 aims to 49 and `fan.power-cord`
        ///   from 49 to 20, and Scene Integrity passed. See
        ///   <see cref="EditorTools.ResearchRayAimBaseline"/> for why that file is never
        ///   written automatically.
        /// * A part list that no longer matches the baseline's, in either direction.
        ///
        /// A part that answers but is fiddly to point at is still only reported — a
        /// screwdriver lying half under a cable is a bench, not a defect, and a gate that
        /// cannot tell the two apart gets muted rather than fixed. What the baseline adds
        /// is that "fiddly" now has to stay as fiddly as it was.
        /// </summary>
        [Test]
        public void EveryInteractableAnswersTheRayAimedAtIt()
        {
            var unreachable = new List<string>();
            var regressed = new List<string>();
            var hard = new List<string>();
            var hits = 0;
            var aims = 0;

            var baseline = EditorTools.ResearchRayAimBaseline.Load();
            Assert.That(baseline.Exists, Is.True,
                "no ray aim baseline is committed, so no part can be compared with anything. " +
                "Record one: Tools > VR Maintenance Research > Visual Audit > " +
                EditorTools.ResearchRayAimBaseline.k_MenuLeaf);
            Assert.That(EditorTools.ResearchRayAimBaseline.CriterionMatches(baseline), Is.True,
                EditorTools.ResearchRayAimBaseline.CriterionComplaint(baseline));

            var evaluated = EditorTools.ResearchRayAimReport.EvaluateAllScenes();
            foreach (var (scene, verdict) in evaluated)
            {
                hits += verdict.Hits;
                aims += verdict.Aims;

                var where = $"{scene}: '{verdict.Id}' answers {verdict.Hits} of {verdict.Aims} aims " +
                            $"from {verdict.PosesAnswering} of {EditorTools.ResearchRayAimReport.Poses.Length} poses";

                var against = EditorTools.ResearchRayAimBaseline.Compare(baseline, scene, verdict);
                if (against.IsRegression)
                    regressed.Add($"{where} — {against.What}");

                if (verdict.Unreachable)
                    unreachable.Add($"{where} — {verdict.Obstruction}");
                else if (verdict.HardToAim)
                    hard.Add($"{where}, best pose {verdict.BestReach * 100f:0}% — {verdict.Obstruction}");
            }

            regressed.AddRange(EditorTools.ResearchRayAimBaseline.Missing(baseline, evaluated));

            if (hard.Count > 0)
                Debug.Log($"[RayAim] {hard.Count} part(s) reachable but hard to aim at:\n  " + string.Join("\n  ", hard));

            Assert.That(unreachable, Is.Empty,
                $"{unreachable.Count} part(s) cannot be selected at all ({hits} of {aims} aims resolve to the part aimed at):\n  " +
                string.Join("\n  ", unreachable));

            Assert.That(regressed, Is.Empty,
                $"{regressed.Count} part(s) got harder to point at than the committed baseline allows.\n" +
                "Fix the scene, or — if the change was deliberate and the new numbers have been looked at —\n" +
                "re-record: Tools > VR Maintenance Research > Visual Audit > " +
                EditorTools.ResearchRayAimBaseline.k_MenuLeaf + "\n  " +
                string.Join("\n  ", regressed));
        }

        /// <summary>
        /// No part's grab volume may stand more than a hand's width outside the part.
        ///
        /// This is the deterministic half of the collider-size gate, and the one that
        /// fails the moment the bug above comes back. A grab volume is allowed to be
        /// larger than what it wraps — a 9 mm cover screw needs something a controller
        /// can actually hit — but "larger" has a ceiling. When the builders' SetCollider
        /// gave up on anything that was not a BoxCollider, eleven parts kept the capsule
        /// primitive's own 1000 x 2000 x 1000 mm collider: an excess of about a metre on
        /// parts 11 to 571 mm across. The widest excess the benches now carry is 52 mm,
        /// on the 200 mm screwdriver.
        ///
        /// Unlike the ray check this needs no pose and no line of sight, so it says
        /// nothing about whether a part can be aimed at — only that no part is claiming
        /// space it does not occupy.
        /// </summary>
        [Test]
        public void NoInteractableClaimsMoreSpaceThanItOccupies()
        {
            const float allowance = 0.100f;
            var oversized = new List<string>();

            foreach (var scenePath in EditorTools.ResearchRayAimReport.Scenes)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (var part in Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    if (!TryBounds(part.GetComponentsInChildren<Renderer>(false)
                            .Where(renderer => renderer.enabled && renderer.GetComponentInParent<ResearchInteractable>() == part)
                            .Select(renderer => renderer.bounds), out var visible))
                        continue;
                    if (!TryBounds(part.GetComponents<Collider>().Select(collider => collider.bounds), out var claimed))
                        continue;

                    var over = Vector3.Max(visible.min - claimed.min, claimed.max - visible.max);
                    var excess = Mathf.Max(over.x, over.y, over.z);
                    if (excess > allowance)
                        oversized.Add($"{scene.name}: '{part.StableObjectId}' draws " +
                                      $"{visible.size.x * 1000f:0} x {visible.size.y * 1000f:0} x {visible.size.z * 1000f:0} mm " +
                                      $"but claims {claimed.size.x * 1000f:0} x {claimed.size.y * 1000f:0} x {claimed.size.z * 1000f:0} mm " +
                                      $"— {excess * 1000f:0} mm beyond itself");
                }
            }

            Assert.That(oversized, Is.Empty,
                $"{oversized.Count} grab volumes reach more than {allowance * 1000f:0} mm past their part:\n  " + string.Join("\n  ", oversized));
        }

        static bool TryBounds(IEnumerable<Bounds> all, out Bounds bounds)
        {
            bounds = default;
            var found = false;
            foreach (var one in all)
            {
                if (!found)
                {
                    bounds = one;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(one);
                }
            }
            return found;
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
        /// Transparency in URP is not one property, and setting the colour's alpha does
        /// not buy it. A material only renders see-through if _Surface is Transparent
        /// and the blend factors are SrcAlpha/OneMinusSrcAlpha with depth writes off;
        /// miss one and the surface goes solid while still looking correct in the
        /// inspector's colour swatch.
        ///
        /// This exists because Lab_FuseGlass lost _SrcBlend repeatedly. URP re-derives
        /// the blend factors on every material import, and _BlendModePreserveSpecular
        /// forces SrcAlpha to One when it is on, so the file was being corrected back to
        /// a value nobody wanted and a hand-revert of the .mat never survived the next
        /// import. _BlendModePreserveSpecular is asserted here too: it is the cause, and
        /// checking only the symptom would let the same drift return silently.
        /// </summary>
        [Test]
        public void TransparentMaterialsKeepTheirBlendState()
        {
            foreach (var name in MustBeTransparent)
            {
                var material = LoadMaterial(name);
                Assert.That(material, Is.Not.Null, name + ": no such material under Assets/VRMaintenanceResearch");

                Assert.That(material.GetFloat("_Surface"), Is.EqualTo(1f),
                    name + ": _Surface is Opaque, so it renders solid whatever its colour alpha says");
                Assert.That(material.GetFloat("_SrcBlend"), Is.EqualTo((float)UnityEngine.Rendering.BlendMode.SrcAlpha),
                    name + ": _SrcBlend is not SrcAlpha(5), so the surface does not blend with what is behind it");
                Assert.That(material.GetFloat("_DstBlend"), Is.EqualTo((float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha),
                    name + ": _DstBlend is not OneMinusSrcAlpha(10)");
                Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0f),
                    name + ": _ZWrite is on, so this writes depth and hides what is behind it");

                // The cause, not the symptom. URP only lands on SrcAlpha if this is off.
                if (material.HasProperty("_BlendModePreserveSpecular"))
                    Assert.That(material.GetFloat("_BlendModePreserveSpecular"), Is.EqualTo(0f),
                        name + ": _BlendModePreserveSpecular is on, so URP will rewrite _SrcBlend to One(1) on the next "
                             + "import and the fix will come undone. Clear it in ResearchMaterialPalette.SetTransparent, "
                             + "do not edit the .mat by hand.");
            }
        }

        static Material LoadMaterial(string name) =>
            AssetDatabase.FindAssets(name + " t:Material", new[] { "Assets/VRMaintenanceResearch" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Material>)
                .FirstOrDefault(asset => asset != null && asset.name == name);

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
                (nameof(NoInteractableClaimsMoreSpaceThanItOccupies), tests.NoInteractableClaimsMoreSpaceThanItOccupies),
                (nameof(EveryInteractableAnswersTheRayAimedAtIt), tests.EveryInteractableAnswersTheRayAimedAtIt),
                (nameof(EveryTaskSceneKeepsTheRepairObjectItsDefinitionRequires), tests.EveryTaskSceneKeepsTheRepairObjectItsDefinitionRequires),
                (nameof(BothTaskOrdersResolveToEnabledBuildScenes), tests.BothTaskOrdersResolveToEnabledBuildScenes),
                (nameof(EveryInformationSourceCarriesAllThreeLanguages), tests.EveryInformationSourceCarriesAllThreeLanguages),
                (nameof(BothTaskScenesCanLocalizeTheWorkOrder), tests.BothTaskScenesCanLocalizeTheWorkOrder),
                (nameof(EveryTaskDefinitionCarriesItsEnglishFallback), tests.EveryTaskDefinitionCarriesItsEnglishFallback),
                (nameof(TransparentMaterialsKeepTheirBlendState), tests.TransparentMaterialsKeepTheirBlendState),
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
