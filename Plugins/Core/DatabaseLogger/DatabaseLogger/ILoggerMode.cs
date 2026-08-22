using System;
using System.Data.Common;
using MySqlConnector;
using Npgsql;

namespace OpenCredential.Plugin.DatabaseLogger
{
    interface ILoggerMode
    {
        bool Log(System.ServiceProcess.SessionChangeDescription changeDescription, OpenCredential.Shared.Types.SessionProperties properties);
        string TestTable();
        string CreateTable();
        void SetConnection(DbConnection connection);
    }

    class LoggerModeFactory
    {
        private static DbConnection m_conn;

        private LoggerModeFactory() { }

        public static ILoggerMode getLoggerMode(LoggerMode mode)
        {
            if (m_conn == null || m_conn.State != System.Data.ConnectionState.Open)
            {
                m_conn = CreateConnection();
            }

            ILoggerMode logger;
            if (mode == LoggerMode.EVENT)
                logger = new EventLogger();
            else if (mode == LoggerMode.SESSION)
                logger = new SessionLogger();
            else
                throw new ArgumentException("Invalid LoggerMode");

            logger.SetConnection(m_conn);
            return logger;
        }

        public static SessionLogger GetSessionLogger()
        {
            if (m_conn == null || m_conn.State != System.Data.ConnectionState.Open)
            {
                m_conn = CreateConnection();
            }

            var logger = new SessionLogger();
            logger.SetConnection(m_conn);
            return logger;
        }

        public static void closeConnection()
        {
            if (m_conn != null)
                m_conn.Close();
            m_conn = null;
        }

        public static DbConnection CreateConnection()
        {
            string connStr = BuildConnectionString();
            switch (Settings.GetDatabaseProvider())
            {
                case Settings.DatabaseProvider.PostgreSql:
                    return new NpgsqlConnection(connStr);
                default:
                    return new MySqlConnection(connStr);
            }
        }

        private static string BuildConnectionString()
        {
            if (Settings.GetDatabaseProvider() == Settings.DatabaseProvider.PostgreSql)
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = Settings.Store.Host,
                    Port = (int)Settings.GetPort(),
                    Username = Settings.Store.User,
                    Database = Settings.Store.Database,
                    Password = Settings.Store.GetEncryptedSetting("Password"),
                    SslMode = SslMode.Disable,
                    Pooling = true
                };

                return builder.ConnectionString;
            }

            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder
            {
                Server = Settings.Store.Host,
                Port = Settings.GetPort(),
                UserID = Settings.Store.User,
                Database = Settings.Store.Database,
                Password = Settings.Store.GetEncryptedSetting("Password")
            };

            return bldr.ConnectionString;
        }
    }
}

