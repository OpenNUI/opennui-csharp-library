using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO.Pipes;

namespace OpenNUI.CSharp.Library
{
    public class SocketInfo
    {
        public readonly Socket Socket;
        public readonly NamedPipeClientStream Pipeline;
        public StateType State;
        public byte[] Buffer;
        public int Index;
        public enum StateType { Header, Data }

        public SocketInfo(Socket socket, short headerLength)
        {
            Socket = socket;
            State = StateType.Header;
            Buffer = new byte[headerLength];
            Index = 0;
        }
        public SocketInfo(NamedPipeClientStream pipeline, short headerLength)
        {
            Pipeline = pipeline;
            State = StateType.Header;
            Buffer = new byte[headerLength];
            Index = 0;
        }
    }
}
