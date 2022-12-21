// -----------------------------------------------------------------------
// <copyright file="CradleState.cs" company="Tenaris">
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
    using System.Runtime.Serialization;

    /// <summary>
    /// Cradle state enum.
    /// </summary>
    [DataContract]
    [Serializable]
    public enum CradleState
    {
        [EnumMember]
        Disable = 0,

        [EnumMember]
        Enable = 1,
    }
}
