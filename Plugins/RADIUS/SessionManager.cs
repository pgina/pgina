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
using System.Threading;

namespace pGina.Plugin.RADIUS
{
    class Session
    {
        public string username { get; set; }
        public Guid id { get; set; } //guid.toString()
        public int? windowsSessionId { get; set; } //Windows session id
        public bool active { get; set; }

        public RADIUSClient client { get; set; }
            
        //public DateTime loginTimestamp { get; set; }
        public Packet.Acct_Terminate_Cause? terminate_cause { get; set; }

        private int interim_update = 0;
        private Timer interim_update_timer = null;

        private int session_limit = 0;
        private Timer session_limit_timer = null;

        private DateTime session_terminate = DateTime.MaxValue;
        private Timer session_terminate_timer = null;


        public Session(Guid Id, string username, RADIUSClient client) //string username,
        {
            this.id = Id;
            this.username = username;
            this.client = client;
            active = true;

        }

        public void SetInterimUpdate(int frequency, TimerCallback tcb){
            this.interim_update = frequency;

            this.interim_update_timer = new Timer(tcb, this, frequency * 1000, frequency * 1000);
        }

        public void SetSessionTimeout(int timeout, TimerCallback tcb)
        {
            this.session_limit = timeout;

            //Create timer, signals logoff every 30 seconds after timeout is reached
            this.session_limit_timer = new Timer(tcb, this, timeout*1000, 30000);            
        }

        public void Set_Session_Terminate(DateTime time, TimerCallback tcb)
        {
            this.session_terminate = time;

            int ms = (int)(time - DateTime.Now).TotalSeconds;

            this.session_terminate_timer = new Timer(tcb, this, ms * 1000, 30000);
            //Create timer
        }

        public void disableCallbacks()
        {
            if (interim_update_timer != null)
            {
                interim_update_timer.Change(Timeout.Infinite, Timeout.Infinite);
                interim_update_timer.Dispose();
            }

            if (session_limit_timer != null)
            {
                session_limit_timer.Change(Timeout.Infinite, Timeout.Infinite);
                session_limit_timer.Dispose();
            }

            if (session_terminate_timer != null)
            {
                session_terminate_timer.Change(Timeout.Infinite, Timeout.Infinite);
                session_terminate_timer.Dispose();
            }
        }

    }
}
