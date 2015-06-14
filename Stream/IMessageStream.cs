using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNUI.CSharp.Library
{
    delegate void MessageReceivedHandler(MessageReader message);
    delegate void StreamDisconnectedHandler(IMessageStream socket);

    interface IMessageStream
    {
        event StreamDisconnectedHandler OnClientDisconnected;
        event MessageReceivedHandler OnMessageReceived;
        void Send(MessageWriter writer);
    }
}
