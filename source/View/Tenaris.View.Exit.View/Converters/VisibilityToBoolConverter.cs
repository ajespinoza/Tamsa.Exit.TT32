// -----------------------------------------------------------------------
// <copyright file="VisibilityToBoolConverter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.View.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;
    using System.Globalization;
    using System.Windows;


    public class VisibilityToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts <see cref="System.Windows.Visibility.Visible"/> to true and <see cref="System.Windows.Visibility.Collapsed"/> to false.
        /// </summary>
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Visibility) {
                return (Visibility)value == Visibility.Visible;
            }
            return value;
        }

        /// <summary>
        /// Converts true to <see cref="System.Windows.Visibility.Visible"/> and false to <see cref="System.Windows.Visibility.Collapsed"/>.
        /// </summary>
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is bool) {
                var boolValue = (bool)value;
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return value;
        }
    }
}
