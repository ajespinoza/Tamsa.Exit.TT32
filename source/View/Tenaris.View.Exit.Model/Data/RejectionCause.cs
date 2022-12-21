// -----------------------------------------------------------------------
// <copyright file="RejectionCause.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.Shared;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RejectionCause : IEntity
    {

        public string Code
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int IdItemStatus
        {
            get;
            set;
        }

        #region Constructor

        public RejectionCause(int id, string code, string name, string description, int idItemStatus)
        {
            this.Id = id;
            this.Code = code;
            this.Name = name;
            this.Description = description;
            this.IdItemStatus = idItemStatus;
        }

        #endregion
    }
}
