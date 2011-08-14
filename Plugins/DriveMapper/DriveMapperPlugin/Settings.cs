using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using pGina.Shared.Settings;

namespace pGina.Plugin.DriveMapper
{
    class DriveEntry
    {
        public string Drive { get; set; }
        public string UncPath { get; set; }
        public bool UseAltCreds { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    class Settings
    {

        private static dynamic m_settings = new DynamicSettings(DriveMapperPlugin.DriveMapperPluginUuid);

        public static List<DriveEntry> Load()
        {
            List<DriveEntry> driveList = new List<DriveEntry>();

            string[] drives = m_settings.Drives;
            string[] paths = m_settings.UncPaths;
            string[] useCreds = m_settings.UseAlternateCreds;
            string[] usernameList = m_settings.Usernames;
            string[] passList = m_settings.Passwords;

            for (int i = 0; i < drives.Count(); i++ )
            {
                driveList.Add(new DriveEntry
                {
                    Drive = drives[i],
                    UncPath = paths[i],
                    UseAltCreds = useCreds[i] == "0" ? false : true,
                    UserName = usernameList[i],
                    Password = passList[i]
                });
            }

            return driveList;
        }

        public static void Save( List<DriveEntry> driveList )
        {
            List<string> driveLetterList = new List<string>();
            List<string> uncPathList = new List<string>();
            List<string> useCredsList = new List<string>();
            List<string> usernameList = new List<string>();
            List<string> passList = new List<string>();
            foreach (DriveEntry d in driveList)
            {
                driveLetterList.Add(d.Drive);
                uncPathList.Add(d.UncPath);
                useCredsList.Add(d.UseAltCreds ? "1" : "0");
                usernameList.Add(d.UserName);
                passList.Add(d.Password);
            }
            m_settings.Drives = driveLetterList.ToArray();
            m_settings.UncPaths = uncPathList.ToArray();
            m_settings.UseAlternateCreds = useCredsList.ToArray();
            m_settings.Usernames = usernameList.ToArray();
            m_settings.Passwords = passList.ToArray();
        }
    }
}
