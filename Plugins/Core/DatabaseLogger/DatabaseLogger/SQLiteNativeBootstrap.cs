using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenCredential.Plugin.DatabaseLogger
{
    internal static class SQLiteNativeBootstrap
    {
        private static bool s_initialized;
        private static readonly object s_syncRoot = new object();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static void EnsureInitialized()
        {
            lock (s_syncRoot)
            {
                if (s_initialized)
                    return;

                string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrWhiteSpace(assemblyDirectory))
                    return;

                string runtimeFolder = Environment.Is64BitProcess ? "win-x64" : "win-x86";
                string nativeDirectory = Path.Combine(assemblyDirectory, "runtimes", runtimeFolder, "native");

                if (!Directory.Exists(nativeDirectory))
                    return;

                string currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                string normalizedNativeDirectory = nativeDirectory.TrimEnd(Path.DirectorySeparatorChar);

                if (currentPath.IndexOf(normalizedNativeDirectory, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    Environment.SetEnvironmentVariable("PATH", normalizedNativeDirectory + Path.PathSeparator + currentPath);
                }

                SetDllDirectory(nativeDirectory);
                s_initialized = true;
            }
        }

        public static string GetNativeDirectory()
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrWhiteSpace(assemblyDirectory))
                return string.Empty;

            string runtimeFolder = Environment.Is64BitProcess ? "win-x64" : "win-x86";
            return Path.Combine(assemblyDirectory, "runtimes", runtimeFolder, "native");
        }

        public static string GetNativeDllPath()
        {
            return Path.Combine(GetNativeDirectory(), "e_sqlite3.dll");
        }
    }
}

