using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.Core.Models;

namespace TaskManager.WPF.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is true ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => v is Visibility.Visible;
    }

    [ValueConversion(typeof(TaskPriority), typeof(string))]
    public class PriorityConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is TaskPriority pr ? pr switch
            {
                TaskPriority.High   => "Высокий",
                TaskPriority.Medium => "Средний",
                _                   => "Низкий"
            } : string.Empty;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(TaskStatus), typeof(string))]
    public class StatusConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is TaskStatus s ? s switch
            {
                TaskStatus.New        => "Новая",
                TaskStatus.InProgress => "В процессе",
                TaskStatus.Completed  => "Завершена",
                _                     => s.ToString()
            } : string.Empty;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class OverdueBrushConverter : IValueConverter
    {
        private static readonly SolidColorBrush Red    = new(Color.FromRgb(0xC0, 0x39, 0x2B));
        private static readonly SolidColorBrush Normal = new(Color.FromRgb(0x2C, 0x20, 0x18));
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is true ? Red : Normal;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(bool), typeof(string))]
    public class ImportantStarConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is true ? "★" : string.Empty;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(TaskPriority), typeof(Brush))]
    public class PriorityBackConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is TaskPriority pr ? pr switch
            {
                TaskPriority.High   => new SolidColorBrush(Color.FromRgb(0xFB, 0xEC, 0xE8)),
                TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xFD, 0xF2, 0xDC)),
                _                   => new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xED))
            } : Brushes.Transparent;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(TaskPriority), typeof(Brush))]
    public class PriorityForeConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is TaskPriority pr ? pr switch
            {
                TaskPriority.High   => new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
                TaskPriority.Medium => new SolidColorBrush(Color.FromRgb(0xB0, 0x70, 0x30)),
                _                   => new SolidColorBrush(Color.FromRgb(0x3A, 0x7A, 0x50))
            } : Brushes.Black;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(TaskStatus), typeof(Brush))]
    public class StatusBackConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is TaskStatus s ? s switch
            {
                TaskStatus.New        => new SolidColorBrush(Color.FromRgb(0xE8, 0xED, 0xF8)),
                TaskStatus.InProgress => new SolidColorBrush(Color.FromRgb(0xFD, 0xF2, 0xDC)),
                TaskStatus.Completed  => new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xED)),
                _                     => Brushes.Transparent
            } : Brushes.Transparent;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }

    [ValueConversion(typeof(TaskStatus), typeof(Brush))]
    public class StatusForeConverter : IValueConverter
    {
        public object Convert(object v, Type t, object p, CultureInfo c)
            => v is TaskStatus s ? s switch
            {
                TaskStatus.New        => new SolidColorBrush(Color.FromRgb(0x3D, 0x4E, 0x6B)),
                TaskStatus.InProgress => new SolidColorBrush(Color.FromRgb(0xB0, 0x70, 0x30)),
                TaskStatus.Completed  => new SolidColorBrush(Color.FromRgb(0x3A, 0x7A, 0x50)),
                _                     => Brushes.Black
            } : Brushes.Black;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    }
}
