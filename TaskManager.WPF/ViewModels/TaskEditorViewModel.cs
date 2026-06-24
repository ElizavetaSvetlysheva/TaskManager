using System;
using System.Windows.Input;
using TaskManager.Core.Models;

namespace TaskManager.WPF.ViewModels
{
    public enum TaskEditorMode { Add, Edit }

    public class TaskEditorViewModel : BaseViewModel
    {
        private readonly TaskItemViewModel _vm;
        public TaskItemViewModel TaskVm => _vm;

        public TaskEditorMode Mode { get; }
        public bool IsNew => Mode == TaskEditorMode.Add;

        private string _tabTitle;
        public string TabTitle
        {
            get => _tabTitle;
            private set { Set(ref _tabTitle, value); }
        }

        public TaskEditorViewModel(TaskItemViewModel vm, TaskEditorMode mode)
        {
            _vm  = vm ?? throw new ArgumentNullException(nameof(vm));
            Mode = mode;
            _tabTitle = mode == TaskEditorMode.Add ? "Новая задача" : TruncateTitle(vm.Title);
            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TaskItemViewModel.Title))
                    TabTitle = mode == TaskEditorMode.Add
                        ? "Новая задача"
                        : TruncateTitle(_vm.Title);
            };
        }

        private static string TruncateTitle(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return "Без названия";
            return t.Length > 22 ? t.Substring(0, 20) + "…" : t;
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set { Set(ref _isActive, value); }
        }

        public ICommand? SaveEditorCommand  { get; set; }
        public ICommand? CloseEditorCommand { get; set; }

        public bool HasError => string.IsNullOrWhiteSpace(_vm.Title);

        private string _errorText = string.Empty;
        public string ErrorText
        {
            get => _errorText;
            set { Set(ref _errorText, value); OnPropertyChanged(nameof(HasVisibleError)); }
        }
        public bool HasVisibleError => !string.IsNullOrEmpty(_errorText);

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(_vm.Title))
            {
                ErrorText = "Введите название задачи";
                return false;
            }
            ErrorText = string.Empty;
            return true;
        }
    }
}
