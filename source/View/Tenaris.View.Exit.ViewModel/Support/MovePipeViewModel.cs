// -----------------------------------------------------------------------
// <copyright file="MovePipeViewModel.cs" company="Techint">
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
    using Infrastructure.InteractionRequests;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
    using System.Collections.ObjectModel;
    using Microsoft.Practices.Prism.ViewModel;
    using Tenaris.View.Exit.Model;
    using Tenaris.Library.Log;
    using Tenaris.Tamsa.HRM.Fat2.Library.LineupColorRules;
    
    

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class MovePipeViewModel :NotificationObject, IPopupWindowActionAware
    {
        public Window HostWindow { get; set; }

        private ICommand closeWindow;
        private ICommand saveHeat;
        private ICommand selectedBundle;


        public List<IBundle> Bundle { get; set; }
        public List<IItem> MovePipe { get; set; }
        public int SelectedPipes { get; set; }
        public double PipesWeight { get; set; }

        public int FromPipes { get; set; }
        public double FromPipesWeight { get; set; }

        public int TotalPipes { get; set; }
        public double TotalWeight { get; set; }

        public int IdBundle { get; set; }

        public bool UpdBundle { get; set; }
        private bool IsCurrent = true;

        /// <summary>
        /// Host Notification
        /// </summary>
        public Notification HostNotification { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }

        #region Constructor

        public MovePipeViewModel(List<IItem> MovePipe, List<IBundle> Bundle, bool IsCurrent)
        {
            try
            {

                this.IsCurrent = IsCurrent;

                int idBundle = MovePipe.FirstOrDefault().IdBundle;
                this.Bundle = Bundle.Where(b => b.IdBundle != idBundle).ToList();
                this.MovePipe = MovePipe;
                this.SelectedPipes = MovePipe.Count();
                this.PipesWeight = MovePipe.Sum(x => x.Weight);

                RaisePropertyChanged(() => this.Bundle);
                RaisePropertyChanged(() => this.MovePipe);
                RaisePropertyChanged(() => SelectedPipes);
                RaisePropertyChanged(() => PipesWeight);
                UpdBundle = false;
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message + " -> MovePipeViewModel");
            }
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

        public ICommand CommandSave
        {
            get
            {
                this.saveHeat = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => MovePipes()
                };

                return this.saveHeat;
            }
        }

        public ICommand CommandSelectedBundle
        {
            get
            {
                this.selectedBundle = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectBundle()
                };

                return this.selectedBundle;
            }
        }
        #endregion


        private void SelectBundle()
        {
            FromPipes= Bundle.FirstOrDefault(x=>x.IdBundle==IdBundle).Items.Count();
            FromPipesWeight = Bundle.FirstOrDefault(x => x.IdBundle == IdBundle).Items.Sum(c=>c.Weight);

            TotalPipes = SelectedPipes + FromPipes;
            TotalWeight = PipesWeight + FromPipesWeight;


            RaisePropertyChanged(() => FromPipes);
            RaisePropertyChanged(() => FromPipesWeight);
            RaisePropertyChanged(() => TotalPipes);
            RaisePropertyChanged(() => TotalWeight);
        }

        private void MovePipes()
        {
            if (IdBundle > 0)
            {
                if (this.IsCurrent)
                {
                    int idCradle = Bundle.FirstOrDefault(b => b.IdBundle == IdBundle).IdCradle;

                    List<IBundle> currentBundles;
                    currentBundles = ExitModel.Instance.GetBundlesbyCradle(idCradle);
                    // Bundle Origen
                    int idBundle = MovePipe.FirstOrDefault().IdBundle;
                    IBundle bundleFrom = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                    IBundle bundleTo = currentBundles.FirstOrDefault(b => b.IdBundle == IdBundle);
                    List<IItem> ItemsFrom = new List<IItem>();
                    ItemsFrom = bundleFrom.Items.ToList();
                    List<IItem> ItemsTo = bundleTo.Items.ToList();
                    lock (this)
                    {
                        currentBundles.Remove(bundleFrom);
                        currentBundles.Remove(bundleTo);
                    }
                    foreach (IItem it in MovePipe)
                    {
                        if (ExitModel.Instance.Data.UpdTrackingBundle(IdBundle, it.Id, it.Weight) > 0)
                        {
                            ItemsFrom.Remove(it);
                            it.IsSelected = false;
                            it.IdCradle = bundleTo.IdCradle;
                            ItemsTo.Add(it);
                        }
                    }
                    bundleFrom.RemoveItems();
                    lock (this)
                    {
                        if (ItemsFrom.Count() > 0)
                        {
                            bundleFrom.AddItems(ItemsFrom);
                            currentBundles.Add(bundleFrom);
                        }
                        bundleTo.RemoveItems();
                        bundleTo.AddItems(ItemsTo);
                        currentBundles.Add(bundleTo);
                        //ExitModel.Instance.CurrentBundles.Remove(bundleTo.IdCradle);
                        //ExitModel.Instance.CurrentBundles.Add(bundleTo.IdCradle, currentBundles);
                    }
                    UpdBundle = true;
                }
                else
                {
                    foreach (IItem it in MovePipe)
                    {
                        if (ExitModel.Instance.Data.UpdTrackingBundle(IdBundle, it.Id, it.Weight) > 0)
                        {
                            UpdBundle = true;
                        }
                    }
                }

                CloseWindows(this);
            }
        }
    }
}
