using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public sealed class ResearchMovementLogger : MonoBehaviour
    {
        [SerializeField] Transform headset;
        [SerializeField] Transform leftController;
        [SerializeField] Transform rightController;
        [SerializeField, Min(0.1f)] float samplingHz = 10f;
        MaintenanceTaskController task;
        float nextSampleAt;

        public void Begin(MaintenanceTaskController owner, float configuredSamplingHz)
        {
            task = owner;
            samplingHz = Mathf.Clamp(configuredSamplingHz, 0.1f, 120f);
            if (headset == null && Camera.main != null)
                headset = Camera.main.transform;
            nextSampleAt = Time.unscaledTime;
        }

        void Update()
        {
            if (task == null || !task.IsActive || Time.unscaledTime < nextSampleAt)
                return;

            nextSampleAt = Time.unscaledTime + 1f / samplingHz;
            var logger = ResearchSessionManager.Instance?.Logger;
            if (logger == null)
                return;

            logger.LogMovement(task.TaskId, task.AttemptId, "Headset", headset, samplingHz);
            logger.LogMovement(task.TaskId, task.AttemptId, "LeftController", leftController, samplingHz);
            logger.LogMovement(task.TaskId, task.AttemptId, "RightController", rightController, samplingHz);
        }
    }
}
