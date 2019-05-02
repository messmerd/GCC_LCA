﻿using System;
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
            Application.ApplicationExit += Application_ApplicationExit;
            
            Application.Run(new Main());
        }
        
        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (SerialInterface.Create().pnpWatcher != null)
            {
                SerialInterface.Create().pnpWatcher.Stop();
                SerialInterface.Create().pnpWatcher.Dispose();
            }
            
            // Maybe this will fix the PnP event problem...
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