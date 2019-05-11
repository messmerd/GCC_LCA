using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;


namespace LCA_SYNC
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += Application_ApplicationExit;  // Might not be needed 
            
            Application.Run(new Main());
        }
        
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            // This event handler is meant to fix the weird problem with the PnP watcher staying active even after the program closes.
            // There are a few methods similar to this one throughout the code for this program. One of them works and 
            // the rest probably don't do anything, but I haven't taken the time see which ones are useless and remove them.

            if (SerialInterface.Create().pnpWatcher != null)
            {
                SerialInterface.Create().pnpWatcher.Stop();
                SerialInterface.Create().pnpWatcher.Dispose();
            }
            
            // Maybe this fixes the PnP event problem...
            if (Application.MessageLoop)
            {
                // WinForms app
                Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }

        }
        
    }
}