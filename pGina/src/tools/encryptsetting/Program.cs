using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace encryptsetting
{
    class Program
    {
        private static dynamic m_settings = null;

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                string assemblyname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                Console.WriteLine("{0} <setting> <encryptthis> [GUID]", assemblyname);
                Console.ResetColor();
                Console.WriteLine("   {0} SearchPW \"my p@asswor&\" 0f52390b-c781-43ae-bd62-553c77fa4cf7", assemblyname);
                Console.WriteLine("      add setting SearchPW and assign \"my p@asswor&\" to pluging 0f523...f7", assemblyname);
                Console.WriteLine("   {0} SearchPW \"my p@asswor&\"", assemblyname);
                Console.WriteLine("      add setting SearchPW and assign \"my p@asswor&\" to pgina config", assemblyname);
                return 3;
            }

            if (args.Length > 2 && !String.IsNullOrEmpty(args[2]))
            {
                try
                {
                    Guid GUID = Guid.Parse(args[2]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("can't parse GUID:{0}", ex.Message);
                    return 4;
                }
            }

            try
            {
                if (args.Length > 2 && !String.IsNullOrEmpty(args[2]))
                    m_settings = new pGina.Shared.Settings.pGinaDynamicSettings(Guid.Parse(args[2]));
                else
                    m_settings = new pGina.Shared.Settings.pGinaDynamicSettings();

                m_settings.SetDefaultEncryptedSetting(args[0], args[1]);
                Abstractions.Settings.DynamicSettings setting = m_settings;

                if (!setting.GetEncryptedSetting(args[0]).Equals(args[1]))
                {
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception:{0}", ex.Message);
                return 2;
            }

            return 0;
        }
    }
}
