using System;
using System.Collections.Generic;
using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces
{
    public interface ITaskService
    {
        IReadOnlyList<TaskItem> GetAll();
        IReadOnlyList<TaskItem> FilterByStatus(TaskStatus status);
        IReadOnlyList<TaskItem> Search(string query);
        IReadOnlyList<TaskItem> SortByPriority(bool descending = true);
        IReadOnlyList<TaskItem> SortByDueDate(bool ascending = true);
        TaskStatistics GetStatistics();

        void AddTask(TaskItem task);
        void UpdateTask(TaskItem task);
        void DeleteTask(Guid id);

        void SaveToFile(string path);
        void LoadFromFile(string path);
        void RestoreSnapshot(IEnumerable<TaskItem> snapshot);
    }
}
