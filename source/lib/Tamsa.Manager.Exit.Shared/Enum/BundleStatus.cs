// -----------------------------------------------------------------------
// <copyright file="BundleStatus.cs" company="Techint">
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
    public enum BundleStatus
    {
        /// <summary>
        /// Bundle In Process Bundle.IsOpen = 1, WithOut Sending
        /// </summary>
        [EnumMember]        
        InProcess  = 0,

        /// <summary>
        /// Sent Bundle, Bundle.IsOpen = 0, Sending.IsSent = 1
        /// </summary>
        [EnumMember]        
        Sent = 1,

        /// <summary>
        /// Pending Bundle for Send, Bundle.IsOpen = 0,  WithOut Sending
        /// </summary>
        [EnumMember]        
        Pending = 2,

        /// <summary>
        /// Error during Sending, Bundle.IsOpen = 0, Sending.IsSent = 0
        /// </summary>
        [EnumMember]        
        ErrorSending = 3


    }
}
