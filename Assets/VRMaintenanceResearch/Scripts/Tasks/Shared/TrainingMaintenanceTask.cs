namespace TMUVR.MaintenanceResearch
{
    public sealed class TrainingMaintenanceTask : MaintenanceTaskController
    {
        public void CompleteTraining()
        {
            End(TaskState.Completed, ResearchEventType.TaskCompleted, "neutral_training_completed");
        }
    }
}
