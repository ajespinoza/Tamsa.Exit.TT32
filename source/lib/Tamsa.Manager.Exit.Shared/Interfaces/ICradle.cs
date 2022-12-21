// -----------------------------------------------------------------------
// <copyright file="ICradle.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the Icradle interface.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.Framework;
    using Tenaris.Library.Shared;
 
    /// <summary>
    /// Interface for cradle type.
    /// </summary>
    public interface ICradle:IEntity
    {
       
        /// <summary>
        /// Maximum pieces support by cradle.
        /// </summary>
        int MaximumPieces { get; set; }

        /// <summary>
        /// Maximum weight support by cradle.
        /// </summary>
        float MaximumWeight { get; set; }


         /// <summary>
        /// Associated Tracking Zone
        /// </summary>
        string TrkZone { get; set; }

        /// <summary>
        /// Asocciated Tracking Zone Id
        /// </summary>
        int IdZone { get; set; }

        /// <summary>
        /// Current bundle on open bundle state.
        /// </summary>
        IBundle CurrentBundle { get; set;  }

        /// <summary>
        /// Operation mode on cradle {Auto, Manual}
        /// </summary>
        CradleMode Mode { get; set; }

        /// <summary>
        /// Cradle state {Enabled, Disabled}
        /// </summary>
        CradleState State { get; set; }
     

        /// <summary>
        /// Indicates the Product into the Cradle
        /// </summary>
        IProductData ProductData { get; set; }
       
        /// <summary>
        /// Preview I
        /// </summary>
        IItem PreviousItem { get; set; }



    }
}
