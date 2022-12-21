// -----------------------------------------------------------------------
// <copyright file="Location.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [Serializable]
    public class Location :ILocation
    {

        #region Constructor

        public Location(int id, string code, string name, string description, ItemStatus status, string productionType, string elaborationStatus)
        {
            this.Id = id;
            this.Code = code;
            this.Name = name;
            this.Description = description;
            this.ElaborationStatus = elaborationStatus;
            this.ProductionType = productionType;
            this.Status = status;

        }

        #endregion

        public string ElaborationStatus
        {
            get;
            private set;
        }

        public string ProductionType
        {
            get;
            private set;
        }

        public ItemStatus Status
        {
            get;
            private set;
        }

        public string Code
        {
            get;
            private set;
        }

        public string Description
        {
            get;
            private set;
        }

        public int Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }
    }
}
