using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public sealed class ResearchMovementLogger : MonoBehaviour
    {
        [SerializeField] Transform headset;
        [SerializeField] Transform leftController;
        [SerializeField] Transform rightController;
        [SerializeField] TaskCoordinateRoot coordinateRoot;
        [SerializeField, Min(0.1f)] float samplingHz = 10f;
        MaintenanceTaskController task;
        float nextSampleAt;

        public void Begin(MaintenanceTaskController owner, float configuredSamplingHz)
        {
            task = owner;
            samplingHz = Mathf.Clamp(configuredSamplingHz, 0.1f, 120f);
            if (headset == null && Camera.main != null)
                headset = Camera.main.transform;
            if (coordinateRoot == null)
                coordinateRoot = FindFirstObjectByType<TaskCoordinateRoot>();
            nextSampleAt = Time.unscaledTime;
        }

        void Update()
        {
            if (task == null || !task.IsActive || Time.unscaledTime < nextSampleAt)
                return;

            if (leftController == null)
                leftController = FindController("Left Controller");
            if (rightController == null)
                rightController = FindController("Right Controller");
            nextSampleAt = Time.unscaledTime + 1f / samplingHz;
            var logger = ResearchSessionManager.Instance?.Logger;
            if (logger == null)
                return;

            logger.LogMovement(task.TaskId, task.AttemptId, "Headset", coordinateRoot, headset, samplingHz);
            logger.LogMovement(task.TaskId, task.AttemptId, "LeftController", coordinateRoot, leftController, samplingHz);
            logger.LogMovement(task.TaskId, task.AttemptId, "RightController", coordinateRoot, rightController, samplingHz);
        }

        static Transform FindController(string name)
        {
            foreach (var candidate in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (candidate.name == name && candidate.GetComponent("TrackedPoseDriver") != null)
                    return candidate;
            return null;
        }
    }
}
