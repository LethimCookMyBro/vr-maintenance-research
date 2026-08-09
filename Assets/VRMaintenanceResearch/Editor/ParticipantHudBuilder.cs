using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TMUVR.MaintenanceResearch.EditorTools
{
    /// <summary>
    /// Puts <see cref="ParticipantHud"/> on the task controller of the two
    /// maintenance scenes.
    ///
    /// Only those two. The display reads DeviceTested / RepairPerformed / Completed,
    /// which are the milestones of a diagnostic repair; VRTraining measures four
    /// unrelated skills and already has its own board, and ResearcherSetup has no
    /// participant in it.
    ///
    /// The component builds nothing until play mode and switches itself off entirely
    /// when all three of the session flags are clear, so having it in the scene is not
    /// the same as having the display on.
    /// </summary>
    public static class ParticipantHudBuilder
    {
        [MenuItem("Tools/VR Maintenance Research/Visual Audit/Attach Participant HUD", priority = 7)]
        public static void AttachAll()
        {
            foreach (var scenePath in new[] { ResearchSceneSet.Computer, ResearchSceneSet.Fan })
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var controller = Object.FindFirstObjectByType<MaintenanceTaskController>();
                if (controller == null)
                {
                    Debug.LogWarning($"[ParticipantHud] no MaintenanceTaskController in {scene.name}");
                    continue;
                }

                if (controller.GetComponent<ParticipantHud>() == null)
                {
                    Undo.AddComponent<ParticipantHud>(controller.gameObject);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[ParticipantHud] attached in {scene.name}");
                }
            }
        }
    }
}
