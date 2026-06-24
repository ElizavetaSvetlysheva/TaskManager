using System;
using System.Text.Json.Serialization;

namespace TaskManager.Core.Models
{
    public enum TaskStatus  { New, InProgress, Completed }
    public enum TaskPriority { Low, Medium, High }

    public class TaskItem
    {
        public Guid         Id          { get; set; } = Guid.NewGuid();
        public string       Title       { get; set; } = string.Empty;
        public string       Description { get; set; } = string.Empty;
        public TaskPriority Priority    { get; set; } = TaskPriority.Medium;
        public DateTime     DueDate     { get; set; } = DateTime.Today.AddDays(7);
        public TaskStatus   Status      { get; set; } = TaskStatus.New;
        public bool         IsImportant { get; set; } = false;
        public DateTime     CreatedAt   { get; set; } = DateTime.Now;

        [JsonIgnore]
        public bool IsOverdue =>
            Status != TaskStatus.Completed && DueDate.Date < DateTime.Today;

        public TaskItem Clone() => new()
        {
            Id = Id, Title = Title, Description = Description,
            Priority = Priority, DueDate = DueDate,
            Status = Status, IsImportant = IsImportant, CreatedAt = CreatedAt
        };
    }
}
