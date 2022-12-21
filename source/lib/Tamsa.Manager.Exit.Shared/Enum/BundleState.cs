// -----------------------------------------------------------------------
// <copyright file="BundleState.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the bundle state enum.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.Serialization;

    /// <summary>
    /// Bundle state enum.
    /// </summary>
    [DataContract]
    [Serializable]
    public enum BundleState
    {
        [EnumMember]
        Open = 0,

        [EnumMember]
        Closed = 1,
    }
}
