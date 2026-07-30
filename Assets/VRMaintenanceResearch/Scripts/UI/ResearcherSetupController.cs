using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public sealed class ResearcherSetupController : MonoBehaviour
    {
        ResearchSessionManager session;
        ResearchSessionConfig config;
        string status = "Enter a pseudonymous Participant Code. Do not enter names, phone numbers, email addresses, or student IDs.";

        void Start()
        {
            session = ResearchSessionManager.Instance;
            if (session == null)
            {
                var sessionObject = new GameObject("Research Session Manager");
                session = sessionObject.AddComponent<ResearchSessionManager>();
            }
            config = session.Configuration;
        }

        void OnGUI()
        {
            if (config == null)
                return;

            GUILayout.BeginArea(new Rect(24, 24, 560, 700), GUI.skin.box);
            GUILayout.Label("VR Maintenance Research — Researcher Setup");
            GUILayout.Label("Participant Code (required)");
            config.participantCode = GUILayout.TextField(config.participantCode, 64);
            GUILayout.Label("Session ID (blank generates automatically)");
            config.sessionId = GUILayout.TextField(config.sessionId, 64);
            GUILayout.Label("Experiment Site");
            config.experimentSite = GUILayout.TextField(config.experimentSite, 64);
            GUILayout.Label("Researcher Code");
            config.researcherCode = GUILayout.TextField(config.researcherCode, 64);
            config.participantGroup = (ParticipantGroup)GUILayout.SelectionGrid((int)config.participantGroup, new[] { "Thai group", "Japanese group" }, 2);
            config.language = (ResearchLanguage)GUILayout.SelectionGrid((int)config.language, new[] { "Thai", "Japanese", "English" }, 3);
            config.taskOrder = (TaskOrder)GUILayout.SelectionGrid((int)config.taskOrder, new[] { "Computer → Fan", "Fan → Computer" }, 2);
            config.trainingRequired = GUILayout.Toggle(config.trainingRequired, "Training required");
            config.developmentMode = GUILayout.Toggle(config.developmentMode, "DEVELOPMENT_TEST mode (separate output folder)");
            config.simulatorMode = GUILayout.Toggle(config.simulatorMode, "XR simulator mode");
            config.firstPersonRecordingConsent = GUILayout.Toggle(config.firstPersonRecordingConsent, "First-person recording consent recorded");
            config.firstPersonRecordingEnabled = GUILayout.Toggle(config.firstPersonRecordingConsent && config.firstPersonRecordingEnabled, "Enable first-person recording (capture pipeline is not implemented)");
            GUILayout.Label("Technical notes (no personal information)");
            config.technicalNotes = GUILayout.TextArea(config.technicalNotes, 300);
            GUILayout.Label("Status: " + status);
            if (GUILayout.Button("Start Session"))
            {
                if (!session.Configure(config, out var error) || !session.StartSession(out error))
                    status = error;
                else
                {
                    status = "Session created at: " + session.Logger.CurrentDataFolder;
                    session.StartConfiguredFlow();
                }
            }
            GUILayout.EndArea();
        }
    }
}
