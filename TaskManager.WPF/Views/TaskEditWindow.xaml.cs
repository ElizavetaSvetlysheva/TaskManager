using System.Windows;
using TaskManager.WPF.ViewModels;

namespace TaskManager.WPF.Views
{
    public partial class TaskEditWindow : Window
    {
        public TaskItemViewModel? Result { get; private set; }

        public TaskEditWindow(TaskItemViewModel vm, string windowTitle = "Новая задача")
        {
            InitializeComponent();
            DataContext = vm;
            Title       = windowTitle;
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var vm = (TaskItemViewModel)DataContext;
            if (string.IsNullOrWhiteSpace(vm.Title))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                TitleBox.Focus();
                return;
            }
            Result       = vm;
            DialogResult = true;
        }

        private void CancelClick(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
