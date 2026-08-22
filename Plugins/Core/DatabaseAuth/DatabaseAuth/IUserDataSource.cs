using System;
using System.Collections.Generic;

namespace OpenCredential.Plugin.DatabaseAuth
{
    interface IUserDataSource : IDisposable
    {
        UserEntry GetUserEntry(string userName);
        List<UserEntry> GetAllUserEntriesForCache();
        bool IsMemberOfGroup(string userName, string groupName);
        void UpdateUserHash(string userName, string newHash, string hashMethod);
        FailedAttemptResult RegisterFailedLoginAttempt(string userName);
        void ResetFailedLoginState(string userName);
    }
}

