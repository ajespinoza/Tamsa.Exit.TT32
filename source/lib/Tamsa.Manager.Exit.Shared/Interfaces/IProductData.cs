// -----------------------------------------------------------------------
// <copyright file="IProduct.cs" company="Techint">
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
    /// Defines Product Data into Cradle
    /// </summary>
    public interface IProductData
    {

        /// <summary>
        ///  Batch Id
        /// </summary>
        int IdBatch { get; set; }

        /// <summary>
        /// Specification Id
        /// </summary>
        int IdSpecification { get; set; }

        /// <summary>
        /// Product Data
        /// </summary>
        Dictionary<string, object> Data { get; set; }

    }
}
