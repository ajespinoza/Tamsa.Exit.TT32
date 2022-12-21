// -----------------------------------------------------------------------
// <copyright file="ExitManager.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the ExitManager class.
// </summary>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Library.Framework.Factory;
using Tenaris.Manager.Forum.Shared;
using Tenaris.Library.Log;
using System.Configuration;
using Tamsa.Manager.Exit.Shared;    
using Tenaris.Library.Framework.Remoting;
using System.Threading;
using Tenaris.Manager.Tracking.Shared;

namespace Tamsa.Manager.Exit
{  
    /// <summary>
    /// Gets or sets to ExitManager class.
    /// </summary>
    [Serializable]
    public partial class ExitManager : ManagerBase, IExitManager
    {

        /// <summary>
        /// cradle changed event.
        /// </summary>
        private readonly RemoteEvent<CradleChangedEventArgs> cradleChanged = new RemoteEvent<CradleChangedEventArgs>(true);

        #region Singleton

        /// <summary>
        /// Exit manager instance.
        /// </summary>
        private static ExitManager instance = null;

        /// <summary>
        /// Lock instance.
        /// </summary>
        private static object lockInstance = new object();

        /// <summary>
        /// Return an  Manager Instance
        /// </summary>
        public static ExitManager Instance
        {
            get
            {
                lock (lockInstance)
                {
                    if (instance == null)
                    {
                        Trace.Message("Creating Exit Manager instance");
                        instance = new ExitManager();
                    }
                } // lock

                return instance;
            }
        }

        #endregion

        #region "Constants"

        // code to find application's id from database
        const string ConfigSection = "ManagerConfiguration";

        #endregion
       

        #region "Propiedades"

        public ExitConfiguration Config { get; private set; }

        public DataAccess dataAccess { get; private set; }


        public WebServiceITAdapter WServiceITAdapter { get; private set; }

        /// <summary>
        /// Cradles list.
        /// </summary>
        public IEnumerable<ICradle> Cradles
        {
            get
            {
                return Library.Instance.Cradles.Values.ToList();
            }
        }


        #endregion

        #region "Constructor"

        /// <summary>
        /// Manager's constructor
        /// </summary>
        internal ExitManager()
        {
          
       
        }

        #endregion

        #region BaseManager Members

        /// <summary>
        /// Override base method Initialize manager factory invoque
        /// </summary>
        protected override void DoInitialize()
        {
            Trace.Message("DoInitialize");
            Trace.Message("Init manager for area : {0}", Area.Code);

            // lee la configuracion
            Config = (ExitConfiguration)ConfigurationManager.GetSection(ConfigSection);
            if (Config == null)
            {
                throw new ApplicationException("Manager config section not found");
            }

            try
            {
                // prepare database access using dbClient from ManagerBase
                // Instantiating dataaccess object.
                dataAccess = new DataAccess(DbClient);

                ////Inicializa Libreria de manejo de Cradles
                Library.Instance.Initialize();

                //Inicializando Tracking Manager           
                TrackingAdapter.Instance.Initialize();

                WServiceITAdapter = new WebServiceITAdapter();

            }
            catch (Exception ex)
            {
                Trace.Error("Error al inicializar manager, deteniendo manager");
                Trace.Exception(ex);
            }
        }

        /// <summary>
        /// Override base method Activate manager factory invoque
        /// </summary>
        protected override void DoActivate()
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Override base method Uninitialize manager factory invoque
        /// </summary>
        protected override void DoUninitialize()
        {
            // throw new NotImplementedException();
        }

        /// <summary>
        /// Override base method Deactivate manager factory invoque
        /// </summary>
        protected override void DoDeactivate()
        {
            try
            {
                TrackingAdapter.Instance.Deactivate();
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }

       

        #endregion

        
        #region Events

        /// <summary>
        /// Cradle change Event args.
        /// </summary>
        public event EventHandler<CradleChangedEventArgs> CradleChanged
        {
            add
            {
                this.cradleChanged.Event += value;
            }

            remove
            {
                this.cradleChanged.Event -= value;
            }
        }

        

        public void OnCradleChanged(CradleChangedEventArgs e)
        {
            try
            {
                cradleChanged.InvokeEventAsync(this, e);
                Trace.Message("Event CradleChangedEventArgs");
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Change cradle condition
        /// </summary>
        /// <param name="idCradle">
        /// Id cradle.
        /// </param>
        /// <param name="mode">
        /// Operation mode.
        /// </param>
        /// <param name="state">
        /// State.
        /// </param>
        /// <param name="maximumPieces">
        /// Maximum pieces.
        /// </param>
        /// <returns>
        /// Result.
        /// </returns>
        public bool ChangeCradleCondition(int idCradle, CradleMode? mode = null, CradleState? state = null, int? maximumPieces = null)
        {
            return Library.Instance.ChangeCradleCondition(idCradle, mode, state, maximumPieces);
        }
      

        //public bool SendBundle(int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, out Dictionary<int, string> errors, out int BundleNumber)
        //{
        //    return WServiceITAdapter.SendBundle(idUser, PipUser, idBundle, comments, rutaSEAS, out errors, out BundleNumber);
        //}

        //Imprimir
        public bool SendBundle(int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, string impresora, string groupElaborationState, string elaborationState, string location, string rejectionCode, out Dictionary<int, string> errors, out int BundleNumber)
        {
            return WServiceITAdapter.SendBundle(idUser, PipUser, idBundle, comments, rutaSEAS, impresora, groupElaborationState, elaborationState, location, rejectionCode, out errors, out BundleNumber);
        }


        //UTILIZADO PARA ACTUALIZAR TODAS LAS VISTAS
        public void UpdateViews(int idCradle)
        {
           
            ICradle cradle = Library.Instance.GetCradle(idCradle, true);
            
            
            
            try
            {
                Trace.Message("Event UpdateViews!!!!!");
                ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                //cradleChanged.InvokeEventAsync(this, new CradleChangedEventArgs(cradle));
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }


        //UTILIZADO PARA CERRAR ATADO ACTIVO 
        public void CloseBundle(ICradle idCradle)
        {
            //Enviar el evento a todas las vistas
            try
            {
                Trace.Message("Event CloseBundle, idBundle: {0}", idCradle.CurrentBundle.IdBundle);
                if (ExitManager.Instance.dataAccess.CloseBundle(idCradle.CurrentBundle.IdBundle))
                {
                    idCradle.CurrentBundle.Close();
                    //Trace.Message("Close BundleId: {0}, ItemStatusId: {1}, with BatchId: {2}, Total Pieces: {3}, Maximum Pieces: {4}, Operation Mode: {5} on Cradle: {6}", idCradle.CurrentBundle.IdBundle, t.Status, idCradle.CurrentBundle.IdBatch, idCradle.CurrentBundle.Items.ToList().Count, idCradle.MaximumPieces, idCradle.Mode, idCradle.Code);
                    idCradle.PreviousItem = null; // Para evitar un error en la apertura del siguiente atado.
                    
                }
                lock (this)
                {
                    Library.Instance.Cradles.Remove(idCradle.Id);
                    Library.Instance.Cradles.Add(idCradle.Id, idCradle);
                }
                ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(idCradle));
            }
            catch (Exception ex)
            {
                Trace.Error(ex.Message);
            }
        }


        #endregion


        #region "override de MarshalByRef"

        /// <summary>
        /// tracking manager is a singleton, disable lifetime
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion


    }

    
}
