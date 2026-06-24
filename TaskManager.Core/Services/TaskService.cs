using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    public class TaskService : ITaskService
    {
        private readonly JsonTaskRepository _repo;

        public TaskService(JsonTaskRepository repository)
        {
            _repo = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public IReadOnlyList<TaskItem> GetAll() => _repo.GetAll();

        public IReadOnlyList<TaskItem> FilterByStatus(TaskStatus status) =>
            _repo.GetAll().Where(t => t.Status == status).ToList();

        public IReadOnlyList<TaskItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return _repo.GetAll();
            var q = query.Trim().ToLowerInvariant();
            return _repo.GetAll()
                        .Where(t => t.Title.ToLowerInvariant().Contains(q)
                                 || t.Description.ToLowerInvariant().Contains(q))
                        .ToList();
        }

        public IReadOnlyList<TaskItem> SortByPriority(bool descending = true)
        {
            var q = _repo.GetAll().AsEnumerable();
            return (descending ? q.OrderByDescending(t => t.Priority)
                               : q.OrderBy(t => t.Priority)).ToList();
        }

        public IReadOnlyList<TaskItem> SortByDueDate(bool ascending = true)
        {
            var q = _repo.GetAll().AsEnumerable();
            return (ascending ? q.OrderBy(t => t.DueDate)
                              : q.OrderByDescending(t => t.DueDate)).ToList();
        }

        public TaskStatistics GetStatistics()
        {
            var all = _repo.GetAll();
            return new TaskStatistics
            {
                Total        = all.Count,
                New          = all.Count(t => t.Status == TaskStatus.New),
                InProgress   = all.Count(t => t.Status == TaskStatus.InProgress),
                Completed    = all.Count(t => t.Status == TaskStatus.Completed),
                Overdue      = all.Count(t => t.IsOverdue),
                Important    = all.Count(t => t.IsImportant),
                HighPriority = all.Count(t => t.Priority == TaskPriority.High),
                MedPriority  = all.Count(t => t.Priority == TaskPriority.Medium),
                LowPriority  = all.Count(t => t.Priority == TaskPriority.Low),
            };
        }

        public void AddTask(TaskItem task)    => _repo.Add(task);
        public void UpdateTask(TaskItem task) => _repo.Update(task);
        public void DeleteTask(Guid id)       => _repo.Delete(id);
        public void SaveToFile(string path)   => _repo.SaveToFile(path);
        public void LoadFromFile(string path) => _repo.LoadFromFile(path);

        public void RestoreSnapshot(IEnumerable<TaskItem> snapshot) =>
            _repo.ReplaceAll(snapshot);
    }
}
