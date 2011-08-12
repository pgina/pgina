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

        protected Pipe(string name, Func<dynamic, dynamic> action)
            : this(name)
        {
            if (action == null)
                throw new ArgumentNullException("action", string.Format("Message action cannot be null, you want us to do *something* with the pipe right?"));

            StreamAction = ((r, w) =>
            {
                return DefaultMessageHandler(r, w, action);
            });
        }

        protected bool DefaultMessageHandler(BinaryReader reader, BinaryWriter writer, Func<dynamic, dynamic> callback)
        {
            int len = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(len);
            dynamic msg = PipeMessage.Demarshal(bytes);
            dynamic reply = callback(msg);
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

        protected void WriteMessage(BinaryWriter writer, dynamic msg)
        {
            byte[] encoded = PipeMessage.Marshal(msg);
            writer.Write((int)encoded.Length);
            writer.Write(encoded);
            writer.Flush();
        }

        protected void HandlePipeConnection(PipeStream pipeStream, dynamic initialMessage)
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
