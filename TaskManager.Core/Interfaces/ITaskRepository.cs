using System;
using System.Collections.Generic;
using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces
{
    public interface ITaskRepository
    {
        IReadOnlyList<TaskItem> GetAll();
        TaskItem? GetById(Guid id);
        void Add(TaskItem task);
        void Update(TaskItem task);
        void Delete(Guid id);
        void SaveToFile(string path);
        void LoadFromFile(string path);
    }
}
