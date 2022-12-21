// -----------------------------------------------------------------------
// <copyright file="ModalDialogActionViewModel.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Infrastructure.InteractionRequests;
    //using Tenaris.Tamsa.HRM.Fat2.View.Entrance.Library;
    using System.Windows;
    using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
    using System.Windows.Input;
    using Tenaris.View.Exit.Model;
    using Tenaris.Library.Log;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ModalDialogActionViewModel : ViewModelBase, IPopupWindowActionAware
    {

        public Window HostWindow { get; set; }
        public string ConfirmMessage { get; set; }
        public string ImagePath { get; set; }
        public string HeaderText { get; set; }        
        public ModalType WindowModalType { get; set; }

        private ICommand closeWindow;

        /// <summary>
        /// Host Notification
        /// </summary>
        public Notification HostNotification { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }

        public ModalDialogActionViewModel(string Message, string Title, ModalType modalType)
        {

            this.WindowModalType = modalType;
            OnPropertyChanged("WindowModalType");

            this.ConfirmMessage = Message;
            OnPropertyChanged("ConfirmMessage");

            this.HeaderText = Title;
            OnPropertyChanged("HeaderText");
        }

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
    }
    
}
