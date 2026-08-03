using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Play-mode checks for the repair loop, run from the menu while the scene is
    /// playing. They drive the controller through the sequence a participant performs —
    /// test the device untouched and expect a fail, perform the repair, test again and
    /// expect a pass, then reset — and assert the state after each step.
    ///
    /// The visual audit cannot see any of this: a scene can look perfect and still have
    /// a rebuild that detached the repair object from its task. These run against the
    /// live controller so a regression in the loop fails here rather than in a session.
    /// </summary>
    public static class ResearchRuntimeChecks
    {
        const string k_ReportPath = "Assets/VRMaintenanceResearch/Docs/Verification/Runtime_Checks.txt";

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Run Runtime Checks (Play Mode)", priority = 3)]
        public static void Run()
        {
            var report = new StringBuilder();
            report.AppendLine("=== runtime checks ===");
            report.AppendLine(Application.isPlaying ? "play mode: yes" : "play mode: NO — results are not valid");
            report.AppendLine();

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            report.AppendLine($"scene: {scene}");

            var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
            if (controller == null)
            {
                report.AppendLine("FAIL no MaintenanceTaskController in scene");
                Write(report, scene);
                return;
            }

            var so = new SerializedObject(controller);
            var repairId = so.FindProperty("requiredRepairObjectId")?.stringValue ?? "";
            report.AppendLine($"required repair id: {repairId}");
            report.AppendLine($"state at start: {controller.State}");

            if (!controller.IsActive)
            {
                report.AppendLine("FAIL task is not Active — cannot drive the loop");
                Write(report, scene);
                return;
            }

            // 1. Device test before any repair must fail and must not end the task.
            controller.RunDeviceTest();
            report.AppendLine($"after test #1 (no repair): state={controller.State} " +
                              $"{(controller.State == TaskState.Active ? "PASS expected fail, task still active" : "FAIL task ended without a repair")}");

            // 2. Perform the repair the definition actually asks for.
            var repair = Object.FindObjectsByType<ResearchInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(i => i.StableObjectId == repairId);
            if (repair == null)
            {
                report.AppendLine($"FAIL no interactable carries the required repair id '{repairId}'");
                Write(report, scene);
                return;
            }
            report.AppendLine($"repair object found: {repair.name} (kind={repair.Kind})");
            controller.RecordInteraction(repair);

            // 3. Device test after the repair must pass and complete the task.
            controller.RunDeviceTest();
            report.AppendLine($"after test #2 (repaired): state={controller.State} " +
                              $"{(controller.State == TaskState.Completed ? "PASS task completed" : "FAIL task did not complete")}");

            // 4. Reset must put the task back to a runnable state.
            controller.ResetDevelopmentTask();
            report.AppendLine($"after reset: state={controller.State} " +
                              $"{(controller.State == TaskState.Active || controller.State == TaskState.NotStarted ? "PASS reset" : "FAIL reset left " + controller.State)}");

            Write(report, scene);
        }

        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Run Training Checks (Play Mode)", priority = 4)]
        public static void RunTraining()
        {
            var report = new StringBuilder();
            report.AppendLine("=== training runtime check ===");
            report.AppendLine(Application.isPlaying ? "play mode: yes" : "play mode: NO — results are not valid");

            var board = Object.FindFirstObjectByType<TrainingInstructions>();
            if (board == null)
            {
                report.AppendLine("FAIL no TrainingInstructions in scene");
                Write(report, "VRTraining");
                return;
            }

            var type = typeof(TrainingInstructions);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var refresh = type.GetMethod("Refresh", flags);

            string[] skills = { "grabSatisfied", "socketSatisfied", "knobSatisfied", "informationSatisfied" };
            foreach (var skill in skills)
            {
                var field = type.GetField(skill, flags);
                if (field == null) { report.AppendLine($"FAIL no field '{skill}'"); continue; }
                field.SetValue(board, true);
                refresh?.Invoke(board, null);
                report.AppendLine($"{skill} = true; continue unlocked = {ContinueUnlocked(board, type, flags)}");
            }

            report.AppendLine(ContinueUnlocked(board, type, flags)
                ? "PASS all four skills complete and Continue is unlocked"
                : "FAIL Continue did not unlock after all four skills");

            foreach (var skill in skills)
                type.GetField(skill, flags)?.SetValue(board, false);
            refresh?.Invoke(board, null);
            report.AppendLine($"after reset: continue unlocked = {ContinueUnlocked(board, type, flags)} " +
                              $"{(ContinueUnlocked(board, type, flags) ? "FAIL still unlocked" : "PASS relocked")}");

            Write(report, "VRTraining");
        }

        /// <summary>
        /// The board has no "ready" property: it unlocks by enabling its Continue
        /// button, so that button's own state is the thing to assert.
        /// </summary>
        static bool ContinueUnlocked(object board, System.Type type, BindingFlags flags)
        {
            var field = type.GetField("continueButton", flags);
            var button = field?.GetValue(board) as UnityEngine.UI.Button;
            return button != null && button.interactable && button.gameObject.activeSelf;
        }

        static void Write(StringBuilder report, string scene)
        {
            var path = k_ReportPath.Replace(".txt", $"_{scene}.txt");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, report.ToString());
            Debug.Log("[RuntimeChecks]\n" + report);
        }
    }
}
