using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


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
            Application.ApplicationExit += Application_ApplicationExit;
            
            Application.Run(new Main());
        }
        
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (Arduino_Serial_Interface.SerialInterface.Create().pnpWatcher != null)
            {
                Arduino_Serial_Interface.SerialInterface.Create().pnpWatcher.Stop();
                Arduino_Serial_Interface.SerialInterface.Create().pnpWatcher.Dispose();
            }
            
            // Maybe this will fix the PnP event problem...
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }

        }
        
    }
}