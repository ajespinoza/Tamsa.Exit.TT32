// -----------------------------------------------------------------------
// <copyright file="Library.cs" company="Techint">
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
    using Tamsa.Manager.Exit.Shared;
    using System.Data;
    using Tenaris.Library.Shared;
    using System.Threading;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Library
    {
        #region "Singleton"

        // 
        static Library sharedManager = null;
        static object lockSharedManager = new object();

        public static Library Instance
        {
            get
            {
                lock (lockSharedManager)
                {
                    if (sharedManager == null)
                    {
                        Trace.Message("Creating Library instance");
                        sharedManager = new Library();
                    }
                } // lock

                return sharedManager;
            }
        }

        #endregion
      

        #region "Propiedades"


        public Dictionary<int, ICradle> Cradles = null;
               

        #endregion

        public Library()
        {

        }

        public void Initialize()
        {
            // Verify the exist area.
            if (ExitManager.Instance.Area == null)
            {
                Trace.Error("The configured area not exist on database.");
            }
            Trace.Message("Loading Cradles...!");            
            Cradles = new Dictionary<int, ICradle>();
            foreach (ICradle cradle in ExitManager.Instance.dataAccess.GetCradles(ExitManager.Instance.Area.Id, false))
            {
                Cradles.Add(cradle.Id, cradle);
                Trace.Debug("Creando Cradle {0}:", cradle.Id);
            }            
        }

        public ICradle GetCradle(int idCradle, bool isManual)
        {
            return ExitManager.Instance.dataAccess.GetCradles(ExitManager.Instance.Area.Id, true).FirstOrDefault(x=>x.Id.Equals(idCradle));
        }

     

        /// <summary>
        /// Change cradle condition method.
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
        internal bool ChangeCradleCondition(int idCradle, CradleMode? mode = null, CradleState? state = null, int? maximumPieces = null)
        {
            ICradle cradle = ExitManager.Instance.Cradles.FirstOrDefault(c => c.Id == idCradle);
            var result = false;
            var lastMode = cradle.Mode;
            var lastState = cradle.State;
            var lastMaximumPieces = cradle.MaximumPieces;
            try
            {
                if (mode.HasValue)
                {
                    if (ExitManager.Instance.dataAccess.UpdateCradle(cradle.Id, Convert.ToInt32(mode.Value), Convert.ToInt32(cradle.State)))
                    {
                        // Change mode.
                        cradle.Mode = mode.Value;
                        Trace.Message("OnDemand change Operation Mode, Last: {0}, Current: {1} on Cradle: {2}", lastMode, cradle.Mode, cradle.Code);
                        result = true;
                    }
                }
                if (state.HasValue)
                {
                    if (ExitManager.Instance.dataAccess.UpdateCradle(cradle.Id, Convert.ToInt32(cradle.Mode), Convert.ToInt32(state.Value)))
                    {
                        // Change state.
                        cradle.State = state.Value;
                        Trace.Message("OnDemand change State, Last: {0}, Current: {1} on Cradle: {2}", lastState, cradle.State, cradle.Code);
                        result = true;
                    }
                }
                if (maximumPieces.HasValue)
                {
                    if (cradle.MaximumPieces != maximumPieces)
                    {
                        // Change maximum pieces.
                        cradle.MaximumPieces = maximumPieces.Value;
                        Trace.Message("OnDemand change Maximum Pieces, Last: {0}, Current: {1} on Cradle: {2}", lastMaximumPieces, cradle.MaximumPieces, cradle.Code);

                        //CAMBIAR EL MAXPIECES EN BD
                        result = ExitManager.Instance.dataAccess.UpdMaxPieces(idCradle, maximumPieces);
                        Trace.Debug("Actualizacion maxPieces relizada en BD");
                    }
                    else
                    {
                        Trace.Debug("La cantidad de maximo de Piezas es igual a la anterior");
                    }
                }
                if (result)
                {
                    Trace.Debug("Status del Current Cradle: {0}", cradle.CurrentBundle.Status);
                    if (cradle.CurrentBundle.State == BundleState.Open)
                    {
                        Trace.Debug("Current IdBundle a cerrar: {0}", cradle.CurrentBundle.IdBundle);
                        if (ExitManager.Instance.dataAccess.CloseBundle(cradle.CurrentBundle.IdBundle))
                        {
                            Trace.Debug("Atado Cerrado en BD");
                            cradle.CurrentBundle.Close();
                            Trace.Debug("Atado Cerrado en Memoria");
                            cradle.PreviousItem = null; // Para evitar un error en la apertura del siguiente atado.
                        }
                    }
                    ExitManager.Instance.OnCradleChanged(new CradleChangedEventArgs(cradle));
                }
                return result;

            }
            catch (Exception ex)
            {
                Trace.Exception(ex, "Error ChangeCradleCondition Cradle {0} : {1}", cradle.Id, ex.ToString());
                return false;
            }
        }

    }
}
