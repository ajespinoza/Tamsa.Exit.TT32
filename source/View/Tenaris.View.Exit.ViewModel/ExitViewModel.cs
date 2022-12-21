// -----------------------------------------------------------------------
// <copyright file="ExitViewModel.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Tamsa.Manager.Exit.Shared;
using Tenaris.Manager.Tracking.Shared;
using Tenaris.View.Exit.Library;
using Tenaris.Library.Shared;
using Tenaris.Service.Specification.Shared;


namespace Tenaris.View.Exit.ViewModel
{
    using Tenaris.Tamsa.HRM.Fat2.Library.LineupColorRules;    
    using Tenaris.View.Exit.Model;
    using Tenaris.Service.Security.Client;
    using Tenaris.Library.Log;
    using Tenaris.Library.Framework;
    using Tenaris.Library.Framework.Utility.Conversion;
    using Tenaris.Service.Security.Shared;
    using System.Collections.ObjectModel;
    using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
    using System.Threading;
    using Tenaris.View.Exit.ViewModel.Support;
    using Microsoft.Practices.Prism.ViewModel;

    
        
    
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExitViewModel : NotificationObject
    {

        #region Variables
           
        private string zoneCode;      
        private ImageSource userPicture;
        private int gridHeight;

        static object lockShared = new object();
    
                     
        private ReadOnlyDictionary<string, ReadOnlyDictionary<string, ISpecificationField>> specificationData;
        private ReadOnlyDictionary<string, object> productionUnitValue;

        private int itemIndex = 0;
        private int ItemIndex = 0;
        private bool IsUpdate = false;
        private bool allFiltered;
        private int idSelectedCradle;
                        
        private int heatNumber;
        private string ee;
        private int selectedIndexTab;

        #region Histórico

        private bool isCheckedDate;
        private bool isCheckedShift;
        private bool isCheckedOrder;

        private ReadOnlyDictionary<int, string> shifts;

        #endregion

        #region Security Commands

        private SecurityCommand sendBundle;
        private SecurityCommand insertBundle;
        private SecurityCommand editBundle;
        private SecurityCommand deleteBundle;
        private SecurityCommand insertPipe;
        private SecurityCommand tagPrint;
        private SecurityCommand deletePipe;
        private SecurityCommand movePipe;
        //private SecurityCommand selectAll;
        #endregion

        #region Commands

        public ICommand ShowLoginCommand { get; private set; }

        private ICommand selectedItem;

        private ICommand selectedHItem;

        private ICommand selectedShift;

        private ICommand searchBundles;

        private ICommand resetFilters;

        private ICommand selectAll;

        private ICommand selectAllHistoric;

        private ICommand selectAllAvailable;

        #endregion

        #endregion

        #region Properties

        public bool DskMgr { get; private set; }
        public int WHeight { get; set; }
        public bool IsCheckedAll { get; set; }
        public bool IsCheckedAllHistoric { get; set; }
        
    
        public int GridHeight
        {
            get 
            {
                return gridHeight; 
            }
            set
            {
                gridHeight = value;
                RaisePropertyChanged("GridHeight");
            }

        }

        public string ZoneCode
        {
            get
            {
                return this.zoneCode;
            }
            private set
            {
                this.zoneCode = value;
                RaisePropertyChanged("ZoneCode");
            }
        }

        public ImageSource UserPicture
        {
            get
            {
                return this.userPicture;
            }
            private set
            {
                this.userPicture = value;
                RaisePropertyChanged("UserPicture");
            }
        }

        public string UserIdentication { get; set; }

        public int IdUser { get; private set; }

        public string PipIdentification { get; private set; }

        public bool ShowLoginButton { get; set; }
      
        public Visibility ShowLoginBar { get; set; }

        public int SelectedIndexTab
        {
            get
            {
                return this.selectedIndexTab = (this.selectedIndexTab == 0) ? 1 : this.selectedIndexTab;

                //if (this.selectedIndexTab == 0)
                //{
                //    return this.selectedIndexTab = 1;
                //}
                //else
                //{
                //    return this.selectedIndexTab;
                //}
            }
            set
            {
                this.selectedIndexTab = value;               
                RaisePropertyChanged("SelectedIndexTab");
            }
        }

        public int BundleIndex { get; set; }
        public int select = 0;
        public int selectHisoric = 0;
        public Record SelectedRecord { get; set; }
        public int CradlesCount { get; set; }

        public bool AllFiltered
        {
            get
            {
                return this.allFiltered;
            }
            set
            {
                this.allFiltered = value;
                foreach (CradleViewModel c in Cradles)
                {
                    c.IsFiltered = value;
                }
                RaisePropertyChanged("AllFiltered");
            }
        }

        public int IdSelectedCradle
        {
            get
            {
                return this.idSelectedCradle;
            }
            set
            {
                this.idSelectedCradle = value;
                foreach (CradleViewModel c in Cradles)
                {
                    c.IsSelected = (c.Cradle.Id == value) ? true : false;
                }
                RaisePropertyChanged("IdSelectedCradle");
            }
        }

        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, ISpecificationField>> SpecificationData
        {
            get
            {
                return this.specificationData;
            }

            set
            {
                this.specificationData = value;
                RaisePropertyChanged("SpecificationData");
            }
        }

        public ReadOnlyDictionary<string, object> ProductionUnitValue
        {
            get { return this.productionUnitValue; }
            set
            {
                this.productionUnitValue = value;
                RaisePropertyChanged("ProductionUnitValue");
            }
        }

        public int HeatNumber
        {
            get
            {
                return this.heatNumber;
            }
            set
            {
                this.heatNumber = value;
                RaisePropertyChanged("HeatNumber");

            }

        }

        public string EE
        {
            get
            {
                return this.ee;
            }
            set
            {
                this.ee = value;
                RaisePropertyChanged("EE");

            }

        }

        public ObservableCollection<Record> CurrentBundles { get; set; }
        public ObservableCollection<IItem> SelectedTrackings { get; set; }

        /// <summary>
        /// Cradles 
        /// </summary>
        public ObservableCollection<CradleViewModel> Cradles { get; set; }

        #region Histórico

        public bool IsCheckedDate
        {
            get
            {
                return this.isCheckedDate;
            }
            set
            {
                this.isCheckedDate = value;
                RaisePropertyChanged("IsCheckedDate");
            }
        }

        public bool IsCheckedShift
        {
            get
            {
                return this.isCheckedShift;
            }
            set
            {
                this.isCheckedShift = value;
                RaisePropertyChanged("IsCheckedShift");
            }
        }

        public bool IsCheckedOrder
        {
            get
            {
                return this.isCheckedOrder;
            }
            set
            {
                this.isCheckedOrder = value;
                RaisePropertyChanged("IsCheckedOrder");
            }
        }

        public int ShiftNumber { get; set; }

        public DateTime ShiftDate { get; set; }

        public string FilterOrder { get; set; }
        
        public ReadOnlyDictionary<int, string> Shifts
        {
            get
            {
                return this.shifts;
              
            }
        }

        public int BundleHIndex { get; set; }
        public Record SelectedHRecord { get; set; }


        public List<IBundle> HistoricBundles { get; set; }
        public ObservableCollection<Record> HistoricRecord { get; set; }
        public ObservableCollection<IItem> SelectedHTrackings { get; set; }

        #endregion
        
        #endregion
        
             
        #region Manejo de Ventanas Modales

        public Window HostWindow { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }
        public InteractionRequest<Confirmation> ConfirmCommentRequest { get; set; } 
        private InteractionRequest<Notification> modalDialogViewWindowRequest { get; set; }
        public InteractionRequest<Notification> ModalDialogViewWindowRequest
        {
            get
            {
                return modalDialogViewWindowRequest ?? (modalDialogViewWindowRequest = new InteractionRequest<Notification>());
            }
        }


        private MovePipeViewModel movePipeViewModel { get; set; }
        private BundleViewModel bundleViewModel { get; set; }
        private BundleEditViewModel bundleEditViewModel { get; set; }
        private CommentsViewModel commentsViewModel { get; set; }
        private SendProductionVM sendProductionVM { get; set; }

        #endregion


        #region SecurityCommand

        private void InitializePrivilegies()
        {
            try
            {

                SCSendBundle = new SecurityCommand(() => SendBundle(), AppCommands.accSendBundle);
                RaisePropertyChanged("SCSendBundle");

                SCInsertBundle = new SecurityCommand(() => InsertBundle(), AppCommands.accInsBundle);
                RaisePropertyChanged("SCInsertBundle");

                SCEditBundle = new SecurityCommand(() => EditBundle(), AppCommands.accUpdBundle);
                RaisePropertyChanged("SCEditBundle");

                SCDeleteBundle = new SecurityCommand(() => DeleteBundle(), AppCommands.accDelBundle);
                RaisePropertyChanged("SCDeleteBundle");

                SCInsertPipe = new SecurityCommand(() => InsertPipe(), AppCommands.accUpdBundle);
                RaisePropertyChanged("SCInsertPipe");

                SCDeletePipe = new SecurityCommand(() => DeletePipe(), AppCommands.accUpdBundle);
                RaisePropertyChanged("SCDeletePipe");

                SCMovePipe = new SecurityCommand(() => MovePipe(), AppCommands.accUpdBundle);
                RaisePropertyChanged("SCMovePipe");

                //SELECCIONAR TODOS
                //SCSelectAll = new SecurityCommand(() => SelectAll(), AppCommands.accUpdBundle);
                //RaisePropertyChanged("SCSelectAll");

                //IMPRIMIR ETIQUETA 
                //SCTagPrint = new SecurityCommand(() => MovePipe(), AppCommands.accUpdBundle);
                //RaisePropertyChanged("SCTagPrint");

            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }
        }

        public SecurityCommand SCSendBundle
        {
            get { return sendBundle; }
            set
            {
                sendBundle = value;
                RaisePropertyChanged("SCSendBundle");
            }
        }

        public SecurityCommand SCInsertBundle
        {
            get { return insertBundle; }
            set
            {
                insertBundle = value;
                RaisePropertyChanged("SCInsertBundle");
            }
        }

        public SecurityCommand SCEditBundle
        {
            get { return editBundle; }
            set
            {
                editBundle = value;
                RaisePropertyChanged("SCEditBundle");
            }
        }

        public SecurityCommand SCDeleteBundle
        {
            get { return deleteBundle; }
            set
            {
                deleteBundle = value;
                RaisePropertyChanged("SCDeleteBundle");
            }
        }

        public SecurityCommand SCInsertPipe
        {
            get { return insertPipe; }
            set
            {
                insertPipe = value;
                RaisePropertyChanged("SCInsertPipe");
            }
        }

        public SecurityCommand SCTagPrint
        {
            get { return tagPrint; }
            set
            {
                tagPrint = value;
                RaisePropertyChanged("SCTagPrint");
            }
        }

        public SecurityCommand SCDeletePipe
        {
            get { return deletePipe; }
            set
            {
                deletePipe = value;
                RaisePropertyChanged("SCDeletePipe");
            }
        }

        public SecurityCommand SCMovePipe
        {
            get { return movePipe; }
            set
            {
                movePipe = value;
                RaisePropertyChanged("SCMovePipe");
            }
        }

        //public SecurityCommand SCSelectAll
        //{
        //    get { return selectAll; }
        //    set
        //    {
        //        selectAll = value;
        //        RaisePropertyChanged("SCSelectAll");
        //    }
        //}


        #endregion
              

        #region Constructor

        public ExitViewModel()
        {
            try
            {


                #region Culture
                CultureInfo culture = null;                  
                if (ExitModel.Instance.Config != null)
                {
                    DskMgr = ExitModel.Instance.Config.DskMgr;
                    this.ZoneCode = ExitModel.Instance.Config.AreaCode;

                    try
                    {
                        culture = CultureInfo.CreateSpecificCulture(ExitModel.Instance.Config.Language);
                    }
                    catch
                    {
                        culture = CultureInfo.CreateSpecificCulture("en-US");
                    }

                }
                else
                {
                    DskMgr = false;
                    culture = CultureInfo.CreateSpecificCulture("en-US");

                }

                #endregion

                #region Security
                //SecurityClient                  
          
                SecurityClient.UserChanged += this.OnUserChanged;

        

                this.ShowLoginCommand = new DelegateCommand(ShowLogin);


                // The login bar is showed if the app is not configured for desk manager
                this.ShowLoginBar = Visibility.Visible;
                if (DskMgr)
                {
                    this.ShowLoginBar = Visibility.Collapsed;
                }

                if (!DskMgr)
                {

                    // Set show button property
                    if (SecurityClient.AuthenticationType == AuthenticationType.Anonymous ||
                        SecurityClient.AuthenticationType == AuthenticationType.None)
                    {
                        this.ShowLoginButton = false;
                        RaisePropertyChanged("ShowLoginButton");

                    }
                    else
                    {
                        this.ShowLoginButton = true;
                        RaisePropertyChanged("ShowLoginButton");
                    }

                    if (SecurityClient.LoggedUser.Identity.IsAuthenticated == false
                        && SecurityClient.AuthenticationType != AuthenticationType.Anonymous
                        && SecurityClient.AuthenticationType != AuthenticationType.None)
                    {
                        ShowLogin();
                        if (!SecurityClient.LoggedUser.Identity.IsAuthenticated)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() => Application.Current.Shutdown()), null);
                            Destroy();
                        }
                    }

                }
                else
                {

                    // lee la identificacion del usuario al inicio con desktop
                    // porque no se recibe el evento de cambio de usuario al
                    // conectarse al security
                    try
                    {
                        this.UserIdentication = SecurityClient.LoggedUserProfile.Identification;
                        IdUser = ExitModel.Instance.Data.GetIdbyIdentification(this.UserIdentication);
                        PipIdentification = ExitModel.Instance.Data.GetPipUser(this.UserIdentication);
                    }
                    catch (Exception ex)
                    {
                        Trace.Error("Error obteniendo credenciales de usuario");
                        Trace.Exception(ex);
                    }

                } // if

                #endregion
               

                InitializePrivilegies();

                ShiftDate = DateTime.Now;
                FilterOrder = String.Empty;
                
                Cradles = new ObservableCollection<CradleViewModel>();
                foreach (ICradle c in ExitModel.Instance.ExitAdapter.GetCradles().ToList())
                {
                    Cradles.Add(new CradleViewModel(c));
                }
                this.AllFiltered = true;
                if (Cradles.Count > 1)
                    CradlesCount = Cradles.Count();
                else
                {
                    CradlesCount = 4;                   
                }
//                Cradles[Cradles.Count() - 1].IsSelected = true;       

                InitializeData();
                this.shifts = ExitModel.Instance.Data.GetShifts();
                RaisePropertyChanged("Shifts");
                
                //this.shifts = ExitModel.Instance.Dat

                WHeight = ExitModel.Instance.Config.GridHeight;

                RaisePropertyChanged("WHeight");

                ExitModel.Instance.ExitAdapter.OnCradleChanged +=new EventHandler<CradleChangedEventArgs>(Instance_OnCradleChanged);                    
                
                // establece el idioma segun configuracion
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
                ConfirmApplyRequest = new InteractionRequest<Confirmation>();
                ConfirmCommentRequest = new InteractionRequest<Confirmation>();
                //MsgNotify = new InteractionRequest<Notification>();
                
                Utility.ReleaseMemory();

                RaisePropertyChanged("BundleIndex");
                RaisePropertyChanged("RejectedIndex");
                                  
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message + "- Exit View Model");
            }
        }

        /// <summary>
        /// Initialize Memory from DataBase (Cradles and Bundles)
        /// </summary>
        private void InitializeData()
        {
            try
            {
                ExitModel.Instance.Initialize();
                int heatNumber = 0;
                string ee = "";

                ExitLibrary.Instance.ClearRecord();
                //CradleViewModel selectedCradel = Cradles.FirstOrDefault(c => c.IsSelected == true);
                foreach (CradleViewModel selectedCradel in Cradles)
                {
                    if (selectedCradel != null)
                    {
                        if (selectedCradel.Cradle.ProductData != null)
                        {
                            if (ExitModel.Instance.SpecAdapter != null)
                            {
                                if (selectedCradel.Cradle.ProductData.IdSpecification > 0)
                                {
                                    this.specificationData = ExitModel.Instance.SpecAdapter.GetSpecification(selectedCradel.Cradle.ProductData.IdSpecification).Data;
                                    this.productionUnitValue = ExitModel.Instance.SpecAdapter.GetProductionUnit(selectedCradel.Cradle.ProductData.IdSpecification).ToReadOnlyDictionary();
                                    ExitModel.Instance.CurrentHeats.TryGetValue(selectedCradel.Cradle.Id, out heatNumber);
                                    if (heatNumber != 0)
                                    {
                                        this.HeatNumber = heatNumber;
                                    }
                                }
                            }
                        }
                        Update(selectedCradel.Cradle);
                    }
                    //CradleViewModel selectedCradel = Cradles[x];

                    //}
                }
                ExitLibrary.Instance.ClearRecord();
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message + "---->InitializeData");
            }
        }

        #endregion
       

        #region Events

        public event EventHandler<UserChangedEventArgs> UserChanged;

        /// <summary>
        /// It triggers when user is Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserChanged(object sender, UserChangedEventArgs e)
        {
            try
            {
                UserPicture = SecurityClient.LoggedUserProfile.Picture != null
                    ? SecurityClient.LoggedUserProfile.Picture.ToBitmapImage()
                    : null;



                this.UserIdentication = SecurityClient.LoggedUserProfile.Identification;
                RaisePropertyChanged("UserIdentication");
                RaisePropertyChanged("UserName");
                RaisePropertyChanged("UserFullName");

                IdUser = ExitModel.Instance.Data.GetIdbyIdentification(this.UserIdentication);
                PipIdentification = ExitModel.Instance.Data.GetPipUser(this.UserIdentication);
                RaisePropertyChanged("IdUser");

                // Propagate event
                if (this.UserChanged != null)
                {
                    this.UserChanged(sender, e);
                }
            }
            catch (Exception exc)
            {
                Trace.Exception(exc, "Error occurred on user changed security event.");
            }
        }

        /// <summary>
        /// It triggers  when a cradle is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_OnCradleChanged(object sender, CradleChangedEventArgs e)
        {
            try
            {
                IsUpdate = true;
                //int idSpecification = 0;
                //int heat = 0;
                //ExitLibrary.Instance.ClearRecord();
                //

                //CradleViewModel ic = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(e.Cradle.Id));

                //ic.Cradle = e.Cradle;

                //foreach (var item in Cradles)
                //{
                    //ExitModel.Instance.CurrentSpecifications.TryGetValue(e.Cradle.Id, out idSpecification);
                    //ExitModel.Instance.CurrentHeats.TryGetValue(e.Cradle.Id, out heat);
                    //if (ExitModel.Instance.SpecAdapter != null)
                    //{
                    //    if (idSpecification > 0)
                    //    {
                    //        this.specificationData = ExitModel.Instance.SpecAdapter.GetSpecification(idSpecification).Data;
                    //        this.productionUnitValue = ExitModel.Instance.SpecAdapter.GetProductionUnit(idSpecification).ToReadOnlyDictionary();
                    //    }
                    //}
                    //if (heat > 0)
                    //{
                    //    this.HeatNumber = heat;
                    //}

                //PRUEBA DE ACTUALIZAR CANASTAS
                if (ExitModel.Instance.CurrentBundles.Count > 0)
                {
                    if(e.Cradle.CurrentBundle.Status == BundleStatus.InProcess)
                    {
                        CradleViewModel ic = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(e.Cradle.Id));

                        if (ic != null)
                        {
                            ic.LoadedPieces = e.Cradle.CurrentBundle.Items.Count();// cradle.CurrentBundle.Items.Count();
                            if (ic.LoadedPieces ==  e.Cradle.MaximumPieces)
                            {
                                e.Cradle.State = CradleState.Disable;
                            }
                            else
                            {
                                e.Cradle.State = CradleState.Enable;
                            }
                        }
                        ic.update();
                    }

                }

                    CurrentBundles.Clear();
                    UpdateRecord(e.Cradle);
                    //Update(e.Cradle);
                //}
                                                
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Error actualizando Cradles");
            }
        }
      
     
        #endregion

        #region Security

        /// <summary>
        /// Shows the login form
        /// </summary>
        private void ShowLogin()
        {
            SecurityClient.Login(this.ZoneCode);
        }

        private void Destroy()
        {
            try
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "MainWindowViewModel.Destroy()");
            }
        }

        #endregion

        #region Implementacion de Comandos

        public ICommand CmdSelectedItem
        {
            get
            {
                this.selectedItem = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => GetBundleIndex((Record)p)
                };
              
                return this.selectedItem;
            }
        }

        public ICommand CmdSelectedHItem
        {
            get
            {
                this.selectedHItem = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => GetBundleHIndex((Record)p)
                };

                return this.selectedHItem;
            }
        }

        public ICommand CmdSelectedShift
        {
            get
            {
                this.selectedShift = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => GetShift((int)p)
                };

                return this.selectedShift;
            }
        }

        public ICommand CmdSearchBundles
        {
            get
            {
                this.searchBundles = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SearchBundles()
                };

                return this.searchBundles;
            }
        }

        public ICommand CmdResetFilters
        {
            get
            {
                this.resetFilters = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => ResetFilters()
                };

                return this.resetFilters;
            }
        }

        //selectAll
        public ICommand CmdSelectAll
        {
            get
            {
                this.selectAll = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectAll()
                };

                return this.selectAll;
            }
        }



        //selectAllHistoric
        public ICommand CmdSelectAllHistoric
        {
            get
            {
                this.selectAllHistoric = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectAllHistoric()
                };

                return this.selectAllHistoric;
            }
        }

        //selectAllAvailable
        public ICommand SelectAllAvailable
        {
            get
            {
                this.selectAllAvailable = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectAllAvailableT()
                };

                return this.selectAllAvailable;
            }
        }
        
        #endregion

        #region Command Methods

        /// <summary>
        /// Get Selected Shift
        /// </summary>
        /// <param name="p"></param>
        private void GetShift(int p)
        {
            if (p != null)
            {
                ShiftNumber = p;
                RaisePropertyChanged("ShiftNumber");
            }
            else
                ShiftNumber = 0;
        }

        /// <summary>
        /// Selects a Bundle (Good)
        /// </summary>
        /// <param name="p"></param>
        private void GetBundleIndex(Record p)
        {
            try
            {
                if (p != null)
                {
                    UpdateView(p);
                    for (int x = 0; x < SelectedTrackings.Count(); x++)
                    {
                        SelectedTrackings[x].IsSelected = false;
                    }

                    IsCheckedAll = false;
                    RaisePropertyChanged("IsCheckedAll");


                    ///Para no perder el foco en el Grid 
                    if (IsUpdate)
                    {
                        BundleIndex = ItemIndex;
                        RaisePropertyChanged("BundleIndex");
                        IsUpdate = false;
                    }
                    else
                    {
                        if (BundleIndex != 0)
                            ItemIndex = BundleIndex;
                    }

                }
            }
            catch (Exception ex)
            {
                Trace.Debug(ex.Message + "------>GetBundleIndex");
            }
        }

        private void UpdateView(Record p)
        {
            int idCradle = Convert.ToInt32(p["IdCradle"].Value.ToString());
            int idBundle = Convert.ToInt32(p["IdBundle"].Value.ToString());
            List<IBundle> currentBundles = new List<IBundle>();
            ExitModel.Instance.CurrentBundles.TryGetValue(idCradle, out currentBundles);
            if (currentBundles != null)
            {
                foreach (var item in currentBundles)
                {
                    if (item.IdBundle.Equals(idBundle))
                    {
                        try
                        {
                            SelectedTrackings = new ObservableCollection<IItem>(currentBundles.FirstOrDefault(b => b.IdBundle == idBundle).Items);
                            RaisePropertyChanged("SelectedTrackings");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Trace.Error(ex.Message + " -----> GetBundleIndex");
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Selects a Bundle (Historico)
        /// </summary>
        /// <param name="p"></param>
        private void GetBundleHIndex(Record p)
        {
            if (p != null)
            {
                UpdateViewRecord(p);
                for (int x = 0; x < SelectedHTrackings.Count(); x++)
                {
                    SelectedHTrackings[x].IsSelected = false;
                }

                IsCheckedAllHistoric = false;
                RaisePropertyChanged("IsCheckedAllHistoric");
            }
        }

        private void UpdateViewRecord(Record p)
        {
            int idCradle = Convert.ToInt32(p["IdCradle"].Value.ToString());
            int idBundle = Convert.ToInt32(p["IdBundle"].Value.ToString());
            if (HistoricBundles != null)
            {
                try
                {
                    SelectedHTrackings = new ObservableCollection<IItem>(HistoricBundles.FirstOrDefault(b => b.IdBundle == idBundle).Items);
                    RaisePropertyChanged("SelectedHRecord");
                    RaisePropertyChanged("SelectedHTrackings");
                }
                catch (Exception ex)
                {
                    Trace.Debug(ex.Message + "----> GetBundleHIndex");
                }
            }
        }

        /// <summary>
        /// Search historic bundles
        /// </summary>
        private void SearchBundles()
        {
            DateTime shiftDate;
            int shiftnumber = 0;
            int idBatch = 0;
            int idCradle=0;
            shiftDate = (IsCheckedDate) ? ShiftDate : DateTime.Now;
            shiftnumber = (IsCheckedShift) ? ShiftNumber : 0; 
            idBatch = (IsCheckedOrder) && (FilterOrder!= String.Empty) ? Convert.ToInt32(FilterOrder)  : 0;
            HistoricBundles = new List<IBundle>();
            HistoricBundles = ExitModel.Instance.Data.GetBundles(IsCheckedDate, shiftDate, shiftnumber, idBatch);
            ExitLibrary.Instance.ClearRecord();
            if (HistoricBundles.Count > 0)
            {
                foreach (Bundle b in HistoricBundles)
                    {
                        List<IItem> trackings = new List<IItem>();
                        idCradle = b.IdCradle;
                        trackings = ExitModel.Instance.Data.GetTrackingsOnBundle(idCradle, b.IdBundle);
                        foreach (IItem t in trackings)
                        {
                            b.AddItem(t);
                            if (t.ExtraData != null)
                            {
                                if (b.ExtraData == null)
                                {
                                    Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                    bundleData = t.ExtraData;
                                    b.ExtraData = bundleData;
                                }
                            }
                        }                       
                    }
                
                ICradle icreadle=ExitModel.Instance.ExitAdapter.GetCradles().FirstOrDefault(x=>x.Id.Equals(idCradle));
                HistoricRecord = ExitLibrary.Instance.FillData(icreadle, HistoricBundles);
                 RaisePropertyChanged("HistoricRecord");            
                }

            ExitLibrary.Instance.ClearRecord();       
        }

        /// <summary>
        /// Resets Historic Bundles
        /// </summary>
        private void ResetFilters()
        {
            ShiftNumber = 0;
            ShiftDate = DateTime.Now;
            FilterOrder = String.Empty;
            IsCheckedDate = false;
            IsCheckedShift = false;
            IsCheckedOrder = false;
            HistoricBundles.Clear();
            HistoricRecord.Clear();            
            RaisePropertyChanged("HistoricRecord");
        }

        #endregion

        #region Security Command Methods

        /// <summary>
        /// Move Pipe between Bundles
        /// </summary>
        private void MovePipe()
        {
            if (selectedIndexTab == 1)
            {
                try
                {
                    Record item = null;
                    item = SelectedRecord;
                    int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                    int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());

                    CradleViewModel selectedCradel = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(idCradle));
                    if (selectedCradel != null)
                    {
                        if (SelectedTrackings.Where(x => x.IsSelected).Count() > 0)
                        {
                            itemIndex = BundleIndex;
                            //int idBundle = SelectedTrackings.Where(x => x.IsSelected).FirstOrDefault().IdBundle;
                            List<IBundle> currentBundles = new List<IBundle>();
                            ExitModel.Instance.CurrentBundles.TryGetValue(idCradle, out currentBundles);
                            if (currentBundles != null)
                            {
                                IBundle bundle = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                                if (bundle.Status != BundleStatus.Sent)
                                {
                                    int heatNumber = Convert.ToInt32(SelectedTrackings.Where(x => x.IsSelected).FirstOrDefault().ExtraData["HeatNumber"]);
                                    int idStatus = Convert.ToInt32(SelectedTrackings.Where(x => x.IsSelected).FirstOrDefault().ExtraData["idItemStatus"]);
                                    //string Side = SelectedTrackings.Where(x => x.IsSelected).FirstOrDefault().ExtraData["Side"].ToString();
                                    if (idBundle > 0)
                                    {
                                        List<IBundle> EndedBundles = currentBundles.Where(bd => Convert.ToInt32(bd.ExtraData["HeatNumber"]) == heatNumber
                                                                                //&& Convert.ToInt32(bd.ExtraData["idItemStatus"]) == idStatus
                                            //&& bd.ExtraData["Side"].Equals(Side)
                                                                                && bd.Status != BundleStatus.Sent).ToList();

                                        movePipeViewModel = new MovePipeViewModel(SelectedTrackings.Where(x => x.IsSelected).ToList(), EndedBundles, true);
                                        MovePipeViewWindowRequest.Raise(new Notification() { Content = movePipeViewModel });
                                        if (movePipeViewModel.UpdBundle)
                                        {
                                            //UpdateRecord(selectedCradel.Cradle);
                                            ExitModel.Instance.ExitAdapter.UpdateViews(idCradle);
                                        }
                                        movePipeViewModel = null;
                                    }
                                }
                                else
                                {
                                    var viewWarning = new ModalDialogActionViewModel("El Atado no puede modificarse porque ya fue enviado", "Mover Tubos", ModalType.Error);
                                    ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Mover Tubos" });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Error(ex.Message + "---> Move Pipe de Gestion");
                }
            }
            else
            {
                try
                {

                    if (SelectedHTrackings.Where(x => x.IsSelected).Count() > 0)
                    {
                        itemIndex = BundleHIndex;
                        int idBundle = SelectedHTrackings.Where(x => x.IsSelected).FirstOrDefault().IdBundle;
                        if (HistoricBundles != null)
                        {
                            IBundle bundle = HistoricBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                            if (bundle.Status != BundleStatus.Sent)
                            {
                                int heatNumber = Convert.ToInt32(SelectedHTrackings.Where(x => x.IsSelected).FirstOrDefault().ExtraData["HeatNumber"]);
                                int idStatus = Convert.ToInt32(SelectedHTrackings.Where(x => x.IsSelected).FirstOrDefault().ExtraData["idItemStatus"]);
                                //string Side = SelectedHTrackings.Where(x => x.IsSelected).FirstOrDefault().ExtraData["Side"].ToString();
                                if (idBundle > 0)
                                {
                                    List<IBundle> EndedBundles = HistoricBundles.Where(bd => Convert.ToInt32(bd.ExtraData["HeatNumber"]) == heatNumber
                                                                            //&& Convert.ToInt32(bd.ExtraData["idItemStatus"]) == idStatus
                                        //&& bd.ExtraData["Side"].Equals(Side)
                                                                            && bd.Status != BundleStatus.Sent).ToList();

                                    movePipeViewModel = new MovePipeViewModel(SelectedHTrackings.Where(x => x.IsSelected).ToList(), EndedBundles, false);
                                    MovePipeViewWindowRequest.Raise(new Notification() { Content = movePipeViewModel });
                                    if (movePipeViewModel.UpdBundle)
                                    {
                                        // Update(selectedCradel.Cradle);
                                        SearchBundles();
                                    }
                                    movePipeViewModel = null;
                                }
                            }
                            else
                            {
                                var viewWarning = new ModalDialogActionViewModel("El Atado no puede modificarse porque ya fue enviado", "Mover Tubos", ModalType.Error);
                                ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Mover Tubos" });
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Trace.Error(ex.Message + "----> Move Pipe del Historico");
                }
            }
        }


        private void SelectAll()
        {
            if (IsCheckedAll)
            {
                for (int x = 0; x < SelectedTrackings.Count(); x++)
                {
                    SelectedTrackings[x].IsSelected = IsCheckedAll;
                }
            }
            else
            {
                for (int x = 0; x < SelectedTrackings.Count(); x++)
                {
                    SelectedTrackings[x].IsSelected = false;
                }
            }
            UpdateView(SelectedRecord);
        }


        private void SelectAllHistoric()
        {
            if (IsCheckedAllHistoric)
            {
                for (int x = 0; x < SelectedHTrackings.Count(); x++)
                {
                    SelectedHTrackings[x].IsSelected = IsCheckedAllHistoric;
                }
            }
            else
            {
                for (int x = 0; x < SelectedHTrackings.Count(); x++)
                {
                    SelectedHTrackings[x].IsSelected = false;
                }
            }
            UpdateViewRecord(SelectedHRecord);
        }

        //SelectAllAvailable
        private void SelectAllAvailableT()
        {
            
            if (IsCheckedAllHistoric)
            {
                for (int x = 0; x < SelectedTrackings.Count(); x++)
                {
                    SelectedHTrackings[x].IsSelected = IsCheckedAllHistoric;
                }
            }
            else
            {
                for (int x = 0; x < SelectedTrackings.Count(); x++)
                {
                    SelectedTrackings[x].IsSelected = false;
                }
            }
            //UpdateViewRecord(SelectedHRecord);
        }

        /// <summary>
        /// Send Bundle to PIP
        /// </summary>
        public void SendBundle()
        {
            string PipUser = PipIdentification;
            Record item = null;
            try
            {

                if (SelectedIndexTab == 1)
                {
                    CradleViewModel selectedCradel = Cradles.FirstOrDefault(c => c.IsSelected == true);

                    if (selectedCradel != null)
                    {
                        if (SelectedRecord != null)
                        {
                            item = SelectedRecord;
                            itemIndex = BundleIndex;
                            int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                            int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                            List<IBundle> currentBundles = new List<IBundle>();
                            ExitModel.Instance.CurrentBundles.TryGetValue(idCradle, out currentBundles);
                            if (currentBundles != null)
                            {
                                IBundle bundle = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                                if (bundle.Status != BundleStatus.Sent)
                                {
                                    //Dictionary<int, string> errors = new Dictionary<int, string>();

                                    //CHECAR SI EL ATADO SIGUE ABIERTO
                                    if (bundle.Status == BundleStatus.InProcess)
                                    {
                                        //CERRAR EL ATADO
                                        CradleViewModel ic = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(idCradle));
                                        ExitModel.Instance.ExitAdapter.CloseBundle(ic.Cradle);
                                        currentBundles.Remove(bundle);
                                        Trace.Debug("Atado abierto, cerrando atado antes de ser reportado");
                                    }

                                    lock (lockShared)
                                    {
                                        sendProductionVM = new SendProductionVM(IdUser, PipUser, idBundle);
                                        SendProductionViewWindowRequest.Raise(new Notification() { Content = sendProductionVM });
                                    }

                                    if (sendProductionVM.errors.Count > 0 && sendProductionVM.BundleNumber == 0)
                                    {
                                        var viewConfirm = new ModalDialogActionViewModel("Error: " + sendProductionVM.errors.Values.FirstOrDefault().ToString(), "Envio de Atado", ModalType.Error);
                                        ModalDialogViewWindowRequest.Raise(new Notification() { Content = viewConfirm, Title = "Envio de Atado" });
                                    }
                                    else
                                    {
                                        if (sendProductionVM.BundleNumber > 0)
                                        {
                                            var viewConfirm = new ModalDialogActionViewModel("Atado Enviado , Nuevo Atado : " + sendProductionVM.BundleNumber.ToString(), "Envio de Atado", ModalType.Information);
                                            bundle.Send(sendProductionVM.BundleNumber);
                                            ModalDialogViewWindowRequest.Raise(new Notification() { Content = viewConfirm, Title = "Envio de Atado" });
                                            ExitModel.Instance.ExitAdapter.UpdateViews(selectedCradel.Cradle.Id);
                                        }
                                    }

                                    //Seteo del BundleNumber
                                    sendProductionVM.BundleNumber = 0;

                                    commentsViewModel = null;
                                    item = null;
                                }
                                else
                                {
                                    var viewWarning = new ModalDialogActionViewModel("El Atado no puede enviarse porque ya fue enviado", "Envio de Atado", ModalType.Error);
                                    ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Envio de Atado" });
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (SelectedHRecord != null)
                    {
                        item = SelectedHRecord;
                        itemIndex = BundleIndex;
                        ExitLibrary.Instance.ClearRecord();
                        int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                        int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                        string EEAnterior = "";
                        string EETrk = "";
                        int aux = 0;
                        bool EEiguales = false;
                        bool EEigualesf = false;
                        if (HistoricRecord.Count > 0)
                        {
                            IBundle bundle = HistoricBundles.FirstOrDefault(hb => hb.IdBundle == idBundle);
                            if (bundle.Status != BundleStatus.Sent)
                            {
                                foreach (var trk in bundle.Items)
                                {
                                    EETrk = trk.ExtraData["EE"].ToString();
                                    if (aux == 0)
                                    {
                                        EEAnterior = EETrk;
                                        EEigualesf = true;
                                    }
                                    else
                                    {
                                        EEiguales = EEAnterior.Equals(EETrk);
                                        if (EEiguales == false)
                                        {
                                            EEigualesf = false;
                                        }

                                    }
                                    aux++;
                                }
                                // inicio del if nuevo
                                if (EEigualesf == true)
                                {

                                    sendProductionVM = new SendProductionVM(IdUser, PipUser, idBundle);
                                    SendProductionViewWindowRequest.Raise(new Notification() { Content = sendProductionVM });

                                    if (sendProductionVM.errors.Count > 0 && sendProductionVM.BundleNumber == 0)
                                    {
                                        var viewConfirm = new ModalDialogActionViewModel("Error: " + sendProductionVM.errors.Values.FirstOrDefault().ToString(), "Envio de Atado", ModalType.Error);
                                        ModalDialogViewWindowRequest.Raise(new Notification() { Content = viewConfirm, Title = "Envio de Atado" });
                                    }
                                    else
                                    {
                                        if (sendProductionVM.BundleNumber > 0)
                                        {
                                            var viewConfirm = new ModalDialogActionViewModel("Atado Enviado , Nuevo Atado : " + sendProductionVM.BundleNumber.ToString(), "Envio de Atado", ModalType.Information);
                                            bundle.Send(sendProductionVM.BundleNumber);
                                            ICradle icradle = ExitModel.Instance.ExitAdapter.GetCradles().ToList().FirstOrDefault(x => x.Id.Equals(idCradle));
                                            HistoricRecord = ExitLibrary.Instance.FillData(icradle, HistoricBundles);
                                            RaisePropertyChanged("HistoricRecord");
                                            ModalDialogViewWindowRequest.Raise(new Notification() { Content = viewConfirm, Title = "Envio de Atado" });
                                        }

                                    }
                                    //Seteo del BundleNumber
                                    sendProductionVM.BundleNumber = 0;

                                    item = null;
                                    // ----- fin del if nuevo
                                }
                                else
                                {
                                    var viewWarning = new ModalDialogActionViewModel("El Atado tiene diferentes EE, favor de verificar", "Envio de Atado", ModalType.Error);
                                    ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Envio de Atado" });
                                }
                            }
                            else
                            {
                                var viewWarning = new ModalDialogActionViewModel("El Atado no puede enviarse porque ya fue enviado", "Envio de Atado", ModalType.Error);
                                ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Envio de Atado" });
                            }
                        }
                    }
                    ExitLibrary.Instance.ClearRecord();
                }
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }

        /// <summary>
        /// Insert a Bundle 
        /// </summary>
        public void InsertBundle()
        {
            if (selectedIndexTab == 1)
            {
                try
                {
                    
                    Record item = null;
                    List<IBundle> currentBundles = new List<IBundle>();
                    item = SelectedRecord;
                    int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                    //int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());

                    CradleViewModel selectedCradel = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(idCradle));
                    //CradleViewModel selectedCradel = Cradles.FirstOrDefault(c => c.IsSelected == true);
                    if (selectedCradel != null)
                    {
                        int idCurrentBatch = 0;
                        if (selectedCradel.Cradle.ProductData != null)
                        {
                            idCurrentBatch = selectedCradel.Cradle.ProductData.IdBatch;
                            List<IItem> items = ExitModel.Instance.Data.GetAvailableItems(idCurrentBatch, idCradle).ToList();
                            bundleViewModel = new BundleViewModel(items, null, IdUser);
                            InsertBundleViewWindowRequest.Raise(new Notification() { Content = bundleViewModel });
                            if (bundleViewModel.UpdCradle)
                            {
                                //currentBundles = getInformationBundle(idCurrentBatch, idCradle);
                                //ExitModel.Instance.CurrentBundles.Remove(idCradle);
                                //ExitModel.Instance.CurrentBundles.Add(idCradle, currentBundles);
                                //UpdateRecord(selectedCradel.Cradle);
                                ExitModel.Instance.ExitAdapter.UpdateViews(selectedCradel.Cradle.Id);
                            }
                            bundleViewModel = null;
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Trace.Error(ex.Message + "---> Insert Bundle Gestion");
                }
            }
            else
            {
                //Historico
                //CradleViewModel selectedCradel = Cradles.FirstOrDefault(c => c.IsSelected == true);
                try
                {
                    if (SelectedHRecord != null)
                    {
                        Record item = null;
                        item = SelectedHRecord;
                        int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                        //Modificar
                        int idCradle = 1;
                        //int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());

                        int idCurrentBatch = 0;
                        int orden = 0;
                        int colada = 0;
                        //if (selectedCradel.Cradle.ProductData != null)
                        //{
                        IBundle bundle = HistoricBundles.FirstOrDefault(hb => hb.IdBundle == idBundle);
                        orden = Convert.ToInt32(bundle.ExtraData["OrderNumber"]);
                        colada = Convert.ToInt32(bundle.ExtraData["HeatNumber"]);
                        idCurrentBatch = bundle.IdBatch;
                        List<IItem> items = ExitModel.Instance.Data.GetAvailableItemsByProduct(orden, colada).ToList();
                        bundleViewModel = new BundleViewModel(items, null, IdUser);
                        InsertBundleViewWindowRequest.Raise(new Notification() { Content = bundleViewModel });
                        if (bundleViewModel.UpdCradle)
                        {
                            //ExitModel.Instance.UpdateCradle(selectedCradel.Cradle);
                            //Update(selectedCradel.Cradle);
                            this.InitializeData();
                            //ExitModel.Instance.ExitAdapter.UpdateViews(selectedCradel.Cradle.Id);
                        }
                        bundleViewModel = null;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Trace.Error(ex.Message + "---> Insert Bundle Historico");
                }
            }
        }
        /// <summary>
        /// Edit Bundle
        /// </summary>
        public void EditBundle()
        {          
            try
            {
                IBundle bundle = null;

                if (selectedIndexTab == 1) // OnLine
                {
                    itemIndex = BundleIndex;
                    Record item = SelectedRecord;
                    int idCradle = Convert.ToInt32(item["IdCradle"].Value);
                    CradleViewModel ic = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(idCradle));

                    CurrentBundles[itemIndex]["Status"].Value = BundleStatus.Pending;
                    RaisePropertyChanged("CurrentBundles");


                    int idBundle = Convert.ToInt32(item["IdBundle"].Value);
                    List<IBundle> currentBundles = new List<IBundle>();

                    //ExitModel.Instance.CurrentBundles.TryGetValue(idCradle, out currentBundles);
                    //     if (currentBundles != null)
                    //     {
                    //         bundle = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                    //         bundle.Close();
                    //     }

                    var viewModel = new ConfirmationViewModel("Confirma que desea cerrar el Atado " + idBundle.ToString() + "?");
                    ConfirmApplyRequest.Raise(new Confirmation { Content = viewModel, Title = "Cerrar Atado" },
                    cb =>
                    {
                        if (cb.Confirmed)
                        {
                                lock (lockShared)
                                {
                                    ExitModel.Instance.ExitAdapter.CloseBundle(ic.Cradle);
                                    currentBundles.Remove(bundle);
                                    //CurrentBundles.Clear();
                                    //ExitModel.Instance.ExitAdapter.UpdateViews(ic.Cradle.Id);
                                    //UpdateRecord(ic.Cradle);
                                    //RaisePropertyChanged("CurrentBundles");
                                    
                                    
                                }

                        }
                    });
                    viewModel = null;
                    bundle = null;

                }
                else
                {
                    //Historico
                    //CradleViewModel selectedCradel = Cradles.FirstOrDefault(c => c.IsSelected == true);
                    //if (selectedCradel != null)
                    //{
                        if (SelectedRecord != null)
                        {
                            Record item = null;
                            itemIndex = BundleIndex;
                            item = SelectedHRecord;
                            ExitLibrary.Instance.ClearRecord();
                            int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                            int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                             bundle = HistoricBundles.FirstOrDefault(hb => hb.IdBundle == idBundle);

                            
                            if (bundle.Status != BundleStatus.Sent)
                            {
                                int idBatch = bundle.IdBatch;
                                bundleEditViewModel = new BundleEditViewModel(bundle);
                                EditBundleViewWindowRequest.Raise(new Notification() { Content = bundleEditViewModel });
                                if (bundleEditViewModel.UpdBundle)
                                {
                                    //Actualizo
                                    ICradle icradle = ExitModel.Instance.ExitAdapter.GetCradles().ToList().FirstOrDefault(x => x.Id.Equals(idCradle));
                                    HistoricRecord = ExitLibrary.Instance.FillData(icradle, HistoricBundles);
                                    RaisePropertyChanged("HistoricRecord");
                                    RaisePropertyChanged("RejectedBundles");
                                    RaisePropertyChanged("SelectedHTrackings");
                                }
                                bundleEditViewModel = null;


                            }
                            else
                            {
                                var viewWarning = new ModalDialogActionViewModel("El Atado no puede editarse porque ya fue enviado", "Edición de Atado", ModalType.Error);
                                ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Edición de Atado" });
                            }
                        }
                        ExitLibrary.Instance.ClearRecord();
                    //}
                }
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }



        }

        /// <summary>
        /// Delete a Selected Bundle
        /// </summary>
        public void DeleteBundle()
        {
             int result;
             try
             {
                 if (SelectedIndexTab == 1) // OnLine
                 {
                     if (SelectedRecord != null)
                     {
                         itemIndex = BundleIndex;
                         Record item = SelectedRecord;
                         int idCradle = Convert.ToInt32(item["IdCradle"].Value);
                         int idBundle = Convert.ToInt32(item["IdBundle"].Value);
                         List<IBundle> currentBundles = new List<IBundle>();
                         ExitModel.Instance.CurrentBundles.TryGetValue(idCradle, out currentBundles);
                         if (currentBundles != null)
                         {
                             IBundle bundle = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                             if (bundle.Status != BundleStatus.Sent)
                             {
                                 var viewModel = new ConfirmationViewModel("Confirma que desea eliminar el Atado " + idBundle.ToString() + "?");
                                 ConfirmApplyRequest.Raise(new Confirmation { Content = viewModel, Title = "Eliminar Registro" },
                                 cb =>
                                 {
                                     if (cb.Confirmed)
                                     {
                                         result = ExitModel.Instance.Data.DelBundle(Convert.ToInt32(item["IdBundle"].Value.ToString()));
                                         if (result > 0)
                                         {
                                             lock (lockShared)
                                             {
                                                 //currentBundles.Remove(bundle);
                                                 //ExitModel.Instance.CurrentBundles.Remove(idCradle);
                                                 //ExitModel.Instance.CurrentBundles.Add(idCradle, currentBundles);
                                                 //ICradle cradle = ExitModel.Instance.ExitAdapter.GetCradles().FirstOrDefault(c => c.Id == idCradle);
                                                 BundleIndex = (currentBundles.Count > 0) ? itemIndex - 1 : 0;
                                                 SelectedRecord = BundleIndex > 0 ? CurrentBundles[BundleIndex] : CurrentBundles[itemIndex];
                                    
                                                 RaisePropertyChanged("SelectedRecord");
                                                 RaisePropertyChanged("BundleIndex");
                                                 //UpdateRecord(cradle);
                                                 ExitModel.Instance.ExitAdapter.UpdateViews(idCradle);
                                             }
                                         }
                                     }
                                 });
                                 viewModel = null;
                                 item = null;
                             }
                             else
                             {
                                 var viewWarning = new ModalDialogActionViewModel("El Atado no puede borrarse porque ya fue enviado", "Borrado de Atado", ModalType.Error);
                                 ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Borrado de Atado" });
                             }
                         }
                     }
                 }
                 else
                 {
                     if (SelectedHRecord != null)
                     {
                         itemIndex = BundleHIndex;
                         Record item = SelectedHRecord;
                         ExitLibrary.Instance.ClearRecord();
                         int idCradle = Convert.ToInt32(item["IdCradle"].Value);
                         int idBundle = Convert.ToInt32(item["IdBundle"].Value);
                         if (HistoricRecord.Count > 0)
                         {
                             IBundle bundle = HistoricBundles.FirstOrDefault(hb => hb.IdBundle == idBundle);
                             if (bundle.Status != BundleStatus.Sent)
                             {
                                 var viewModel = new ConfirmationViewModel("Confirma que desea eliminar el Atado " + idBundle.ToString() + "?");
                                 ConfirmApplyRequest.Raise(new Confirmation { Content = viewModel, Title = "Eliminar Registro" },
                                 cb =>
                                 {
                                     if (cb.Confirmed)
                                     {
                                         result = ExitModel.Instance.Data.DelBundle(Convert.ToInt32(item["IdBundle"].Value.ToString()));
                                         if (result > 0)
                                         {
                                            ICradle icradle = ExitModel.Instance.ExitAdapter.GetCradles().ToList().FirstOrDefault(x => x.Id.Equals(idCradle));
                                            HistoricBundles.Remove(bundle);
                                            HistoricRecord = ExitLibrary.Instance.FillData(icradle, HistoricBundles);
                                            //ExitModel.Instance.ExitAdapter.UpdateViews(idCradle);
                                            RaisePropertyChanged("HistoricRecord");      
                                            BundleHIndex = (HistoricRecord.Count > 0) ? itemIndex - 1 : 0;                                                                                                                                       
                                         }
                                     }
                                 });
                                 viewModel = null;
                                 item = null;
                             }
                             else
                             {
                                 var viewWarning = new ModalDialogActionViewModel("El Atado no puede borrarse porque ya fue enviado", "Borrado de Atado", ModalType.Error);
                                 ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Borrado de Atado" });
                             }
                         }


                     }
                     ExitLibrary.Instance.ClearRecord();
                 }
             }
             catch (Exception ex)
             {
                 Trace.Error(ex.Message);
             }


        }

        /// <summary>
        /// Insert Items into Selected Bundle
        /// </summary>
        public void InsertPipe()
        {             
              try
              {
                  if (selectedIndexTab == 1) // OnLine
                  {
                      if (SelectedRecord != null)
                      {
                          itemIndex = BundleIndex;
                          Record item = SelectedRecord;
                          int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                          int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                          List<IBundle> currentBundles = ExitModel.Instance.GetBundlesbyCradle(idCradle);
                          IBundle bundle = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                          if (bundle.Status != BundleStatus.Sent)
                          {
                              int idBatch = bundle.IdBatch;
                              ICradle cradle = Cradles.FirstOrDefault(c => c.Cradle.Id == idCradle).Cradle;
                              List<IItem> items = ExitModel.Instance.Data.GetAvailableItems(idBatch, idCradle).ToList();
                              List<IItem> availableItems = items.Where(i => i.IdBatch == idBatch
                                  && Convert.ToInt32(i.ExtraData["HeatNumber"]) == Convert.ToInt32(bundle.ExtraData["HeatNumber"])
                                  && i.ExtraData["EE"].ToString().Equals(bundle.ExtraData["EE"].ToString()) 
                                  ).ToList();
                              // Obtener los tubos disponibles para ese Batch y esa colada. 

                              //Verificar si el cradle al que le inserto es el activo
                              if (cradle.CurrentBundle.Status == BundleStatus.InProcess)
                              {
                                  Trace.Debug("Es el activo");
                              }
                     
                              bundleViewModel = new BundleViewModel(availableItems, bundle, IdUser);
                              InsertBundleViewWindowRequest.Raise(new Notification() { Content = bundleViewModel });
                              if (bundleViewModel.UpdBundle)
                              {

                                  ExitModel.Instance.ExitAdapter.UpdateViews(idCradle);
                                  //UpdateRecord(cradle);
                              }
                              bundleViewModel = null;
                          }
                          else
                          {
                              var viewWarning = new ModalDialogActionViewModel("El Atado no puede modificarse porque ya fue enviado", "Insertar Tubos", ModalType.Error);
                              ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Insertar Tubos" });
                          }
                      }
                  }
                  else
                  {
                      //Historico
                      if (SelectedHRecord != null)
                      {
                          Record item = null;  
                          itemIndex = BundleIndex;
                          item = SelectedHRecord;
                          ExitLibrary.Instance.ClearRecord();
                          int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                          int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                          int orden = 0;
                          int colada = 0;

                          IBundle bundle = HistoricBundles.FirstOrDefault(hb => hb.IdBundle == idBundle);
                          orden = Convert.ToInt32(bundle.ExtraData["OrderNumber"]);
                          colada = Convert.ToInt32(bundle.ExtraData["HeatNumber"]);
                          if (bundle.Status != BundleStatus.Sent)
                          {
                              int idBatch = bundle.IdBatch;
                              ICradle cradle = Cradles.FirstOrDefault(c => c.Cradle.Id == idCradle).Cradle;
                              //List<IItem> items = ExitModel.Instance.Data.GetAvailableItems(idBatch, idCradle).ToList();
                              List<IItem> items = ExitModel.Instance.Data.GetAvailableItemsByProduct(orden, colada).ToList();
                              List<IItem> availableItems = items.Where(i => i.IdBatch == idBatch
                                  && Convert.ToInt32(i.ExtraData["HeatNumber"]) == Convert.ToInt32(bundle.ExtraData["HeatNumber"])
                                  && i.ExtraData["EE"].ToString().Equals(bundle.ExtraData["EE"].ToString()) 
                                  ).ToList();
                              // Obtener los tubos disponibles para ese Batch y esa colada.                      
                              bundleViewModel = new BundleViewModel(availableItems, bundle, IdUser);
                              InsertBundleViewWindowRequest.Raise(new Notification() { Content = bundleViewModel });
                              if (bundleViewModel.UpdBundle)
                              {
                                  //Entra
                                  //Update(cradel);
                                  ICradle icradle = ExitModel.Instance.ExitAdapter.GetCradles().ToList().FirstOrDefault(x => x.Id.Equals(idCradle));
                                  HistoricRecord = ExitLibrary.Instance.FillData(icradle, HistoricBundles);
                                  RaisePropertyChanged("HistoricRecord");
                                  RaisePropertyChanged("RejectedBundles");
                                  RaisePropertyChanged("SelectedHTrackings");
                                  
                              }
                              bundleViewModel = null;
                          }
                          else
                          {
                              var viewWarning = new ModalDialogActionViewModel("El Atado no puede modificarse porque ya fue enviado", "Insertar Tubos", ModalType.Error);
                              ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Insertar Tubos" });
                          }
                      }
                      ExitLibrary.Instance.ClearRecord();

                  }
              }
              catch (Exception ex)
              {
                  Trace.Error(ex.Message);
              }

        }


        /// <summary>
        /// Delete Items from Selected Bundle
        /// </summary>
        public void DeletePipe()
        {
            
            if (selectedIndexTab == 1) // OnLine
            {
                if (SelectedRecord != null)
                {
                    itemIndex = BundleIndex;
                    Record item = SelectedRecord;
                    int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                    int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                    List<IBundle> currentBundles = ExitModel.Instance.GetBundlesbyCradle(idCradle);
                    IBundle bundle = currentBundles.FirstOrDefault(b => b.IdBundle == idBundle);
                    if (SelectedTrackings.Where(x => x.IsSelected).Count() > 0)
                    {
                        List<IItem> SelectedToDelete = SelectedTrackings.Where(x => x.IsSelected).ToList();
                        if (bundle.Status != BundleStatus.Sent)
                        {
                            foreach (IItem itemToDelete in SelectedToDelete)
                            {
                                if (ExitModel.Instance.Data.DelTrackingBundle(itemToDelete.Id) > 0)
                                {
                                    bundle.RemoveItem(itemToDelete);
                                }
                            }
                            if (SelectedToDelete.Count > 0)
                            {
                                ICradle cradel = ExitModel.Instance.ExitAdapter.GetCradles().FirstOrDefault(c => c.Id == idCradle);
                                if (bundle.Items.Count() == 0)
                                {
                                    lock (lockShared)
                                    {
                                        //currentBundles.Remove(bundle);
                                        //ExitModel.Instance.CurrentBundles.Remove(idCradle);
                                        //ExitModel.Instance.CurrentBundles.Add(idCradle, currentBundles);
                                        //BundleIndex = (currentBundles.Count > 0) ? itemIndex - 1 : 0;
                                        //UpdateRecord(cradel);
                                        ExitModel.Instance.ExitAdapter.UpdateViews(idCradle);
                                    }
                                }
                                else
                                {
                                    //UpdateRecord(cradel);
                                    ExitModel.Instance.ExitAdapter.UpdateViews(idCradle);
                                }
                            }
                        }
                        else
                        {
                            var viewWarning = new ModalDialogActionViewModel("El Atado no puede modificarse porque ya fue enviado", "Borrar Tubos", ModalType.Error);
                            ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Borrar Tubos" });
                        }
                    }
                }
            }
            else
            {
                //Historico
                Record item = null;  
                if (SelectedHRecord != null)
                {
                    item = SelectedHRecord;
                    itemIndex = BundleIndex;
                    ExitLibrary.Instance.ClearRecord();
                    int idCradle = Convert.ToInt32(item["IdCradle"].Value.ToString());
                    int idBundle = Convert.ToInt32(item["IdBundle"].Value.ToString());
                    if (HistoricRecord.Count > 0)
                    {
                        IBundle bundle = HistoricBundles.FirstOrDefault(hb => hb.IdBundle == idBundle);
                        if (SelectedHTrackings.Where(x => x.IsSelected).Count() > 0)
                        {
                            List<IItem> SelectedToDelete = SelectedHTrackings.Where(x => x.IsSelected).ToList();
                            if (bundle.Status != BundleStatus.Sent)
                            {
                                foreach (IItem itemToDelete in SelectedToDelete)
                                {
                                    if (ExitModel.Instance.Data.DelTrackingBundle(itemToDelete.Id) > 0)
                                    {
                                        bundle.RemoveItem(itemToDelete);
                                        
                                        
                                    }
                                    //GetBundleHIndex(SelectedHRecord);
                                }
                                if (SelectedToDelete.Count > 0)
                                {
                                    ICradle cradel = ExitModel.Instance.ExitAdapter.GetCradles().FirstOrDefault(c => c.Id == idCradle);
                                    if (bundle.Items.Count() == 0)
                                    {
                                        lock (lockShared)
                                        {
                                            HistoricBundles.Remove(bundle);
                                            ExitModel.Instance.CurrentBundles.Remove(idCradle);
                                            ExitModel.Instance.CurrentBundles.Add(idCradle, HistoricBundles);
                                            BundleIndex = (HistoricBundles.Count > 0) ? itemIndex - 1 : 0;
                                            Update(cradel);
                                        }
                                    }
                                    else
                                    {
                                        //Update(cradel);
                                        ICradle icradle = ExitModel.Instance.ExitAdapter.GetCradles().ToList().FirstOrDefault(x => x.Id.Equals(idCradle));
                                        HistoricRecord = ExitLibrary.Instance.FillData(icradle, HistoricBundles);
                                        RaisePropertyChanged("HistoricRecord"); 
                                        RaisePropertyChanged("RejectedBundles");
                                        RaisePropertyChanged("SelectedHTrackings");

                                        //SelectedHRecord = HistoricRecord[BundleIndex];
                                        //RaisePropertyChanged("BundleIndex");
                                        //int idBundleH = Convert.ToInt32(SelectedHRecord["IdBundle"].Value.ToString());
                                        //SelectedHTrackings = new ObservableCollection<IItem>(HistoricBundles.FirstOrDefault(b => b.IdBundle == idBundleH).Items);
                                        

             
                                    }
                                }
                            }
                            else
                            {
                                var viewWarning = new ModalDialogActionViewModel("El Atado no puede modificarse porque ya fue enviado", "Borrar Tubos", ModalType.Error);
                                ModalDialogViewWindowRequest.Raise(new Notification { Content = viewWarning, Title = "Borrar Tubos" });
                            }
                        }
                    }
                                
                }
                ExitLibrary.Instance.ClearRecord();
            }
        }

        #endregion
      

        #region Modal Windows

        private InteractionRequest<Notification> movePipeViewWindowRequest { get; set; }
        private InteractionRequest<Notification> insertBundleViewWindowRequest { get; set; }
        private InteractionRequest<Notification> editBundleViewWindowRequest { get; set; }
        private InteractionRequest<Notification> commentViewWindowRequest { get; set; }
        private InteractionRequest<Notification> sendProductionViewWindowRequest { get; set; }


        public InteractionRequest<Notification> CommentViewWindowRequest
        {
            get
            {
                return commentViewWindowRequest ?? (commentViewWindowRequest = new InteractionRequest<Notification>());
            }
        }

        public InteractionRequest<Notification> MovePipeViewWindowRequest
        {
            get
            {
                return movePipeViewWindowRequest ?? (movePipeViewWindowRequest = new InteractionRequest<Notification>());
            }
        }

        public InteractionRequest<Notification> InsertBundleViewWindowRequest
        {
            get
            {
                return insertBundleViewWindowRequest ?? (insertBundleViewWindowRequest = new InteractionRequest<Notification>());
            }
        }

        public InteractionRequest<Notification> EditBundleViewWindowRequest
        {
            get
            {
                return editBundleViewWindowRequest ?? (editBundleViewWindowRequest = new InteractionRequest<Notification>());
            }
        }

        public InteractionRequest<Notification> SendProductionViewWindowRequest
        {
            get
            {
                return sendProductionViewWindowRequest ?? (sendProductionViewWindowRequest = new InteractionRequest<Notification>());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// update View Model
        /// </summary>
        private void Update(ICradle cradle)
        {
            bool isSelected = false;
            itemIndex = BundleIndex;

            //CradleViewModel cvm = Cradles.FirstOrDefault(c => c.Cradle.Id == cradle.Id);
            //if (cvm != null)
            //{
            //    isSelected = cvm.IsSelected;
            //    Cradles.Remove(cvm);
            //    Cradles.Add(new CradleViewModel(cradle));
            //    Cradles.FirstOrDefault(c => c.Cradle.Id == cradle.Id).IsSelected = isSelected;
            //    RaisePropertyChanged("Cradles");
            //}


            List<IBundle> currentbundles = ExitModel.Instance.GetBundlesbyCradle(cradle.Id);
            currentbundles = currentbundles.OrderByDescending(b => b.IdBundle).ToList();
            if (currentbundles != null)
            {
                
                CurrentBundles = ExitLibrary.Instance.FillData(cradle, currentbundles);

                //CurrentBundles
                if (CurrentBundles.Count > 0)
                {
                    RaisePropertyChanged("CurrentBundles");
                    
                    //LANZAR EVENTO PARA ACTUALIZAR CANASTAS
                    CradleViewModel ic = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(cradle.Id));

                    for (int x = 0; x < CurrentBundles.Count; x++)
                    {
                        if (CurrentBundles[x].Properties.Count > 18)
                        {
                            EE = CurrentBundles[x]["EE"].Value.ToString();
                        }
                    }

                    if (ic != null)
                    {
                        ic.LoadedPieces = cradle.CurrentBundle.Items.Count();
                        if (ic.LoadedPieces == cradle.MaximumPieces)
                        {
                            cradle.State = CradleState.Disable;
                        }
                        else
                        {
                            cradle.State = CradleState.Enable;
                        }
                    }
                    ic.update();
                    
                    
                    //BundleIndex = itemIndex;


                    //SelectedRecord = BundleIndex > 0 ? CurrentBundles[BundleIndex - 1] : CurrentBundles[BundleIndex];
                    //if (BundleIndex > 0)
                    //{
                    //    SelectedRecord = CurrentBundles[BundleIndex - 1];

                    //    //int idBundle = Convert.ToInt32(SelectedRecord["IdBundle"].Value.ToString());
                    //    //SelectedTrackings = new ObservableCollection<IItem>(currentbundles.FirstOrDefault(b => b.IdBundle == idBundle).Items);
                    //    //RaisePropertyChanged("SelectedTrackings");    
                    //}
                    //else
                    //{
                    //SelectedRecord = CurrentBundles[BundleIndex];
                    ////}
                    //RaisePropertyChanged("BundleIndex");                              
                }
            }
            
           }


        //Update Mejorado
        private void UpdateRecord(ICradle cradle)
        {
            try
            {
                lock (this)
                {
                    int idSpecification = 0;
                    int heat = 0;

                    ExitLibrary.Instance.ClearRecord();
                    CradleViewModel ic = Cradles.FirstOrDefault(x => x.Cradle.Id.Equals(cradle.Id));

                    ExitModel.Instance.UpdateCradle(cradle);

                    if (ic != null)
                    {
                        ic.Cradle = cradle;
                        foreach (var item in Cradles)
                        {
                            ExitModel.Instance.CurrentSpecifications.TryGetValue(cradle.Id, out idSpecification);
                            ExitModel.Instance.CurrentHeats.TryGetValue(cradle.Id, out heat);
                            if (ExitModel.Instance.SpecAdapter != null)
                            {
                                if (idSpecification > 0)
                                {
                                    this.specificationData = ExitModel.Instance.SpecAdapter.GetSpecification(idSpecification).Data;
                                    this.productionUnitValue = ExitModel.Instance.SpecAdapter.GetProductionUnit(idSpecification).ToReadOnlyDictionary();
                                }
                            }
                            if (heat > 0)
                            {
                                this.HeatNumber = heat;
                            }
                            Update(item.Cradle);
                        }
                    }
                    ExitLibrary.Instance.ClearRecord();
                }
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message + " ---> UpdateRecord");
            }
        }


        private List<IBundle> getInformationBundle(int idCurrentBatch, int idCradle)
        {

            List<IBundle> datos = new List<IBundle>();
            List<IBundle> currentBundles = new List<IBundle>();
            ////List<IBundle> currentBundles = ExitModel.Instance.GetBundlesbyCradle(idCradle);
            //cradle = ExitModel.Instance.ExitAdapter.GetCradles();

            datos = ExitModel.Instance.Data.GetBundles(idCurrentBatch, idCradle);

            lock (this)
            {
                foreach (Bundle b in datos)
                {
                    List<IItem> trackings = new List<IItem>();
                    trackings = ExitModel.Instance.Data.GetTrackingsOnBundle(idCradle, b.IdBundle);
                    foreach (IItem t in trackings)
                    {
                        b.AddItem(t);
                        if (t.ExtraData != null)
                        {
                            if (b.ExtraData == null)
                            {
                                Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                bundleData = t.ExtraData;
                                b.ExtraData = bundleData;
                            }
                        }
                    }
                    currentBundles.Add(b);
                }

                foreach (ICradle cradle in ExitModel.Instance.ExitAdapter.GetCradles())
                {
                    if (cradle.Id == idCradle)
                    {
                        currentBundles.Add(cradle.CurrentBundle);
                    }
                }

                return currentBundles;
            }

        }
        
      
        

        #endregion

    }
}
