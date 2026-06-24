using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TaskManager.Core.Models;
using TaskManager.Core.Services;

namespace TaskManager.WPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly TaskService _service;
        private const double BarMaxWidth = 360.0;
        private readonly Stack<(List<TaskItem> snapshot, string label)> _history = new();
        private string _currentFileLabel = "Начальные задачи";

        public bool   CanGoBack       => _history.Count > 0;
        public string BackButtonLabel => _history.Count > 0 ? $"← {_history.Peek().label}" : "← Назад";

        public MainViewModel()
        {
            _service     = new TaskService(new JsonTaskRepository());
            Tasks        = new ObservableCollection<TaskItemViewModel>();
            OpenEditors  = new ObservableCollection<TaskEditorViewModel>();

            AddCommand          = new RelayCommand(_ => ExecuteAdd());
            EditCommand         = new RelayCommand(_ => ExecuteEdit(), _ => SelectedTask != null);
            DeleteCommand       = new RelayCommand(_ => ExecuteDelete(), _ => SelectedTask != null);
            SaveCommand         = new RelayCommand(_ => ExecuteSave());
            LoadCommand         = new RelayCommand(_ => ExecuteLoad());
            BackCommand         = new RelayCommand(_ => ExecuteBack(), _ => CanGoBack);
            ClearFilterCommand  = new RelayCommand(_ => ClearFilter());
            ShowListCommand     = new RelayCommand(_ => ShowList());
            ActivateEditorCommand = new RelayCommand(p => { if (p is TaskEditorViewModel e) { ActiveEditor = e; SelectedTab = 0; } });

            AddDemoData();
            Refresh();
        }


        public ObservableCollection<TaskItemViewModel> Tasks { get; }

        private TaskItemViewModel? _selectedTask;
        public TaskItemViewModel? SelectedTask
        {
            get => _selectedTask;
            set { Set(ref _selectedTask, value); }
        }

        public ObservableCollection<TaskEditorViewModel> OpenEditors { get; }

        private TaskEditorViewModel? _activeEditor;
        public TaskEditorViewModel? ActiveEditor
        {
            get => _activeEditor;
            set
            {
                Set(ref _activeEditor, value);
                foreach (var e in OpenEditors)
                    e.IsActive = e == value;
                OnPropertyChanged(nameof(IsListVisible));
                OnPropertyChanged(nameof(IsEditorVisible));
                OnPropertyChanged(nameof(IsTasksTabVisible));
                OnPropertyChanged(nameof(IsStatsTabVisible));
            }
        }

        public bool IsEditorVisible => ActiveEditor != null && SelectedTab == 0;
        public bool IsListVisible   => ActiveEditor == null && SelectedTab == 0;


        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                Set(ref _selectedTab, value);
                if (value != 0) ActiveEditor = null;
                OnPropertyChanged(nameof(IsTasksTabVisible));
                OnPropertyChanged(nameof(IsStatsTabVisible));
                OnPropertyChanged(nameof(IsListVisible));
                OnPropertyChanged(nameof(IsEditorVisible));
            }
        }

        public bool IsTasksTabVisible => SelectedTab == 0 && ActiveEditor == null;
        public bool IsStatsTabVisible => SelectedTab == 1;


        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set { Set(ref _searchQuery, value); Refresh(); }
        }

        private string _statusFilter = "Все статусы";
        public string StatusFilter
        {
            get => _statusFilter;
            set { Set(ref _statusFilter, value); Refresh(); }
        }

        private string _priorityFilter = "Все приоритеты";
        public string PriorityFilter
        {
            get => _priorityFilter;
            set { Set(ref _priorityFilter, value); Refresh(); }
        }

        private string _sortMode = "По умолчанию";
        public string SortMode
        {
            get => _sortMode;
            set { Set(ref _sortMode, value); Refresh(); }
        }

        public string[] StatusOptions   { get; } = { "Все статусы",    "Новая", "В процессе", "Завершена" };
        public string[] PriorityOptions { get; } = { "Все приоритеты", "Высокий", "Средний", "Низкий" };
        public string[] SortOptions     { get; } = { "По умолчанию", "По приоритету ↓", "По сроку ↑", "По названию" };


        private TaskStatistics _stats = new();
        public TaskStatistics Stats
        {
            get => _stats;
            private set
            {
                Set(ref _stats, value);
                OnPropertyChanged(nameof(CompletedPercent));
                OnPropertyChanged(nameof(OverduePercent));
                OnPropertyChanged(nameof(InProgressPercent));
                OnPropertyChanged(nameof(CompletedBarWidth));
                OnPropertyChanged(nameof(OverdueBarWidth));
                OnPropertyChanged(nameof(InProgressBarWidth));
                OnPropertyChanged(nameof(HighPriorityBarWidth));
                OnPropertyChanged(nameof(MedPriorityBarWidth));
                OnPropertyChanged(nameof(LowPriorityBarWidth));
            }
        }

        public int CompletedPercent   => Stats.Total == 0 ? 0 : (int)Math.Round(Stats.Completed  * 100.0 / Stats.Total);
        public int OverduePercent     => Stats.Total == 0 ? 0 : (int)Math.Round(Stats.Overdue    * 100.0 / Stats.Total);
        public int InProgressPercent  => Stats.Total == 0 ? 0 : (int)Math.Round(Stats.InProgress * 100.0 / Stats.Total);

        public double CompletedBarWidth  => Stats.Total == 0 ? 0 : Math.Max(4, Stats.Completed  / (double)Stats.Total * BarMaxWidth);
        public double OverdueBarWidth    => Stats.Total == 0 ? 0 : Math.Max(4, Stats.Overdue    / (double)Stats.Total * BarMaxWidth);
        public double InProgressBarWidth => Stats.Total == 0 ? 0 : Math.Max(4, Stats.InProgress / (double)Stats.Total * BarMaxWidth);

        private double PriMax => Math.Max(1, Math.Max(Stats.HighPriority, Math.Max(Stats.MedPriority, Stats.LowPriority)));
        public double HighPriorityBarWidth => Stats.HighPriority / PriMax * BarMaxWidth;
        public double MedPriorityBarWidth  => Stats.MedPriority  / PriMax * BarMaxWidth;
        public double LowPriorityBarWidth  => Stats.LowPriority  / PriMax * BarMaxWidth;


        private string _statusBarText = string.Empty;
        public string StatusBarText
        {
            get => _statusBarText;
            private set { Set(ref _statusBarText, value); }
        }


        public ICommand AddCommand          { get; }
        public ICommand EditCommand         { get; }
        public ICommand DeleteCommand       { get; }
        public ICommand SaveCommand         { get; }
        public ICommand LoadCommand         { get; }
        public ICommand BackCommand         { get; }
        public ICommand ClearFilterCommand  { get; }
        public ICommand ShowListCommand     { get; }
        public ICommand ActivateEditorCommand { get; }


        public void OpenAddEditor()
        {
            var editor = new TaskEditorViewModel(new TaskItemViewModel(), TaskEditorMode.Add);
            BindEditorCommands(editor);
            OpenEditors.Add(editor);
            ActiveEditor = editor;
            SelectedTab  = 0;
        }

        public void OpenEditEditor(TaskItemViewModel task)
        {
           
            var existing = OpenEditors.FirstOrDefault(e => e.TaskVm.Id == task.Id);
            if (existing != null)
            {
                ActiveEditor = existing;
                SelectedTab  = 0;
                return;
            }

            var clone  = new TaskItemViewModel(task.Model.Clone());
            var editor = new TaskEditorViewModel(clone, TaskEditorMode.Edit);
            BindEditorCommands(editor);
            OpenEditors.Add(editor);
            ActiveEditor = editor;
            SelectedTab  = 0;
        }

        private void BindEditorCommands(TaskEditorViewModel editor)
        {
            editor.SaveEditorCommand  = new RelayCommand(_ => SaveEditor(editor));
            editor.CloseEditorCommand = new RelayCommand(_ => CloseEditor(editor));
        }

        private void SaveEditor(TaskEditorViewModel editor)
        {
            if (!editor.Validate()) return;

            if (editor.IsNew)
                _service.AddTask(editor.TaskVm.Model);
            else
                _service.UpdateTask(editor.TaskVm.Model);

            CloseEditor(editor);
            Refresh();
        }

        private void CloseEditor(TaskEditorViewModel editor)
        {
            var idx = OpenEditors.IndexOf(editor);
            OpenEditors.Remove(editor);

            
            if (ActiveEditor == editor)
            {
                if (OpenEditors.Count > 0)
                    ActiveEditor = OpenEditors[Math.Min(idx, OpenEditors.Count - 1)];
                else
                    ActiveEditor = null;
            }
        }

        private void ShowList()
        {
            ActiveEditor = null;
            SelectedTab  = 0;
        }


        private void ExecuteAdd()
        {
            var dlg = new Views.TaskEditWindow(new TaskItemViewModel(), "Новая задача");
            if (dlg.ShowDialog() == true)
            {
                _service.AddTask(dlg.Result!.Model);
                Refresh();
            }
        }

        private void ExecuteEdit()
        {
            if (SelectedTask == null) return;
            var clone = new TaskItemViewModel(SelectedTask.Model.Clone());
            var dlg   = new Views.TaskEditWindow(clone, "Изменить задачу");
            if (dlg.ShowDialog() == true)
            {
                _service.UpdateTask(dlg.Result!.Model);
                Refresh();
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedTask == null) return;
            var res = MessageBox.Show(
                $"Удалить задачу «{SelectedTask.Title}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                var openEditor = OpenEditors.FirstOrDefault(e => e.TaskVm.Id == SelectedTask.Id);
                if (openEditor != null) CloseEditor(openEditor);

                _service.DeleteTask(SelectedTask.Id);
                Refresh();
            }
        }

        private void ExecuteSave()
        {
            var dlg = new SaveFileDialog { Filter = "JSON файлы (*.json)|*.json", FileName = "tasks" };
            if (dlg.ShowDialog() == true)
            {
                _service.SaveToFile(dlg.FileName);
                MessageBox.Show("Задачи сохранены.", "Сохранение",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ExecuteLoad()
        {
            var dlg = new OpenFileDialog { Filter = "JSON файлы (*.json)|*.json" };
            if (dlg.ShowDialog() != true) return;
        
            var snapshot = _service.GetAll().Select(t => t.Clone()).ToList();
            var label    = _currentFileLabel;
            _history.Push((snapshot, label));
            _currentFileLabel = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);

            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(BackButtonLabel));

            _service.LoadFromFile(dlg.FileName);
            OpenEditors.Clear();
            ActiveEditor = null;
            StatusBarText = $"Открыт файл: {_currentFileLabel}";
            Refresh();
        }

        private void ExecuteBack()
        {
            if (_history.Count == 0) return;
            var (snapshot, label) = _history.Pop();
            _currentFileLabel = label;
            _service.RestoreSnapshot(snapshot);
            OpenEditors.Clear();
            ActiveEditor = null;
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(BackButtonLabel));
            StatusBarText = $"Возврат: {label}";
            Refresh();
        }

        private void ClearFilter()
        {
            SearchQuery    = string.Empty;
            StatusFilter   = "Все статусы";
            PriorityFilter = "Все приоритеты";
            SortMode       = "По умолчанию";
        }

        private void Refresh()
        {
            var statusMap = new Dictionary<string, TaskStatus?>
            {
                { "Новая",      TaskStatus.New        },
                { "В процессе", TaskStatus.InProgress },
                { "Завершена",  TaskStatus.Completed  }
            };
            var priorityMap = new Dictionary<string, TaskPriority?>
            {
                { "Высокий", TaskPriority.High   },
                { "Средний", TaskPriority.Medium },
                { "Низкий",  TaskPriority.Low    }
            };

            var items = statusMap.TryGetValue(StatusFilter, out var s) && s.HasValue
                ? _service.FilterByStatus(s.Value).AsEnumerable()
                : _service.GetAll().AsEnumerable();

            if (priorityMap.TryGetValue(PriorityFilter, out var p) && p.HasValue)
                items = items.Where(t => t.Priority == p.Value);

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToLowerInvariant();
                items = items.Where(t =>
                    t.Title.ToLowerInvariant().Contains(q) ||
                    t.Description.ToLowerInvariant().Contains(q));
            }

            items = SortMode switch
            {
                "По приоритету ↓" => items.OrderByDescending(t => t.Priority),
                "По сроку ↑"      => items.OrderBy(t => t.DueDate),
                "По названию"     => items.OrderBy(t => t.Title),
                _                 => items
            };

            Tasks.Clear();
            foreach (var t in items) Tasks.Add(new TaskItemViewModel(t));

            Stats = _service.GetStatistics();
            StatusBarText = $"Показано: {Tasks.Count} из {Stats.Total} задач";
        }


        private void AddDemoData()
        {
            _service.AddTask(new TaskItem
            {
                Title = "Подготовить презентацию",
                Description = "Квартальный отчёт",
                Priority = TaskPriority.High, Status = TaskStatus.InProgress,
                IsImportant = true, DueDate = DateTime.Today.AddDays(2)
            });
            _service.AddTask(new TaskItem
            {
                Title = "Оплатить счёт за аренду офиса",
                Description = "Перевод до 20-го числа",
                Priority = TaskPriority.High, Status = TaskStatus.New,
                IsImportant = true, DueDate = DateTime.Today.AddDays(-1)
            });
            _service.AddTask(new TaskItem
            {
                Title = "Записаться к врачу",
                Description = "Плановый осмотр, взять направление",
                Priority = TaskPriority.Medium, Status = TaskStatus.New,
                DueDate = DateTime.Today.AddDays(5)
            });
            _service.AddTask(new TaskItem
            {
                Title = "Купить подарок на день рождения",
                Description = " Др Андрея — 29-го числа",
                Priority = TaskPriority.Medium, Status = TaskStatus.New,
                DueDate = DateTime.Today.AddDays(8)
            });
            _service.AddTask(new TaskItem
            {
                Title = "Прочитать книгу",
                Description = "Записать в читательский дневник",
                Priority = TaskPriority.Low, Status = TaskStatus.InProgress,
                DueDate = DateTime.Today.AddDays(14)
            });
            _service.AddTask(new TaskItem
            {
                Title = "Обновить резюме",
                Description = "Добавить новые проекты",
                Priority = TaskPriority.Low, Status = TaskStatus.Completed,
                DueDate = DateTime.Today.AddDays(-3)
            });
        }
    }
}
