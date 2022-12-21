// -----------------------------------------------------------------------
// <copyright file="CommandVM.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CommandVM : ICommand
    {
        public Predicate<object> CanExecuteDelegate;
        public Action<object> ExecuteDelegate;

        public bool CanExecute(object parameter)
        {
            return CanExecuteDelegate(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            ExecuteDelegate(parameter);
        }
    }
}
