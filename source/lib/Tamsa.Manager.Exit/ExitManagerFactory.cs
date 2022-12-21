// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExitManagerFactory.cs" company="Tenaris">
//   Tenaris.
// </copyright>
// <summary>
//   Defines the ExitManagerFactory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.Framework.Factory;
    using Tenaris.Manager.Forum.Shared;
    using Tamsa.Manager.Exit.Shared;


    /// <summary>
    /// Factory for Exit Manager
    /// </summary>
    public class ExitManagerFactory : Factory<IExitManager>
    {
        
        /// <summary>
        /// Initializes A Single Instance Of Exit Manager
        /// </summary>
        /// <returns>
        /// Exit Manager Reference
        /// </returns>
        public override IExitManager Create()
        {
            return ExitManager.Instance;
        }

        /// <summary>
        /// Override deserialization configuration.
        /// </summary>
        /// <param name="configString">
        /// Configuration XML Class.
        /// </param>
        /// <returns>
        /// ConfigSection class.
        /// </returns>
        protected override object DeserializeConfiguration(string configString)
        {
           return null;
        }

        /// <summary>
        /// Do configure method.
        /// </summary>
        protected override void DoConfigure()
        {

        }
    }
}

