using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Diagnostics;



namespace Tenaris.View.Exit.ViewModel
{
    /// <summary>
    /// Base Class For ViewModel
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Implementation of INotifyPropertyChanged

        /// <summary>
        /// Event Raised When A Property Is Changed 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Verifies that a property name exists in this ViewModel. This method
        /// can be called before the property is used, for instance before
        /// calling OnPropertyChanged. It avoids errors when a property name
        /// is changed but some places are missed.
        /// <para>This method is only active in DEBUG mode.</para>
        /// </summary>
        /// <param name="propertyName"></param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            var type = GetType();
            if (type.GetProperty(propertyName) == null)
            {
                throw new ArgumentException(propertyName);
            }
        }


        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        #endregion
    }
}
