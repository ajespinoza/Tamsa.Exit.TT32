// -----------------------------------------------------------------------
// <copyright file="CradleMode.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the cradle mode enum.
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
    /// Cradle mode enum.
    /// </summary>
    [DataContract]
    [Serializable]
    public enum CradleMode
    {
        [EnumMember]
        Automatic = 0,

        [EnumMember]
        Manual = 1,
    }
}
