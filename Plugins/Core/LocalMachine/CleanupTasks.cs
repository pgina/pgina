using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;

namespace pGina.Plugin.LocalMachine
{
    public enum CleanupAction
    {
        DELETE_PROFILE, SCRAMBLE_PASSWORD
    }

    class CleanupTask
    {
        private static readonly string DELIMITER = "|@|";

        private string m_username;
        public string UserName { get { return m_username; } }

        private CleanupAction m_action;
        public CleanupAction Action { get { return m_action; } }

        private DateTime m_time;
        public DateTime TimeStamp { get { return m_time; } }

        public CleanupTask(string user, CleanupAction action)
        {
            this.m_action = action;
            this.m_username = user.ToUpper();
            this.m_time = DateTime.Now;
        }

        public static CleanupTask FromRegString(string regString)
        {
            string[] parts = regString.Split(new string[] { DELIMITER }, StringSplitOptions.RemoveEmptyEntries);

            string uname = parts[0];
            DateTime timestamp = DateTime.FromBinary(long.Parse(parts[1]));
            CleanupAction action;

            // This check is for backward compatibility.  If there happens to be stuff left over
            // from older versions of pGina, there will not be a third part in the string, in
            // which case we choose based on the settings;
            if (parts.Length > 2)
            {
                action = (CleanupAction)Enum.Parse(typeof(CleanupAction), parts[2]);
            }
            else
            {
                bool deleteProfiles = Settings.Store.RemoveProfiles;
                if (deleteProfiles) action = CleanupAction.DELETE_PROFILE;
                else action = CleanupAction.SCRAMBLE_PASSWORD;
            }

            CleanupTask task = new CleanupTask(uname, action);
            task.m_time = timestamp;

            return task;
        }

        public string ToRegString()
        {
            string[] parts = { m_username, m_time.ToBinary().ToString(), ((int)m_action).ToString() };
            return string.Join(DELIMITER, parts);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", m_username, m_action);
        }
    }

    class CleanupTasks
    {
        // Static lock for accessing the registry
        private static object s_lock = new object();
        private static ILog m_logger;

        static CleanupTasks()
        {
            m_logger = LogManager.GetLogger("CleanupTasks");
        }

        private static Dictionary<string, CleanupTask> LoadTasks()
        {
            lock (s_lock)
            {
                string[] cleanupUsers = Settings.Store.CleanupUsers;

                Dictionary<string, CleanupTask> tasks = new Dictionary<string, CleanupTask>();

                foreach (string item in cleanupUsers)
                {
                    try
                    {
                        CleanupTask task = CleanupTask.FromRegString(item);
                        tasks.Add(task.UserName, task);
                    }
                    catch (Exception e)
                    {
                        m_logger.ErrorFormat("Unable to parse registry entry! {0}", e);
                        throw;
                    }
                }

                return tasks;
            }
        }

        private static void SaveTasks( Dictionary<string, CleanupTask> tasks )
        {
            lock (s_lock)
            {
                List<string> list = new List<string>();
                foreach (KeyValuePair<string, CleanupTask> pair in tasks)
                {
                    list.Add(pair.Value.ToRegString());
                }
                Settings.Store.CleanupUsers = list.ToArray();
            }
        }

        public static void AddTask(CleanupTask newTask)
        {
            lock (s_lock)
            {
                Dictionary<string, CleanupTask> tasks = CleanupTasks.LoadTasks();

                if (tasks.ContainsKey(newTask.UserName))
                {
                    // We already have a task for this user... get it.
                    CleanupTask currentTask = tasks[newTask.UserName];

                    // We only need to change the action if it is not "DELETE_PROFILE" because if
                    // it is "DELETE_PROFILE" then we want to do both actions (delete and scramble),
                    // in which case it is pointless to do the scramble.
                    if (currentTask.Action != CleanupAction.DELETE_PROFILE)
                    {
                        // Current task action must be SCRAMBLE, so we only change if the new
                        // task is not SCRAMBLE (i.e. delete).
                        if (currentTask.Action != newTask.Action)
                        {
                            tasks[newTask.UserName] = newTask;
                        }
                    }
                }
                else
                {
                    tasks.Add(newTask.UserName, newTask);
                }

                CleanupTasks.SaveTasks(tasks);
            }
        }

        /// <summary>
        /// Get the list of tasks that have time stamps that are at least 30
        /// seconds in the past.
        /// </summary>
        /// <returns></returns>
        public static List<CleanupTask> GetEligibleTasks()
        {
            lock (s_lock)
            {
                Dictionary<string, CleanupTask> tasks = CleanupTasks.LoadTasks();
                List<CleanupTask> result = new List<CleanupTask>();

                foreach (KeyValuePair<string, CleanupTask> pair in tasks)
                {
                    if ((DateTime.Now - pair.Value.TimeStamp) > TimeSpan.FromSeconds(30))
                    {
                        result.Add(pair.Value);
                    }
                }
                return result;
            }
        }

        public static void RemoveTaskForUser(string user)
        {
            lock (s_lock)
            {
                Dictionary<string, CleanupTask> tasks = CleanupTasks.LoadTasks();
                
                // Upper-case the user name to remain case insensitive (see CleanupTask constructor)
                user = user.ToUpper();

                if (tasks.ContainsKey(user))
                {
                    tasks.Remove(user);
                    CleanupTasks.SaveTasks(tasks);
                }
            }
        }
    }
}
