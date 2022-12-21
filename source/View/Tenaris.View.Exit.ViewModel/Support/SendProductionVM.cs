using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.InteractionRequests;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Tenaris.Library.Log;
using Tenaris.View.Exit.Model;
using System.Collections.ObjectModel;
using Tamsa.Manager.Exit.Shared;
using Microsoft.Practices.Prism;

namespace Tenaris.View.Exit.ViewModel.Support
{
    public class SendProductionVM : IPopupWindowActionAware, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Window HostWindow { get; set; }

        private ICommand closeWindow;
        private ICommand send;
        private ICommand commandSelectionChangeGroupEE;
        private ICommand commandSelectionChangeEE;

        public int IdUser { get; set; }
        public string PipUser { get; set; }
        public int IdBundle { get; set; }
        private string comment;
        private bool IsChecked { get; set; }
        private bool IsCheckedSinRuta { get; set; }
        private bool IsSuperVisor{ get; set; }
        private int impresora { get; set; }
        private FilterItem shift;
        List<FilterItem> shiftList = new List<FilterItem>();

        public Dictionary<int, string> errors = new Dictionary<int, string>();
        public int BundleNumber = 0;

        private ObservableCollection<GroupElaborationState> groupElaborationStates;
        private ObservableCollection<ElaborationState> elaborationStates;
        private ObservableCollection<RejectionCode> rejectionCodes;
        private List<ILocation> locations;

        private GroupElaborationState groupElaborationStateSelected;
        private ElaborationState elaborationStateSelected;
        private RejectionCode rejectionCodeSelected;
        private ILocation locationSelected;

        private bool isEnableLocationToSend;
        private bool isEnableRejectionToSend;

        private InteractionRequest<Notification> modalDialogViewWindowRequest { get; set; }
        public InteractionRequest<Notification> ModalDialogViewWindowRequest
        {
            get
            {
                return modalDialogViewWindowRequest ?? (modalDialogViewWindowRequest = new InteractionRequest<Notification>());
            }
        }

        /// <summary>
        /// Host Notification
        /// </summary>
        public Notification HostNotification { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }


  
        public SendProductionVM(int idUser, string pipUser, int idBundle)
        {
            int x = 1;
            this.IdUser = idUser;
            this.PipUser = pipUser;
            this.IdBundle = idBundle;
            IsChecked = true;
            List<string> printMachines = new List<string>();
            printMachines = ExitModel.Instance.Config.ITPrintMachines.Split(',').ToList();
            //AGREGARLOS DESDE EL CONFIG
            foreach (var item in printMachines)
            {
                shiftList.Add(new FilterItem(item, x));
                x++;
            }
            //int machines = ExitModel.Instance.Config.ITPrintMachines.Count();
            //shiftList.Add(new FilterItem("MTFH", 1));
            //shiftList.Add(new FilterItem("MTF4", 2));
            //shiftList.Add(new FilterItem("MTF1", 3));
            //shiftList.Add(new FilterItem("MTEA", 4));
            Shift = shiftList[0];
            Supervisor = (ExitModel.Instance.Config.IsActiveSinSeas.ToUpper().Equals("TRUE")) ? true : false;

            GroupElaborationStates = ExitModel.Instance.GetGroupElaborationStateList();
            GroupElaborationStateSelected = GroupElaborationStates.FirstOrDefault(g => g.Code.Equals(ExitModel.Instance.Config.DefaultGroupEECode));

            ElaborationStates = GetElaborationStates();
            ElaborationStateSelected = ElaborationStates.FirstOrDefault(e => e.Code.Equals(ExitModel.Instance.Config.DefaultEECode));

            RejectionCodes = ExitModel.Instance.GetRejectionCausesByElaborationState(ElaborationStateSelected != null ? ElaborationStateSelected.IdElaborationState : 0);

            Locations = ExitModel.Instance.Data.GetLocations().ToList();
        }

        private ObservableCollection<ElaborationState> GetElaborationStates()
        {
            ObservableCollection<ElaborationState> states;
            if(GroupElaborationStateSelected != null)
            {
                states = new ObservableCollection<ElaborationState>();
                states.Add(new ElaborationState(0, String.Empty, String.Empty, String.Empty));
                states.AddRange(ExitModel.Instance.GetElaborationStatesByGroup(GroupElaborationStateSelected.IdGroupElaborationState));
            }
            else
            {
                states = new ObservableCollection<ElaborationState>();
            }
            return states;
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

        public ICommand CommandSend
        {
            get
            {
                this.send = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SendIT()
                };

                return this.send;
            }
        }

        public ICommand CommandSelectionChangeGroupEE
        {
            get
            {
                this.commandSelectionChangeGroupEE = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectionChangeGroupEE()
                };
                return this.commandSelectionChangeGroupEE;
            }
        }

        public ICommand CommandSelectionChangeEE
        {
            get
            {
                this.commandSelectionChangeEE = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectionChangeEE()
                };
                return this.commandSelectionChangeEE;
            }
        }

        public List<FilterItem> ShiftList
        {
            get { return shiftList; }
        }

        //
        /// <summary>
        /// Gets or sets the Bundle Text.
        /// </summary>
        public string Comment
        {
            get
            {
                return this.comment;
            }

            set
            {
                this.comment = value;
                //RaisePropertyChanged("Comment");
            }
        }
        //

        public bool Ruta
        {
            get
            {
                return this.IsChecked;
            }
            set
            {
                this.IsChecked = value;
            }
        }

        public bool SinRuta
        {
            get
            {
                return this.IsCheckedSinRuta;
            }
            set
            {
                this.IsCheckedSinRuta = value;
            }
        }

        public bool Supervisor
        {
            get
            {
                return this.IsSuperVisor;
            }
            set
            {
                this.IsSuperVisor = value;
            }
        }

        //
        public FilterItem Shift
        {
            get { return shift; }
            set
            {
                if ((value != null) && (shift != value))
                {
                    shift = value;
                    //RaisePropertyChanged("Comment");
                } // if
            }
        }
        //

        public ObservableCollection<GroupElaborationState> GroupElaborationStates
        {
            get
            {
                return this.groupElaborationStates;
            }
            set
            {
                this.groupElaborationStates = value;
                OnPropertyChanged("GroupElaborationStates");
            }
        }

        public ObservableCollection<ElaborationState> ElaborationStates
        {
            get
            {
                return this.elaborationStates;
            }
            set
            {
                this.elaborationStates = value;
                OnPropertyChanged("ElaborationStates");
            }
        }
        public ObservableCollection<RejectionCode> RejectionCodes
        {
            get
            {
                return this.rejectionCodes;
            }
            set
            {
                this.rejectionCodes = value;
                OnPropertyChanged("RejectionCodes");
            }
        }

        public List<ILocation> Locations
        {
            get
            {
                return this.locations;
            }
            set
            {
                this.locations = value;
                OnPropertyChanged("Locations");
            }
        }

        public GroupElaborationState GroupElaborationStateSelected
        {
            get
            {
                return this.groupElaborationStateSelected;
            }
            set
            {
                this.groupElaborationStateSelected = value;
                OnPropertyChanged("GroupElaborationStateSelected");
            }
        }
        public ElaborationState ElaborationStateSelected
        {
            get
            {
                return this.elaborationStateSelected;
            }
            set
            {
                this.elaborationStateSelected = value;
                OnPropertyChanged("ElaborationStateSelected");
            }
        }
        public RejectionCode RejectionCodeSelected
        {
            get
            {
                return this.rejectionCodeSelected;
            }
            set
            {
                this.rejectionCodeSelected = value;
                OnPropertyChanged("RejectionCodeSelected");
            }
        }

        public ILocation LocationSelected
        {
            get
            {
                return this.locationSelected;
            }
            set
            {
                this.locationSelected = value;
                OnPropertyChanged("LocationSelected");
            }
        }
        public bool IsEnableLocationToSend
        {
            get
            {
                return this.isEnableLocationToSend;
            }
            set
            {
                this.isEnableLocationToSend = value;
                OnPropertyChanged("IsEnableLocationToSend");
            }
        }

        public bool IsEnableRejectionToSend
        {
            get
            {
                return this.isEnableRejectionToSend;
            }
            set
            {
                this.isEnableRejectionToSend = value;
                OnPropertyChanged("IsEnableRejectionToSend");
            }
        }

        private bool IsValidToSendIt()
        {
            if (!GroupElaborationStateSelected.Code.Equals("BUE"))
            {
                //if (LocationSelected == null)
                //    return false;

                if (RejectionCodeSelected == null)
                    return false;
            }
            return true;
        }
        private void SendIT()
        {
            try
            {
                if(ElaborationStateSelected != null)
                {
                    if (IsValidToSendIt())
                    {
                        bool rutaSeas;
                        string imp;

                        rutaSeas = (IsCheckedSinRuta) ? false : true;

                        comment = (comment == null) ? "" : comment;

                        imp = (Shift.DisplayValue.ToString());
                        //int  = Convert.ToInt32(Shift.ItemValue);

                        string groupEECode = GroupElaborationStateSelected != null ? GroupElaborationStateSelected.Code : "";
                        string EECode = ElaborationStateSelected != null ? ElaborationStateSelected.Code : "";
                        string locationId = LocationSelected != null ? LocationSelected.Id.ToString() : "";
                        string rejectionCode = RejectionCodeSelected != null ? RejectionCodeSelected.Code : "";

                        ExitModel.Instance.SendBundle(this.IdUser, this.PipUser, this.IdBundle, comment, rutaSeas, imp, groupEECode, EECode, locationId, rejectionCode, out errors, out BundleNumber);

                        ConfirmApplyRequest = new InteractionRequest<Confirmation>();

                        GC.Collect();
                        CloseWindow();
                    }
                    else
                    {
                        var viewWarning = new ModalDialogActionViewModel("Es requerido un motivo de descarte", "Envio de Atado", ModalType.Error);
                        ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Envio de Atado" });
                    }
                }
                else
                {
                    var viewWarning = new ModalDialogActionViewModel("Es requerido un estado de elaboración", "Envio de Atado", ModalType.Error);
                    ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Envio de Atado" });
                }
                
                
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }

        private void SelectionChangeGroupEE()
        {
            try
            {
                if (GroupElaborationStateSelected != null)
                {
                    ElaborationStates = GetElaborationStates();
                    if (GroupElaborationStateSelected.Code.Equals("BUE"))
                    {
                        //IsEnableLocationToSend = false;
                        //LocationSelected = null;
                        IsEnableRejectionToSend = false;
                        RejectionCodeSelected = null;
                    }
                    else
                    {
                        //IsEnableLocationToSend = true;
                        IsEnableRejectionToSend = true;
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }

        }

        private void SelectionChangeEE()
        {
            try
            {
                if (ElaborationStateSelected != null)
                {
                    RejectionCodes = ExitModel.Instance.Data.GetRejections(ElaborationStateSelected.IdElaborationState);
                }

            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }

        }

        #region INotifyPropertyChanged

        /// <summary>
        /// Verifies that a property name exists in this ViewModel. This method
        /// can be called before the property is used, for instance before
        /// calling OnPropertyChanged. It avoids errors when a property name
        /// is changed but some places are missed.
        /// <para>This method is only active in DEBUG mode.</para>
        /// </summary>
        /// <param name="propertyName"></param>
        //[Conditional("DEBUG")]
        //[DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
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
        private void OnPropertyChanged(string propertyName)
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
