// -----------------------------------------------------------------------
// <copyright file="IExitManager.cs" company="Tenaris">
//  Tamsa.
// </copyright>
// <summary>
//  Define the IExitManager interface.
// </summary>
// -----------------------------------------------------------------------

namespace Tamsa.Manager.Exit.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using Tenaris.Manager.Forum.Shared;

    /// <summary>
    /// The IExitManager class.
    /// </summary>
    public interface IExitManager
    {
        
        /// <summary>
        /// IEnumerable cradles.
        /// </summary>
        IEnumerable<ICradle> Cradles { get; }
        
        /// <summary>
        /// Method for change cradle condition.
        /// </summary>
        /// <param name="idCradle">
        /// Id cradle to modify.
        /// </param>
        /// <param name="mode">
        /// Cradle mode.
        /// </param>
        /// <param name="state">
        /// Cradle name.
        /// </param>
        /// <param name="maximumPieces">
        /// Maximu pieces for bundle.
        /// </param>
        /// <returns></returns>
        bool ChangeCradleCondition(int idCradle, CradleMode? mode, CradleState? state, int? maximumPieces);

   
        /// <summary>
        /// Method for sending Bundle to PIP
        /// </summary>
        /// <param name="shiftDate"></param>
        /// <param name="shiftNumber"></param>
        /// <param name="idMachine"></param>
        /// <param name="codeITMachine"></param>
        /// <param name="idUser"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        //bool SendBundle( int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, out Dictionary<int, string> errors, out int BundleNumber);

        bool SendBundle(int idUser, string PipUser, int idBundle, string comments, bool rutaSEAS, string impresora, string groupElaborationState, string elaborationState, string location, string rejectionCode, out Dictionary<int, string> errors, out int BundleNumber);

        void UpdateViews(int idCradle);

        void CloseBundle(ICradle idCradle);

        /// <summary>
        /// Event cradle changed on manager.
        /// </summary>
        event EventHandler<CradleChangedEventArgs> CradleChanged;

    }
}