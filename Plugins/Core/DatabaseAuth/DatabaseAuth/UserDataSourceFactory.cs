using System;

namespace OpenCredential.Plugin.DatabaseAuth
{
    static class UserDataSourceFactory
    {
        public static IUserDataSource Create()
        {
            switch (Settings.GetDatabaseProvider())
            {
                case Settings.DatabaseProvider.MySql:
                    return new MySqlUserDataSource();
                case Settings.DatabaseProvider.PostgreSql:
                    return new PostgreSqlUserDataSource();
                default:
                    throw new NotSupportedException("Unsupported database provider.");
            }
        }
    }
}

