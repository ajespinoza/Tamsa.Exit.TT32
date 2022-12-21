// -----------------------------------------------------------------------
// <copyright file="Cradle.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text; 
    using Tenaris.Library.Shared;
    using Tenaris.Library.Framework.Remoting;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class Cradle : ICradle
    {
            

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
        public Cradle(int id, string code, string name, string description, int idZone, string trkZone, float maximumWeight, int cradleMode, int cradleState)
        {

            // Get cradle information.
            this.Id = id;
            this.Code = code;
            this.Name = name;
            this.Description = description;
            this.TrkZone = trkZone;
            this.IdZone = idZone;

            ////Calculate limits
            this.MaximumWeight = maximumWeight;
            this.MaximumPieces = 0;

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
        /// Gets maximum pieces on cradle.
        /// </summary>
        public int MaximumPieces { get; set; }

        /// <summary>
        /// Gets maximum weight on cradle.
        /// </summary>
        public float MaximumWeight { get; set; }

       
        /// <summary>
        /// Gets tracking zone.
        /// </summary>
        public string TrkZone { get;  set; }

        /// <summary>
        /// Get Zone Id
        /// </summary>
        public int IdZone { get; set; }

        /// <summary>
        /// Gets current bundle.
        /// </summary>
        public IBundle CurrentBundle { get; set; }


        /// <summary>
        /// Gets cradle operation mode.
        /// </summary>
        public CradleMode Mode { get; set; }

        /// <summary>
        /// Gets cradle state.
        /// </summary>
        public CradleState State { get; set; }

        /// <summary>
        /// Last Tracking in the Cradle
        /// </summary>
        public IItem PreviousItem { get; set; }

        #endregion

        /// <summary>
        /// Get Product Data
        /// </summary>
        public IProductData ProductData
        {
            get;
            set;
        }
      
       
    }


}
