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

            // 4. A finished task must tell the participant it is over, in their own
            //    language, and must not hand them a control: the headset comes off
            //    between the two tasks for NASA-TLX, so anything pressable here could
            //    load the next task before that questionnaire was administered.
            const BindingFlags boardFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var board = Object.FindFirstObjectByType<TaskStatusBoard>();
            var boardRefresh = typeof(TaskStatusBoard).GetMethod("Refresh", boardFlags);
            if (board == null)
            {
                report.AppendLine("FAIL no TaskStatusBoard in scene — the participant is told nothing when the task ends");
            }
            else
            {
                // Update() has not run since the state changed inside this one call, so
                // drive the refresh the same way a frame would.
                boardRefresh?.Invoke(board, new object[] { true });
                var shown = NoticeShown(board, boardFlags);
                report.AppendLine($"status board notice after completion: shown={shown} " +
                                  $"{(shown ? "PASS participant is told the task is over" : "FAIL nothing tells the participant the task ended")}");

                var buttons = board.GetComponentsInChildren<UnityEngine.UI.Button>(true).Length;
                report.AppendLine($"status board controls: {buttons} " +
                                  $"{(buttons == 0 ? "PASS read-only, cannot skip NASA-TLX" : "FAIL the participant can press something on the board")}");
            }

            // 5. Reset must put the task back to a runnable state, and must take the
            //    finished notice away again so a live task does not read as over.
            controller.ResetDevelopmentTask();
            report.AppendLine($"after reset: state={controller.State} " +
                              $"{(controller.State == TaskState.Active || controller.State == TaskState.NotStarted ? "PASS reset" : "FAIL reset left " + controller.State)}");

            if (board != null)
            {
                boardRefresh?.Invoke(board, new object[] { true });
                var stillShown = NoticeShown(board, boardFlags);
                report.AppendLine($"status board notice after reset: shown={stillShown} " +
                                  $"{(stillShown ? "FAIL a running task still reads as finished" : "PASS hidden while the task is running")}");
            }

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
        /// First-person capture, both ways round: with consent there is a real file with
        /// real bytes in it, and without consent there is no file at all.
        ///
        /// This has to be a play-mode check on a timer rather than a unit test, because
        /// what could plausibly break is not the gate — that is covered by
        /// <c>FirstPersonRecordingIsRefusedWithoutConsentAndOutsideTheTwoMaintenanceTasks</c>
        /// — but everything after it: a camera that renders nothing under URP, a
        /// readback that throws, a stream that is opened and never written. All three
        /// would leave `first_person_recording_enabled` reading TRUE in
        /// session_manifest.csv beside a folder with no footage in it, which is the one
        /// failure this feature must not have. So the check runs the recorder for real
        /// and then looks on disk.
        ///
        /// Run it with a maintenance scene playing. It flips consent on, restarts the
        /// attempt so the recorder is created, waits for capture to accumulate, and
        /// reports.
        /// </summary>
        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Run Recording Checks (Play Mode)", priority = 5)]
        public static void RunRecording()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            var session = ResearchSessionManager.Instance;
            var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
            if (!Application.isPlaying || session == null || controller == null)
            {
                var early = new StringBuilder();
                early.AppendLine("=== first-person recording check ===");
                early.AppendLine("FAIL not in play mode in a maintenance scene");
                Write(early, scene + "_Recording");
                return;
            }

            var folder = session.Logger.CurrentDataFolder;
            var config = session.Configuration;

            // Half one, and it is already true when this runs: development sessions
            // start with consent off, so whatever the current attempt wrote is the
            // consent-off case.
            var beforeCount = CaptureFiles(folder).Length;

            config.firstPersonRecordingConsent = true;
            config.firstPersonRecordingEnabled = true;
            controller.ResetDevelopmentTask();

            // Six seconds is thirty frames at the capture rate: enough that a stream
            // which opens and then writes nothing is distinguishable from one that is
            // working, and short enough to sit through.
            var finishAt = EditorApplication.timeSinceStartup + 6d;
            void Poll()
            {
                if (EditorApplication.timeSinceStartup < finishAt && Application.isPlaying)
                    return;

                EditorApplication.update -= Poll;

                var report = new StringBuilder();
                report.AppendLine("=== first-person recording check ===");
                report.AppendLine($"scene: {scene}");
                report.AppendLine($"session folder: {folder}");
                report.AppendLine($"consent OFF, before enabling: {beforeCount} capture file(s) " +
                                  $"{(beforeCount == 0 ? "PASS nothing was written without consent" : "FAIL footage exists without recorded consent")}");

                var files = CaptureFiles(folder);
                report.AppendLine($"consent ON, after {6} s: {files.Length} capture file(s)");
                foreach (var file in files)
                {
                    var info = new System.IO.FileInfo(file);
                    report.AppendLine($"  {info.Name}: {info.Length / 1024} KB " +
                                      $"{(info.Length > 0 ? "PASS non-empty" : "FAIL opened but never written")}");
                }

                if (files.Length == 0)
                    report.AppendLine("FAIL consent and enable were both on and no file was produced");

                // The name has to carry participant code, task and attempt, because that
                // is what ties a recording back to a row in the CSVs. A file that cannot
                // be attributed is a confidentiality problem as much as a data one.
                var expected = ResearchIdentifiers.SanitizeForPath(config.participantCode) + "_" + controller.TaskId + "_attempt" + controller.AttemptId + ".mjpeg";
                report.AppendLine($"expected name for the current attempt: {expected} " +
                                  $"{(files.Any(f => System.IO.Path.GetFileName(f) == expected) ? "PASS present" : "FAIL not found")}");

                Write(report, scene + "_Recording");
            }

            EditorApplication.update += Poll;
        }

        static string[] CaptureFiles(string folder) =>
            string.IsNullOrEmpty(folder) || !System.IO.Directory.Exists(folder)
                ? System.Array.Empty<string>()
                : System.IO.Directory.GetFiles(folder, "*.mjpeg");

        /// <summary>
        /// The training board has no "ready" property: it unlocks by enabling its
        /// Continue button, so that button's own state is the thing to assert.
        /// </summary>
        static bool ContinueUnlocked(object board, System.Type type, BindingFlags flags)
        {
            var field = type.GetField("continueButton", flags);
            var button = field?.GetValue(board) as UnityEngine.UI.Button;
            return button != null && button.interactable && button.gameObject.activeSelf;
        }

        /// <summary>The status board signals a finished task by enabling the panel its notice sits in.</summary>
        static bool NoticeShown(TaskStatusBoard board, BindingFlags flags)
        {
            var label = typeof(TaskStatusBoard).GetField("finishedLabel", flags)?.GetValue(board) as TMPro.TextMeshProUGUI;
            return label != null && label.transform.parent.gameObject.activeSelf;
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
