using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaskManager.Core.Models;
using TaskManager.Core.Services;
using Xunit;

namespace TaskManager.Tests
{
    public class JsonTaskRepositoryTests
    {
        private static JsonTaskRepository CreateRepo() => new();

        private static TaskItem MakeTask(string title = "Test task",
                                         TaskStatus status = TaskStatus.New,
                                         TaskPriority priority = TaskPriority.Medium) =>
            new() { Title = title, Status = status, Priority = priority };


        [Fact]
        public void Add_ValidTask_IncreasesCount()
        {
            var repo = CreateRepo();
            repo.Add(MakeTask());
            Assert.Single(repo.GetAll());
        }

        [Fact]
        public void Add_NullTask_ThrowsArgumentNullException()
        {
            var repo = CreateRepo();
            Assert.Throws<ArgumentNullException>(() => repo.Add(null!));
        }

        [Fact]
        public void Add_EmptyTitle_ThrowsArgumentException()
        {
            var repo = CreateRepo();
            Assert.Throws<ArgumentException>(() => repo.Add(new TaskItem { Title = "  " }));
        }

        [Fact]
        public void Add_DuplicateId_ThrowsInvalidOperationException()
        {
            var repo = CreateRepo();
            var task = MakeTask();
            repo.Add(task);
            Assert.Throws<InvalidOperationException>(() => repo.Add(task));
        }

        [Fact]
        public void GetById_ExistingId_ReturnsTask()
        {
            var repo = CreateRepo();
            var task = MakeTask("Hello");
            repo.Add(task);
            var result = repo.GetById(task.Id);
            Assert.NotNull(result);
            Assert.Equal("Hello", result!.Title);
        }

        [Fact]
        public void GetById_UnknownId_ReturnsNull()
        {
            var repo = CreateRepo();
            Assert.Null(repo.GetById(Guid.NewGuid()));
        }


        [Fact]
        public void Update_ExistingTask_ChangesData()
        {
            var repo = CreateRepo();
            var task = MakeTask("Old");
            repo.Add(task);

            var updated = task.Clone();
            updated.Title = "New";
            repo.Update(updated);

            Assert.Equal("New", repo.GetById(task.Id)!.Title);
        }

        [Fact]
        public void Update_NonExistingTask_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepo();
            Assert.Throws<KeyNotFoundException>(() => repo.Update(MakeTask()));
        }

        [Fact]
        public void Delete_ExistingTask_RemovesIt()
        {
            var repo = CreateRepo();
            var task = MakeTask();
            repo.Add(task);
            repo.Delete(task.Id);
            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void Delete_UnknownId_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepo();
            Assert.Throws<KeyNotFoundException>(() => repo.Delete(Guid.NewGuid()));
        }

        [Fact]
        public void SaveAndLoad_RoundTrip_PreservesData()
        {
            var path = Path.GetTempFileName();
            try
            {
                var repo = CreateRepo();
                var task = new TaskItem
                {
                    Title       = "Persist me",
                    Description = "desc",
                    Priority    = TaskPriority.High,
                    Status      = TaskStatus.InProgress,
                    IsImportant = true
                };
                repo.Add(task);
                repo.SaveToFile(path);

                var repo2 = CreateRepo();
                repo2.LoadFromFile(path);

                var loaded = repo2.GetById(task.Id);
                Assert.NotNull(loaded);
                Assert.Equal("Persist me", loaded!.Title);
                Assert.Equal(TaskPriority.High, loaded.Priority);
                Assert.True(loaded.IsImportant);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void LoadFromFile_MissingFile_ThrowsFileNotFoundException()
        {
            var repo = CreateRepo();
            Assert.Throws<FileNotFoundException>(() => repo.LoadFromFile("no_such_file.json"));
        }

        [Fact]
        public void SaveToFile_EmptyPath_ThrowsArgumentException()
        {
            var repo = CreateRepo();
            repo.Add(MakeTask());
            Assert.Throws<ArgumentException>(() => repo.SaveToFile("   "));
        }

        [Fact]
        public void LoadFromFile_EmptyPath_ThrowsArgumentException()
        {
            var repo = CreateRepo();
            Assert.Throws<ArgumentException>(() => repo.LoadFromFile("   "));
        }
    }


    public class TaskItemTests
    {

        [Fact]
        public void Clone_ReturnsIndependentCopy()
        {
            var original = new TaskItem
            {
                Title       = "Original",
                Description = "Desc",
                Priority    = TaskPriority.High,
                Status      = TaskStatus.InProgress,
                IsImportant = true,
                DueDate     = new DateTime(2025, 12, 31)
            };

            var clone = original.Clone();

            Assert.Equal(original.Id,          clone.Id);
            Assert.Equal(original.Title,        clone.Title);
            Assert.Equal(original.Description,  clone.Description);
            Assert.Equal(original.Priority,     clone.Priority);
            Assert.Equal(original.Status,       clone.Status);
            Assert.Equal(original.IsImportant,  clone.IsImportant);
            Assert.Equal(original.DueDate,      clone.DueDate);

            original.Title  = "Changed";
            original.Status = TaskStatus.Completed;
            Assert.Equal("Original",           clone.Title);
            Assert.Equal(TaskStatus.InProgress, clone.Status);
        }

        [Fact]
        public void Clone_ReturnsDifferentObjectReference()
        {
            var original = new TaskItem { Title = "Task" };
            var clone    = original.Clone();
            Assert.False(ReferenceEquals(original, clone));
        }

        [Fact]
        public void IsOverdue_PastDueDate_NotCompleted_ReturnsTrue()
        {
            var task = new TaskItem
            {
                Title   = "Late task",
                Status  = TaskStatus.New,
                DueDate = DateTime.Today.AddDays(-1)
            };
            Assert.True(task.IsOverdue);
        }

        [Fact]
        public void IsOverdue_PastDueDate_Completed_ReturnsFalse()
        {
            var task = new TaskItem
            {
                Title   = "Done task",
                Status  = TaskStatus.Completed,
                DueDate = DateTime.Today.AddDays(-1)
            };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void IsOverdue_FutureDueDate_ReturnsFalse()
        {
            var task = new TaskItem
            {
                Title   = "Future task",
                Status  = TaskStatus.New,
                DueDate = DateTime.Today.AddDays(5)
            };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void IsOverdue_DueTodayNotCompleted_ReturnsFalse()
        {
            var task = new TaskItem
            {
                Title   = "Due today",
                Status  = TaskStatus.New,
                DueDate = DateTime.Today
            };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void IsOverdue_InProgressOverdue_ReturnsTrue()
        {
            var task = new TaskItem
            {
                Title   = "In progress late",
                Status  = TaskStatus.InProgress,
                DueDate = DateTime.Today.AddDays(-2)
            };
            Assert.True(task.IsOverdue);
        }
    }

    public class TaskServiceTests
    {
        private static (TaskService svc, JsonTaskRepository repo) Create()
        {
            var repo = new JsonTaskRepository();
            return (new TaskService(repo), repo);
        }

        private static TaskItem MakeTask(string title = "Task",
                                          TaskStatus status = TaskStatus.New,
                                          TaskPriority priority = TaskPriority.Medium,
                                          int dueDaysFromNow = 7) =>
            new() { Title = title, Status = status, Priority = priority,
                    DueDate = DateTime.Today.AddDays(dueDaysFromNow) };


        [Fact]
        public void FilterByStatus_ReturnsOnlyMatchingTasks()
        {
            var (svc, _) = Create();
            svc.AddTask(MakeTask("A", TaskStatus.New));
            svc.AddTask(MakeTask("B", TaskStatus.Completed));
            svc.AddTask(MakeTask("C", TaskStatus.New));

            var result = svc.FilterByStatus(TaskStatus.New);
            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.Equal(TaskStatus.New, t.Status));
        }


        [Fact]
        public void Search_ByTitle_ReturnMatches()
        {
            var (svc, _) = Create();
            svc.AddTask(new TaskItem { Title = "Fix bug", Description = "critical" });
            svc.AddTask(new TaskItem { Title = "Write tests", Description = "" });

            var result = svc.Search("fix");
            Assert.Single(result);
            Assert.Equal("Fix bug", result[0].Title);
        }

        [Fact]
        public void Search_ByDescription_ReturnMatches()
        {
            var (svc, _) = Create();
            svc.AddTask(new TaskItem { Title = "Task 1", Description = "contains keyword" });
            svc.AddTask(new TaskItem { Title = "Task 2", Description = "nothing" });

            Assert.Single(svc.Search("keyword"));
        }

        [Fact]
        public void Search_EmptyQuery_ReturnsAll()
        {
            var (svc, _) = Create();
            svc.AddTask(MakeTask("A"));
            svc.AddTask(MakeTask("B"));

            Assert.Equal(2, svc.Search("").Count);
        }

        [Fact]
        public void SortByPriority_Descending_HighFirst()
        {
            var (svc, _) = Create();
            svc.AddTask(MakeTask("Low",    priority: TaskPriority.Low));
            svc.AddTask(MakeTask("High",   priority: TaskPriority.High));
            svc.AddTask(MakeTask("Medium", priority: TaskPriority.Medium));

            var sorted = svc.SortByPriority(descending: true);
            Assert.Equal(TaskPriority.High,   sorted[0].Priority);
            Assert.Equal(TaskPriority.Medium, sorted[1].Priority);
            Assert.Equal(TaskPriority.Low,    sorted[2].Priority);
        }

        [Fact]
        public void SortByDueDate_Ascending_EarliestFirst()
        {
            var (svc, _) = Create();
            svc.AddTask(MakeTask("Late",  dueDaysFromNow: 10));
            svc.AddTask(MakeTask("Early", dueDaysFromNow: 1));

            var sorted = svc.SortByDueDate(ascending: true);
            Assert.Equal("Early", sorted[0].Title);
        }

        [Fact]
        public void GetStatistics_ReturnsCorrectCounts()
        {
            var (svc, _) = Create();
            svc.AddTask(MakeTask("A", TaskStatus.New));
            svc.AddTask(MakeTask("B", TaskStatus.InProgress));
            svc.AddTask(MakeTask("C", TaskStatus.Completed));
            svc.AddTask(new TaskItem
            {
                Title = "Overdue", Status = TaskStatus.New,
                DueDate = DateTime.Today.AddDays(-1)
            });
            svc.AddTask(new TaskItem { Title = "Imp", IsImportant = true });

            var s = svc.GetStatistics();
            Assert.Equal(5, s.Total);
            Assert.Equal(3, s.New);         
            Assert.Equal(1, s.InProgress);
            Assert.Equal(1, s.Completed);
            Assert.Equal(1, s.Overdue);
            Assert.Equal(1, s.Important);
        }

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TaskService(null!));
        }
    }
}
