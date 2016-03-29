using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace proquota
{
    static class Program
    {
        public static string uPath = "";
        public static long uPath_size_max;
        private static EventLog evntlog = new EventLog();
        private static string logging = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\quota.log";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            if (args.Length != 2)
            {
                Program.Log("Parameter error:\nI need two parameters", EventLogEntryType.Error);
                return 1;
            }
            if (!System.IO.Directory.Exists(args[0]))
            {
                Program.Log(String.Format("Parameter error:\nCan't find:{0}", args[0]), EventLogEntryType.Error);
                return 2;
            }
            if (!long.TryParse(args[1], out uPath_size_max))
            {
                Program.Log(String.Format("Parameter error:\nCan't parse {0} to long", args[1]), EventLogEntryType.Error);
                Application.Exit();
            }
            if (uPath_size_max <= 10 * 1024)
            {
                Program.Log(String.Format("{0} is less or equal {1}\nQuota limit to low", args[1], 10 * 1024), EventLogEntryType.Warning);
                Application.Exit();
            }
            uPath = args[0];

            evntlog.Source = "proquota";
            evntlog.Log = "Application";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            return 0;
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = args.ExceptionObject as Exception;
            Log(String.Format("CurrentDomain_UnhandledException:\n{0}", e.ToString()), EventLogEntryType.Error);
            Application.Exit();
        }

        public static void Log(string text, EventLogEntryType type)
        {
            evntlog.WriteEntry(text, type);
            /*lock (evntlog)
            {
                System.IO.File.AppendAllText(logging, String.Format("{0:HH:mm:ss.ff} {1}{2}", DateTime.UtcNow,text, Environment.NewLine));
            }*/
        }
    }
}
