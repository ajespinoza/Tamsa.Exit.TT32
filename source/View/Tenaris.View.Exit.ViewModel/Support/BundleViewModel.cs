// -----------------------------------------------------------------------
// <copyright file="BundleViewModel.cs" company="Techint">
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



    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class BundleViewModel : NotificationObject, IPopupWindowActionAware
    {
        public Window HostWindow { get; set; }

        private ICommand cancel;
        private ICommand closeWindow;
        private ICommand updateBundle;
        private ICommand selectAllAvailable;

        private ICommand selectedItem;

        public ICommand SelectedItem
        {
            get
            {
                this.selectedItem = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    //ExecuteDelegate = p => Calculating((int)p)                   
                        //ExecuteDelegate = p => Calculating(0)    
                };

                return this.selectedItem;
            }
        }

        private void Calculating(int p)
        {            
            if (p >= 0)
            {
                SelectedPieces = AvailableItems.Count(i => i.IsSelected == true);
                SelectedWeight = AvailableItems.Where(i => i.IsSelected == true).Sum(i => Convert.ToSingle(i.Weight));               
                CalculateTotal();        
                RaisePropertyChanged("SelectedPieces");
                RaisePropertyChanged("SelectedWeight");
            }
        }

       

        /// <summary>
        /// Host Notification
        /// </summary>
        public Notification HostNotification { get; set; }
        public InteractionRequest<Confirmation> ConfirmApplyRequest { get; set; }
        public ObservableCollection<IItem> AvailableItems { get; set; }
        public IBundle Bundle { get; set; } // En caso de editar el atado..

        public int BundlePieces { get; set; }
        public float BundleWeight { get; set; }
        public int SelectedPieces { get; set; }
        public float SelectedWeight { get; set; }

        public int TotalPieces { get; set; }
        public float TotalWeight { get; set; }
        public int ItemIndex { get; set; }

        public int IdUser { get; set; }

        public bool UpdCradle { get; set; }
        public bool UpdBundle { get; set; }
        public bool IsCheckedAllAvailable { get; set; }
                
        private List<IItem> Items = new List<IItem>();
         

        #region Constructor

        public BundleViewModel(List<IItem> items, IBundle bundle, int idUser)
        {
            UpdCradle = false;
            UpdBundle = false;
            IdUser = idUser;
            AvailableItems = new ObservableCollection<IItem>(items);            
            if (bundle != null)
            {
                Bundle = bundle;
                BundlePieces = bundle.Items.Count();
                BundleWeight = bundle.Weight;
                SelectedPieces = 0;
                SelectedWeight = 0;
                CalculateTotal();
            }
            else
            {
                BundlePieces = 0;
                BundleWeight = 0;
                SelectedPieces = 0;
                SelectedWeight = 0;
                CalculateTotal();                
            }
            RaisePropertyChanged("BundlePieces");
            RaisePropertyChanged("AvailableItems");
        }

        #endregion

        private void CalculateTotal()
        {
            TotalPieces = BundlePieces + SelectedPieces;
            TotalWeight = Convert.ToSingle(Math.Round(BundleWeight + SelectedWeight, 0));
            RaisePropertyChanged("TotalPieces");
            RaisePropertyChanged("TotalWeight");
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
                    ExecuteDelegate = p => SelectedPiecesClear()
                };

                return this.cancel;
            }
        }

        public ICommand SelectAllAvailable
        {
            get
            {
                this.selectAllAvailable = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => SelectAllAvailableItems()
                };

                return this.selectAllAvailable;
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

        public ICommand CommandUpdBundle
        {
            get
            {
                this.updateBundle = new CommandVM()
                {
                    CanExecuteDelegate = p => true,
                    ExecuteDelegate = p => CreateBundles()

                };

                return this.updateBundle;
            }
        }


        public void SelectAllAvailableItems()
        {
            List<IItem> Availableitem = new List<IItem>();
            foreach (var item in AvailableItems)
            {
                IItem ii = item;
                ii.IsSelected = IsCheckedAllAvailable;
                Availableitem.Add(ii);

            }

            AvailableItems = new ObservableCollection<IItem>(Availableitem);
            RaisePropertyChanged("AvailableItems");

        }


        private void CreateBundles()
        {          
            if (Bundle == null)
            {
                List<IItem> itemSelected = AvailableItems.Where(i => i.IsSelected == true).ToList();
                int idCradle = 0;
                int idBatch = 0;
                var groups = itemSelected.GroupBy(x => new
                {
                    x.IdCradle,
                    x.IdBatch,
                    HeatNumber = Convert.ToInt32(x.ExtraData["HeatNumber"]),
                    IdItemStatus = Convert.ToInt32(x.ExtraData["idItemStatus"])
                }).Select(group => new { group.Key });
                foreach (var group in groups)
                {
                    idCradle = group.Key.IdCradle;
                    idBatch = group.Key.IdBatch;                                          
                    int idBundle = ExitModel.Instance.Data.InsBundle(group.Key.IdCradle,
                         group.Key.IdBatch, group.Key.IdItemStatus, this.IdUser);
                    if (idBundle > 0)
                    {
                        var items = itemSelected.Select(i => new
                        {
                            i.Id,
                            i.IdCradle,
                            i.IdBatch,
                            HeatNumber = Convert.ToInt32(i.ExtraData["HeatNumber"]),
                            IdItemStatus = Convert.ToInt32(i.ExtraData["idItemStatus"])
                        }).Select(item => new { item }).Where(it => it.item.IdCradle == group.Key.IdCradle && it.item.IdBatch == group.Key.IdBatch && it.item.IdItemStatus == group.Key.IdItemStatus && it.item.HeatNumber == group.Key.HeatNumber);
                        foreach (var it in items)
                        {
                            ExitModel.Instance.Data.InsItem(idBundle, it.item.Id);
                        }                        
                    }
                    UpdCradle = true;
                }                
            }
            else
            {
                List<IBundle> currentBundles = ExitModel.Instance.GetBundlesbyCradle(Bundle.IdCradle);
                currentBundles.Remove(Bundle);
                List<IItem> itemSelected = AvailableItems.Where(i => i.IsSelected == true).ToList();
                foreach (var it in itemSelected)
                {
                    if (ExitModel.Instance.Data.InsItem(Bundle.IdBundle, it.Id) >0)
                    {
                        Bundle.AddItem(it);
                    }
                }
                currentBundles.Add(Bundle);
                //lock (this)
                //{

                //    ExitModel.Instance.CurrentBundles.Remove(Bundle.IdCradle);
                //    ExitModel.Instance.CurrentBundles.Add(Bundle.IdCradle, currentBundles);
                //}
                UpdBundle = true;
            }
            CloseWindows(this);
        }

        private void SelectedPiecesClear()
        {

            Items.Clear();

            foreach (var item in AvailableItems)
            {
                IItem ii = item;
                ii.IsSelected = false;
                Items.Add(ii);

            }

            AvailableItems = new ObservableCollection<IItem>(this.Items);

            BundlePieces = 0;
            BundleWeight = 0;
            SelectedPieces = 0;
            SelectedWeight = 0;

            TotalPieces = 0;
            TotalWeight = 0;

            RaisePropertyChanged("TotalPieces");
            RaisePropertyChanged("TotalWeight");
            RaisePropertyChanged("SelectedPieces");
            RaisePropertyChanged("SelectedWeight");
            RaisePropertyChanged("AvailableItems");
        }

        #endregion




    }
}
