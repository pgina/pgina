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
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

using Abstractions.Logging;

namespace Abstractions.Pipes
{
    public class PipeClient : Pipe
    {                                
        public PipeClient(string name, Func<BinaryReader, BinaryWriter, bool> action) 
            : base(name, action)
        {            
        }

        public PipeClient(string name, Func<IDictionary<string, object>, IDictionary<string, object>> action)
            : base(name, action)
        {            
        }

        public PipeClient(string name)
            : base(name)
        {
            // user must use Start(action) flavor...
        }

        public void Start(dynamic initialMessage)
        {
            Start(initialMessage, Timeout.Infinite);
        }

        public void Start(dynamic initialMessage, int timeout)
        {
            if (StreamAction == null)
                throw new ArgumentException(string.Format("You cannot use Start() having constructed the client without an action, use Start(<action>) instead."));

            Start(StreamAction, initialMessage, timeout);
        }

        public void Start(Func<IDictionary<string, object>, IDictionary<string, object>> action, IDictionary<string, object> initialMessage, int timeout)
        {                
            Start(
                (Func<BinaryReader, BinaryWriter, bool>)((r, w) =>
                {
                    return DefaultMessageHandler(r, w, action);
                }), 
            initialMessage, timeout);
        }

        public void Start(Func<BinaryReader, BinaryWriter, bool> action, IDictionary<string, object> initialMessage, int timeout)
        {
            StreamAction = action;

            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", Name, PipeDirection.InOut,
                PipeOptions.WriteThrough, TokenImpersonationLevel.None, HandleInheritability.None))
            {                
                try
                {
                    pipeClient.Connect(timeout);
                }
                catch(Exception e)
                {
                    LibraryLogging.Error("Error connecting PipeClient: {0}", e);
                    return;
                }

                // Write the initial message to get the pumps running,
                //  not in a try/catch so that errors bubble up
                HandlePipeConnection(pipeClient, initialMessage);                
            }                         
        }        
    }
}
