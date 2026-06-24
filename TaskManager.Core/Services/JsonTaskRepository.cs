using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.Core.Services
{
    public class JsonTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = new();

        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public IReadOnlyList<TaskItem> GetAll() => _tasks.AsReadOnly();

        public TaskItem? GetById(Guid id) =>
            _tasks.FirstOrDefault(t => t.Id == id);

        public void Add(TaskItem task)
        {
            if (task is null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Название задачи не может быть пустым.", nameof(task));
            if (_tasks.Any(t => t.Id == task.Id))
                throw new InvalidOperationException($"Задача с Id {task.Id} уже существует.");
            _tasks.Add(task);
        }

        public void Update(TaskItem task)
        {
            if (task is null) throw new ArgumentNullException(nameof(task));
            var index = _tasks.FindIndex(t => t.Id == task.Id);
            if (index < 0)
                throw new KeyNotFoundException($"Задача с Id {task.Id} не найдена.");
            _tasks[index] = task;
        }

        public void Delete(Guid id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Задача с Id {id} не найдена.");
            _tasks.Remove(task);
        }

        public void ReplaceAll(IEnumerable<TaskItem> items)
        {
            _tasks.Clear();
            _tasks.AddRange(items);
        }

        public void SaveToFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));
            var json = JsonSerializer.Serialize(_tasks, _options);
            File.WriteAllText(path, json);
        }

        public void LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден.", path);
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<List<TaskItem>>(json, _options)
                         ?? new List<TaskItem>();
            _tasks.Clear();
            _tasks.AddRange(loaded);
        }
    }
}
