using System;
using System.Windows.Forms;

namespace WSR.Settings.GUI
{
    internal static class Program
    {
        /// <summary>
        ///     Main entry point
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServiceManager());
        }
    }
}
