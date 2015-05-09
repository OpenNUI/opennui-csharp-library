using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNUI.CSharp.Library
{
    public delegate void MessageReceivedHandler(MessageReader message);
    public delegate void StreamDisconnectedHandler(IMessageStream socket);

    public interface IMessageStream
    {
        event StreamDisconnectedHandler OnClientDisconnected;
        event MessageReceivedHandler OnMessageReceived;
        void Send(MessageWriter writer);
    }
}
