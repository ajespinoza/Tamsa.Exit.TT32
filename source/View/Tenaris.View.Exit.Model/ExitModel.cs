// -----------------------------------------------------------------------
// <copyright file="ExitModel.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    using Tenaris.View.Exit.Model.Configuration;
    using System.Threading;
    using System.Configuration;
    using Tenaris.View.Exit.Model.Data;
    using Tenaris.Library.DbClient;
    using Tamsa.Manager.Exit.Shared;
    using Tenaris.Library.Log;
    using Tenaris.Manager.Tracking.Shared;
    using Tenaris.Library.Framework;
    using System.Collections.ObjectModel;


    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ExitModel
    {

        #region "Variables"

        SynchronizationContext context;
  

        private static ExitModel instance = null;
        private static readonly object Sync = new object();

        #endregion

        #region "Singleton"

        /// <summary>
        /// 
        /// </summary>
        public static ExitModel Instance
        {
            get
            {
                lock (Sync)
                {
                    if (instance == null)
                    {
                        instance = ExitModel.instance ?? (instance = new ExitModel());
                    }
                }
                return instance;
            }
        }

        #endregion

        #region "Propiedades"

        /// <summary>
        /// 
        /// </summary>
        public ExitViewConfiguration Config { get; private set; }


        // propiedad para el acceso a la base de datos
        public DataAccess Data { get; private set; }


        /// <summary>
        /// Adapter for Exit Manager
        /// </summary>
        public ExitManagerAdapter ExitAdapter { get; private set; }

        /// <summary>
        /// Adapter for Specification Client
        /// </summary>
        public SpecificationAdapter SpecAdapter { get; private set; }

     
        /// <summary>
        /// Batches by Cradles <idCradle, idBatch>
        /// </summary>
        public Dictionary<int, int> CurrentBatches { get; set; }

        /// <summary>
        /// Heats by Cradles <idCradle, Heat>
        /// </summary>
        public Dictionary<int, int> CurrentHeats { get; set; }

        ///Specifications by Cradles <idCradle, idSpec>
        public Dictionary<int, int> CurrentSpecifications { get; set; }

        /// <summary>
        /// Bundles by Cradle
        /// </summary>
        public Dictionary<int, List<IBundle>> CurrentBundles { get; set; }
       

        #endregion

        #region "Constructor"

        public ExitModel()
        {
            try
            {
                context = SynchronizationContext.Current;

                // Obtener configuracion 
                Config = (ExitViewConfiguration)ConfigurationManager.GetSection("ViewConfig");

                // Crear Instancia de Base de datos
                DbClient db = new DbClient(Config.DBConnection);
                Data = new DataAccess(db);
                // Crear Instancia para comunicacion con Manager
                ExitAdapter = new ExitManagerAdapter();
                if (ExitAdapter != null)
                {
                     ExitAdapter.Start(Config.ExitManagerSection);
                }
                
                // Crear Instancia para comunicacion con Specification
                SpecAdapter = new SpecificationAdapter(Config.SpecificationServer, Config.SpecificationPort);                

            }
            catch (Exception ex)
            {
                Trace.Error("Error initializing Exit Model {0}", ex.Message);
            }
        }


        #endregion

        public void Initialize()
        {
            try
            {
                object value;
                List<IBundle> Bundles;
                CurrentBundles = new Dictionary<int, List<IBundle>>();
                CurrentBatches = new Dictionary<int, int>();
                CurrentHeats = new Dictionary<int, int>();
                CurrentSpecifications = new Dictionary<int, int>();
                foreach (ICradle cradle in ExitAdapter.GetCradles())
                {
                    if (cradle.ProductData != null)
                    {
                        Bundles = new List<IBundle>();
                        CurrentBatches.Add(cradle.Id, cradle.ProductData.IdBatch);
                        CurrentSpecifications.Add(cradle.Id, cradle.ProductData.IdSpecification);
                        cradle.ProductData.Data.TryGetValue("HeatNumber", out value);
                        if (value != null)
                        {
                            CurrentHeats.Add(cradle.Id, Convert.ToInt32(value));
                        }
                        if (cradle.CurrentBundle != null)
                        {
                            if (cradle.Mode == CradleMode.Automatic)
                            {
                                if (cradle.CurrentBundle.Status != BundleStatus.Pending)
                                Bundles.Add(cradle.CurrentBundle);
                            }
                        }
                        List<IBundle> bundles = Data.GetBundles(cradle.ProductData.IdBatch, cradle.Id);

                        if(bundles != null)
                        foreach (Bundle b in bundles)
                        {
                            List<IItem> trackings = new List<IItem>();
                            trackings = Data.GetTrackingsOnBundle(cradle.Id, b.IdBundle);
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
                        }

                        CurrentBundles.Add(cradle.Id, Bundles);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message + "-----> Initialize");
            }
        }

        public List<IBundle> GetBundlesbyCradle(int idCradle)
        {
            List<IBundle> Bundles;
            CurrentBundles.TryGetValue(idCradle, out Bundles);
            return Bundles;
        }

        public void UpdateCradle(ICradle cradle)
        {
            List<IBundle> Bundles;
            if (cradle.ProductData != null)
            {
                Bundles = new List<IBundle>();
                if (cradle.CurrentBundle != null)
                {
                    if (cradle.Mode == CradleMode.Automatic)
                    {
                        if (cradle.CurrentBundle.Status != BundleStatus.Pending)
                            Bundles.Add(cradle.CurrentBundle);
                    }
                }
                List<IBundle> bundles = Data.GetBundles(cradle.ProductData.IdBatch, cradle.Id);

                //if (!bundles.Select(x => x.IdBundle.Equals(cradle.CurrentBundle.IdBundle)).FirstOrDefault())
                foreach (Bundle b in bundles)
                {
                    //if (b.IdBundle != cradle.CurrentBundle.IdBundle)
                    //{

                        List<IItem> trackings = new List<IItem>();
                        trackings = Data.GetTrackingsOnBundle(cradle.Id, b.IdBundle);
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
                CurrentBundles.Remove(cradle.Id);
                CurrentBundles.Add(cradle.Id, Bundles);
                //ExitModel.Instance.ExitAdapter.UpdateViews(cradle.Id);
            }
        }

        public List<IBundle> getInformationBundle(int idCurrentBatch, int idCradle)
        {

            List<IBundle> datos = new List<IBundle>();
            List<IBundle> currentBundles = new List<IBundle>();
            ////List<IBundle> currentBundles = ExitModel.Instance.GetBundlesbyCradle(idCradle); List<IBundle>
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
                CurrentBundles.Remove(idCradle);                
                CurrentBundles.Add(idCradle, currentBundles);
                return currentBundles;
          
                
            }
            
        }

        //public void GetBundles(int idCradle, int idBatch, bool onlyDB)
        //{
        //    List<IBundle> Bundles;
        //    lock (this)
        //    {
        //        if (onlyDB)
        //        {
        //            Bundles.Clear();
        //            Bundles.RemoveAll(b => b.Status == BundleStatus.Pending);
        //            Bundles = new List<IBundle>();
        //        }
        //        foreach (Bundle b in Data.GetBundles(idBatch))
        //        {
        //            List<IItem> trackings = new List<IItem>();
        //            trackings = Data.GetTrackingsOnBundle(idCradle, b.IdBundle);
        //            foreach (IItem t in trackings)
        //            {
        //                b.AddItem(t);
        //                if (t.ExtraData != null)
        //                {
        //                    if (b.ExtraData == null)
        //                    {
        //                        Dictionary<string, object> bundleData = new Dictionary<string, object>();
        //                        bundleData = t.ExtraData;
        //                        b.ExtraData = bundleData;
        //                    }
        //                }
        //            }
        //            Bundles.Add(b);
        //        }
        //    }

        //}

        //public void SendBundle(int idUser,string PipUser, int idBundle, string comments, bool rutaSEAS, out Dictionary<int,string> errors, out int BundleNumber)
        //{
        //    ExitAdapter.SendBundle(idUser, PipUser, idBundle, comments, rutaSEAS, out errors, out BundleNumber);
        //}

        //ENCRAGADO DE IMPRIMIR
        public void SendBundle(int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, string impresora, string groupElaborationState, string elaborationState, string location, string rejectionCode, out Dictionary<int, string> errors, out int BundleNumber)
        {
            ExitAdapter.SendBundle(idUser, PipUser, idBundle, comments, rutaSEAS, impresora, groupElaborationState, elaborationState, location, rejectionCode, out errors, out BundleNumber);
        }

        public void UpdateViews(int idCradle)
        {
           ExitAdapter.UpdateViews(idCradle);
        }

        public bool ChangeCradleCondition(int idCradle, CradleMode mode, CradleState state, int maximumPieces)
        {
            return ExitAdapter.ChangeCradleCondition(idCradle, mode,state,maximumPieces);
        }

        public ObservableCollection<GroupElaborationState> GetGroupElaborationStateList()
        {
            return Data.GetGroupElaborationState();
        }

        public ObservableCollection<ElaborationState> GetElaborationStatesByGroup(int IdGroupElaborationState)
        {
            return Data.GetElaborationStateByGroup(IdGroupElaborationState);
        }
        public ObservableCollection<RejectionCode> GetRejectionCausesByElaborationState(int IdElaborationState)
        {
            return Data.GetRejections(IdElaborationState);
        }
    }
}

