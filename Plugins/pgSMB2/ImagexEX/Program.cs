using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ImagexEX
{
    class Program
    {
        static int Main(string[] args)
        {
            List<string> exclude = new List<string>();
            List<string> dontCompress = new List<string>();
            string Imagex_cli = "";
            string sourcePath = "";
            string ini = String.Format("{0}\\{1}.ini", Path.GetTempPath(), DateTime.Now.Ticks);
            string error = "";

            for (int args_loop = 0; args_loop < args.Length; args_loop++)
            {
                if (args[args_loop].StartsWith("-x", StringComparison.CurrentCultureIgnoreCase))
                {
                    exclude.Add(Masquerade(args[args_loop].Remove(0, 2)));
                }
                else if (args[args_loop].StartsWith("-ms", StringComparison.CurrentCultureIgnoreCase))
                {
                    string[] ms = args[args_loop].Remove(0, 3).Split(';');
                    foreach (string extension in ms)
                    {
                        dontCompress.Add(String.Format(@"*.{0}", extension));
                    }
                }
                else
                {
                    if (args[args_loop].Equals("/CAPTURE", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sourcePath = args[args_loop+1];
                    }
                    if (args[args_loop].Equals("/CONFIG", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ini = args[args_loop + 1];
                    }
                    Imagex_cli += " " + args[args_loop];
                }
            }
            /*
            foreach (string excl in exclude)
            {
                Console.WriteLine("{0}\n\n", excl);
            }
            Console.WriteLine("{0}\n\n", Imagex_cli);
            Console.WriteLine("{0}\n\n", sourcePath);
            */

            if (Imagex_cli.ToUpper().Contains("/CAPTURE"))
            {
                try { File.Delete(ini); }
                catch { }
                using (StreamWriter sw = File.CreateText(ini))
                {
                    sw.WriteLine("[ExclusionList]");
                    foreach (string e in Exclude(sourcePath, exclude))
                    {
                        Console.WriteLine("exclude:{0}", e);
                        sw.WriteLine(e);
                    }

                    sw.WriteLine("[CompressionExclusionList]");
                    foreach (string e in dontCompress)
                    {
                        //Console.WriteLine("dont compress:{0}", e);
                        sw.WriteLine(e);
                    }
                }
            }

            Console.WriteLine("Run:imagex.exe {0}", Imagex_cli);
            int ret = RunWait(AppDomain.CurrentDomain.BaseDirectory + "\\imagex.exe", Imagex_cli, out error);
            if (ret != 0)
            {
                Console.WriteLine(error);
            }
            /*
            if (Imagex_cli.ToUpper().Contains("/CAPTURE"))
            {
                try { File.Delete(ini); }
                catch { }
            }*/

            return ret;
        }

        private static string Masquerade(string input)
        {
            input = input.Replace(@"\", @"\\");
            input = input.Replace(".", @"\.");
            input = input.Replace("*", ".*");
            input = input.Replace("?", ".");

            return input;
        }

        private static List<string> Exclude(string d, List<string> excl)
        {
            List<string> dirs = new List<string>() { d };
            dirs.AddRange(GetDirectories(d, excl));
            List<int> remove = new List<int>();
            List<string> ret = new List<string>();

            for (int dirs_loop = 0; dirs_loop < dirs.Count; dirs_loop++)
            {
                foreach (string exc in excl)
                {
                    if (exc.EndsWith(@"\") && Regex.IsMatch(dirs[dirs_loop], exc, RegexOptions.IgnoreCase))
                    {
                        if (!ret.Any(r => r.StartsWith(dirs[dirs_loop], StringComparison.CurrentCultureIgnoreCase)))
                        {
                            ret.Add(dirs[dirs_loop].TrimEnd(new char[] { '\\' }));
                        }
                        remove.Add(dirs_loop);

                        break;
                    }
                }
            }

            remove.Reverse();
            foreach (int rem in remove)
            {
                dirs.RemoveAt(rem);
            }
            remove.Clear();

            List<string> files = GetFiles(dirs);
            for (int files_loop = 0; files_loop < files.Count; files_loop++)
            {
                foreach (string exc in excl)
                {
                    string regex = exc.Replace(@"\\", "/");
                    string file = files[files_loop].Replace(@"\", "/");
                    //Console.WriteLine("{0} {1}", regex, file);
                    if (!exc.EndsWith(@"/") && Regex.IsMatch(file, regex, RegexOptions.IgnoreCase))
                    {
                        string s = Regex.Match(regex, @"[^/]*$", RegexOptions.IgnoreCase).Value;
                        string f = Regex.Match(file, @"[^/]*$", RegexOptions.IgnoreCase).Value;
                        //Console.WriteLine("{0} {1} {2} {3}", regex, file, s, f);
                        if (Regex.IsMatch(f, s, RegexOptions.IgnoreCase))
                        {
                            ret.Add(files[files_loop]);
                            break;
                        }
                    }
                }
            }

            return ret;
        }

        private static List<string> GetFiles(List<string> dirs)
        {
            List<string> ret = new List<string>();

            try
            {
                foreach (string dir in dirs)
                {
                    string[] files = Directory.GetFiles(dir, "*", System.IO.SearchOption.TopDirectoryOnly);
                    ret.AddRange(files.ToList());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetFiles() error:", ex.Message);
            }

            return ret;
        }

        private static List<string> GetDirectories(string d, List<string> exclude)
        {
            List<string> ret = new List<string>();

            try
            {
                string[] dirs = Directory.GetDirectories(d, "*", System.IO.SearchOption.TopDirectoryOnly);
                foreach (string dir in dirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    if (!dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                    {
                        ret.Add(dir + @"\");
                        if (!exclude.Any(e => e.EndsWith(@"\") && Regex.IsMatch(dir + @"\", e, RegexOptions.IgnoreCase)))
                        {
                            ret.AddRange(GetDirectories(dir, exclude));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDirectories() error:", ex.Message);
            }

            return ret;
        }

        private static int RunWait(string application, string arguments, out string stdmerge)
        {
            stdmerge = "";
            string stdout = "";
            string stderr = "";
            int ret = -1;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = application;
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            try
            {
                using (Process p = Process.Start(startInfo))
                {
                    using (StreamReader streamReader = p.StandardOutput)
                    {
                        stdout = streamReader.ReadToEnd();
                    }
                    using (StreamReader streamReader = p.StandardError)
                    {
                        stderr = streamReader.ReadToEnd();
                    }
                    p.WaitForExit();
                    ret = p.ExitCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RunWait failed error:{0}", ex.Message);
                return -1;
            }

            stdmerge += String.Format("{0}\r\n{1}", stdout.TrimEnd(), stderr.TrimEnd()).TrimEnd();

            return ret;
        }
    }
}
