using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.InteractionRequests;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using System.Collections.ObjectModel;
using Microsoft.Practices.Prism.ViewModel;

namespace Tenaris.View.Exit.ViewModel.Support
{
    public class CommentsViewModel : NotificationObject, IPopupWindowActionAware
    {
        public Window HostWindow { get; set; }

        private ICommand cancel;
        private ICommand closeWindow;
        private ICommand updateBundle;
        private string bundleComment;

        public Notification HostNotification { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }

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
        public CommentsViewModel(string confirmMessage)
        {
            this.ConfirmMessage = confirmMessage;
        }

        /// <summary>
        /// Gets or sets the Bundle Text.
        /// </summary>
        public string BundleComments
        {
            get
            {
                return this.bundleComment;
            }

            set
            {
                this.bundleComment = value;
                RaisePropertyChanged("BundleComments");
            }
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

        #region Methods
        /// <summary>
        /// Close window
        /// </summary>
        public void CloseWindows(object param)
        {
            if (this.HostWindow != null)
            {
                this.HostWindow.Close();
            }
        }

        private void CloseWindow()
        {
            CloseWindows(this);
        }


        public ICommand CommandCancel
        {
            get
            {
                this.cancel = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                };

                return this.cancel;
            }
        }

        public ICommand CommandClose
        {
            get
            {
                this.closeWindow = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => CloseWindow()
                };

                return this.closeWindow;
            }
        }

        

        #endregion
    }
}
