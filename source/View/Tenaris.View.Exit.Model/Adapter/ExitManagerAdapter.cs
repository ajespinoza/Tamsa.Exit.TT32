// -----------------------------------------------------------------------
// <copyright file="ExitManagerAdapter.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tamsa.Manager.Exit.Shared;
    using Tenaris.Library.Framework.Factory;
    using Tenaris.Library.ConnectionMonitor;
    using Tenaris.Library.Log;
    using System.Threading;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExitManagerAdapter : IDisposable
    {

        #region Variables
        
        IExitManager manager;
        private static readonly object Sync = new object();
        object zonesLock = new object();
        Dictionary <int,ICradle> Cradles;

        public event EventHandler<CradleChangedEventArgs> OnCradleChanged;
        SynchronizationContext context;
      
        #endregion

        #region Constructor

        public ExitManagerAdapter()
        {
           context = SynchronizationContext.Current;
           ConnectionMonitor.Instance.StateChanged += new EventHandler<StateChangeEventArgs>(ConnectionMonitorStateChanged);
        }


        public void Start(string ConfigExit)
        {
            ProxyConfiguration.ConfigureRemoting();
            IFactory<IExitManager> Factory = FactoryProvider.Instance.CreateFactory<IExitManager>(ConfigExit);
            manager = Factory.Create();           
       
        }

        public void Stop()
        {
            // detiene remoting            
            ConnectionMonitor.Instance.StateChanged -= new EventHandler<StateChangeEventArgs>(ConnectionMonitorStateChanged);
            ConnectionMonitor.Instance.Stop();
            ConnectionMonitor.Instance.Dispose();
            Unitialize();
        }

        public IEnumerable<ICradle> GetCradles()
        {
            return manager.Cradles;
        }

        //public void SendBundle(int idUser,string PipUser,int idBundle, string comments, bool rutaSEAS, out Dictionary<int,string> errors, out int BundleNumber)
        //{
        //    manager.SendBundle(idUser, PipUser, idBundle, comments, rutaSEAS, out errors, out BundleNumber);
        //}

        //USADO PARA IMPRESION
        public void SendBundle(int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, string impresora, string groupElaborationState, string elaborationState, string location, string rejectionCode, out Dictionary<int, string> errors, out int BundleNumber)
        {
            manager.SendBundle(idUser, PipUser, idBundle, comments, rutaSEAS, impresora, groupElaborationState, elaborationState, location, rejectionCode, out errors, out BundleNumber);
        }

        public void UpdateViews(int idCradle)
        {
                manager.UpdateViews(idCradle);
        }

        public void CloseBundle(ICradle idCradle)
        {
            manager.CloseBundle(idCradle);
        }

        public bool ChangeCradleCondition(int idCradle, CradleMode? mode = null, CradleState? state = null, int? maximumPieces = null)
        {
            try
            {
                return manager.ChangeCradleCondition(idCradle, mode, state, maximumPieces);
            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Exception Change Cradle Condition for Cradle {0}, Mode {1} : {2}", idCradle, mode, ex.ToString());
                return false;
            }
        }

    
        #endregion

        internal void Initialize()
        {
            manager.CradleChanged += new EventHandler<CradleChangedEventArgs>(exitManager_CradleChanged);
            Cradles = new Dictionary<int, ICradle>();
            try
            {
                foreach (ICradle cradle in manager.Cradles)
                {
                    Trace.Message("Add Cradle {0} ", cradle.Code);
                    Cradles.Add(cradle.Id, cradle);                   
                }
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }

        internal void Unitialize()
        {
            manager.CradleChanged -= exitManager_CradleChanged;
            Cradles = null;
            manager = null;
        }

        public void ConnectionMonitorStateChanged(object sender, StateChangeEventArgs e)
        {
            try
            {
                object objMgr = e.Connection;

                if (objMgr is IExitManager)
                {
                    if (e.IsConnected)
                    {

                        Trace.Message("{0}  -> [EXITMANAGER] Connection Monitor; Tracking is changed state to ==> [CONNECTED]", DateTime.Now);
                        Initialize();
                    }
                    else
                    {
                        Unitialize();
                        Trace.Message("{0} -> [EXITMANAGER] Connection Monitor; Change state to ==> [DISCONNECTED]", DateTime.Now);

                    }
                }


            }
            catch (Exception ex)
            {
                Trace.Message("{0} [ERROR] Connection Monitor Error {1}", DateTime.Now, ex.Message);
            }
        }
              


        void exitManager_CradleChanged(object sender, CradleChangedEventArgs e)
        {
            List<IBundle> currentBundles = new List<IBundle>();

            if (ExitModel.Instance.CurrentBundles != null)
            {
                ExitModel.Instance.CurrentBundles.TryGetValue(e.Cradle.Id, out currentBundles);
                // Cradle en memoria
                ICradle cradle = Cradles.FirstOrDefault(c => c.Key == e.Cradle.Id).Value;
                if (cradle != null)
                {
                    ////Valida sino existe canasta activa
                    //if (e.Cradle.ProductData == null)
                    //    e.Cradle.ProductData = cradle.ProductData;

                    // Verifica si hay cambio de Producto
                    if (cradle.ProductData.IdBatch == e.Cradle.ProductData.IdBatch)
                    {
                        if (e.Cradle.CurrentBundle == null)
                        {//CCD
                            List<IBundle> Bundles = new List<IBundle>();

                            List<IBundle> bundles = ExitModel.Instance.Data.GetBundles(cradle.ProductData.IdBatch, cradle.Id);

                            foreach (Bundle b in bundles)
                            {
                                //if (b.IdBundle != cradle.CurrentBundle.IdBundle)
                                //{

                                List<IItem> trackings = new List<IItem>();
                                trackings = ExitModel.Instance.Data.GetTrackingsOnBundle(cradle.Id, b.IdBundle);
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
                                Bundles.Add(b);
                                //}
                            }

                            //e.Cradle.CurrentBundle = cradle.CurrentBundle;
                            if (currentBundles != null)
                            {
                                //IBundle b = currentBundles.FirstOrDefault(bn => bn.IdBundle == cradle.CurrentBundle.IdBundle);
                                //if (b != null)
                                //{
                                    //currentBundles.Remove(cradle.Id);
                                    //currentBundles.Insert(0, e.Cradle.CurrentBundle);
                                currentBundles = Bundles ;
                                    ExitModel.Instance.CurrentBundles.Remove(cradle.Id);
                                    ExitModel.Instance.CurrentBundles.Add(cradle.Id, currentBundles);
                                    cradle.CurrentBundle = e.Cradle.CurrentBundle;
                                    Cradles.Remove(cradle.Id);
                                    Cradles.Add(cradle.Id, cradle);
                                    CradleChanged(sender, e);
                                //}
                            }
                            
                        }
                        else
                        {//CCD
                            // Verifica si hay cambio de atado
                            if (cradle.CurrentBundle.IdBundle == e.Cradle.CurrentBundle.IdBundle)
                            {
                                if (currentBundles != null)
                                {
                                    IBundle b = currentBundles.FirstOrDefault(bn => bn.IdBundle == cradle.CurrentBundle.IdBundle);
                                    if (b != null)
                                    {
                                        currentBundles.Remove(b);
                                        currentBundles.Insert(0, e.Cradle.CurrentBundle);
                                        ExitModel.Instance.CurrentBundles.Remove(cradle.Id);
                                        ExitModel.Instance.CurrentBundles.Add(cradle.Id, currentBundles);
                                        cradle.CurrentBundle = e.Cradle.CurrentBundle;
                                        Cradles.Remove(cradle.Id);
                                        Cradles.Add(cradle.Id, cradle);
                                        CradleChanged(sender, e);
                                    }
                                }
                            }
                            else
                            {
                                IBundle b = currentBundles.FirstOrDefault(bn => bn.IdBundle == cradle.CurrentBundle.IdBundle);
                                if (b != null)
                                {
                                    b.Close();
                                }
                                currentBundles.Insert(0, e.Cradle.CurrentBundle);
                                ExitModel.Instance.CurrentBundles.Remove(cradle.Id);
                                ExitModel.Instance.CurrentBundles.Add(cradle.Id, currentBundles);
                                cradle.CurrentBundle = e.Cradle.CurrentBundle;
                                Cradles.Remove(cradle.Id);
                                Cradles.Add(cradle.Id, cradle);
                                CradleChanged(sender, e);
                            }
                        }
                    }
                    else // Hay cambio de  Producto
                    {
                        object value;
                        ExitModel.Instance.CurrentBatches.Remove(e.Cradle.Id);
                        ExitModel.Instance.CurrentHeats.Remove(e.Cradle.Id);
                        ExitModel.Instance.CurrentSpecifications.Remove(e.Cradle.Id);
                        ExitModel.Instance.CurrentBundles.Remove(e.Cradle.Id);
                        if (e.Cradle.ProductData != null)
                        {
                            ExitModel.Instance.CurrentBatches.Add(e.Cradle.Id, e.Cradle.ProductData.IdBatch);
                            ExitModel.Instance.CurrentSpecifications.Add(e.Cradle.Id, e.Cradle.ProductData.IdSpecification);
                            e.Cradle.ProductData.Data.TryGetValue("HeatNumber", out value);
                            if (value != null)
                            {
                                ExitModel.Instance.CurrentHeats.Add(e.Cradle.Id, Convert.ToInt32(value));
                            }
                            if (e.Cradle.CurrentBundle != null)
                            {
                                if (cradle.Mode == CradleMode.Automatic)
                                    currentBundles.Add(cradle.CurrentBundle);
                            }
                            List<IBundle> bundles = ExitModel.Instance.Data.GetBundles(cradle.ProductData.IdBatch, e.Cradle.Id);
                            foreach (Bundle b in bundles)
                            {
                                List<IItem> trackings = new List<IItem>();
                                trackings = ExitModel.Instance.Data.GetTrackingsOnBundle(e.Cradle.Id, b.IdBundle);
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
                            ExitModel.Instance.CurrentBundles.Add(e.Cradle.Id, currentBundles);
                            cradle.CurrentBundle = e.Cradle.CurrentBundle;
                            Cradles.Remove(cradle.Id);
                            Cradles.Add(cradle.Id, cradle);

                            CradleChanged(sender, e);
                        }
                    }
                }
            }
            //ExitModel.Instance.getInformationBundle(e.Cradle.ProductData.IdBatch, e.Cradle.Id);
            //CradleChanged(sender, e);
        }


        private void CradleChanged(object sender, CradleChangedEventArgs e)
        {
            SendOrPostCallback callback = delegate(object state)
            {
                try
                {
                    if (OnCradleChanged != null)
                    {
                        OnCradleChanged(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    Trace.Error("Error en procesamiento de evento");
                    Trace.Exception(ex);
                }
            };

            context.Send(callback, null);
        }


        #region IDisposable

        public void Dispose()
        {
            Unitialize();
        }

        #endregion
    }
}

