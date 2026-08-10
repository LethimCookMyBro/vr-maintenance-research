using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public static class ResearchFoundationDirectTestRunner
    {
        [MenuItem("VR Maintenance Research/Run Foundation Edit Mode Tests")]
        public static void Run()
        {
            var tests = new ResearchFoundationEditorTests();
            var cases = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>(nameof(tests.SessionConfigRejectsRecordingWithoutConsent), tests.SessionConfigRejectsRecordingWithoutConsent),
                new KeyValuePair<string, Action>(nameof(tests.FirstPersonRecordingIsRefusedWithoutConsentAndOutsideTheTwoMaintenanceTasks), tests.FirstPersonRecordingIsRefusedWithoutConsentAndOutsideTheTwoMaintenanceTasks),
                new KeyValuePair<string, Action>(nameof(tests.EveryTranslationIsPresentAndKeepsTheEnglishNumerals), tests.EveryTranslationIsPresentAndKeepsTheEnglishNumerals),
                new KeyValuePair<string, Action>(nameof(tests.CsvUsesInvariantNumbersAndEscapesInternationalText), tests.CsvUsesInvariantNumbersAndEscapesInternationalText),
                new KeyValuePair<string, Action>(nameof(tests.DevelopmentLoggerCreatesSeparateSessionFiles), tests.DevelopmentLoggerCreatesSeparateSessionFiles),
                new KeyValuePair<string, Action>(nameof(tests.LoggerCanStartAnotherSessionAfterClosing), tests.LoggerCanStartAnotherSessionAfterClosing),
                new KeyValuePair<string, Action>(nameof(tests.TaskEventTimestampIsRelativeToTaskStart), tests.TaskEventTimestampIsRelativeToTaskStart),
                new KeyValuePair<string, Action>(nameof(tests.SessionSummaryContainsOneRowPerCompletedTask), tests.SessionSummaryContainsOneRowPerCompletedTask)
            };

            foreach (var testCase in cases)
            {
                testCase.Value();
                Debug.Log("[ResearchFoundationDirectTestRunner] PASS " + testCase.Key);
            }

            Debug.Log("[ResearchFoundationDirectTestRunner] PASS " + cases.Count + " tests");
        }
    }
}
