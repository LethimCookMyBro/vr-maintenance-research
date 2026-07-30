using System;
using System.Globalization;
using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public sealed class ResearchFoundationEditorTests
    {
        [Test]
        public void SessionConfigRejectsRecordingWithoutConsent()
        {
            var config = new ResearchSessionConfig { firstPersonRecordingEnabled = true, firstPersonRecordingConsent = false };
            Assert.That(config.Validate(out var error), Is.False);
            Assert.That(error, Does.Contain("consent"));
        }

        [Test]
        public void CsvUsesInvariantNumbersAndEscapesInternationalText()
        {
            Assert.That(CsvUtility.Cell(1.25f), Is.EqualTo("1.250000"));
            var escaped = CsvUtility.Cell("Thai, Japanese " + '"' + "quoted" + '"');
            Assert.That(escaped[0], Is.EqualTo('"'));
            Assert.That(escaped, Does.Contain("quoted"));
        }

        [Test]
        public void DevelopmentLoggerCreatesSeparateSessionFiles()
        {
            var sessionObject = new GameObject("LoggerTest");
            var definition = ScriptableObject.CreateInstance<ResearchTaskDefinition>();
            definition.taskId = ResearchTaskId.Computer;
            definition.layoutId = "computer-layout-test";
            definition.taskContext = "computer-maintenance";
            var logger = sessionObject.AddComponent<ResearchLogService>();
            var config = new ResearchSessionConfig { participantCode = "TEST_LOGGER", sessionId = "test_logger_" + Guid.NewGuid().ToString("N").Substring(0, 8), developmentMode = true };

            Assert.That(logger.BeginSession(config, out var error), Is.True, error);
            logger.BeginTask(definition, 1);
            logger.LogEvent(ResearchTaskId.Computer, 1, definition.taskContext, definition.layoutId, ResearchEventType.DeviceTestStarted, "computer.power-button", "device", "", "", "", "observed", TaskState.Active, "test", Vector3.zero, Quaternion.identity, "test");
            logger.EndTask(ResearchTaskId.Computer, TaskState.Completed);
            logger.EndSession("TestCompleted");

            Assert.That(File.Exists(Path.Combine(logger.CurrentDataFolder, "session_manifest.csv")), Is.True);
            Assert.That(File.Exists(Path.Combine(logger.CurrentDataFolder, "Computer", "events.csv")), Is.True);
            Assert.That(File.Exists(Path.Combine(logger.CurrentDataFolder, "Computer", "movement.csv")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(logger.CurrentDataFolder, "Computer", "events.csv")), Does.Contain("event_sequence_number"));

            UnityEngine.Object.DestroyImmediate(definition);
            UnityEngine.Object.DestroyImmediate(sessionObject);
        }

        [Test]
        public void LoggerCanStartAnotherSessionAfterClosing()
        {
            var sessionObject = new GameObject("LoggerRestartTest");
            var logger = sessionObject.AddComponent<ResearchLogService>();
            var first = new ResearchSessionConfig { participantCode = "TEST_RESTART", sessionId = "restart_one_" + Guid.NewGuid().ToString("N").Substring(0, 8), developmentMode = true };
            var second = new ResearchSessionConfig { participantCode = "TEST_RESTART", sessionId = "restart_two_" + Guid.NewGuid().ToString("N").Substring(0, 8), developmentMode = true };

            Assert.That(logger.BeginSession(first, out var firstError), Is.True, firstError);
            logger.EndSession("FirstComplete");
            Assert.That(logger.BeginSession(second, out var secondError), Is.True, secondError);
            Assert.That(logger.IsStarted, Is.True);
            logger.EndSession("SecondComplete");

            UnityEngine.Object.DestroyImmediate(sessionObject);
        }

        [Test]
        public void TaskEventTimestampIsRelativeToTaskStart()
        {
            var sessionObject = new GameObject("TaskTimestampTest");
            var logger = sessionObject.AddComponent<ResearchLogService>();
            var definition = ScriptableObject.CreateInstance<ResearchTaskDefinition>();
            definition.taskId = ResearchTaskId.Computer;
            definition.layoutId = "timestamp-layout";
            definition.taskContext = "timestamp-test";
            var config = new ResearchSessionConfig { participantCode = "TEST_TIME", sessionId = "task_time_" + Guid.NewGuid().ToString("N").Substring(0, 8), developmentMode = true };

            Assert.That(logger.BeginSession(config, out var error), Is.True, error);
            Thread.Sleep(25);
            logger.BeginTask(definition, 1);
            logger.EndTask(ResearchTaskId.Computer, TaskState.Completed);
            logger.EndSession("TestCompleted");

            var firstEvent = File.ReadAllLines(Path.Combine(logger.CurrentDataFolder, "Computer", "events.csv"))[1].Split(',');
            Assert.That(double.Parse(firstEvent[11], CultureInfo.InvariantCulture), Is.LessThan(0.1d));

            UnityEngine.Object.DestroyImmediate(definition);
            UnityEngine.Object.DestroyImmediate(sessionObject);
        }

        [Test]
        public void SessionSummaryContainsOneRowPerCompletedTask()
        {
            var sessionObject = new GameObject("SummaryRowsTest");
            var logger = sessionObject.AddComponent<ResearchLogService>();
            var computer = ScriptableObject.CreateInstance<ResearchTaskDefinition>();
            var fan = ScriptableObject.CreateInstance<ResearchTaskDefinition>();
            computer.taskId = ResearchTaskId.Computer;
            computer.taskContext = "computer";
            computer.layoutId = "computer-layout";
            fan.taskId = ResearchTaskId.Fan;
            fan.taskContext = "fan";
            fan.layoutId = "fan-layout";
            var config = new ResearchSessionConfig { participantCode = "TEST_SUMMARY", sessionId = "summary_rows_" + Guid.NewGuid().ToString("N").Substring(0, 8), developmentMode = true };

            Assert.That(logger.BeginSession(config, out var error), Is.True, error);
            logger.BeginTask(computer, 1);
            logger.EndTask(ResearchTaskId.Computer, TaskState.Completed);
            logger.BeginTask(fan, 1);
            logger.EndTask(ResearchTaskId.Fan, TaskState.Completed);
            logger.EndSession("TestCompleted");

            var rows = File.ReadAllLines(Path.Combine(logger.CurrentDataFolder, "task_summary.csv"));
            Assert.That(rows, Has.Length.EqualTo(3));
            Assert.That(rows[1], Does.Contain(",Computer,"));
            Assert.That(rows[2], Does.Contain(",Fan,"));

            UnityEngine.Object.DestroyImmediate(computer);
            UnityEngine.Object.DestroyImmediate(fan);
            UnityEngine.Object.DestroyImmediate(sessionObject);
        }
    }
}
