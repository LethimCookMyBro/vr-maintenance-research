using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    [DisallowMultipleComponent]
    public sealed class ResearchLogService : MonoBehaviour
    {
        const string SchemaVersion = "1.0";
        static readonly string[] EventHeader = { "schema_version", "participant_code", "session_id", "task_id", "task_attempt_id", "task_context", "task_order", "participant_group", "language", "layout_id", "information_source_layout_id", "timestamp_from_task_start_seconds", "absolute_timestamp_utc", "event_sequence_number", "event_type", "object_id", "object_category", "information_source_id", "information_source_type", "source_slot", "action_result", "task_state", "measurement_method", "coordinate_space_id", "position_x_m", "position_y_m", "position_z_m", "rotation_x", "rotation_y", "rotation_z", "rotation_w", "additional_value", "application_build_version", "task_content_version" };
        static readonly string[] MovementHeader = { "schema_version", "participant_code", "session_id", "task_id", "task_attempt_id", "timestamp_from_task_start_seconds", "absolute_timestamp_utc", "sample_sequence_number", "device_type", "coordinate_space_id", "position_x_m", "position_y_m", "position_z_m", "rotation_x", "rotation_y", "rotation_z", "rotation_w", "tracking_valid", "simulator_mode", "sampling_frequency_hz", "application_build_version", "task_content_version" };
        static readonly string[] SummaryHeader = { "schema_version", "participant_code", "session_id", "task_id", "task_attempt_id", "derivation_version", "first_meaningful_action", "first_meaningful_action_timestamp_seconds", "first_information_source_opened", "first_information_source_timestamp_seconds", "action_occurred_before_first_information_access", "returned_to_information_after_unsuccessful_action", "information_source_switch_count", "repeated_information_access_count", "total_information_viewing_duration_seconds", "manual_viewing_duration_seconds", "text_guide_viewing_duration_seconds", "video_viewing_duration_seconds", "visual_guide_viewing_duration_seconds", "unsuccessful_action_count", "retry_count", "device_test_count", "low_activity_period_count", "total_low_activity_duration_seconds", "completion_status", "completion_time_seconds", "timeout_status", "abort_status" };
        readonly Dictionary<ResearchTaskId, TaskLog> taskLogs = new Dictionary<ResearchTaskId, TaskLog>();
        ResearchSessionConfig config;
        string rootPath;
        DateTimeOffset sessionStartedUtc;
        long nextEventSequence;
        bool started;
        bool closed;
        StreamWriter sessionEvents;
        StreamWriter technicalLog;

        public string CurrentDataFolder => rootPath ?? string.Empty;
        public bool IsStarted => started && !closed;

        public bool BeginSession(ResearchSessionConfig sessionConfig, out string error)
        {
            error = string.Empty;
            if (IsStarted)
                return true;
            if (sessionConfig == null || !sessionConfig.Validate(out error))
                return false;

            try
            {
                closed = false;
                nextEventSequence = 0;
                config = sessionConfig;
                config.EnsureSessionId();
                sessionStartedUtc = DateTimeOffset.UtcNow;
                rootPath = Path.Combine(Application.persistentDataPath, "VRMaintenanceResearchData", config.developmentMode ? "Development" : "Sessions", config.sessionId);
                Directory.CreateDirectory(rootPath);
                sessionEvents = CsvUtility.CreateUtf8Writer(Path.Combine(rootPath, "session_events.csv"), EventHeader);
                technicalLog = new StreamWriter(Path.Combine(rootPath, "technical_log.txt"), true, new UTF8Encoding(false)) { AutoFlush = true };
                started = true;
                WriteManifest("InProgress");
                LogEvent(ResearchTaskId.Session, 0, "session", "session", ResearchEventType.SessionStarted, "session", "session", "", "", "", "success", TaskState.Active, "system", Vector3.zero, Quaternion.identity, "session_started");
                return true;
            }
            catch (Exception exception)
            {
                error = "Could not create the research data folder.";
                WriteTechnical("BeginSession failed: " + exception.GetType().Name);
                CloseAll();
                return false;
            }
        }

        public void BeginTask(ResearchTaskDefinition definition, int attemptId)
        {
            if (!IsStarted || definition == null || taskLogs.ContainsKey(definition.taskId))
                return;

            var taskFolder = Path.Combine(rootPath, definition.taskId.ToString());
            var taskLog = new TaskLog
            {
                definition = definition,
                attemptId = attemptId,
                startedAtSeconds = ElapsedSeconds,
                events = CsvUtility.CreateUtf8Writer(Path.Combine(taskFolder, "events.csv"), EventHeader),
                movement = CsvUtility.CreateUtf8Writer(Path.Combine(taskFolder, "movement.csv"), MovementHeader),
            };
            taskLogs.Add(definition.taskId, taskLog);
            LogEvent(definition.taskId, attemptId, definition.taskContext, definition.layoutId, ResearchEventType.TaskLoaded, "task." + definition.taskId.ToString().ToLowerInvariant(), "task", "", "", "", "success", TaskState.NotStarted, "system", Vector3.zero, Quaternion.identity, "task_loaded");
            LogEvent(definition.taskId, attemptId, definition.taskContext, definition.layoutId, ResearchEventType.TaskStarted, "task." + definition.taskId.ToString().ToLowerInvariant(), "task", "", "", "", "success", TaskState.Active, "system", Vector3.zero, Quaternion.identity, "task_started");
        }

        public void LogEvent(ResearchTaskId taskId, int attemptId, string taskContext, string layoutId, ResearchEventType eventType, string objectId, string objectCategory, string informationSourceId, string informationSourceType, string sourceSlot, string actionResult, TaskState taskState, string measurementMethod, Vector3 position, Quaternion rotation, string additionalValue)
        {
            if (!IsStarted)
                return;

            try
            {
                var sessionElapsedSeconds = ElapsedSeconds;
                var taskElapsedSeconds = taskId == ResearchTaskId.Session
                    ? sessionElapsedSeconds
                    : taskLogs.TryGetValue(taskId, out var activeTaskLog)
                        ? Math.Max(0d, sessionElapsedSeconds - activeTaskLog.startedAtSeconds)
                        : sessionElapsedSeconds;
                var record = new EventRecord
                {
                    elapsedSeconds = taskElapsedSeconds,
                    sequence = ++nextEventSequence,
                    eventType = eventType,
                    objectId = objectId ?? string.Empty,
                    informationSourceId = informationSourceId ?? string.Empty,
                    informationSourceType = informationSourceType ?? string.Empty,
                };
                var writer = taskId == ResearchTaskId.Session ? sessionEvents : taskLogs.TryGetValue(taskId, out var taskLog) ? taskLog.events : null;
                if (writer == null)
                {
                    WriteTechnical("Dropped event before task writer: " + eventType);
                    return;
                }

                CsvUtility.WriteRow(writer, SchemaVersion, config.participantCode, config.sessionId, taskId, attemptId, taskContext, config.taskOrder, config.participantGroup, config.language, layoutId, config.informationSourceLayoutId, record.elapsedSeconds, sessionStartedUtc.AddSeconds(sessionElapsedSeconds), record.sequence, eventType, objectId, objectCategory, informationSourceId, informationSourceType, sourceSlot, actionResult, taskState, measurementMethod, "task-local", position.x, position.y, position.z, rotation.x, rotation.y, rotation.z, rotation.w, additionalValue, config.applicationBuildVersion, config.taskContentVersion);
                if (taskId != ResearchTaskId.Session && taskLogs.TryGetValue(taskId, out taskLog))
                    taskLog.records.Add(record);
            }
            catch (Exception exception)
            {
                WriteTechnical("Event writer failure: " + exception.GetType().Name);
            }
        }

        public void LogMovement(ResearchTaskId taskId, int attemptId, string deviceType, Transform pose, float samplingHz)
        {
            if (!IsStarted || !taskLogs.TryGetValue(taskId, out var taskLog))
                return;
            try
            {
                var sessionElapsedSeconds = ElapsedSeconds;
                var taskElapsedSeconds = Math.Max(0d, sessionElapsedSeconds - taskLog.startedAtSeconds);
                var hasPose = pose != null;
                var position = hasPose ? pose.position : Vector3.zero;
                var rotation = hasPose ? pose.rotation : Quaternion.identity;
                CsvUtility.WriteRow(taskLog.movement, SchemaVersion, config.participantCode, config.sessionId, taskId, attemptId, taskElapsedSeconds, sessionStartedUtc.AddSeconds(sessionElapsedSeconds), ++taskLog.movementSequence, deviceType, "task-local", PoseCell(hasPose, position.x), PoseCell(hasPose, position.y), PoseCell(hasPose, position.z), RotationCell(hasPose, rotation.x), RotationCell(hasPose, rotation.y), RotationCell(hasPose, rotation.z), RotationCell(hasPose, rotation.w), hasPose, config.simulatorMode, samplingHz, config.applicationBuildVersion, config.taskContentVersion);
            }
            catch (Exception exception)
            {
                WriteTechnical("Movement writer failure: " + exception.GetType().Name);
            }
        }

        public void EndTask(ResearchTaskId taskId, TaskState terminalState)
        {
            if (!taskLogs.TryGetValue(taskId, out var taskLog))
                return;
            WriteTaskSummary(taskLog, terminalState);
            taskLog.events.Dispose();
            taskLog.movement.Dispose();
            taskLogs.Remove(taskId);
        }

        public void EndSession(string completionStatus)
        {
            if (!IsStarted || closed)
                return;
            LogEvent(ResearchTaskId.Session, 0, "session", "session", ResearchEventType.SessionEnded, "session", "session", "", "", "", completionStatus, TaskState.Completed, "system", Vector3.zero, Quaternion.identity, "session_ended");
            WriteManifest(completionStatus);
            CloseAll();
        }

        void OnApplicationQuit() => EndSession("ApplicationQuit");
        void OnDestroy() => CloseAll();

        void WriteManifest(string completionStatus)
        {
            var path = Path.Combine(rootPath, "session_manifest.csv");
            var temporaryPath = path + ".tmp";
            using (var writer = new StreamWriter(temporaryPath, false, new UTF8Encoding(false)))
            {
                CsvUtility.WriteRow(writer, "schema_version", "participant_code", "session_id", "participant_group", "language", "task_order", "experiment_site", "researcher_code", "session_start_utc", "headset_model", "platform", "unity_version", "xri_version", "application_build_version", "git_commit_hash", "task_content_version", "configuration_version", "computer_layout_id", "fan_layout_id", "information_source_layout_id", "movement_sampling_hz", "simulator_mode", "development_mode", "first_person_recording_consent", "first_person_recording_enabled", "session_completion_status", "logging_status", "technical_notes", "manifest_updated_utc");
                CsvUtility.WriteRow(writer, SchemaVersion, config.participantCode, config.sessionId, config.participantGroup, config.language, config.taskOrder, config.experimentSite, config.researcherCode, sessionStartedUtc, config.headsetModel, Application.platform, Application.unityVersion, "3.4.0", config.applicationBuildVersion, config.gitCommitHash, config.taskContentVersion, config.configurationVersion, config.computerLayoutId, config.fanLayoutId, config.informationSourceLayoutId, config.movementSamplingHz, config.simulatorMode, config.developmentMode, config.firstPersonRecordingConsent, config.firstPersonRecordingEnabled, completionStatus, "active", config.technicalNotes, DateTimeOffset.UtcNow);
            }
            if (File.Exists(path))
                File.Delete(path);
            File.Move(temporaryPath, path);
        }

        void WriteTaskSummary(TaskLog taskLog, TaskState terminalState)
        {
            var records = taskLog.records.OrderBy(record => record.sequence).ToList();
            var meaningful = records.FirstOrDefault(record => IsMeaningful(record.eventType));
            var firstSource = records.FirstOrDefault(record => record.eventType == ResearchEventType.InformationSourceOpened);
            var firstFailure = records.FirstOrDefault(record => IsFailure(record.eventType));
            var sourceOpens = records.Where(record => record.eventType == ResearchEventType.InformationSourceOpened).ToList();
            var seenSources = new HashSet<string>();
            var previousSource = string.Empty;
            var switches = 0;
            var repeats = 0;
            foreach (var source in sourceOpens)
            {
                if (!string.IsNullOrEmpty(previousSource) && previousSource != source.informationSourceId)
                    switches++;
                if (!seenSources.Add(source.informationSourceId))
                    repeats++;
                previousSource = source.informationSourceId;
            }
            var durations = DeriveDurations(records);
            var summaryPath = Path.Combine(rootPath, "task_summary.csv");
            var hasSummary = File.Exists(summaryPath) && new FileInfo(summaryPath).Length > 0;
            using var writer = hasSummary
                ? new StreamWriter(summaryPath, true, new UTF8Encoding(false)) { AutoFlush = true }
                : CsvUtility.CreateUtf8Writer(summaryPath, SummaryHeader);
            var completionTime = records.Count == 0 ? 0d : records[records.Count - 1].elapsedSeconds;
            var actionBeforeInformation = meaningful.sequence > 0 && (firstSource.sequence == 0 || (meaningful.eventType != ResearchEventType.InformationSourceOpened && meaningful.sequence < firstSource.sequence));
            CsvUtility.WriteRow(writer, SchemaVersion, config.participantCode, config.sessionId, taskLog.definition.taskId, taskLog.attemptId, "1.0", meaningful.sequence == 0 ? "" : meaningful.eventType.ToString(), meaningful.sequence == 0 ? "" : (object)meaningful.elapsedSeconds, firstSource.sequence == 0 ? "" : firstSource.informationSourceId, firstSource.sequence == 0 ? "" : (object)firstSource.elapsedSeconds, actionBeforeInformation, firstFailure.sequence > 0 && sourceOpens.Any(source => source.sequence > firstFailure.sequence), switches, repeats, durations.totalInformation, durations.manual, durations.text, durations.video, durations.visual, records.Count(record => IsFailure(record.eventType)), records.Count(record => record.eventType == ResearchEventType.RetryStarted), records.Count(record => record.eventType == ResearchEventType.DeviceTestStarted), durations.lowActivityPeriods, durations.lowActivity, terminalState.ToString(), completionTime, terminalState == TaskState.TimedOut, terminalState == TaskState.Aborted || terminalState == TaskState.SafetyStopped);
        }

        static (double totalInformation, double manual, double text, double video, double visual, int lowActivityPeriods, double lowActivity) DeriveDurations(List<EventRecord> records)
        {
            var sourceOpens = new Dictionary<string, EventRecord>();
            var lowStart = default(EventRecord);
            double manual = 0d, text = 0d, video = 0d, visual = 0d, low = 0d;
            var lowPeriods = 0;
            foreach (var record in records)
            {
                if (record.eventType == ResearchEventType.InformationSourceOpened)
                    sourceOpens[record.informationSourceId] = record;
                else if (record.eventType == ResearchEventType.InformationSourceClosed && sourceOpens.TryGetValue(record.informationSourceId, out var opened))
                {
                    var seconds = Math.Max(0d, record.elapsedSeconds - opened.elapsedSeconds);
                    switch (opened.informationSourceType)
                    {
                        case nameof(InformationSourceType.ProductManual): manual += seconds; break;
                        case nameof(InformationSourceType.TextTroubleshootingGuide): text += seconds; break;
                        case nameof(InformationSourceType.InstructionalVideo): video += seconds; break;
                        case nameof(InformationSourceType.VisualStepByStepGuide): visual += seconds; break;
                    }
                    sourceOpens.Remove(record.informationSourceId);
                }
                else if (record.eventType == ResearchEventType.LowActivityStarted)
                    lowStart = record;
                else if (record.eventType == ResearchEventType.LowActivityEnded && lowStart.sequence > 0)
                {
                    low += Math.Max(0d, record.elapsedSeconds - lowStart.elapsedSeconds);
                    lowPeriods++;
                    lowStart = default;
                }
            }
            return (manual + text + video + visual, manual, text, video, visual, lowPeriods, low);
        }

        static bool IsMeaningful(ResearchEventType type) => type == ResearchEventType.InformationSourceOpened || type == ResearchEventType.DeviceHovered || type == ResearchEventType.ComponentHovered || type == ResearchEventType.ToolHovered || type == ResearchEventType.ObjectGrabbed || type == ResearchEventType.ObjectPlaced || type == ResearchEventType.ComponentRemoved || type == ResearchEventType.ComponentInstalled || type == ResearchEventType.ToolSelected || type == ResearchEventType.ToolUsed || type == ResearchEventType.DeviceTestStarted || type == ResearchEventType.RetryStarted || IsFailure(type);
        static bool IsFailure(ResearchEventType type) => type == ResearchEventType.UnsuccessfulAction || type == ResearchEventType.IncorrectToolSelected || type == ResearchEventType.IncorrectComponentInteraction || type == ResearchEventType.DeviceTestFailed;
        static string PoseCell(bool tracked, float value) => tracked ? value.ToString("0.000000", CultureInfo.InvariantCulture) : "";
        static string RotationCell(bool tracked, float value) => tracked ? value.ToString("0.0000000", CultureInfo.InvariantCulture) : "";
        double ElapsedSeconds => Math.Max(0d, (DateTimeOffset.UtcNow - sessionStartedUtc).TotalSeconds);
        void WriteTechnical(string text) { try { technicalLog?.WriteLine(DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture) + " " + text); } catch { } }
        void CloseAll()
        {
            if (closed)
                return;
            closed = true;
            foreach (var taskLog in taskLogs.Values)
            {
                taskLog.events?.Dispose();
                taskLog.movement?.Dispose();
            }
            taskLogs.Clear();
            sessionEvents?.Dispose();
            technicalLog?.Dispose();
        }

        sealed class TaskLog
        {
            public ResearchTaskDefinition definition;
            public int attemptId;
            public double startedAtSeconds;
            public long movementSequence;
            public StreamWriter events;
            public StreamWriter movement;
            public readonly List<EventRecord> records = new List<EventRecord>();
        }

        struct EventRecord
        {
            public double elapsedSeconds;
            public long sequence;
            public ResearchEventType eventType;
            public string objectId;
            public string informationSourceId;
            public string informationSourceType;
        }
    }
}
