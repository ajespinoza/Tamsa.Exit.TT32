// -----------------------------------------------------------------------
// <copyright file="ItemStatus.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.Serialization;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [DataContract]
    [Serializable]
    public enum ItemStatus
    {
        /// <summary>
        /// Good Item
        /// </summary>
        [EnumMember] 
        Good,

        /// <summary>
        ///  Rejected Item
        /// </summary>
        [EnumMember] 
        Rejected,

        /// <summary>
        /// Warned Item
        /// </summary>
        [EnumMember] 
        Warned,

        /// <summary>
        /// Pending Item
        /// </summary>
        [EnumMember] 
        Pending,

        /// <summary>
        /// Rejected Item
        /// </summary>
        [EnumMember] 
        Discarded,

        /// <summary>
        /// Holded Item
        /// </summary>
        [EnumMember] 
        Hold,


        /// <summary>
        /// Canceled Item
        /// </summary>
        [EnumMember] 
        Canceled,

        /// <summary>
        /// Reworked Item
        /// </summary>
        [EnumMember] 
        Reworked     
       
    }
}
