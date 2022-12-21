// -----------------------------------------------------------------------
// <copyright file="TrackingAdapter.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.Log;
    using Tenaris.Library.Framework.Factory;
    using Tenaris.Manager.Tracking.Shared;
    using Tenaris.Library.ConnectionMonitor;
    using System.Threading;
    using Tamsa.Manager.Exit.Shared;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class TrackingAdapter
    {

        #region "Constants"

        const string ConfigSection = "Tenaris.Manager.Tracking.TrackingManager";

        #endregion

        #region "Singleton"

        // singleton reference to generic manager
        static TrackingAdapter sharedManager = null;
        static object lockSharedManager = new object();

                  
        public static TrackingAdapter Instance
        {
            get
            {
                lock (lockSharedManager)
                {
                    if (sharedManager == null)
                    {
                        Trace.Message("Creating Tracking Manager instance");
                        sharedManager = new TrackingAdapter();
                    }
                } // lock

                return sharedManager;
            }
        }

        #endregion

        #region Variables

        private static ITrackingManager trackingManager;  

        /// <summary>
        /// The zoneChanged locker object.
        /// </summary>
        private readonly object zoneChangedLocker = new object();

        /// <summary>
        /// Is tracking manager connected.
        /// </summary>
        private bool isTrackingManagerConnected = false;
              

        /// <summary>
        /// Utility synchronization locker for connection monitor state changed events
        /// </summary>
        private readonly object connectionMonitorStateChangedLocker = new object();


        /// <summary>
        /// Utility locker for synchronization
        /// </summary>
        private readonly object stateChangeLocker = new object();

     
        /// <summary>
        /// Zones.
        /// </summary>
        private Dictionary<string, List<Tracking>> zones;

        /// <summary>
        /// Sudscriptions to tracking manager.
        /// </summary>
        private Dictionary<string, ZoneSubscription> subscriptions = new Dictionary<string, ZoneSubscription>();



        #endregion

        #region Constructor 
        public TrackingAdapter()
        {

        }

        public void Initialize()
        {
            try
            {
                ConnectionMonitor.Instance.StateChanged += ConnectionMonitorStateChanged;
                IFactory<ITrackingManager> trkFactory = FactoryProvider.Instance.CreateFactory<ITrackingManager>(ConfigSection);
                trackingManager = trkFactory.Create();
              
            }
            catch (Exception ex)
            {
                Trace.Error("Error Message: {0}", ex.Message);
                Trace.Error("Error StackTrace: {0}", ex.StackTrace);
            }
        }

        public void Deactivate()
        {
            try
            {
                trackingManager = null;
                subscriptions = null;
                zones = null;
                ConnectionMonitor.Instance.StateChanged -= ConnectionMonitorStateChanged;
                ConnectionMonitor.Instance.Stop();
                ConnectionMonitor.Instance.Dispose();
                Trace.Warning("Managers Deactivates");
            }
            catch (Exception ex)
            {
                Trace.Error("Managers is Down: {0}", ex.Message);
            }

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Connection monitor state changed
        /// </summary>
        /// <param name="sender">
        /// The sender object.
        /// </param>
        /// <param name="e">
        /// The connection monitor changed event.
        /// </param>
        private void ConnectionMonitorStateChanged(object sender, Tenaris.Library.ConnectionMonitor.StateChangeEventArgs e)
        {          
            
            lock (this.connectionMonitorStateChangedLocker)
            {
                try
                {
                    if (e.IsConnected)
                    {
                        if (e.Connection is ITrackingManager)
                        {
                            this.isTrackingManagerConnected = true;                           
                                try
                                {
                                    this.SubscribeZones();
                                }
                                catch (Exception ex)
                                {
                                    Trace.Exception(ex, "Exception on Suscription to tracking manager zones!", ex.Message);
                                }
                          

                        }
                    }
                    else
                    {
                        if (e.Connection is ITrackingManager)
                        {
                            this.isTrackingManagerConnected = false;

                                try
                                {
                                    this.UnSubscribeZones();
                                }
                                catch (Exception ex)
                                {
                                    Trace.Exception(ex, "Exception on Unsuscription to tracking manager zones!", ex.Message);
                                }                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.Exception(ex, "Exception on connection monitor callback: {0}", ex.Message);
                }
            }
        }

        /// <summary>
        /// Subscribe zones.
        /// </summary>
        internal void SubscribeZones()
        {
            this.zones = new Dictionary<string, List<Tracking>>();
            // Set zones code.
            try
            {
                foreach (var cradle in ExitManager.Instance.Cradles)
                {
                    var zone = trackingManager.Zones.FirstOrDefault(c => c.Code == cradle.TrkZone);

                    if (!this.subscriptions.ContainsKey(cradle.TrkZone))
                    {
                        this.subscriptions.Add(cradle.TrkZone, new ZoneSubscription());
                        var machineSubscription = this.subscriptions[cradle.TrkZone];
                        zone.ZoneChanged -= machineSubscription.ZoneChangedSent.Handler;
                        zone.ZoneChanged += machineSubscription.ZoneChangedSent.Handler;
                        machineSubscription.ZoneChangedSent.Event += this.OnZoneChanged;
                        this.zones.Add(cradle.TrkZone, zone.Tracking.ToList());
                        Trace.Message("     Suscription to Tracking zone: {0} Ready!", cradle.TrkZone);
                    }

                }

            }
            catch (Exception ex)
            {
                Trace.Message("Error SubcribeZones:" + ex.Message);
            }
        }

        /// <summary>
        /// UnSuscribeZones
        /// </summary>
        internal void UnSubscribeZones()
        {
            try
            {
                foreach (var z in this.zones)
                {
                    // Subscribe zone changed event
                    if (this.subscriptions.ContainsKey(z.Key))
                    {                      
                        var machineSubscription = this.subscriptions[z.Key];
                        this.subscriptions.Remove(z.Key);
                        machineSubscription.ZoneChangedSent.Event -= this.OnZoneChanged;                       
                        Trace.Message(" Desuscription to Tracking zone: {0} Ready!", z.Key);                        
                    }
                }
                this.zones.Clear();
            }
            catch (Exception ex)
            {
                Trace.Message("Error UnSubcribeZones:" + ex.Message);
            }
        }


        #endregion

        

        /// <summary>
        /// On zone changed.
        /// </summary>
        /// <param name="sender">
        /// Sender.
        /// </param>
        /// <param name="e">
        /// ZoneChange Event args.
        /// </param>
        internal void OnZoneChanged(object sender, ZoneChangedEventArgs e)
        {
            lock (this.zoneChangedLocker)
            {
                try
                {
                    if (sender != null)
                    {
                        Trace.Message("OnZoneChanged received");
                        // Get code zone.
                        string codeZone = ((Tenaris.Manager.Tracking.Shared.IZone)sender).Code;
                        Trace.Message("CodeZone: {0}", codeZone);
                        
                        // Get local copie from trk manager.
                        IEnumerable<Tenaris.Manager.Tracking.Shared.Tracking> trks = ((Tenaris.Manager.Tracking.Shared.IZone)sender).Tracking;

                        // Compare remote and internal list for to detect if the zone change is for a new tracking.
                        var newTrackings = trks.Except(this.zones[codeZone], new Compare()).ToList();

                        // If exists new trackings, then laucher a cradle event.
                        foreach (var t in newTrackings)
                        {
                            // Launcher cradle event.
                            Trace.Message("New tracking was found: {0}", t.Id);
                            this.OnTrackingAtZoneChanged(this, t);
                        }

                        // Update internal list with the remote list.
                        this.zones[codeZone] = trks.ToList();
                    }
                }
                catch (Exception ex)
                {
                    Trace.Error("Error in the on tracking pass machine changed : {0}", ex.Message);
                }
            }
        }

        /// <summary>
        /// On tracking at zone changed.
        /// </summary>
        /// <param name="sender">
        /// Sender.
        /// </param>
        /// <param name="e">
        /// Tracking object.
        /// </param>
        internal void OnTrackingAtZoneChanged(object sender, Tracking e)
        {
            try
            {
                Trace.Message("OnTrackingAtZoneChanged received");
                var mach = Library.Instance.Cradles.Values.Where(c => c.IdZone == e.IdZone).Single();
                ICradle cradle = Library.Instance.Cradles.Values.FirstOrDefault(p => p.Code == mach.Code);
                OnCradleChanged(cradle, e);
            }
            catch (Exception ex)
            {
                Trace.Error("Error in the on tracking pass machine changed : {0}", ex.Message);
            }
        }


        /// <summary>
        /// On cradle changed method.
        /// </summary>
        /// <param name="t">
        /// Tracking object.
        /// </param>
        internal void OnCradleChanged(ICradle cradle, Tracking t)
        {
            try
            {
                Trace.Message("OnCradleChanged received");
                if (cradle.PreviousItem != null)
                {
                    #region Modo Automatico
                    if (cradle.Mode == CradleMode.Automatic)
                    {
                        if (t.Id != cradle.PreviousItem.Id) // Verifica no procesar el mismo IdTracking
                        {
                            IProductData productData = null;
                            Item i = ExitManager.Instance.dataAccess.GetItemData(cradle.Id, t.Id, ref productData);
                            if (productData != null)
                            {
                                if (cradle.ProductData != null)
                                {
                                    #region Pieza en Linea
                                    if (cradle.ProductData.Equals(productData))
                                    {
                                        lock (this.stateChangeLocker)
                                        {
                                            Trace.Message("Estado del atado: {0}, Piezas cargadas: {1}, Piezas Maximas: {2} DB PreviousItem: {3}, Cradle PreviousItem: {4}", cradle.CurrentBundle.State, cradle.CurrentBundle.Items.ToList().Count, cradle.MaximumPieces, i.Id, cradle.PreviousItem.Id);

                                            if (cradle.CurrentBundle.State == BundleState.Open &&
                                                cradle.CurrentBundle.Items.ToList().Count < cradle.MaximumPieces) //&&
                                                //i.Equals(cradle.PreviousItem))
                                            {
                                                Trace.Message("CreateTrackingBundle");
                                                if (ExitManager.Instance.dataAccess.CreateTrackingBundle(cradle.CurrentBundle.IdBundle, t.Id, i.Weight))
                                                {
                                                    // Create new tracking on bundle (memory).
                                                    cradle.CurrentBundle.AddItem(i);
                                                    cradle.PreviousItem = i;
                                                    Trace.Message("Processed TrackingId: {0}, ItemStatusId: {1}, with BatchId: {2}, Add to BundleId: {3}, Accumulated Pieces: {4}, Maximum Pieces: {5}, Operation Mode: {6} on Cradle: {7}", t.Id, t.Status, t.IdProductionHistory, cradle.CurrentBundle.IdBundle, cradle.CurrentBundle.Items.ToList().Count, cradle.MaximumPieces, cradle.Mode, cradle.Code);
                                                }
                                                // Close bundle by max capacity.
                                                if (cradle.CurrentBundle.Items.ToList().Count == cradle.MaximumPieces)
                                                {
                                                    // Close current bundle.
                                                    if (ExitManager.Instance.dataAccess.CloseBundle(cradle.CurrentBundle.IdBundle))
                                                    {
                                                        cradle.CurrentBundle.Close();
                                                        Trace.Message("Close BundleId: {0}, ItemStatusId: {1}, with BatchId: {2}, Total Pieces: {3}, Maximum Pieces: {4}, Operation Mode: {5} on Cradle: {6}", cradle.CurrentBundle.IdBundle, t.Status, cradle.CurrentBundle.IdBatch, cradle.CurrentBundle.Items.ToList().Count, cradle.MaximumPieces, cradle.Mode, cradle.Code);
                                                        cradle.PreviousItem = null; // Para evitar un error en la apertura del siguiente atado.
                                                    }
                                                }
                                                Trace.Message("Pieza en Linea");
                                                Library.Instance.Cradles.Remove(cradle.Id);
                                                Library.Instance.Cradles.Add(cradle.Id, cradle);
                                                ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                                            }
                                            else
                                            {
                                                Trace.Message("Estado Cerrado/Piezas arriba del limite/Previous Item diferente");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Close bundle by change batch.
                                        Trace.Message("Processed TrackingId: {0}, ItemStatusId: {1}, with BatchId: {2}", t.Id, t.Status, t.IdProductionHistory);
                                        if (ExitManager.Instance.dataAccess.CloseBundle(cradle.CurrentBundle.IdBundle))
                                        {
                                            cradle.CurrentBundle.Close();
                                            Trace.Message("Close BundleId: {0}, ItemStatusId: {1}, with BatchId: {2}, Total Pieces: {3}, Maximum Pieces: {4}, Operation Mode: {5} on Cradle: {6}", cradle.CurrentBundle.IdBundle, t.Status, cradle.CurrentBundle.IdBatch, cradle.CurrentBundle.Items.ToList().Count, cradle.MaximumPieces, cradle.Mode, cradle.Code);
                                            cradle.PreviousItem = null; //Para evitar un error en la apertura del siguiente atado.
                                            //Actualizar Specification
                                        }
                                        lock (this.stateChangeLocker)
                                        {
                                            Trace.Warning("Assign {0} to Cradle {1}", productData.ToString(), cradle.Code);
                                            IBundle bundle = ExitManager.Instance.dataAccess.CreateBundlebyTracking(cradle.Id, t.Id);
                                            bundle.AddItem(i);
                                            cradle.PreviousItem = i;
                                            cradle.ProductData = productData;
                                            Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                            bundleData = i.ExtraData;
                                            bundle.ExtraData = bundleData;
                                            cradle.CurrentBundle = bundle;
                                            Trace.Message("Trace1 Open New BundleId: {0}, ItemStatusId: {1}, with BatchId: {2}, Maximum Pieces: {3}, Operation Mode: {4} on Cradle: {5}", cradle.CurrentBundle.IdBundle, t.Status, cradle.CurrentBundle.IdBatch, cradle.MaximumPieces, cradle.Mode, cradle.Code);
                                            ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                                            Library.Instance.Cradles.Remove(cradle.Id);
                                            Library.Instance.Cradles.Add(cradle.Id, cradle);
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    // No tiene actualizado el ProductData
                                    lock (this.stateChangeLocker)
                                    {
                                        if (i != null) // Lo puede anexar a un atado si no es Manual
                                        {
                                            Trace.Message("Processed TrackingId: {0}, ItemStatusId: {1}, with BatchId: {2}", t.Id, t.Status, t.IdProductionHistory);
                                            Trace.Warning("Assign {0} to Cradle {1}", productData.ToString(), cradle.Code);
                                            IBundle bundle = ExitManager.Instance.dataAccess.CreateBundlebyTracking(cradle.Id, t.Id);
                                            bundle.AddItem(i);
                                            cradle.PreviousItem = i;
                                            cradle.ProductData = productData;
                                            Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                            bundleData = i.ExtraData;
                                            bundle.ExtraData = bundleData;
                                            cradle.CurrentBundle = bundle;
                                            Trace.Message("Trace2 Open New BundleId: {0}, ItemStatusId: {1}, with BatchId: {2}, Maximum Pieces: {3}, Operation Mode: {4} on Cradle: {5}", cradle.CurrentBundle.IdBundle, t.Status, cradle.CurrentBundle.IdBatch, cradle.MaximumPieces, cradle.Mode, cradle.Code);
                                            ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                                            Library.Instance.Cradles.Remove(cradle.Id);
                                            Library.Instance.Cradles.Add(cradle.Id, cradle);
                                        }
                                        else
                                        {
                                            Trace.Message("ItemData es nulo");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Trace.Warning("Processed TrackingId MANUAL OutLine: {0}, ItemStatusId: {1}, with BatchId: {2} is Manual", t.Id, t.Status, t.IdProductionHistory);
                            }
                        }
                        else
                        {
                            Trace.Message("Mismo idtracking: {0}", t.Id);
                        }
                    }
                    #endregion
                    #region Modo Manual
                    else
                    {
                        if (t.Id != cradle.PreviousItem.Id) // Verifica no procesar el mismo IdTracking
                        {
                            lock (this.stateChangeLocker)
                            {
                                Trace.Message("Processed TrackingId: {0}, ItemStatusId: {1}, with BatchId: {2}", t.Id, t.Status, t.IdProductionHistory);
                                IProductData productData = null;
                                Item i = ExitManager.Instance.dataAccess.GetItemData(cradle.Id, t.Id, ref productData);
                                if (i != null)
                                {
                                    if (productData != null) // Tracking en Linea
                                    {
                                        if (!cradle.ProductData.Equals(productData))
                                        {
                                            Trace.Warning("Assign {0} to Cradle {1}", productData.ToString(), cradle.Code);
                                            cradle.ProductData = productData;
                                            Library.Instance.Cradles.Remove(cradle.Id);
                                            Library.Instance.Cradles.Add(cradle.Id, cradle);
                                            ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                                        }
                                    }
                                    else
                                    {
                                        Trace.Warning("Processed TrackingId MANUAL OutLine: {0}, ItemStatusId: {1}, with BatchId: {2} is Manual", t.Id, t.Status, t.IdProductionHistory);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Trace.Message("Procesando mismo Idtracking");
                        }
                    }
                    #endregion
                }
                else
                {  // No habia ningun atado ni Tracking  Arranque 
                    #region Arranque 
                    lock (this.stateChangeLocker)
                    {
                        IProductData productData = null;
                        Item i = ExitManager.Instance.dataAccess.GetItemData(cradle.Id, t.Id, ref productData);
                        if (i != null) // Lo puede anexar a un atado si no es Manual
                        {
                            if (productData != null)
                            {
                                Trace.Message("Processed TrackingId: {0}, ItemStatusId: {1}, with BatchId: {2}", t.Id, t.Status, t.IdProductionHistory);
                                if (cradle.Mode == CradleMode.Automatic)
                                {
                                    Trace.Warning("Assign {0} to Cradle {1}", productData.ToString(), cradle.Code);
                                    IBundle bundle = ExitManager.Instance.dataAccess.CreateBundlebyTracking(cradle.Id, t.Id);
                                    bundle.AddItem(i);
                                    cradle.PreviousItem = i;
                                    cradle.ProductData = productData;
                                    Dictionary<string, object> bundleData = new Dictionary<string, object>();
                                    bundleData = i.ExtraData;
                                    bundle.ExtraData = bundleData;
                                    cradle.CurrentBundle = bundle;
                                    Trace.Message("Trace3 Open New BundleId: {0}, ItemStatusId: {1}, with BatchId: {2}, Maximum Pieces: {3}, Operation Mode: {4} on Cradle: {5}", cradle.CurrentBundle.IdBundle, t.Status, cradle.CurrentBundle.IdBatch, cradle.MaximumPieces, cradle.Mode, cradle.Code);
                                    Library.Instance.Cradles.Remove(cradle.Id);
                                    Library.Instance.Cradles.Add(cradle.Id, cradle);
                                    ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                                }
                                else
                                {
                                    Trace.Warning("Assign {0} to Cradle {1}", productData.ToString(), cradle.Code);
                                    cradle.ProductData = productData;
                                    Library.Instance.Cradles.Remove(cradle.Id);
                                    Library.Instance.Cradles.Add(cradle.Id, cradle);
                                    ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                                }
                            }
                            else
                            {
                                Trace.Warning("Processed TrackingId MANUAL OutLine: {0}, ItemStatusId: {1}, with BatchId: {2} is Manual", t.Id, t.Status, t.IdProductionHistory);
                            }
                        }
                        else
                        {
                            Trace.Message("Processed TrackingId: {0}, ItemStatusId: {1}, with BatchId: {2} is Manual", t.Id, t.Status, t.IdProductionHistory);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Exception on CradleChanged event: {0}", cradle.Code, ex.Message);
            }
        }      
    }
}
