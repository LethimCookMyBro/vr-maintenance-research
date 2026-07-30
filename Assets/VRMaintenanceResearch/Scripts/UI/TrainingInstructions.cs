using UnityEngine;

namespace TMUVR.MaintenanceResearch
{
    public sealed class TrainingInstructions : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, 620, 170), GUI.skin.box);
            GUILayout.Label("Neutral XR Training");
            GUILayout.Label("Use the official XR Interaction Simulator to move the headset/controllers, grab objects, place an object in the socket, interact with the neutral information source, and press Reset if needed.");
            GUILayout.Label("This scene contains no Computer or Fan maintenance solution.");
            GUILayout.EndArea();
        }
    }
}
