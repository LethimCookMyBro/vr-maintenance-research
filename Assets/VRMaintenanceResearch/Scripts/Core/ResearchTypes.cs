using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace TMUVR.MaintenanceResearch
{
    public enum ResearchTaskId { Session, Training, Computer, Fan }
    public enum ParticipantGroup { Thai, Japanese }
    public enum ResearchLanguage { Thai, Japanese, English }
    public enum TaskOrder { ComputerThenFan, FanThenComputer }
    public enum InformationSourceType { ProductManual, TextTroubleshootingGuide, InstructionalVideo, VisualStepByStepGuide }
    public enum TaskState { NotStarted, Active, Paused, Completed, TimedOut, Aborted, SafetyStopped, Reset }
    public enum ResearchEventType
    {
        SessionStarted, SessionEnded, TaskLoaded, TaskStarted, TaskPaused, TaskResumed, TaskCompleted, TaskTimedOut, TaskAborted, TaskReset, SafetyStop, TechnicalError,
        InformationSourceHovered, InformationSourceOpened, InformationSourceClosed, InformationPageChanged, VideoPlayed, VideoPaused, VideoStopped, VideoRestarted, VideoSeeked, VideoCompleted,
        DeviceHovered, ComponentHovered, ToolHovered, UIElementHovered, ControllerRayEntered, ControllerRayExited, HeadRayDwellStarted, HeadRayDwellEnded,
        ObjectGrabbed, ObjectReleased, ObjectPlaced, ComponentRemoved, ComponentInstalled, ToolSelected, ToolUsed, IncorrectToolSelected, IncorrectComponentInteraction,
        UnsuccessfulAction, DeviceTestStarted, DeviceTestPassed, DeviceTestFailed, RetryStarted, ResearcherAction, LowActivityStarted, LowActivityEnded
    }

    [Serializable]
    public sealed class ResearchSessionConfig
    {
        public string participantCode = "DEVELOPMENT_TEST";
        public string sessionId = "";
        public ParticipantGroup participantGroup = ParticipantGroup.Thai;
        public ResearchLanguage language = ResearchLanguage.English;
        public TaskOrder taskOrder = TaskOrder.ComputerThenFan;
        public string experimentSite = "DEVELOPMENT_SITE";
        public string researcherCode = "DEVELOPMENT_RESEARCHER";
        public string headsetModel = "XR Interaction Simulator";
        public string applicationBuildVersion = "development";
        public string gitCommitHash = "configured-at-build";
        public string taskContentVersion = "research-v2";
        public string configurationVersion = "development-1";
        public string computerLayoutId = "computer-layout-development-a";
        public string fanLayoutId = "fan-layout-development-a";
        public string informationSourceLayoutId = "sources-layout-development-a";
        public float movementSamplingHz = 10f;
        public bool simulatorMode = true;
        public bool developmentMode = true;
        public bool firstPersonRecordingConsent;
        public bool firstPersonRecordingEnabled;
        public bool trainingRequired = true;
        [UnityEngine.TextArea] public string technicalNotes = "";

        public bool Validate(out string reason)
        {
            if (string.IsNullOrWhiteSpace(participantCode))
            {
                reason = "Participant Code is required.";
                return false;
            }

            if (participantCode.Length > 64 || !ResearchIdentifiers.IsSafeIdentifier(participantCode))
            {
                reason = "Participant Code may contain only letters, numbers, hyphens, and underscores.";
                return false;
            }

            if (movementSamplingHz <= 0f || movementSamplingHz > 120f)
            {
                reason = "Movement sampling frequency must be between 0 and 120 Hz.";
                return false;
            }

            if (firstPersonRecordingEnabled && !firstPersonRecordingConsent)
            {
                reason = "First-person recording requires recorded consent.";
                return false;
            }

            if (!ResearchIdentifiers.IsTechnicalNoteSafe(technicalNotes))
            {
                reason = "Technical notes appear to contain personal information; remove names, emails, or phone-like numbers.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void EnsureSessionId()
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                sessionId = DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture) + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            sessionId = ResearchIdentifiers.SanitizeForPath(sessionId);
        }
    }

    public static class ResearchIdentifiers
    {
        static readonly Regex SafeIdentifier = new Regex("^[A-Za-z0-9_-]+$", RegexOptions.Compiled);
        static readonly Regex PhoneLike = new Regex("[0-9][0-9 ()-]{6,}[0-9]", RegexOptions.Compiled);
        static readonly string[] PersonalMarkers = { "@", "student id", "studentid", "phone", "email", "full name", "firstname", "surname" };

        public static bool IsSafeIdentifier(string value) => !string.IsNullOrWhiteSpace(value) && SafeIdentifier.IsMatch(value);

        public static string SanitizeForPath(string value)
        {
            var chars = (value ?? string.Empty).Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray();
            var sanitized = new string(chars);
            return string.IsNullOrEmpty(sanitized) ? "invalid" : sanitized;
        }

        public static bool IsTechnicalNoteSafe(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var lower = value.ToLowerInvariant();
            return !PersonalMarkers.Any(marker => lower.Contains(marker)) && !PhoneLike.IsMatch(value);
        }
    }
}
