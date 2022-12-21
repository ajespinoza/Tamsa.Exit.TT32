// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZoneSubscription.cs" company="Tenaris">
//   Tenaris.
// </copyright>
// <summary>
//   Defines the ZoneSubscription class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.Framework.Factory;
    using Tenaris.Library.Framework.Remoting;

    /// <summary>
    /// Zone suscription class.
    /// </summary>
    public class ZoneSubscription
    {
        /// <summary>
        /// Remotable event for stage changed tracking manager event
        /// </summary>
        public readonly RemotableEvent<Tenaris.Manager.Tracking.Shared.ZoneChangedEventArgs> ZoneChangedSent = new RemotableEvent<Tenaris.Manager.Tracking.Shared.ZoneChangedEventArgs>();
    }
}
