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

using Abstractions.Logging;

namespace Abstractions.Pipes
{
    public abstract class Pipe
    {
        public string Name { get; private set; }
        public Func<BinaryReader, BinaryWriter, bool> StreamAction { get; protected set; }

        protected Pipe(string name)
        {
            StreamAction = null;
            Name = name;
        }

        protected Pipe(string name, Func<BinaryReader, BinaryWriter, bool> action) 
            : this(name)
        {
            if (action == null)
                throw new ArgumentNullException("action", string.Format("Stream action cannot be null, you want us to do *something* with the pipe right?"));

            StreamAction = action;            
        }

        protected Pipe(string name, Func<IDictionary<string, object>, IDictionary<string, object>> action)
            : this(name)
        {
            if (action == null)
                throw new ArgumentNullException("action", string.Format("Message action cannot be null, you want us to do *something* with the pipe right?"));

            StreamAction = ((r, w) =>
            {
                return DefaultMessageHandler(r, w, action);
            });
        }

        protected bool DefaultMessageHandler(BinaryReader reader, BinaryWriter writer, Func<IDictionary<string, object>, IDictionary<string, object>> callback)
        {
            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);
            IDictionary<string, object> msg = PipeMessage.Demarshal(bytes);
            IDictionary<string, object> reply = callback(msg);
            if (reply != null)
            {                            
                WriteMessage(writer, reply);

                if (((IDictionary<String, Object>)reply).ContainsKey("LastMessage"))
                {
                    ((IDictionary<String, Object>)reply).Remove("LastMessage"); // Don't marshal this property
                    return false;
                }
            }
            
            return (reply != null);
        }

        protected void WriteMessage(BinaryWriter writer, IDictionary<string, object> msg)
        {
            byte[] encoded = PipeMessage.Marshal(msg);
            writer.Write((int)encoded.Length);
            writer.Write(encoded);
            writer.Flush();
        }

        protected void HandlePipeConnection(PipeStream pipeStream, IDictionary<string, object> initialMessage)
        {
            // You think we'd scope these with using() right? They are IDisposable
            //  after all... but nope, the implementation of these is such that 
            //  disposing of them also disposes the underlying stream.  Leaving us
            //  with a double (or triple if we close the pipeServer stream ourselves)
            //  close.  Yay.  Instead we abandoned these to the GC knowing that they
            //  are only wrappers anyway and have/use little/no resources of their own.
            BinaryReader reader = new BinaryReader(pipeStream, Encoding.Unicode);
            BinaryWriter writer = new BinaryWriter(pipeStream, Encoding.Unicode);

            try
            {
                // If we should announce with a specific message, do so
                if (initialMessage != null)
                {
                    WriteMessage(writer, initialMessage);
                }

                // So long as our Func<> returns true, we keep going.  When it returns false,
                //  it is done and its time to closeup and look for another client.
                while (StreamAction(reader, writer)) { }
            }
            catch(Exception e)
            {
                LibraryLogging.Error("Error while using pipe connection: {0}", e);
            }

            try
            {
                pipeStream.Flush();
                pipeStream.WaitForPipeDrain();
                pipeStream.Close();
            }
            catch(Exception e)
            {
                LibraryLogging.Error("Error while flushing/closing pipe connection: {0}", e);
            }
        }
    }
}
