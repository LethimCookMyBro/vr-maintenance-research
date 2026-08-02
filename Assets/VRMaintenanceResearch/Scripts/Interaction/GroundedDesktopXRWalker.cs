using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace TMUVR.MaintenanceResearch
{
    /// <summary>
    /// Keeps the editor/desktop simulator on the existing CharacterController gravity path.
    /// WASD and thumbstick movement remain available; Q/E vertical simulation is disabled.
    /// </summary>
    public sealed class GroundedDesktopXRWalker : MonoBehaviour
    {
        XRInteractionSimulator simulator;

        void OnEnable()
        {
            simulator = FindFirstObjectByType<XRInteractionSimulator>();
            DisableVerticalSimulation();
        }

        void Update() => DisableVerticalSimulation();

        void DisableVerticalSimulation()
        {
            if (simulator == null)
                return;
            if (simulator.translateYInput.inputSourceMode != XRInputValueReader.InputSourceMode.Unused)
            {
                simulator.translateYInput.inputSourceMode = XRInputValueReader.InputSourceMode.Unused;
                Debug.Log("[VRMaintenanceResearch] Grounded desktop locomotion enabled: simulator vertical input disabled; gravity path retained.", this);
            }
        }
    }
}
