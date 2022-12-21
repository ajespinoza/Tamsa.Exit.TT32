// -----------------------------------------------------------------------
// <copyright file="ILocation.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
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
    /// TODO: Update summary.
    /// </summary>
    public interface ILocation : IEntity
    {
        string ElaborationStatus { get; }

        string ProductionType { get; }

        ItemStatus Status { get; }

    }
}
