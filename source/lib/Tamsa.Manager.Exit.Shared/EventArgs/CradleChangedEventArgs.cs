// -----------------------------------------------------------------------
// <copyright file="CradleChangedEventArgs.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the cradle state enum.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class provides data for the event CradleChanged.This class cannot be inherited
    /// </summary>
    [Serializable]
    public sealed class CradleChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CradleChangedEventArgs"/> class.
        /// </summary>
        /// <param name="cradle">
        /// The cradle object.
        /// </param>
        public CradleChangedEventArgs(ICradle cradle)
        {
            this.Cradle = cradle;
        }

        /// <summary>
        /// Gets cradle changed.
        /// </summary>
        public ICradle Cradle
        {
            get;
            private set;
        }
    }
}
