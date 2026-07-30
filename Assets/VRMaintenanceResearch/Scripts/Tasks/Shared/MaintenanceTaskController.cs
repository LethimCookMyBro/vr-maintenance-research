using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    [DisallowMultipleComponent]
    public class MaintenanceTaskController : MonoBehaviour
    {
        [SerializeField] ResearchTaskDefinition definition;
        [SerializeField] string requiredRepairObjectId = "repair.correct";
        [SerializeField] bool allowDevelopmentReset = true;
        [SerializeField] ResearchMovementLogger movementLogger;
        [SerializeField, Min(1f)] float lowActivityThresholdSeconds = 30f;
        ResearchLogService logger;
        float activeElapsed;
        float lastMeaningfulActionAt;
        float lastHoverAt;
        bool repairActionPerformed;
        bool lowActivityOpen;
        int openInformationSourceCount;
        bool videoPlaying;

        public ResearchTaskId TaskId => definition == null ? ResearchTaskId.Session : definition.taskId;
        public int AttemptId { get; private set; } = 1;
        public TaskState State { get; private set; } = TaskState.NotStarted;
        public bool IsActive => State == TaskState.Active;
        public ResearchTaskDefinition Definition => definition;

        protected virtual void Start()
        {
            var session = ResearchSessionManager.Instance;
            if (session == null)
            {
                var sessionObject = new GameObject("Research Session Manager (Development)");
                session = sessionObject.AddComponent<ResearchSessionManager>();
            }

            if (!session.Logger.IsStarted && !session.StartSession(out var error))
            {
                Debug.LogError("Research session could not start: " + error);
                return;
            }

            logger = session.Logger;
            if (definition == null || definition.taskId == ResearchTaskId.Session)
            {
                Debug.LogError("A non-session ResearchTaskDefinition is required.");
                return;
            }

            StartAttempt();
        }

        protected virtual void Update()
        {
            if (!IsActive)
                return;

            activeElapsed += Time.unscaledDeltaTime;
            if (definition.maximumTimeSeconds > 0f && activeElapsed >= definition.maximumTimeSeconds)
            {
                End(TaskState.TimedOut, ResearchEventType.TaskTimedOut, "maximum_time_reached");
                return;
            }

            if (openInformationSourceCount > 0 || videoPlaying)
            {
                CloseLowActivity("information_active");
                return;
            }

            if (!lowActivityOpen && activeElapsed - Mathf.Max(lastMeaningfulActionAt, lastHoverAt) >= lowActivityThresholdSeconds)
            {
                lowActivityOpen = true;
                Log(ResearchEventType.LowActivityStarted, "task.low-activity", "task", "observed", "interaction_inactivity");
            }
        }

        public void RecordHover(ResearchInteractable interactable)
        {
            if (!IsActive || interactable == null)
                return;

            lastHoverAt = activeElapsed;
            var eventType = interactable.Kind == ResearchInteractionKind.Device ? ResearchEventType.DeviceHovered :
                interactable.Kind == ResearchInteractionKind.Tool ? ResearchEventType.ToolHovered : ResearchEventType.ComponentHovered;
            Log(eventType, interactable.StableObjectId, interactable.ObjectCategory, "observed", "controller-ray hover");
        }

        public void RecordInteraction(ResearchInteractable interactable)
        {
            if (!IsActive || interactable == null)
                return;

            RegisterMeaningfulAction();
            switch (interactable.Kind)
            {
                case ResearchInteractionKind.DeviceTest:
                    RunDeviceTest(interactable);
                    break;
                case ResearchInteractionKind.RepairAction:
                    var correct = interactable.StableObjectId == requiredRepairObjectId;
                    if (correct)
                    {
                        repairActionPerformed = true;
                        Log(ResearchEventType.ComponentInstalled, interactable.StableObjectId, interactable.ObjectCategory, "success", "repair_action");
                    }
                    else
                    {
                        Log(ResearchEventType.IncorrectComponentInteraction, interactable.StableObjectId, interactable.ObjectCategory, "failure", "incorrect_repair_action");
                    }
                    break;
                case ResearchInteractionKind.Tool:
                    Log(interactable.IsCorrect ? ResearchEventType.ToolUsed : ResearchEventType.IncorrectToolSelected, interactable.StableObjectId, interactable.ObjectCategory, interactable.IsCorrect ? "success" : "failure", "selected");
                    break;
                default:
                    Log(ResearchEventType.ObjectGrabbed, interactable.StableObjectId, interactable.ObjectCategory, "observed", "selected");
                    break;
            }
        }

        public void RunDeviceTest(ResearchInteractable source = null)
        {
            if (!IsActive)
                return;
            RegisterMeaningfulAction();
            var objectId = source == null ? "device.test" : source.StableObjectId;
            Log(ResearchEventType.DeviceTestStarted, objectId, "device", "observed", "device_test_started");
            if (repairActionPerformed)
            {
                Log(ResearchEventType.DeviceTestPassed, objectId, "device", "success", "device_test_passed");
                End(TaskState.Completed, ResearchEventType.TaskCompleted, "device_repaired");
            }
            else
            {
                Log(ResearchEventType.DeviceTestFailed, objectId, "device", "failure", "device_test_failed");
            }
        }

        public void PauseTask()
        {
            if (!IsActive)
                return;
            State = TaskState.Paused;
            Log(ResearchEventType.TaskPaused, "task.pause", "researcher-control", "success", "researcher");
        }

        public void ResumeTask()
        {
            if (State != TaskState.Paused)
                return;
            State = TaskState.Active;
            RegisterMeaningfulAction();
            Log(ResearchEventType.TaskResumed, "task.resume", "researcher-control", "success", "researcher");
        }

        public void AbortTask() => End(TaskState.Aborted, ResearchEventType.TaskAborted, "researcher_abort");
        public void SafetyStop() => End(TaskState.SafetyStopped, ResearchEventType.SafetyStop, "safety_stop");

        public void ResetDevelopmentTask()
        {
            if (!allowDevelopmentReset || ResearchSessionManager.Instance == null || !ResearchSessionManager.Instance.Configuration.developmentMode)
                return;

            Log(ResearchEventType.TaskReset, "task.reset", "researcher-control", "success", "development_reset");
            logger.EndTask(TaskId, TaskState.Reset);
            AttemptId++;
            StartAttempt();
        }

        public void NotifyInformation(InformationSourceDefinition source, ResearchEventType eventType, string detail)
        {
            if (!IsActive || source == null)
                return;

            switch (eventType)
            {
                case ResearchEventType.InformationSourceOpened:
                    openInformationSourceCount++;
                    RegisterMeaningfulAction();
                    break;
                case ResearchEventType.InformationSourceClosed:
                    openInformationSourceCount = Mathf.Max(0, openInformationSourceCount - 1);
                    RegisterMeaningfulAction();
                    break;
                case ResearchEventType.InformationPageChanged:
                case ResearchEventType.VideoSeeked:
                    RegisterMeaningfulAction();
                    break;
                case ResearchEventType.VideoPlayed:
                    videoPlaying = true;
                    RegisterMeaningfulAction();
                    break;
                case ResearchEventType.VideoPaused:
                case ResearchEventType.VideoStopped:
                    videoPlaying = false;
                    RegisterMeaningfulAction();
                    break;
            }

            Log(eventType, source.sourceId, "information-source", "observed", detail, source);
        }

        public void Retry()
        {
            if (!IsActive)
                return;
            RegisterMeaningfulAction();
            Log(ResearchEventType.RetryStarted, "task.retry", "task", "observed", "retry");
        }

        protected void End(TaskState terminalState, ResearchEventType eventType, string detail)
        {
            if (State == TaskState.Completed || State == TaskState.TimedOut || State == TaskState.Aborted || State == TaskState.SafetyStopped)
                return;
            if (lowActivityOpen)
            {
                lowActivityOpen = false;
                Log(ResearchEventType.LowActivityEnded, "task.low-activity", "task", "observed", "task_terminal");
            }
            State = terminalState;
            Log(eventType, "task." + TaskId.ToString().ToLowerInvariant(), "task", terminalState == TaskState.Completed ? "success" : "stopped", detail);
            logger.EndTask(TaskId, terminalState);
        }

        void StartAttempt()
        {
            activeElapsed = 0f;
            lastMeaningfulActionAt = 0f;
            lastHoverAt = 0f;
            repairActionPerformed = false;
            lowActivityOpen = false;
            openInformationSourceCount = 0;
            videoPlaying = false;
            State = TaskState.Active;
            logger.BeginTask(definition, AttemptId);
            if (movementLogger == null)
                movementLogger = gameObject.GetComponent<ResearchMovementLogger>() ?? gameObject.AddComponent<ResearchMovementLogger>();
            movementLogger.Begin(this, definition.movementSamplingHz > 0 ? definition.movementSamplingHz : ResearchSessionManager.Instance.Configuration.movementSamplingHz);
        }

        void RegisterMeaningfulAction()
        {
            lastMeaningfulActionAt = activeElapsed;
            CloseLowActivity("meaningful_action");
        }

        void CloseLowActivity(string detail)
        {
            if (!lowActivityOpen)
                return;
            lowActivityOpen = false;
            Log(ResearchEventType.LowActivityEnded, "task.low-activity", "task", "observed", detail);
        }

        void Log(ResearchEventType eventType, string objectId, string category, string result, string detail, InformationSourceDefinition source = null)
        {
            if (logger == null)
                return;
            logger.LogEvent(TaskId, AttemptId, definition.taskContext, definition.layoutId, eventType, objectId, category, source == null ? "" : source.sourceId, source == null ? "" : source.sourceType.ToString(), source == null ? "" : source.sourceSlot, result, State, "observed", transform.position, transform.rotation, detail);
        }
    }
}
