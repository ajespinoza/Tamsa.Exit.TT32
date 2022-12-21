// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExitServer.cs" company="Tenaris">
//   Tamsa.
// </copyright>
// <summary>
//   Defines the ExitServer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Tenaris.Library.Framework.Factory;
    using Tamsa.Manager.Exit.Shared;
    using Tenaris.Manager.Forum.Shared;
    using Tenaris.Library.Log;


    /// <summary>
    /// The exit application server class.
    /// </summary>
    class ExitServer
    {
        /// <summary>
        /// The console application for to test the exit application manager. 
        /// </summary>
        /// <param name="args">
        /// The args object.
        /// </param>
        static void Main(string[] args)
        {
            try
            {
                // Instantiating manager using factory.
                IFactory<IExitManager> exitFactory = FactoryProvider.Instance.CreateFactory<IExitManager>("Tamsa.Manager.Exit.ExitManager");
                IExitManager exit = exitFactory.Create();

                // Cast with Tenaris.Library.Common.IManager simlate forum manager.
                IManagerBase manager = (IManagerBase)exit;

                System.Runtime.Remoting.RemotingConfiguration.Configure(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, false);
                Trace.Message("Exit Manager object created");

                // Instantiating  manager.
                manager.Initialize();
                Trace.Message("Initialized...");

                // Active manager.
                manager.Activate();
                Trace.Message("Activated...");

                System.Runtime.Remoting.RemotingServices.Marshal((MarshalByRefObject)manager, "Tamsa.Manager.ExitManager.soap");
                Trace.Message("Published...");

                Console.WriteLine("ESPERANDO ...  PRESIONE CUALQUIER TECLA PARA TERMINAR");
                Console.ReadLine();

                // Deactive manager.
                manager.Deactivate();

                // Uninstantiating manager.
                manager.Uninitialize();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Excepción = " + ex.Message);
                Console.WriteLine("Stack = " + ex.StackTrace);
                Trace.Error(ex.Message);
            }
        }
    }
}
