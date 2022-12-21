// -----------------------------------------------------------------------
// <copyright file="BundleEditViewModel.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
using Tamsa.Manager.Exit.Shared;

namespace Tenaris.View.Exit.ViewModel.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Practices.Prism.ViewModel;
    using Infrastructure.InteractionRequests;
    using System.Windows;
    using System.Windows.Input;
    using Tenaris.View.Exit.Model;
    using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
  

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class BundleEditViewModel : NotificationObject, IPopupWindowActionAware
    {
        public Window HostWindow { get; set; }
        private ICommand closeWindow;
        private ICommand updateBundle;
        private ICommand CmdSelectedRejectionCode;
        private ICommand CmdSelectedStatus;
        private ICommand CmdSelectedLocation;
        

        
        public List<RejectionCause> RejectionCauses { get; set; }
        public List<ItemStatus> Statuses { get; set; }
        public List<ILocation> Locations { get; set; }
            

        public int OrderNumber { get; set; }
        public int HeatNumber { get; set; }
        public string EE { get; set; }
        public int TotalPieces { get; set; }
        public double TotalWeight { get; set; }
        public ItemStatus SelectedStatus { get; set; }
        public int IdLocation { get; set; }
        public bool EditLocation { get; set; }
        public bool EditRejectionCode { get; set; }


        private IBundle CurrentBundle;
        public string RejectionCode { get; set; }

        public bool UpdBundle { get; set; }
  



        /// <summary>
        /// Host Notification
        /// </summary>
        public Notification HostNotification { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }


        #region Constructor
        public BundleEditViewModel(IBundle bundle)
        {
            UpdBundle = false;
            this.CurrentBundle = bundle;
            object value;
            this.EditLocation = (bundle.ItemStatus == ItemStatus.Good) ? false : true;
            this.EditRejectionCode = (bundle.ItemStatus == ItemStatus.Good) ? false : true;

            RejectionCauses = ExitModel.Instance.Data.GetRejectionCauses(bundle.ItemStatus).ToList();
            Locations = ExitModel.Instance.Data.GetLocations().ToList();
            Statuses = Locations.Select(l => l.Status).Distinct().ToList();
            Locations = Locations.Where(l => l.Status != ItemStatus.Good).ToList();

            if (EditRejectionCode)
            {
                this.RejectionCode = bundle.RejectionCause;                
            }
            
            RaisePropertyChanged(() => this.RejectionCauses);
            RaisePropertyChanged(() => this.Locations);
            RaisePropertyChanged(() => this.Statuses);

            bundle.ExtraData.TryGetValue("OrderNumber", out value);
            if (value != null)
                OrderNumber = Convert.ToInt32(value);
            bundle.ExtraData.TryGetValue("HeatNumber", out value);
            if (value != null)
                HeatNumber = Convert.ToInt32(value);
            //FER ver EE en Vista
            bundle.ExtraData.TryGetValue("EE", out value);
            if (value != null)
                EE = value.ToString();
            TotalPieces = bundle.Items.Count();
            TotalWeight = Math.Round(bundle.Weight,2);
            SelectedStatus = bundle.ItemStatus;
            IdLocation = bundle.Location.Id;

            RaisePropertyChanged(() => this.OrderNumber);
            RaisePropertyChanged(() => this.HeatNumber);
            RaisePropertyChanged(() => this.TotalPieces);
            RaisePropertyChanged(() => this.TotalWeight);
            
            
        }

        #endregion


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

        public ICommand CommandCancel
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

        public ICommand CommandUpdBundle
        {
            get
            {
                this.updateBundle = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => EditBundle()

                };

                return this.updateBundle;
            }
        }

        public ICommand CommandSelectRejectedCode
        {
            get
            {
                this.CmdSelectedRejectionCode = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectedRejectionCode()
                };

                return this.CmdSelectedRejectionCode;
            }
        }

        public ICommand CommandSelectStatus
        {
            get
            {
                this.CmdSelectedStatus = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectStatus()
                };

                return this.CmdSelectedStatus;
            }
        }
        #endregion


        private void SelectedRejectionCode()
        {
             RaisePropertyChanged(() => this.RejectionCode);
        }

        private void SelectStatus()
        {
            if (SelectedStatus == ItemStatus.Good)
            {
                EditLocation = false;
                EditRejectionCode = false;
                RejectionCode = "";
                IdLocation = ExitModel.Instance.Data.GetLocations().ToList().FirstOrDefault(l => l.Status == ItemStatus.Good).Id;
            }
            else
            {
                EditLocation = true;
                EditRejectionCode = true;
            }
            RaisePropertyChanged(() => this.RejectionCode);
            RaisePropertyChanged(() => this.IdLocation);
            RaisePropertyChanged(() => this.EditLocation);
            RaisePropertyChanged(() => this.EditRejectionCode);
            RaisePropertyChanged(() => this.SelectedStatus);
        }



        #region Private Methods

        private void EditBundle()
        {            
            if (this.CurrentBundle != null)
            {
                if (this.CurrentBundle.ItemStatus != SelectedStatus || this.CurrentBundle.Location.Id != IdLocation 
                    ||  this.CurrentBundle.RejectionCause != RejectionCode)                    
                {
                    if (ExitModel.Instance.Data.UpdRejectBundle(CurrentBundle.IdBundle, RejectionCode, IdLocation))
                    {
                        int idCradle = CurrentBundle.IdCradle;
                        List<IBundle> currentBundles;
                        currentBundles = ExitModel.Instance.GetBundlesbyCradle(idCradle);
                        currentBundles.Remove(CurrentBundle);                        
                        CurrentBundle.Location = ExitModel.Instance.Data.GetLocation(IdLocation);
                        CurrentBundle.RejectionCause = RejectionCode;
                        CurrentBundle.ItemStatus = SelectedStatus;
                        currentBundles.Add(CurrentBundle);
                        UpdBundle = true;
                    }
                }
               
            }
           CloseWindows(this);        
        }

        #endregion

    }
}
