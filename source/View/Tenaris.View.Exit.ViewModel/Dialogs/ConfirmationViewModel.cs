// -----------------------------------------------------------------------
// <copyright file="ConfirmationViewModel.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Practices.Prism.ViewModel;



    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ConfirmationViewModel : NotificationObject
    {
        /// <summary>
        /// confirm Message
        /// </summary>
        private string confirmMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationViewModel"/> class.
        /// </summary>
        /// <param name="confirmMessage">
        /// The label text.
        /// </param>
        public ConfirmationViewModel(string confirmMessage)
        {
            this.ConfirmMessage = confirmMessage;
        }

        /// <summary>
        /// Gets or sets LabelText.
        /// </summary>
        public string ConfirmMessage
        {
            get
            {
                return this.confirmMessage;
            }

            set
            {
                this.confirmMessage = value;
                RaisePropertyChanged("ConfirmMessage");
            }
        }
    }
}
