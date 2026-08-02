using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>Keeps the XRI simulator HUD out of participant view; F10 reveals it only in development sessions.</summary>
    [DefaultExecutionOrder(-10000)]
    [RequireComponent(typeof(XRInteractionSimulator))]
    public sealed class SimulatorHudGate : MonoBehaviour
    {
        [SerializeField] GameObject simulatorUiPrefab;
        GameObject simulatorUi;

        void Awake()
        {
            GetComponent<XRInteractionSimulator>().interactionSimulatorUI = null;
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
                Toggle();
        }

        void Toggle()
        {
            var session = ResearchSessionManager.Instance;
            if (session != null && !session.Configuration.developmentMode)
                return;

            if (simulatorUi == null && simulatorUiPrefab != null)
                simulatorUi = Instantiate(simulatorUiPrefab, transform);
            else if (simulatorUi != null)
                simulatorUi.SetActive(!simulatorUi.activeSelf);
        }
    }
}
