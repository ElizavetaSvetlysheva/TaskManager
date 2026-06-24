namespace TaskManager.Core.Models
{
    public class TaskStatistics
    {
        public int Total       { get; set; }
        public int New         { get; set; }
        public int InProgress  { get; set; }
        public int Completed   { get; set; }
        public int Overdue     { get; set; }
        public int Important   { get; set; }
        public int HighPriority { get; set; }
        public int MedPriority  { get; set; }
        public int LowPriority  { get; set; }
    }
}
