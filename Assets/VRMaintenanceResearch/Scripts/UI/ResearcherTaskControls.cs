using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public sealed class ResearcherTaskControls : MonoBehaviour
    {
        [SerializeField] MaintenanceTaskController task;
        [SerializeField] bool trainingScene;

        void Awake()
        {
            if (task == null)
                task = FindFirstObjectByType<MaintenanceTaskController>();
        }

        void OnGUI()
        {
            var session = ResearchSessionManager.Instance;
            if (session == null || !session.Configuration.developmentMode)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 260, 16, 244, 260), GUI.skin.box);
            GUILayout.Label(trainingScene ? "Development Training Controls" : "Researcher Controls (Development)");
            if (trainingScene)
            {
                if (GUILayout.Button("Continue to First Task"))
                {
                    FindFirstObjectByType<TrainingMaintenanceTask>()?.CompleteTraining();
                    session.StartFirstTaskAfterTraining();
                }
                if (GUILayout.Button("Skip Training (development only)"))
                {
                    FindFirstObjectByType<TrainingMaintenanceTask>()?.CompleteTraining();
                    session.SkipTrainingWhenPermitted();
                }
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label(task == null ? "Task unavailable" : "State: " + task.State);
            if (GUILayout.Button("Pause")) task?.PauseTask();
            if (GUILayout.Button("Resume")) task?.ResumeTask();
            if (GUILayout.Button("Retry")) task?.Retry();
            if (GUILayout.Button("Reset Task")) task?.ResetDevelopmentTask();
            if (GUILayout.Button("Abort Task")) task?.AbortTask();
            if (GUILayout.Button("Safety Stop")) task?.SafetyStop();
            if (task != null && (task.State == TaskState.Completed || task.State == TaskState.Aborted || task.State == TaskState.TimedOut || task.State == TaskState.SafetyStopped))
            {
                if (GUILayout.Button("Continue to Next Task"))
                    session.CompleteCurrentTaskAndAdvance();
            }
            GUILayout.EndArea();
        }
    }
}
