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

        /// <summary>
        /// Whether Unity's OnMouseEnter/OnMouseDown path may raise research events.
        ///
        /// The deployment is a PC running Quest Link, so the game view sits on the
        /// researcher's monitor while the participant works. Those handlers fire on a
        /// plain mouse click over that view, and a researcher reaching for the window
        /// would write a hover or a grab into the participant's event stream under
        /// interactor=mouse. It stays available in development mode, where it is how the
        /// scenes are driven without a headset, and is off in a participant session.
        ///
        /// True when no session exists yet, so editor tooling still works.
        /// </summary>
        public static bool MousePathPermitted =>
            Instance == null || Instance.configuration == null || Instance.configuration.developmentMode;

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
            if (!logger.BeginSession(configuration, out error))
                return false;

            completedTasks = 0;
            return true;
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

        /// <summary>True once both tasks are done. The scene deliberately does not change.</summary>
        public bool SessionComplete { get; private set; }

        public void CompleteCurrentTaskAndAdvance()
        {
            completedTasks++;
            if (completedTasks < 2)
            {
                SceneManager.LoadScene(NextTaskScene());
                return;
            }

            // Stay in the finished task scene. ResearcherSetup has no XR Origin and its
            // Setup Camera carries no TrackedPoseDriver, so loading it here put a
            // participant who was still wearing the headset in front of a view that does
            // not follow their head - and it would show them the configuration screen,
            // participant code included. The researcher returns to setup from the desktop
            // panel once the headset is off.
            SessionComplete = true;
            logger.EndSession("Completed");
        }

        /// <summary>Researcher-only, from the desktop panel, once the headset is off.</summary>
        public void ReturnToSetup()
        {
            SessionComplete = false;
            completedTasks = 0;
            SceneManager.LoadScene(researcherSetupScene);
        }

        string FirstTaskScene() => configuration.taskOrder == TaskOrder.ComputerThenFan ? computerScene : fanScene;
        string NextTaskScene() => configuration.taskOrder == TaskOrder.ComputerThenFan ? fanScene : computerScene;
    }
}
