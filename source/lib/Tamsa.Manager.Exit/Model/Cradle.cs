// -----------------------------------------------------------------------
// <copyright file="Cradle.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the Cradle class.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tamsa.Manager.Exit.Shared;
    using Tenaris.Library.Framework.Remoting;
    using Tenaris.Library.Log;

    [Serializable]
    public class Cradle:ICradle
    {

        #region Fields

        /// <summary>
        /// Data access object.
        /// </summary>
        private DataAccess dataAccess;

        /// <summary>
        /// Preview processed tracking.
        /// </summary>
        private Tracking previewTracking;

        /// <summary>
        /// Current specification.
        /// </summary>
        private Specification currentSpecification;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Cradle"/> class.
        /// </summary>
        /// <param name="id">
        /// Id cradle.
        /// </param>
        /// <param name="code">
        /// Cradle code.
        /// </param>
        /// <param name="name">
        /// Cradle name.
        /// </param>
        /// <param name="description">
        /// Cradle description.
        /// </param>
        /// <param name="trkZone">
        /// Cradle tracking zone.
        /// </param>
        /// <param name="maximumWeight">
        /// Maximum weight on cradle.
        /// </param>
        /// <param name="cradleMode">
        /// Cradle operation mode.
        /// </param>
        /// <param name="cradleState">
        /// Cradle state.
        /// </param>
        public Cradle(int id, string code, string name, string description, string trkZone, float maximumWeight, int cradleMode, int cradleState)
        {
            // Instantiating dataaccess object.
            this.dataAccess = new DataAccess();

            // Activate DataAccess
            this.dataAccess.Activate();

            // Get cradle information.
            this.Id = id;
            this.Code = code;
            this.Name = name;
            this.Description = description;
            this.TrkZone = trkZone;

            // Get current specification.
            this.currentSpecification = this.dataAccess.GetCurrentBatchSpecification(this.TrkZone);

            //Calculate limits
            this.MaximumWeight = maximumWeight;
            this.MaximumPieces = Convert.ToInt32(this.MaximumWeight / (this.currentSpecification.KgMWeight * this.currentSpecification.NominalLength));

            //Cradle Mode
            switch (cradleMode)
            {
                case 0: { this.Mode = CradleMode.Automatic; break; }
                case 1: { this.Mode = CradleMode.Manual; break; }
            }

            // Cradle State
            switch (cradleState)
            {
                case 0: { this.State = CradleState.Disable; break; }
                case 1: { this.State = CradleState.Enable; break; }
            }

            // Get current open bundle.
            this.CurrentBundle = this.dataAccess.GetOpenBundle(this.Id);

            // If exist open bundle.
            if (this.CurrentBundle.State == BundleState.Open)
            {
                // Get trackings on bundle.
                var trackings = this.dataAccess.GetTrackingsOnBundle(this.CurrentBundle.IdBundle);

                foreach (var t in trackings)
                {
                    // Add tracking
                    this.CurrentBundle.AddTracking(t);
                }

                // If mode is equal to automatic.
                if (this.currentSpecification.IdBatch != this.CurrentBundle.IdBatch && this.Mode == CradleMode.Automatic)
                {
                    // Get current specification.
                    this.currentSpecification = this.dataAccess.GetBatchSpecification(this.CurrentBundle.IdBatch);

                    //Calculate limits
                    this.MaximumWeight = maximumWeight;
                    this.MaximumPieces = Convert.ToInt32(this.MaximumWeight / (this.currentSpecification.KgMWeight * this.currentSpecification.NominalLength));
                }
            }

            // initialize preview tracking.
            this.previewTracking = new Tracking();

            Trace.Message("     Cradle: {0} for Tracking Zone: {1} created!, Operation Mode: {2}, CurrentBundleID: {3}, BatchId: {4}, ItemStatusId: {5}, Maximum Weight(Kg) for Bundle: {6}, Maximum Pieces for Bundle: {7}", code, trkZone, this.Mode, this.CurrentBundle.IdBundle, this.currentSpecification.IdBatch, this.CurrentBundle.IdItemStatus, this.MaximumWeight, this.MaximumPieces);

        }

        #endregion

        #region Events

        /// <summary>
        /// Cradle event changed.
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

        #endregion

        #region Properties

        /// <summary>
        /// Gets Id cradle.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets cradle code.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Gets cradle name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets cradle description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets tracking zone.
        /// </summary>
        public string TrkZone { get; private set; }

        /// <summary>
        /// Gets maximum pieces on cradle.
        /// </summary>
        public int MaximumPieces { get; private set; }

        /// <summary>
        /// Gets maximum weight on cradle.
        /// </summary>
        public float MaximumWeight { get; private set; }

        /// <summary>
        /// Gets current bundle.
        /// </summary>
        public Bundle CurrentBundle { get; private set; }

        /// <summary>
        /// Gets cradle operation mode.
        /// </summary>
        public CradleMode Mode { get; private set; }

        /// <summary>
        /// Gets cradle state.
        /// </summary>
        public CradleState State { get; private set; }

        #endregion
       

      

        #region Private Methods

        /// <summary>
        /// Cradle event changed.
        /// </summary>
        private readonly RemoteEvent<CradleChangedEventArgs> cradleChanged = new RemoteEvent<CradleChangedEventArgs>(false);

        #endregion

    }
}
