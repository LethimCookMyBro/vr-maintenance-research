using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMUVR.MaintenanceResearch
{
    [DisallowMultipleComponent]
    public sealed class ResearchSessionManager : MonoBehaviour
    {
        public static ResearchSessionManager Instance { get; private set; }

        [SerializeField] ResearchSessionConfig configuration = new ResearchSessionConfig();
        [SerializeField] string researcherSetupScene = "ResearcherSetup";
        [SerializeField] string trainingScene = "VRTraining";
        [SerializeField] string computerScene = "ComputerRepairTask";
        [SerializeField] string fanScene = "FanRepairTask";
        ResearchLogService logger;
        int completedTasks;

        public ResearchSessionConfig Configuration => configuration;
        public ResearchLogService Logger => logger;
        public bool IsConfigured { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            logger = GetComponent<ResearchLogService>() ?? gameObject.AddComponent<ResearchLogService>();
        }

        public bool Configure(ResearchSessionConfig value, out string error)
        {
            error = string.Empty;
            if (value == null || !value.Validate(out error))
                return false;

            configuration = value;
            IsConfigured = true;
            return true;
        }

        public bool StartSession(out string error)
        {
            error = string.Empty;
            if (!IsConfigured && !configuration.Validate(out error))
                return false;

            IsConfigured = true;
            return logger.BeginSession(configuration, out error);
        }

        public void StartConfiguredFlow()
        {
            if (!logger.IsStarted && !StartSession(out _))
                return;

            SceneManager.LoadScene(configuration.trainingRequired ? trainingScene : FirstTaskScene());
        }

        public void StartFirstTaskAfterTraining() => SceneManager.LoadScene(FirstTaskScene());

        public void SkipTrainingWhenPermitted()
        {
            if (configuration.developmentMode)
                SceneManager.LoadScene(FirstTaskScene());
        }

        public void CompleteCurrentTaskAndAdvance()
        {
            completedTasks++;
            if (completedTasks < 2)
            {
                SceneManager.LoadScene(NextTaskScene());
                return;
            }

            logger.EndSession("Completed");
            SceneManager.LoadScene(researcherSetupScene);
        }

        string FirstTaskScene() => configuration.taskOrder == TaskOrder.ComputerThenFan ? computerScene : fanScene;
        string NextTaskScene() => configuration.taskOrder == TaskOrder.ComputerThenFan ? fanScene : computerScene;
    }
}
