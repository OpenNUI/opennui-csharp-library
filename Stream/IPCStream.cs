using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;

namespace OpenNUI.CSharp.Library
{
    class IPCStream : IMessageStream
    {
        public event StreamDisconnectedHandler OnClientDisconnected;
        public event MessageReceivedHandler OnMessageReceived;

        private NamedPipeClientStream _pipe_cts;
        private NamedPipeClientStream _pipe_stc;

        public IPCStream(string pipeName)
        {
            _pipe_cts = new NamedPipeClientStream(".", pipeName + "_cts", PipeDirection.Out, PipeOptions.Asynchronous);
            _pipe_cts.Connect();
            _pipe_stc = new NamedPipeClientStream(".", pipeName + "_stc", PipeDirection.In, PipeOptions.Asynchronous);
            _pipe_stc.Connect();
            this.WaitForData();
        }

        private void WaitForData()
        {
            WaitForData(new SocketInfo(_pipe_stc, 16));
        }

        private void WaitForData(SocketInfo socketInfo)
        {
            try
            {
                _pipe_stc.BeginRead(socketInfo.Buffer,
                    socketInfo.Index,
                    socketInfo.Buffer.Length - socketInfo.Index, 
                    new AsyncCallback(OnDataReceived),
                    socketInfo);
            }
            catch
            {
              
                if (OnClientDisconnected != null)
                    OnClientDisconnected(this);
            }
        }
        private void OnDataReceived(IAsyncResult iar)
        {
            SocketInfo socketInfo = (SocketInfo)iar.AsyncState;

            int received = _pipe_stc.EndRead(iar);

            if (received == 0)
            {
                if (OnClientDisconnected != null)
                    OnClientDisconnected(this);

                return;
            }

            socketInfo.Index += received;

            if (socketInfo.Index == socketInfo.Buffer.Length)
            {
                switch (socketInfo.State)
                {
                    case SocketInfo.StateType.Header:
                        MessageReader headerReader = new MessageReader(socketInfo.Buffer);
                        headerReader.ReadBytes(2);

                        int packetLength = headerReader.ReadInt();
                        headerReader.ReadBytes(10); // 16바이트 읽음..
                        socketInfo.State = SocketInfo.StateType.Data;
                        socketInfo.Buffer = new byte[packetLength];
                        socketInfo.Index = 0;
                        WaitForData(socketInfo);
                        break;
                    case SocketInfo.StateType.Data:
                        byte[] data = socketInfo.Buffer;
                        if (data.Length != 0 && OnMessageReceived != null)
                            OnMessageReceived(new MessageReader(data));
                        WaitForData();
                        break;
                }
            }

        }
        public void Send(MessageWriter message)
        {
            byte[] originBuffer = message.ToArray();
            byte[] sendBuffer = new byte[originBuffer.Length + 16];
            byte[] header = new byte[] { 45, 127 };
            byte[] packetLen = BitConverter.GetBytes(originBuffer.Length);

            System.Buffer.BlockCopy(header, 0, sendBuffer, 0, header.Length);
            System.Buffer.BlockCopy(packetLen, 0, sendBuffer, header.Length, packetLen.Length);
            System.Buffer.BlockCopy(originBuffer, 0, sendBuffer, 16, originBuffer.Length);

            if (_pipe_cts != null)
            {
                _pipe_cts.Write(sendBuffer, 0, sendBuffer.Length);
                _pipe_cts.Flush();
                _pipe_cts.WaitForPipeDrain();
            }
        }
    }
}
