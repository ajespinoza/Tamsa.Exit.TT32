// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Specification.cs" company="Tenaris">
//   Tamsa.
// </copyright>
// <summary>
//   Defines the SSpecification class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Bundle class. 
    /// </summary>
    public class Specification
    {
        /// <summary>
        /// Id batch.
        /// </summary>
        public int IdBatch { get; private set; }

        /// <summary>
        /// Weight.
        /// </summary>
        public float KgMWeight { get; private set; }

        /// <summary>
        /// Nominal length.
        /// </summary>
        public float NominalLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Specification"/> class.
        /// </summary>
        public Specification()
        {
 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Specification"/> class.
        /// </summary>
        /// <param name="idBatch">
        /// Id batch.
        /// </param>
        /// <param name="kgMWeight">
        /// KgMWeight.
        /// </param>
        /// <param name="nominalLength">
        /// Nominal Length.
        /// </param>
        public Specification( int idBatch, float kgMWeight, float nominalLength)
        {
            this.IdBatch = idBatch;
            this.KgMWeight = kgMWeight;
            this.NominalLength = nominalLength;
        }

    }
}
