// -----------------------------------------------------------------------
// <copyright file="StoredProcedures.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Model.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Definicion de los nombres de los stored usados por la vista
    /// </summary>
    public static class StoredProcedures
    {

        #region Stored Procedures Common

        public const string SpGetAreaComamnd = "Common.GetArea";

        public const string SpGetMachinesComamnd = "Common.GetMachines";

        public const string FSGetIdUser = "[Security].[FSGetIdUser]";

        public const string SpGetStatuses = "[Tracking].[GetItemStatus]";

        #endregion

        public const string SpGetUserInformation = "[Exit].[GetUserInformation]";

        public const string SpGetBundlesbyBatch = "[Exit].[GetBundlesByBatch]";

        public const string SpGetTrackingsOnBundle = "[Exit].[GetTrackingsOnBundle]";
               
        public const string SpGetAvailableItems = "[Exit].[GetAvailableItems]";

        public const string SpGetAvailableItemsByProduct = "[Exit].[GetAvailableItemsByProduct]";

        public const string SpDelBundle = "[Exit].[DelBundle]";

        public const string SpInsbundle = "[Exit].[InsBundle]";

        public const string SpUpdRejectBundle = "[Exit].[UpdRejectBundle]";

        public const string SpCreateTrackingOnBundle = "[Exit].[CreateTrackingOnBundle]";

        public const string SpUpdTrackingBundle = "[Exit].[UpdTrackingBundle]";

        public const string SpDelTrackingBundle = "[Exit].[DelTrackingBundle]";

        public const string SpGetQualityCodes = "[Quality].[GetQualityCodes]";

        public const string SpGetShifts = "[Shift].[GetShifts]";

        public const string SpGetHistoricBundles = "[Exit].[GetHistoricBundles]";

        public const string SpGetLocations = "[Exit].[GetLocations]";

        /// <summary>
        /// Get Bundle Data for sending
        /// </summary>
        public const string SpGetBundleData = "[Exit].[GetBundleData]";

        public const string SpGetGroupElaborationState = "[Exit].[GetGroupElaborationState]";
        public const string SpGetElaborationStateByGroup = "[Exit].[GetElaborationStateByGroup]";
        public const string SpGetRejectionCodeByElaborationState = "[Exit].[GetRejectionCodeByElaborationState]";
    }
}
