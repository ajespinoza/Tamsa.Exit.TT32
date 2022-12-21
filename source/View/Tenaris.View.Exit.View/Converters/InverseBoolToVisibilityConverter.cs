// -----------------------------------------------------------------------
// <copyright file="InverseBoolToVisibilityConverter.cs" company="">
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

    /// <summary>
    /// OppositeVisibility Converter class.
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            return !System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
