namespace DentalHealthTracker.Models
{
    public enum GoalStatusType
    {
        NotStarted,
        InProgress,
        Completed,
        Cancelled
    }

    public class GoalStatus
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GoalId { get; set; }
        public Goal? Goal { get; set; }
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public string? ImagePath { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
} 