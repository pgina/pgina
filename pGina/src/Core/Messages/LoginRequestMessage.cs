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
using System.Text;
using System.IO;
using System.Dynamic;

namespace pGina.Core.Messages
{
    public class LoginRequestMessage : MessageBase
    {
        public enum LoginReason
        {
            Login = 0,
            Unlock,
            CredUI
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public int Session { get; set; }
        public LoginReason Reason { get; set; }

        public LoginRequestMessage(dynamic expandoVersion)
        {
            FromExpando(expandoVersion);
        }

        public LoginRequestMessage()
        {
        }

        public override void FromExpando(dynamic expandoVersion)
        {
            Username = expandoVersion.Username;
            Password = expandoVersion.Password;
            Domain = expandoVersion.Domain;
            Session = expandoVersion.Session;
            Reason = (LoginReason)((byte)expandoVersion.Reason);
        }

        public override dynamic ToExpando()
        {
            dynamic exp = new ExpandoObject();
            exp.Username = this.Username;
            exp.Password = this.Password;
            exp.Domain = this.Domain;
            exp.Session = this.Session;
            exp.Reason = (byte)this.Reason;
            exp.MessageType = (byte) MessageType.LoginRequest;
            return exp;
        }
    }
}
