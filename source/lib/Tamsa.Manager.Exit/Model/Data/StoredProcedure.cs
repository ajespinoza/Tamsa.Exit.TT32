// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StoredProcedure.cs" company="Tenaris">
//   Tamsa.
// </copyright>
// <summary>
//   Defines the StoredProcedure class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Stored procedures class.
    /// </summary>
    public static class StoredProcedure
    {

        /// <summary>
        /// Update Maxpieces in Cradle
        /// </summary>
        public const string SpUpdMaxPieces = "[Exit].[UpdMaxPieces]";


        /// <summary>
        /// Get Maxpieces in Cradle
        /// </summary>
        public const string SpGetMaxPieces = "[Exit].[GetMaximumPieces]";

        /// <summary>
        /// Gets cradles for area.
        /// </summary>
        public const string SpGetCradles = "[Exit].[GetCradles]";


        /// <summary>
        ///  Get Last Item from Asociated Zone to Cradle
        /// </summary>
        public const string SpGetLastItem = "[Exit].[GetLastItem]";
        
        /// <summary>
        /// Gets open bundle on cradle.
        /// </summary>
        public const string SpGetOpenBundle = "[Exit].[GetOpenBundle]";

        /// <summary>
        /// Create a Bundle depending of Tracking
        /// </summary>
        public const string SpCreateBundlebyTracking = "[Exit].[CreateBundlebyTracking]";


        /// <summary>
        /// Get Bundle Data for sending
        /// </summary>
        public const string SpGetBundleData = "[Exit].[GetBundleData]";
     

        /// <summary>
        /// Gets trackings on bundle.
        /// </summary>
        public const string SpGetTrackingsOnBundle = "[Exit].[GetTrackingsOnBundle]";

        /// <summary>
        /// Create tracking on bundle.
        /// </summary>
        public const string SpCreateTrackingOnBundle = "[Exit].[CreateTrackingOnBundle]";

        /// <summary>
        /// Update bundle.
        /// </summary>
        public const string SpUpdCloseBundle = "[Exit].[UpdCloseBundle]";

        /// <summary>
        /// Update cradle.
        /// </summary>
        public const string SpUpdateCradle = "[Exit].[UpdateCradle]";

        /// <summary>
        ///  Get Statuses for Items
        /// </summary>
        public const string SpGetStatuses = "[Tracking].[GetItemStatus]";

        /// <summary>
        /// Get Tracking Additional Data
        /// </summary>
        public const string SpGetTrackingData = "[Exit].[GetTrackingData]";

       
        /// <summary>
        /// Insert Sending Data 
        /// </summary>
        public const string SpInsSending = "[Exit].[InsSending]";

        /// <summary>
        /// Get Location Data of Bundle
        /// </summary>
        public const string SpGetLocation = "[Exit].[GetLocation]";
        

    }
}


