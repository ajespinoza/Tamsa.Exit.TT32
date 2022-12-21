// -----------------------------------------------------------------------
// <copyright file="IBundle.cs" company="Techint">
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
    public interface IBundle
    {
        int IdBundle { get; }

        int IdCradle { get; }

        int IdBatch { get;  }

        int IdSpecification { get; }

        int BundleNumber { get; }

        string RejectionCause { get; set; }

        ItemStatus ItemStatus { get; set; }

        BundleStatus Status { get; set; }

        BundleState State { get; }

        DateTimeOffset CreationDate { get; }

        DateTimeOffset SendingDate { get; }

        int Pieces { get; }

        float Weight { get; }

        string Comments { get; }

        string ITLocation { get; set; }

        ILocation Location { get; set; }
     
        bool IsManual { get; }

        int Sent { get; set; }

        #region Methods

        void Close();

        void Open();

        void Send(int number);

        #endregion

        Dictionary<string, object> ExtraData { get; set; }

        #region Items

        IEnumerable<IItem> Items { get; }

        void AddItem(IItem item);

        void RemoveItem(IItem item);

        void AddItems(List<IItem> items);

        void RemoveItems();

        #endregion
      
    }
}
