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
    public class PipeServer : Pipe
    {
        public int MaxClients { get; private set; }
        
        private Thread[] m_serverThreads = null;
        private bool m_running = false;
        private bool Running
        {
            get { lock (this) { return m_running; } }
            set { lock (this) { m_running = value; } }
        }
        
        public PipeServer(string name, int maxClients, Func<BinaryReader, BinaryWriter, bool> action) 
            : base(name, action)
        {
            MaxClients = maxClients;
        }

        public PipeServer(string name, int maxClients, Func<dynamic, dynamic> action)
            : base(name, action) 
        {
            MaxClients = maxClients;
        }

        public void Start()
        {
            StartServerThreads();
        }

        public void Stop()
        {
            StopServerThreads();
        }

        private void StartServerThreads()
        {
            if (Running)
                return;

            lock (this)
            {
                Running = true;

                m_serverThreads = new Thread[MaxClients];
                for (int x = 0; x < MaxClients; x++)
                {
                    m_serverThreads[x] = new Thread(new ThreadStart(ServerThread));
                    m_serverThreads[x].Start();
                }
            }
        }

        private void StopServerThreads()
        {
            if (!Running)
                return;

            lock (this)
            {
                Running = false;

                // Some or all of our threads may be blocked waiting for connections,
                // this is a bit nasty, but since I can't seem to get the async 
                // wait working, we do this - poke em! 
                for (int x = 0; x < MaxClients; x++)
                {
                    FakeClientToWakeEmAndShakem();
                }

                for (int x = 0; x < MaxClients; x++)
                {
                    m_serverThreads[x].Join();
                }

                m_serverThreads = null;
            }
        }
        
        private void ServerThread()
        {                        
            PipeSecurity security = new PipeSecurity();                
            // Anyone can talk to us
            security.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite, AccessControlType.Allow)); 
            // But only we have full control (including the 'create' right, which allows us to be the server side of this equation)
            security.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().Owner, PipeAccessRights.FullControl, AccessControlType.Allow));

            while (Running)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(Name, PipeDirection.InOut, MaxClients,
                        PipeTransmissionMode.Byte, PipeOptions.WriteThrough, 0, 0, security, HandleInheritability.None))
                {
                    try
                    {
                        pipeServer.WaitForConnection();
                    }
                    catch (Exception e)
                    {
                        LibraryLogging.Error("Error in server connection handler: {0}", e);
                        continue;
                    }

                    // Handle this connection, note that we always expect client to initiate the
                    //  flow of messages, so we do not include an initial message
                    HandlePipeConnection(pipeServer, null);
                }
            }
        }

        private void FakeClientToWakeEmAndShakem()
        {
            try
            {
                PipeClient client = new PipeClient(Name);
                client.Start(((r, w) => { return false; }), null, 100);                
            }
            catch { /* intentionally ignored */ }
        }
    }
}
