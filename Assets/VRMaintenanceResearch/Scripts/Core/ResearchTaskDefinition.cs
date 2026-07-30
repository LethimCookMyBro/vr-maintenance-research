using System;
using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    [CreateAssetMenu(menuName = "VR Maintenance Research/Task Definition", fileName = "MaintenanceTaskDefinition")]
    public sealed class ResearchTaskDefinition : ScriptableObject
    {
        public ResearchTaskId taskId;
        public string taskContext = "maintenance";
        public string taskContentVersion = "development-1";
        public string activeFaultId = "development-fault";
        public string layoutId = "development-layout-a";
        public string informationSourceLayoutId = "sources-layout-development-a";
        public float maximumTimeSeconds = 900f;
        public bool showTimerToParticipant;
        [TextArea] public string englishTitle = "Development maintenance task";
        [TextArea(3, 12)] public string englishParticipantInstructions = "Inspect, use any information source if desired, and test the device when ready.";
        public string[] availableComponents = Array.Empty<string>();
        public string[] availableTools = Array.Empty<string>();
        public string[] availableReplacementParts = Array.Empty<string>();
        public InformationSourceDefinition[] informationSources = Array.Empty<InformationSourceDefinition>();
        public string[] permittedInteractions = Array.Empty<string>();
        public string[] validationRules = Array.Empty<string>();
        public string[] stoppingConditions = Array.Empty<string>();
        public string scenePath = "";
        public float movementSamplingHz = 10f;
        public float lowActivityThresholdSeconds = 30f;
    }
}
