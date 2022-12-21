// -----------------------------------------------------------------------
// <copyright file="IItem.cs" company="Techint">
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
    public interface IItem
    {
        /// <summary>
        /// Item Id 
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Bundle Id
        /// </summary>
        int IdBundle { get; set; }

        /// <summary>
        /// Cradle Id
        /// </summary>
        int IdCradle { get; set; }

        /// <summary>
        /// Item Number
        /// </summary>
        int Number { get; set; }
       


        /// <summary>
        /// Batch Id
        /// </summary>
        int IdBatch { get; set; }


        /// <summary>
        /// Item Length
        /// </summary>
        float Length { get; set; }


        /// <summary>
        /// Item Weight
        /// </summary>
        float Weight { get; set; }


        /// <summary>
        /// Item ExitTime 
        /// </summary>
        DateTimeOffset ExitTime { get; set; }

        /// <summary>
        /// Extra Data for each Item
        /// </summary>
        Dictionary<string, object> ExtraData { get; set; }

        /// <summary>
        /// It Indicates if item is selected
        /// </summary>
        bool IsSelected { get; set; }



    }
}
