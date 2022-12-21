// -----------------------------------------------------------------------
// <copyright file="CradelViewModel.cs" company="Techint">
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
    using Tenaris.Service.Security.Client;
    using Tenaris.View.Exit.Library;
    using Tenaris.Library.Log;
    
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CradleViewModel : NotificationObject
    {

        #region Variables

        private bool isSelected;
        private bool isFiltered;

        private ICommand selectedCradle;

        #endregion

        #region Properties

        public ICradle Cradle { get; set; }

        public int LoadedPieces { get; set; }

        public double LoadedWeight { get; set; }

        public List<CradleMode> Modes { get; set; }

        public CradleMode SelectedMode { get; set; }

        public int OrderNumber { get; set; }

        private ICommand setMaxPieces;

        private int idCradle;

        private int maxPieces;

        public bool IsSelected {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        public bool IsFiltered
        {
            get
            {
                return this.isFiltered;
            }
            set
            {
                this.isFiltered = value;
                RaisePropertyChanged(() => IsFiltered);
            }
        }

        public ICommand SCChangeSelected
        {
            get
            {
                this.selectedCradle = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectedCradle()
                };

                return this.selectedCradle;
            }
        }


        //SET EL VALOR DE CANASTA
        public ICommand CommandSetMaxPieces
        {
            get
            {
                this.setMaxPieces = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SetMaxPieces()
                };

                return this.setMaxPieces;
            }
        }


        private void SetMaxPieces()
        {
            try
            {
                idCradle = Cradle.Id;
                maxPieces = Cradle.MaximumPieces;
                
                //PASARLE LOS NUEVOS PARAMETROS AL MANAGER
                ExitModel.Instance.ChangeCradleCondition(idCradle, CradleMode.Automatic, CradleState.Enable, maxPieces);
                
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }

        #endregion

        #region Security Commands

        private SecurityCommand changeMode;

        #endregion

        #region Constructor

        public void update()
        {
            RaisePropertyChanged(() => Cradle);
            RaisePropertyChanged(() => LoadedPieces);
        }

        public CradleViewModel(ICradle cradle)
        {
            object value;
            this.Modes = new List<CradleMode>();
            Modes.Add(CradleMode.Automatic);
            Modes.Add(CradleMode.Manual);
            this.Cradle = cradle;
            this.SelectedMode = cradle.Mode;
            this.IsSelected = true;         
            if (cradle.CurrentBundle != null)
            {
                LoadedPieces = cradle.CurrentBundle.Items.Count();
                LoadedWeight = Math.Round(cradle.CurrentBundle.Items.Sum(i => i.Weight), 2);
                cradle.CurrentBundle.ExtraData.TryGetValue("OrderNumber", out value);
                if (value != null)
                    OrderNumber = Convert.ToInt32(value);
                else
                    OrderNumber = 0;
            }
            else
            {
                LoadedPieces = 0;
                LoadedWeight = 0;
                OrderNumber = 0;
           }
            RaisePropertyChanged(() => Cradle);
            RaisePropertyChanged(() => Modes);
            RaisePropertyChanged(() => LoadedPieces);
            RaisePropertyChanged(() => LoadedWeight);
            RaisePropertyChanged(() => OrderNumber);

            SCChangeMode = new SecurityCommand(() => ChangeMode(), AppCommands.accChangeMode);
            RaisePropertyChanged("SCChangeMode");
        }

        #endregion

        #region Command
        public SecurityCommand SCChangeMode
        {
            get { return changeMode; }
            set
            {
                changeMode = value;
                RaisePropertyChanged("SCChangeMode");
            }
        }
        
        #endregion

        #region Implementacion de Comandos

        private void ChangeMode()
        {
            bool result = false;
            if (this.SelectedMode != Cradle.Mode)
            {
               result = ExitModel.Instance.ExitAdapter.ChangeCradleCondition(this.Cradle.Id, this.SelectedMode, null, null);
               Cradle.Mode = this.SelectedMode;
            }

            RaisePropertyChanged("SelectedMode");            
        }

        private void SelectedCradle()
        {
            lock (this)
            {
                this.IsSelected = true;             
            }
        }
        #endregion

    }
}
