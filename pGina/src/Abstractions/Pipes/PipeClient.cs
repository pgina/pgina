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

        public PipeClient(string name, Func<dynamic, dynamic> action)
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

        public void Start(Func<dynamic, dynamic> action, dynamic initialMessage, int timeout)
        {                
            Start(
                (Func<BinaryReader, BinaryWriter, bool>)((r, w) =>
                {
                    return DefaultMessageHandler(r, w, action);
                }), 
            initialMessage, timeout);
        }

        public void Start(Func<BinaryReader, BinaryWriter, bool> action, dynamic initialMessage, int timeout)
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
