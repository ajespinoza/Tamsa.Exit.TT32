// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExitConfiguration.cs" company="Tenaris">
//   Tamsa.
// </copyright>
// <summary>
//   Defines the Compare class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tamsa.Manager.Exit.Shared;
    using Tenaris.Manager.Tracking.Shared;

    public class Compare : IEqualityComparer<Tenaris.Manager.Tracking.Shared.Tracking>
    {
        /// <summary>
        /// The comparer method
        /// </summary>
        /// <param name="x">
        /// Tracking object x.
        /// </param>
        /// <param name="y">
        /// Tracking object y.
        /// </param>
        /// <returns>
        /// Result value.
        /// </returns>
        public bool Equals(Tenaris.Manager.Tracking.Shared.Tracking x, Tenaris.Manager.Tracking.Shared.Tracking y)
        {
            if (x.Id == y.Id )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get Tracking.
        /// </summary>
        /// <param name="item">
        /// Tracking value.
        /// </param>
        /// <returns>
        /// Return int value.
        /// </returns>
        public int GetHashCode(Tenaris.Manager.Tracking.Shared.Tracking item)
        {
            return item.Id.GetHashCode();
        }
    }
}
