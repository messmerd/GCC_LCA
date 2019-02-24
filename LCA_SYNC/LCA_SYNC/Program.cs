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

            /*
            char sot = '\x02';
            char eot = '\x03';
            */

            /*
            List<char> c = new List<char>(); ;
            c.AddRange(data);
            List<byte> n = new List<byte>(Encoding.ASCII.GetBytes(data));
            

            String data = '\x01'.ToString();
            */
            /*
            string key = "qlc9KNMKi0mAyT4o";
            char[] key_char = new char[] { (char)0x01, (char)0xF0 };
            byte[] key_byte = new byte[] { 0x01, 240 };

            List<char> test = new List<char>();
            test.Add((char)0x01);
            test.Add((char)0xF0);

            StringBuilder sb_data = new StringBuilder(sot.ToString(), key_char.Length + 2);
            try
            {
                Console.WriteLine(sb_data.ToString());
                foreach (var b in key_byte)
                {
                    sb_data.Append(b);
                }
                
                Console.WriteLine(sb_data.ToString());
                sb_data.Append(key);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine(sb_data.ToString());
            sb_data.Append(eot);
            Console.WriteLine(sb_data.ToString());

            Console.WriteLine(BitConverter.ToString(key_byte));

            //List<byte> str = Encoding.UTF8.GetBytes(sb_data.ToString()).ToList();
            //Console.WriteLine(BitConverter.ToString(str.ToArray()));
            
            //string data = ('\x01').ToString() + ('\xF0').ToString() + "qlc9KNMKi0mAyT4o"


            //StringBuilder sb = new StringBuilder(sot, sb_data.Length + 2);
            //sb.Append(data);
            //sb.Append(eot);

            List<byte> str2 = Encoding.ASCII.GetBytes("\x02\x01\xF0qlc9KNMKi0mAyT4o\x03").ToList();
            Console.WriteLine(BitConverter.ToString(str2.ToArray()));


            Console.WriteLine();
            //System.Text.Encoding.
            //System.Text.Encoding.GetEncoding(28591);

            */

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