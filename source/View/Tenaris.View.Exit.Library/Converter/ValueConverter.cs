// -----------------------------------------------------------------------
// <copyright file="ValueConverter.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;
    using System.Windows.Media;
    using Tenaris.Library.Log;
    using System.Globalization;
    using System.Windows;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        private SolidColorBrush ColorBrush = null;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {


            if (value == null)
                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (value is Color)
                return new SolidColorBrush((Color)value);

            if (value is string)
            {
                ColorBrush = new SolidColorBrush();
                var color = (Color)ColorConverter.ConvertFromString((string)value);
                ColorBrush.Color = color;
                return new SolidColorBrush(color);
            }

            ColorBrush = null;
            Trace.Error("ColorToBurshConverter only supports converting from Color and String");
            return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Color Parse(string color)
        {
            var offset = color.StartsWith("#") ? 1 : 0;

            var a = Byte.Parse(color.Substring(0 + offset, 2), NumberStyles.HexNumber);
            var r = Byte.Parse(color.Substring(2 + offset, 2), NumberStyles.HexNumber);
            var g = Byte.Parse(color.Substring(4 + offset, 2), NumberStyles.HexNumber);
            var b = Byte.Parse(color.Substring(6 + offset, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var isVisible = (bool)value;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;

            return visibility == Visibility.Visible;
        }
    }
    

}
