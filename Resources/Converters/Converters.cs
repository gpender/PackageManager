using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PackageManager.Resources.Converters
{
    public class ZeroToVisibileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val;
            if (int.TryParse(value.ToString(), out val))
            {
                if (val == 0)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class OneToVisibileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val;
            if (int.TryParse(value.ToString(), out val))
            {
                if (val == 1)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class HideIfNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    return Visibility.Visible;
                }
            }
            catch { }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShowIfNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class VisToBool : IValueConverter
    {

        private bool _inverted = false;
        private bool _not = false;

        public bool Inverted
        {
            get { return _inverted; }
            set { _inverted = value; }
        }

        public bool Not
        {
            get { return _not; }
            set { _not = value; }
        }

        private object VisibilityToBool(object value)
        {
            if (!(value is Visibility))
                return DependencyProperty.UnsetValue;
            return (((Visibility)value) == Visibility.Visible) ^ Not;
        }

        private object BoolToVisibility(object value)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            return ((bool)value ^ Not) ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Inverted ? BoolToVisibility(value)
                : VisibilityToBool(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Inverted ? VisibilityToBool(value)
                : BoolToVisibility(value);
        }
    }

}
