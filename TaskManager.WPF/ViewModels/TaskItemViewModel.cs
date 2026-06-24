using System;
using System.Collections.Generic;
using TaskManager.Core.Models;

namespace TaskManager.WPF.ViewModels
{
    public class TaskItemViewModel : BaseViewModel
    {
        private TaskItem _model;

        public TaskItemViewModel() : this(new TaskItem()) { }
        public TaskItemViewModel(TaskItem model)
            => _model = model ?? throw new ArgumentNullException(nameof(model));

        public TaskItem Model => _model;
        public Guid     Id    => _model.Id;

        public string Title
        {
            get => _model.Title;
            set { _model.Title = value; OnPropertyChanged(); }
        }
        public string Description
        {
            get => _model.Description;
            set { _model.Description = value; OnPropertyChanged(); }
        }
        public TaskPriority Priority
        {
            get => _model.Priority;
            set { _model.Priority = value; OnPropertyChanged(); OnPropertyChanged(nameof(PriorityColor)); OnPropertyChanged(nameof(PriorityRu)); }
        }
        public TaskStatus Status
        {
            get => _model.Status;
            set { _model.Status = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusRu)); }
        }
        public DateTime DueDate
        {
            get => _model.DueDate;
            set { _model.DueDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsOverdue)); }
        }
        public bool IsImportant
        {
            get => _model.IsImportant;
            set { _model.IsImportant = value; OnPropertyChanged(); }
        }
        public bool IsOverdue => _model.IsOverdue;

        public string PriorityColor => Priority switch
        {
            TaskPriority.High   => "#C0392B",
            TaskPriority.Medium => "#A05900",
            _                   => "#1E6E3A"
        };

        public string StarGlyph => IsImportant ? "★" : "☆";

        public static IReadOnlyList<string> PriorityList { get; } =
            new[] { "Высокий", "Средний", "Низкий" };

        public static IReadOnlyList<string> StatusList { get; } =
            new[] { "Новая", "В процессе", "Завершена" };

        public string PriorityRu
        {
            get => Priority switch
            {
                TaskPriority.High   => "Высокий",
                TaskPriority.Medium => "Средний",
                _                   => "Низкий"
            };
            set => Priority = value switch
            {
                "Высокий" => TaskPriority.High,
                "Средний" => TaskPriority.Medium,
                _         => TaskPriority.Low
            };
        }

        public string StatusRu
        {
            get => Status switch
            {
                TaskStatus.New        => "Новая",
                TaskStatus.InProgress => "В процессе",
                _                     => "Завершена"
            };
            set => Status = value switch
            {
                "Новая"      => TaskStatus.New,
                "В процессе" => TaskStatus.InProgress,
                _            => TaskStatus.Completed
            };
        }
    }
}
