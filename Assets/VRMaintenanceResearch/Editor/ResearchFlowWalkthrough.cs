using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Drives a whole session the way a participant does — setup, training, first task,
    /// second task, end — across the scene loads that separate them, and asserts after
    /// each step. Every other check in this project runs inside one scene, so none of
    /// them can see the handover between tasks, which is exactly where the session used
    /// to stop dead.
    ///
    /// It runs with developmentMode off, because that is the configuration an actual
    /// participant run uses and the one where the researcher panel used to return early
    /// and leave no way forward at all.
    ///
    /// It lives in an Editor folder on purpose: the component exists in the editor's
    /// play mode and cannot reach a build.
    ///
    /// What it does not prove: that a controller ray actually lands on these buttons.
    /// It asserts the preconditions for that — a world-space canvas carrying a
    /// TrackedDeviceGraphicRaycaster, and a button graphic that is a raycast target —
    /// then invokes onClick. Aiming is a headset check.
    /// </summary>
    public sealed class ResearchFlowWalkthrough
    {
        const float k_StepTimeout = 12f;
        string ReportPath => "Assets/VRMaintenanceResearch/Docs/Verification/Full_Flow_Walkthrough_" + order + ".txt";

        /// <summary>
        /// Open ResearcherSetup, press Play, then run this — the same contract as the
        /// other two play-mode checks. It cannot start play mode for you, because
        /// entering play mode reloads the domain and nothing set up beforehand survives.
        /// </summary>
        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Run Full Flow Walkthrough A-B (Play Mode)", priority = 6)]
        public static void RunComputerThenFan() => Run(TaskOrder.ComputerThenFan);

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Run Full Flow Walkthrough B-A (Play Mode)", priority = 7)]
        public static void RunFanThenComputer() => Run(TaskOrder.FanThenComputer);

        static void Run(TaskOrder order)
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("[Walkthrough] enter play mode on ResearcherSetup first.");
                return;
            }

            if (Scene() != "ResearcherSetup")
            {
                Debug.LogError("[Walkthrough] start from ResearcherSetup; the active scene is " + Scene() + ".");
                return;
            }

            steps.Clear();
            steps.Push(new ResearchFlowWalkthrough(order).Drive());
            EditorApplication.update -= Step;
            EditorApplication.update += Step;
        }

        // The driver is not a MonoBehaviour and puts nothing in the scene: Unity refuses
        // to AddComponent a script that lives in an Editor folder, and a driver that had
        // to sit outside one would ship in the build. So the coroutine is stepped from
        // the editor's own update loop, which keeps ticking across the scene loads
        // between tasks. The stack is what makes `yield return SomeOtherIEnumerator()`
        // behave the way it would inside StartCoroutine.
        static readonly System.Collections.Generic.Stack<IEnumerator> steps = new System.Collections.Generic.Stack<IEnumerator>();

        static void Step()
        {
            if (!Application.isPlaying || steps.Count == 0)
            {
                EditorApplication.update -= Step;
                steps.Clear();
                return;
            }

            var current = steps.Peek();
            if (!current.MoveNext())
                steps.Pop();
            else if (current.Current is IEnumerator nested)
                steps.Push(nested);
        }

        readonly StringBuilder report = new StringBuilder();
        readonly TaskOrder order;
        int failures;

        ResearchFlowWalkthrough(TaskOrder taskOrder) => order = taskOrder;

        IEnumerator Drive()
        {
            report.AppendLine("=== full flow walkthrough: " + order + " ===");
            report.AppendLine("participant-style: no Inspector edits and no desktop researcher panel between steps");

            // ---- setup -------------------------------------------------------
            yield return Until("session manager exists", () => ResearchSessionManager.Instance != null);
            var session = ResearchSessionManager.Instance;
            if (session == null)
                yield break;

            var config = new ResearchSessionConfig
            {
                participantCode = "WALKTHROUGH",
                participantGroup = ParticipantGroup.Thai,
                language = ResearchLanguage.Thai,
                taskOrder = order,
                developmentMode = false,   // the configuration a real run uses
                simulatorMode = true,
                trainingRequired = true,
            };
            Check("session accepts a non-development configuration", session.Configure(config, out var error), error);
            Check("session starts", session.StartSession(out error), error);
            report.AppendLine("      data folder: " + session.Logger.CurrentDataFolder);
            Check("real sessions write outside the Development folder",
                session.Logger.CurrentDataFolder.Replace('\\', '/').Contains("/Sessions/"), session.Logger.CurrentDataFolder);

            session.StartConfiguredFlow();

            // ---- training ----------------------------------------------------
            yield return Until("training scene loads", () => Scene() == "VRTraining");
            var training = Object.FindFirstObjectByType<TrainingInstructions>();
            Check("training board exists", training != null, "no TrainingInstructions");
            if (training == null)
                yield break;

            var continueButton = Field<Button>(training, "continueButton");
            Check("training Continue is locked before the skills are practised",
                continueButton != null && !continueButton.gameObject.activeSelf, "Continue was already available");

            foreach (var skill in new[] { "grabSatisfied", "socketSatisfied", "knobSatisfied", "informationSatisfied" })
                Set(training, skill, true);
            yield return null;

            continueButton = Field<Button>(training, "continueButton");
            Check("training Continue unlocks once all four skills are practised",
                continueButton != null && continueButton.gameObject.activeSelf && continueButton.interactable, "still locked");
            CheckReachable("training Continue", continueButton);
            continueButton.onClick.Invoke();

            // ---- first task, then second ------------------------------------
            var computerFirst = order == TaskOrder.ComputerThenFan;
            yield return RunTask("first", computerFirst ? "ComputerRepairTask" : "FanRepairTask");
            yield return RunTask("second", computerFirst ? "FanRepairTask" : "ComputerRepairTask");

            // ---- end of session ---------------------------------------------
            var lastTaskScene = Scene();
            yield return Until("session is marked complete", () => session.SessionComplete);
            Check("the participant is left in a head-tracked scene, not ResearcherSetup",
                Scene() == lastTaskScene, "scene changed to " + Scene());
            Check("logger closed the session", !session.Logger.IsStarted, "logger still open");

            // The researcher's route back, taken once the headset is off.
            session.ReturnToSetup();
            yield return Until("researcher can return to setup", () => Scene() == "ResearcherSetup");

            var folder = config.sessionId;
            var root = Path.Combine(Application.persistentDataPath, "VRMaintenanceResearchData", "Sessions", folder);
            foreach (var file in new[] { "session_manifest.csv", "session_events.csv", "task_summary.csv" })
                Check("wrote " + file, File.Exists(Path.Combine(root, file)), Path.Combine(root, file));

            var summary = File.Exists(Path.Combine(root, "task_summary.csv"))
                ? File.ReadAllLines(Path.Combine(root, "task_summary.csv"))
                : new string[0];
            // Training is a task with its own definition, so it writes a summary row too:
            // header plus Training, Computer and Fan.
            var rows = summary.Skip(1).Where(row => !string.IsNullOrWhiteSpace(row)).ToList();
            Check("one summary row per completed task, training included", rows.Count == 3, "data rows: " + rows.Count);
            Check("the two maintenance tasks and training each appear once",
                new[] { "Training", "Computer", "Fan" }.All(task => rows.Count(row => row.Split(',')[3] == task) == 1),
                string.Join(", ", rows.Select(row => row.Split(',')[3])));

            var columns = summary.Length == 0 ? 0 : summary[0].Split(',').Length;
            Check("every summary row has the full column count",
                rows.All(row => row.Split(',').Length == columns), "header has " + columns);
            report.AppendLine("      tasks: " + string.Join(", ", rows.Select(row => row.Split(',')[3] + "=" + row.Split(',')[24])));

            Finish();
        }

        IEnumerator RunTask(string which, string expectedScene)
        {
            yield return Until(which + " task scene loads (" + expectedScene + ")", () => Scene() == expectedScene);

            var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
            Check(which + " task has a controller", controller != null, "none");
            if (controller == null)
                yield break;

            yield return Until(which + " task becomes active", () => controller.IsActive);

            // Read an information source, the way a participant may choose to.
            var source = Object.FindObjectsByType<InformationSourceController>(FindObjectsSortMode.None).FirstOrDefault();
            Check(which + " task offers an information source", source != null, "none");
            if (source != null)
            {
                source.Open();
                yield return null;
                Check(which + " task information source opens", source.IsOpen, "did not open");
                source.Close();
            }

            // Test the device untouched, repair it, test again.
            controller.RunDeviceTest();
            Check(which + " task survives a failed device test", controller.IsActive, "state=" + controller.State);

            var requiredId = new SerializedObject(controller).FindProperty("requiredRepairObjectId").stringValue;
            var repair = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(interactable => interactable.StableObjectId == requiredId);
            Check(which + " task keeps its repair object '" + requiredId + "'", repair != null, "missing");
            if (repair == null)
                yield break;

            controller.RecordInteraction(repair);
            controller.RunDeviceTest();
            Check(which + " task completes after the repair", controller.State == TaskState.Completed, "state=" + controller.State);

            // The board tells the participant to wait; it must not offer them a way on,
            // because the headset comes off between tasks for NASA-TLX.
            var board = Object.FindFirstObjectByType<TaskStatusBoard>();
            Check(which + " task has a status board", board != null, "none");
            if (board == null)
                yield break;

            yield return null;
            var finished = Field<TMPro.TextMeshProUGUI>(board, "finishedLabel");
            Check(which + " task board shows the finished notice",
                finished != null && finished.transform.parent.gameObject.activeSelf, "notice not shown");
            Check(which + " task board notice is in the participant's language",
                finished != null && finished.text.Contains("กรุณารอผู้วิจัย"), finished == null ? "no label" : finished.text);
            Check(which + " task board carries no control the participant could press",
                board.GetComponentsInChildren<Button>(true).Length == 0,
                board.GetComponentsInChildren<Button>(true).Length + " buttons on the board");

            // Advancing is the researcher's, from the desktop panel.
            ResearchSessionManager.Instance.CompleteCurrentTaskAndAdvance();
        }

        // ---------- assertions ----------

        void Check(string what, bool passed, string detail)
        {
            if (passed)
            {
                report.AppendLine("PASS " + what);
                return;
            }
            failures++;
            report.AppendLine("FAIL " + what + "\n      " + detail);
        }

        /// <summary>
        /// The preconditions for a controller ray reaching a uGUI button: a world-space
        /// canvas with a TrackedDeviceGraphicRaycaster, and a graphic that accepts
        /// raycasts. A button wired correctly but sitting on a screen-space canvas is
        /// invisible to the headset and perfectly clickable with a mouse, which is the
        /// failure this catches.
        /// </summary>
        void CheckReachable(string what, Button button)
        {
            if (button == null)
            {
                Check(what + " is reachable by a controller ray", false, "no button");
                return;
            }

            var canvas = button.GetComponentInParent<Canvas>();
            Check(what + " sits on a world-space canvas",
                canvas != null && canvas.renderMode == RenderMode.WorldSpace,
                canvas == null ? "no canvas" : canvas.renderMode.ToString());
            Check(what + " canvas raycasts tracked devices",
                canvas != null && canvas.GetComponentInParent<TrackedDeviceGraphicRaycaster>() != null, "no TrackedDeviceGraphicRaycaster");
            Check(what + " graphic accepts raycasts",
                button.targetGraphic != null && button.targetGraphic.raycastTarget, "raycastTarget is off");
        }

        IEnumerator Until(string what, System.Func<bool> condition)
        {
            for (var waited = 0f; waited < k_StepTimeout; waited += Time.unscaledDeltaTime)
            {
                if (condition())
                {
                    report.AppendLine("PASS " + what);
                    yield break;
                }
                yield return null;
            }

            failures++;
            report.AppendLine("FAIL " + what + "\n      timed out after " + k_StepTimeout + "s in scene '" + Scene() + "'");
            Finish();
        }

        /// <summary>Clears the step stack as well, so a timeout stops the run rather than letting the caller carry on past a step that never happened.</summary>
        void Finish()
        {
            report.AppendLine(failures == 0 ? "WALKTHROUGH PASSED" : failures + " CHECKS FAILED");
            File.WriteAllText(ReportPath, report.ToString());
            Debug.Log("[Walkthrough]\n" + report);
            steps.Clear();
            EditorApplication.update -= Step;
            EditorApplication.isPlaying = false;
        }

        // ---------- reflection helpers ----------
        //
        // The boards expose no "ready" property: they unlock by enabling their button,
        // and the training board tracks its skills in private fields. Reading and
        // setting those directly is what lets this drive the flow without an Inspector.

        const System.Reflection.BindingFlags Flags =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

        static T Field<T>(object target, string name) where T : class =>
            target.GetType().GetField(name, Flags)?.GetValue(target) as T;

        static void Set(object target, string name, object value) =>
            target.GetType().GetField(name, Flags)?.SetValue(target, value);

        static string Scene() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}
