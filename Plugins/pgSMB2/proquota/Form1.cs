using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace proquota
{
    public partial class Form1 : Form
    {
        private string over_max_warning_title = "";
        private string over_max_error_title = "";
        private string over_max_error_text = "";
        private string over_max_free = "";
        private string over_max_exceeded = "";
        private string over_max_userprofile = "";
        private string over_max_calculate_text = "";
        private string exclude_dir = "";

        private System.Windows.Forms.Form top;
        private Thread msg;
        private Thread onchanged;
        private long uPath_rest;
        private bool? over_max = null;
        private bool mouseover = false;
        private long mouseover_timer = DateTime.UtcNow.Ticks;
        private Point mouse_pos = Point.Empty;
        private ReaderWriterLockSlim mouseoverlock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim Calculon_Lock = new ReaderWriterLockSlim(); //does it calc == locked, or sleep == unlocked
        private ReaderWriterLockSlim LastEventTriggered_lock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim BallonTip_Lock = new ReaderWriterLockSlim();
        private long LastEventTriggered = 0;
        private const int sleep_timer = 100;
        private const int Balloon_sleep = 5000;
        private long mouse_hover = 0;

        #region pinvoke
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool ShutdownBlockReasonCreate(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] string pwszReason);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool ShutdownBlockReasonDestroy(IntPtr hWnd);

        ///<summary>
        /// Flags that define appearance and behaviour of a standard message box displayed by a call to the MessageBox function.
        /// </summary>
        [Flags]
        public enum MessageBoxOptions : uint
        {
            OkOnly = 0x000000,
            OkCancel = 0x000001,
            AbortRetryIgnore = 0x000002,
            YesNoCancel = 0x000003,
            YesNo = 0x000004,
            RetryCancel = 0x000005,
            CancelTryContinue = 0x000006,
            IconHand = 0x000010,
            IconQuestion = 0x000020,
            IconExclamation = 0x000030,
            IconAsterisk = 0x000040,
            UserIcon = 0x000080,
            IconWarning = IconExclamation,
            IconError = IconHand,
            IconInformation = IconAsterisk,
            IconStop = IconHand,
            DefButton1 = 0x000000,
            DefButton2 = 0x000100,
            DefButton3 = 0x000200,
            DefButton4 = 0x000300,
            ApplicationModal = 0x000000,
            SystemModal = 0x001000,
            TaskModal = 0x002000,
            Help = 0x004000,
            NoFocus = 0x008000,
            SetForeground = 0x010000,
            DefaultDesktopOnly = 0x020000,
            Topmost = 0x040000,
            Right = 0x080000,
            RTLReading = 0x100000
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint MessageBox(IntPtr hWnd, String text, String caption, MessageBoxOptions options);
        #endregion

        #region http://csharptest.net/1043/how-to-prevent-users-from-killing-your-service-process/
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool GetKernelObjectSecurity(IntPtr Handle, int securityInformation, [Out] byte[] pSecurityDescriptor, uint nLength, out uint lpnLengthNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool SetKernelObjectSecurity(IntPtr Handle, int securityInformation, [In] byte[] pSecurityDescriptor);

        internal RawSecurityDescriptor GetProcessSecurityDescriptor(IntPtr processHandle)
        {
            const int DACL_SECURITY_INFORMATION = 0x00000004;
            byte[] psd = new byte[0];
            uint bufSizeNeeded;
            RawSecurityDescriptor ret = null;

            // Call with 0 size to obtain the actual size needed in bufSizeNeeded
            GetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, psd, 0, out bufSizeNeeded);
            if (bufSizeNeeded < 0 || bufSizeNeeded > short.MaxValue)
            {
                Program.Log(String.Format("GetKernelObjectSecurity get size error:{0}", Abstractions.WindowsApi.pInvokes.LastError()), EventLogEntryType.Error);
            }
            else
            {
                // Allocate the required bytes and obtain the DACL
                psd = new byte[bufSizeNeeded];
                if (!GetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, psd, bufSizeNeeded, out bufSizeNeeded))
                {
                    Program.Log(String.Format("GetKernelObjectSecurity get DACL error:{0}", Abstractions.WindowsApi.pInvokes.LastError()), EventLogEntryType.Error);
                }
                else
                {
                    // Use the RawSecurityDescriptor class from System.Security.AccessControl to parse the bytes:
                    ret = new RawSecurityDescriptor(psd, 0);
                }
            }

            return ret;
        }

        internal bool SetProcessSecurityDescriptor(IntPtr processHandle, RawSecurityDescriptor dacl)
        {
            const int DACL_SECURITY_INFORMATION = 0x00000004;
            byte[] rawsd = new byte[dacl.BinaryLength];
            dacl.GetBinaryForm(rawsd, 0);
            if (!SetKernelObjectSecurity(processHandle, DACL_SECURITY_INFORMATION, rawsd))
            {
                Program.Log(String.Format("SetKernelObjectSecurity error:{0}", Abstractions.WindowsApi.pInvokes.LastError()), EventLogEntryType.Error);
                return false;
            }

            return true;
        }
        #endregion

        public void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Program.Log(String.Format("Application_ThreadException:\n{0}", e.Exception.ToString()), EventLogEntryType.Error);
            Application.Exit();
        }

        public Form1()
        {
            InitializeComponent();

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            setACE();
            getSettings();

            this.Text = over_max_error_title;

            // wait until userinit doesnt run anymore
            //Program.Log("wait for userinit", EventLogEntryType.Information);
            try
            {
                for (int x = 0; x < 60; x++ )
                {
                    if (Process.GetProcessesByName("userinit").Length == 0)
                    {
                        if (Process.GetProcessesByName("explorer").Length > 0)
                        {
                            break;
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch
            {
                Program.Log("exception userinit", EventLogEntryType.Information);
            }
            //Program.Log("no userinit", EventLogEntryType.Information);

            notifyIcon1.MouseMove += new MouseEventHandler(OnMouseOver);
            notifyIcon1.BalloonTipShown += new EventHandler(OnBalloonTipShown);

            // polling
            onchanged = new Thread(Calculon);
            onchanged.Start();

            //mouse pos polling
            Thread mousepos = new Thread(MousePos);
            mousepos.Start();

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = proquota.Program.uPath;
            watcher.IncludeSubdirectories = true;
            watcher.Filter= "";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }

        private void setACE()
        {
            IntPtr hProcess = System.Diagnostics.Process.GetCurrentProcess().Handle; //get handle
            RawSecurityDescriptor RawSecDesc = GetProcessSecurityDescriptor(hProcess); //get current SDDL
            if (RawSecDesc == null)
            {
                Application.Exit();
            }

            RawSecurityDescriptor RawSecDesc_new = new RawSecurityDescriptor("D:"); //start from scratch
            foreach (CommonAce genACE in RawSecDesc.DiscretionaryAcl)
            {
                if (genACE.SecurityIdentifier.IsWellKnown(WellKnownSidType.LogonIdsSid)) //the logon Session
                {
                    genACE.AccessMask &= ~(0x00040000 /*WRITE_DAC*/| 0x00080000 /*WRITE_OWNER*/| 0x0001 /*PROCESS_CREATE_PROCESS*/);
                }
                if (genACE.SecurityIdentifier == WindowsIdentity.GetCurrent().User) //the logged in user
                {
                    genACE.AccessMask &= ~(0x00040000 /*WRITE_DAC*/| 0x00080000 /*WRITE_OWNER*/| 0x0001 /*PROCESS_CREATE_PROCESS*/);
                }

                RawSecDesc_new.DiscretionaryAcl.InsertAce(0, genACE);
            }

            if (!SetProcessSecurityDescriptor(hProcess, RawSecDesc_new))
            {
                Application.Exit();
            }
        }

        private void getSettings()
        {
            try
            {
                Abstractions.Settings.DynamicSettings settings = new Abstractions.Settings.DynamicSettings(pGina.Plugin.pgSMB2.PluginImpl.PluginUuid, "global");
                Dictionary<string,string> set = settings.GetSettings(new string[] {});
                foreach (string text in set["MaxStoreText"].Split('\0'))
                {
                    string name = text.Split(new char[] { '\t' }, 2).First();
                    string value = text.Split(new char[] { '\t' }, 2).Last();

                    switch (name)
                    {
                        case "MaxStoreUserprofile":
                            over_max_userprofile = value.Trim();
                            break;
                        case "MaxStorefree":
                            over_max_free = value.Trim();
                            break;
                        case "MaxStoreExceeded":
                            over_max_exceeded = value.Trim();
                            break;
                        case "MaxStoreWarningTitle":
                            over_max_warning_title = value.Trim();
                            break;
                        case "MaxStoreErrorTitle":
                            over_max_error_title = value.Trim();
                            break;
                        case "MaxStoreErrorText":
                            over_max_error_text = value.Trim();
                            break;
                        case "MaxStoreCalculateText":
                            over_max_calculate_text = value.Trim();
                            break;
                    }
                }
                exclude_dir = set["MaxStoreExclude"];

                top = new System.Windows.Forms.Form();
                msg = new Thread(() => MsgBox("is not null", "init"));
            }
            catch (Exception ex)
            {
                Program.Log(String.Format("Error in getSettings():{0}", ex.ToString()), EventLogEntryType.Error);
                Application.Exit();
            }
        }

        private void OnBalloonTipShown(object Sender, EventArgs e)
        {
            Thread ballon = new Thread(BalloonTipShown);
            ballon.Start();
        }

        private void BalloonTipShown()
        {
            try
            {
                if (BallonTip_Lock.TryEnterWriteLock(Convert.ToInt32(Balloon_sleep*0.75)))
                {
                    //Program.Log(String.Format("BalloonTipShown thread:{0} Locked", Thread.CurrentThread.ManagedThreadId), EventLogEntryType.Error);
                    Thread.Sleep(Balloon_sleep);
                }
            }
            catch { }
            finally
            {
                if (BallonTip_Lock.IsWriteLockHeld)
                {
                    BallonTip_Lock.ExitWriteLock();
                    //Program.Log(String.Format("BalloonTipShown thread:{0} unlocked", Thread.CurrentThread.ManagedThreadId), EventLogEntryType.Error);
                }
            }
        }

        private void OnMouseOver(object Sender, MouseEventArgs e)
        {
            //win 10 crap triggers a MouseMove event on the notify icon when user got logged in
            //and if you call the taskmanager from the taskbar
            long now = DateTime.UtcNow.Ticks;
            if (now > mouse_hover)
            {
                //Program.Log(String.Format("mouseover event trashed"), EventLogEntryType.Information);
                mouse_hover = now + (20 * TimeSpan.TicksPerMillisecond);
                return;
            }
            //Program.Log(String.Format("mouseover event"), EventLogEntryType.Information);
            Thread mouse_over = new Thread(() => MouseOver(Cursor.Position));
            mouse_over.Start();
        }

        private void MouseOver(Point mouse_point)
        {
            long now = DateTime.UtcNow.Ticks;
            long delay = 3 * 1000 * TimeSpan.TicksPerMillisecond;

            try
            {
                if (mouseoverlock.TryEnterWriteLock(0))
                {
                    mouse_pos = mouse_point;
                    if (mouseover_timer + delay < now)
                    {
                        mouseover = true;
                        mouseover_timer = now;
                        //Program.Log(String.Format("mouseover enabled:{0} mouse_pos:{1}", Thread.CurrentThread.ManagedThreadId, mouse_pos.ToString()), EventLogEntryType.Information);
                    }
                }
            }
            catch
            {
                return;
            }
            finally
            {
                if (mouseoverlock.IsWriteLockHeld)
                {
                    mouseoverlock.ExitWriteLock();
                }
            }
        }

        private void MousePos()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Point mouse_pos_cur = Cursor.Position;
                Point mouse_pos_old = Point.Empty;

                try
                {
                    if (mouseoverlock.TryEnterReadLock(0))
                    {
                        mouse_pos_old = mouse_pos;
                    }
                }
                catch
                {
                    return;
                }
                finally
                {
                    if (mouseoverlock.IsReadLockHeld)
                    {
                        mouseoverlock.ExitReadLock();
                    }
                }

                if (!mouse_pos_old.IsEmpty && mouse_pos_cur.X == mouse_pos_old.X && mouse_pos_cur.Y == mouse_pos_old.Y)
                {
                    //Program.Log(String.Format("init MouseOver mouse_pos_old={0} mouse_pos_cur={1}", mouse_pos_old.ToString(), mouse_pos_cur.ToString()), EventLogEntryType.Information);
                    MouseOver(mouse_pos_cur);
                }
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // http://stackoverflow.com/questions/239988/filesystemwatcher-vs-polling-to-watch-for-file-changes
            // The FileSystemWatcher may also miss changes during busy times, if the number of queued changes overflows the buffer provided.
            // This is not a limitation of the .NET class per se, but of the underlying Win32 infrastructure.
            // In our experience, the best way to minimize this problem is to dequeue the notifications as quickly as possible and deal with them on another thread.
            // As mentioned by @ChillTemp above, the watcher may not work on non-Windows shares. For example, it will not work at all on mounted Novell drives.
            // I agree that a good compromise is to do an occasional poll to pick up any missed changes.

            // sure you can use override FileSystemWatcher.OnError (FileSystemWatcher.InternalBufferSize, InternalBufferOverflowException)
            // and do a "poll" (Calculon()) to reget control of all files again
            // but than you need to call your threads in order, or you will mess up
            // real world: delete file -> create a file -> modify file
            // unsorted threads: create a file -> modify file -> delete file

            // Ill do it the other way arround
            // As soon as the events triggered drops below a limit. Ill do a "poll" (Calculon()) on all files
            Thread onchanged = new Thread(() => OnChanged(e, DateTime.UtcNow.Ticks));
            onchanged.Start();
        }

        private void OnChanged(FileSystemEventArgs e, long ticks)
        {
            string path = e.FullPath;
            string path_root = Path.GetPathRoot(path);

            // is this file or folder part of our exlusion
            while (!path.Equals(path_root))
            {
                if (Regex.IsMatch(path, exclude_dir))
                {
                    //Program.Log(String.Format("path exclude:{0}", e.FullPath), EventLogEntryType.Warning);
                    return;
                }
                path = Path.GetDirectoryName(path);
            }
            // is it a directory
            try
            {
                FileAttributes attr = File.GetAttributes(e.FullPath);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    //Program.Log(String.Format("path directory:{0}", e.FullPath), EventLogEntryType.Warning);
                    return;
                }
            }
            catch { }

            // its a file and we care about it
            // the time may not be exact but thats not so important
            try
            {
                if (LastEventTriggered_lock.TryEnterWriteLock(-1))
                {
                    LastEventTriggered = ticks;
                    //Program.Log(String.Format("LastEventTriggered = {0}\n{1}", LastEventTriggered, e.FullPath), EventLogEntryType.Information);
                }
            }
            catch { }
            finally
            {
                LastEventTriggered_lock.ExitWriteLock();
            }
        }

        private void Calculon()
        {
            bool domouse = true;
            pGina.Plugin.pgSMB2.Roaming ro = new pGina.Plugin.pgSMB2.Roaming();
            long trigger = 500 * TimeSpan.TicksPerMillisecond;
            long poll = 60 * 1000 * TimeSpan.TicksPerMillisecond;
            long lastrun = -1;
            int sleep_write_lock = Convert.ToInt32(sleep_timer/10);
            long timer = 0;

            while (true)
            {
                Thread.Sleep(sleep_timer);

                try
                {
                    if (mouseoverlock.TryEnterReadLock(-1))
                    {
                        if (mouseover)
                        {
                            domouse = true;
                            mouseover = false;
                        }
                    }
                }
                catch { }
                finally
                {
                    mouseoverlock.ExitReadLock();
                }

                try
                {
                    if (LastEventTriggered_lock.TryEnterWriteLock(sleep_write_lock))
                    {
                        long nowticks = DateTime.UtcNow.Ticks;
                        if (!domouse) // mouseover always force a recalculation
                        {
                            //Program.Log(String.Format("{0} == {1}", LastEventTriggered, lastrun), EventLogEntryType.Information);
                            if (LastEventTriggered == lastrun) // nothing happend
                            {
                                continue;
                            }
                            // es ist was pasiert
                            //Program.Log(String.Format("{0} > {1}", trigger, nowticks - LastEventTriggered), EventLogEntryType.Information);
                            if (trigger > nowticks - LastEventTriggered) // less than 500 ms have passed since the last event
                            {
                                if (timer == 0) // set timer to current time
                                {
                                    timer = nowticks;
                                }

                                //Program.Log(String.Format("{0} > {1}", nowticks - timer, poll), EventLogEntryType.Information);
                                if (nowticks - timer < poll) // less than 60 sec have passed since we wait for events to no longer occur
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                catch { }
                finally
                {
                    if (LastEventTriggered_lock.IsWriteLockHeld)
                    {
                        LastEventTriggered_lock.ExitWriteLock();
                    }
                }

                if (domouse)
                {
                    try
                    {
                        if (BallonTip_Lock.TryEnterReadLock(0))
                        {
                            notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1}", over_max_userprofile, over_max_calculate_text), ToolTipIcon.Info);
                        }
                    }
                    catch { }
                    finally
                    {
                        if (BallonTip_Lock.IsReadLockHeld)
                        {
                            BallonTip_Lock.ExitReadLock();
                        }
                    }
                }
                //Program.Log(String.Format("Calculon lastrun = {0}", lastrun), EventLogEntryType.Information);
                lastrun = LastEventTriggered;
                timer = 0;

                try
                {
                    if (Calculon_Lock.TryEnterWriteLock(-1))
                    {
                        long uPath_size = ro.GetDirectorySize(proquota.Program.uPath, exclude_dir);
                        if (uPath_size == 0)
                        {
                            continue;
                        }
                        uPath_size = Convert.ToInt64(uPath_size / 1024);
                        bool? over_max_old = over_max;
                        uPath_rest = Program.uPath_size_max - uPath_size;

                        if (uPath_size < Program.uPath_size_max - (Convert.ToInt64(Program.uPath_size_max / 20))) //fits
                        {
                            over_max = null;
                        }
                        else //fits barely
                        {
                            if (uPath_size > Program.uPath_size_max) //oversized
                            {
                                over_max = true;
                            }
                            else
                            {
                                over_max = false;
                            }
                        }

                        if (over_max != over_max_old)
                        {
                            switch (over_max)
                            {
                                case null:
                                    notifyIcon1.Icon = new Icon(Properties.Resources._1, 16, 16);
                                    notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1} {2} {3:0.00} {4} {5}", over_max_userprofile, uPath_rest.ToString(), "KBytes", (double)uPath_rest / 1024, "MBytes", over_max_free), ToolTipIcon.Info);
                                    if (msg.IsAlive)
                                    {
                                        top.Close();
                                    }
                                    break;
                                case false:
                                    notifyIcon1.Icon = new Icon(Properties.Resources._2, 16, 16);
                                    notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1} {2} {3:0.00} {4} {5}", over_max_warning_title, uPath_rest.ToString(), "KBytes", (double)uPath_rest / 1024, "MBytes", over_max_free), ToolTipIcon.Info);
                                    if (msg.IsAlive)
                                    {
                                        top.Close();
                                    }
                                    break;
                                case true:
                                    notifyIcon1.Icon = new Icon(Properties.Resources._3, 16, 16);
                                    notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1} {2} {3:0.00} {4} {5}", over_max_error_title, uPath_rest.ToString(), "KBytes", (double)uPath_rest / 1024, "MBytes", over_max_exceeded), ToolTipIcon.Info);
                                    if (!msg.IsAlive)
                                    {
                                        msg = new Thread(() => MsgBox(over_max_error_text.Replace("\\n", System.Environment.NewLine), over_max_error_title));
                                        msg.Start();
                                    }
                                    break;
                            }
                            domouse = false;
                        }
                        else
                        {
                            if (domouse)
                            {
                                //Program.Log("mouseover", EventLogEntryType.Information);
                                switch (over_max)
                                {
                                    case null:
                                        notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1} {2} {3:0.00} {4} {5}", over_max_userprofile, uPath_rest.ToString(), "KBytes", (double)uPath_rest / 1024, "MBytes", over_max_free), ToolTipIcon.Info);
                                        break;
                                    case false:
                                        notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1} {2} {3:0.00} {4} {5}", over_max_warning_title, uPath_rest.ToString(), "KBytes", (double)uPath_rest / 1024, "MBytes", over_max_free), ToolTipIcon.Info);
                                        break;
                                    case true:
                                        notifyIcon1.ShowBalloonTip(1000, "", String.Format("{0}\n{1} {2} {3:0.00} {4} {5}", over_max_error_title, uPath_rest.ToString(), "KBytes", (double)uPath_rest / 1024, "MBytes", over_max_exceeded), ToolTipIcon.Info);
                                        break;
                                }
                                domouse = false;
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    Calculon_Lock.ExitWriteLock();
                }
            }
        }

        private void MsgBox(string body, string title)
        {
            top = new System.Windows.Forms.Form();
            top.TopMost = true;
            top.ShowInTaskbar = false;
            MessageBox(top.Handle, body, title, MessageBoxOptions.OkOnly | MessageBoxOptions.Topmost | MessageBoxOptions.DefButton1 | MessageBoxOptions.IconError | MessageBoxOptions.SetForeground);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            ShutdownBlockReasonCreate(this.Handle, String.Format("{0}\n{1}", over_max_userprofile, over_max_calculate_text));

            bool? l_over_max = true;
            try
            {
                if (mouseoverlock.TryEnterWriteLock(-1))
                {
                    //Program.Log("Onclosing set mouse true", EventLogEntryType.Information);
                    mouseover = true;
                }
            }
            catch { }
            finally
            {
                mouseoverlock.ExitWriteLock();
            }

            Thread.Sleep(Convert.ToInt32(sleep_timer + (sleep_timer / 4)));

            try
            {
                if (Calculon_Lock.TryEnterReadLock(-1))
                {
                    l_over_max = over_max;
                }
            }
            catch { }
            finally
            {
                Calculon_Lock.ExitReadLock();
            }

            //Program.Log("Calculon finished", EventLogEntryType.Information);

            if (l_over_max == true)
            {
                ShutdownBlockReasonCreate(this.Handle, over_max_error_text.Replace("\\n", System.Environment.NewLine).Remove(128));
                top.Close();
                msg = new Thread(() => MsgBox(over_max_error_text.Replace("\\n", System.Environment.NewLine), over_max_error_title));
                msg.Start();
            }
            else
            {
                top.Close();
                ShutdownBlockReasonDestroy(this.Handle);
                e.Cancel = false;
            }
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();
        }
    }
}
