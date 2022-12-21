// -----------------------------------------------------------------------
// <copyright file="Utility.cs" company="Techint">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Tenaris.View.Exit.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;
  /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Utility
    {

        #region Liberar Memoria
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize, int maximumWorkingSetSize);
        public static void ReleaseMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }
        #endregion
    }
}
