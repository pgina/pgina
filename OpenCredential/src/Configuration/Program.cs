/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;
using Abstractions.WindowsApi;

namespace OpenCredential.Configuration
{
    static class Program
    {
        private const int MaxAuthenticationAttempts = 3;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!EnsureAuthorizedAdministrator())
                return;

            Application.Run(new ConfigurationUI());
        }

        private static bool EnsureAuthorizedAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            string currentSid = identity.User != null ? identity.User.Value : null;

            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show(
                    "OpenCredential Configuration can only be opened by a local administrator.",
                    "Access denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            string expectedDomain;
            string expectedUser;
            GetIdentityParts(identity, out expectedDomain, out expectedUser);

            for (int attempt = 1; attempt <= MaxAuthenticationAttempts; attempt++)
            {
                NetworkCredential credential = pInvokes.GetCredentials(
                    "OpenCredential Configuration",
                    "Re-enter your current Windows administrator credentials to continue.");

                if (credential == null)
                    return false;

                string suppliedDomain = credential.Domain ?? string.Empty;
                string suppliedUser = credential.UserName ?? string.Empty;
                NormalizeCredentialIdentity(ref suppliedDomain, ref suppliedUser);

                string authenticatedIdentityName;
                string authenticatedIdentitySid;
                if (pInvokes.TryValidateCredentialsAndGetIdentity(
                    suppliedUser,
                    suppliedDomain,
                    credential.Password,
                    out authenticatedIdentityName,
                    out authenticatedIdentitySid))
                {
                    string authenticatedDomain;
                    string authenticatedUser;
                    GetIdentityParts(authenticatedIdentityName, out authenticatedDomain, out authenticatedUser);

                    if (MatchesCurrentIdentity(expectedDomain, expectedUser, authenticatedDomain, authenticatedUser) &&
                        string.Equals(currentSid ?? string.Empty, authenticatedIdentitySid ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    MessageBox.Show(
                        "Please authenticate with the same Windows administrator account that is currently signed in.",
                        "Authentication failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    continue;
                }

                MessageBox.Show(
                    attempt < MaxAuthenticationAttempts
                        ? "The Windows credentials could not be validated. Please try again."
                        : "The Windows credentials could not be validated.",
                    "Authentication failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return false;
        }

        private static void GetIdentityParts(WindowsIdentity identity, out string domain, out string user)
        {
            GetIdentityParts(identity != null ? identity.Name : null, out domain, out user);
        }

        private static void GetIdentityParts(string identityName, out string domain, out string user)
        {
            identityName = identityName ?? string.Empty;
            string[] parts = identityName.Split(new[] { '\\' }, 2);

            if (parts.Length == 2)
            {
                domain = parts[0];
                user = parts[1];
                return;
            }

            domain = Environment.MachineName;
            user = identityName;
        }

        private static bool MatchesCurrentIdentity(string expectedDomain, string expectedUser, string suppliedDomain, string suppliedUser)
        {
            if (!string.Equals(expectedUser, suppliedUser, StringComparison.OrdinalIgnoreCase))
                return false;

            string normalizedExpectedDomain = NormalizeDomain(expectedDomain);
            string normalizedSuppliedDomain = NormalizeDomain(suppliedDomain);

            if (string.IsNullOrWhiteSpace(normalizedSuppliedDomain))
                normalizedSuppliedDomain = NormalizeDomain(Environment.MachineName);

            return string.Equals(normalizedExpectedDomain, normalizedSuppliedDomain, StringComparison.OrdinalIgnoreCase);
        }

        private static void NormalizeCredentialIdentity(ref string domain, ref string user)
        {
            domain = NormalizeDomain(domain);
            user = user == null ? string.Empty : user.Trim();

            if (string.IsNullOrWhiteSpace(user))
                return;

            string[] qualifiedParts = user.Split(new[] { '\\' }, 2);
            if (qualifiedParts.Length == 2)
            {
                domain = NormalizeDomain(qualifiedParts[0]);
                user = qualifiedParts[1].Trim();
            }
        }

        private static string NormalizeDomain(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return string.Empty;

            string normalized = domain.Trim();
            if (normalized == ".")
                return Environment.MachineName;

            return normalized;
        }
    }
}
