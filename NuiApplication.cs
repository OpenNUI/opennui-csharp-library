using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OpenNUI.CSharp.Library.Channel;

namespace OpenNUI.CSharp.Library
{
    public class NuiApplication
    {
        public delegate void SensorConnectHandler(NuiSensor sensor);
        public delegate void SensorDisconnectHandler(NuiSensor sensor);
        public delegate void HandStatusChangeHandler(JointType type, HandStatus status);
        public delegate void FaceDataChangeHandler(int x, int y, int width, int height);

        public delegate void EventHandler();

        public event EventHandler OnLoad;
        public event EventHandler OnFail;
        public event SensorConnectHandler OnSensorConnected;
        public event SensorDisconnectHandler OnSensorDisconnected;
        public event HandStatusChangeHandler OnHandStatusChanged;
        public event FaceDataChangeHandler OnFaceDataChanged;

        private IPCStream _messageStream;
        private string _appName;
        private int _sessionId;
        private Socket _lifeSocket;
        private TCPStream _lifeStream;
        private Dictionary<int, NuiSensor> _sensors;

        public NuiApplication(string appName)
        {
            _sensors = new Dictionary<int, NuiSensor>();
            _lifeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _appName = appName;
        }

        public void Start()
        {
            _lifeSocket.Connect(IPAddress.Loopback, 8000);
            _lifeStream = new TCPStream(_lifeSocket);
            _lifeStream.OnMessageReceived += _lifeStream_OnMessageReceived;
            _lifeStream.OnClientDisconnected += _lifeStream_OnClientDisconnected;
        }

       private void _lifeStream_OnMessageReceived(MessageReader reader)
        {
            STCHeader header = (STCHeader)reader.ReadShort();
            switch (header)
            {
                case STCHeader.REQUEST_PLATFORM_TYPE:
                    _sessionId = reader.ReadInt();
                    MessageWriter message = new MessageWriter(CTSHeader.HELLO_ACK);
                    message.WriteInt(0);
                    _lifeStream.Send(message);
                    break;
                case STCHeader.REQUEST_PIPE_CONNECTION:
                    string pipeName = reader.ReadString();
                    _messageStream = new IPCStream(pipeName);
                    _messageStream.OnMessageReceived += _messageStream_OnMessageReceived;
                    _messageStream.OnClientDisconnected += _messageStream_OnClientDisconnected;
                    _messageStream.Send(new MessageWriter(CTSHeader.READY_CONNECTION_ACK));

                    if (OnLoad != null)
                        OnLoad.Invoke();
                    break;
            }
        }
       private void _lifeStream_OnClientDisconnected(IMessageStream socket)
        {
            if (OnFail != null)
                OnFail();
        }
       private void _messageStream_OnMessageReceived(MessageReader reader)
        {
            STCHeader header = (STCHeader)reader.ReadShort();
            switch (header)
            {
                case STCHeader.RESPONSE_COLOR_FRAME:
                    ResponseColorFrame(reader); break;
                case STCHeader.RESPONSE_DEPTH_FRAME:
                    ResponseDepthFrame(reader); break;
                case STCHeader.RESPONSE_BODY_FRAME:
                    ResponseBodyFrame(reader); break;

                case STCHeader.SEND_ALL_SENSOR_INFO:
                    ReceiveAllSensorInfo(reader); break;
                case STCHeader.SEND_NEW_SENSOR_INFO:
                   ReceiveNewSensorInfo(reader); break;
                case STCHeader.SEND_CHANGED_SENSOR_INFO:
                    ReceiveChangedSensorInfo(reader); break;

                case STCHeader.SEND_TRIGGER_EVENT_DATA:
                    ReceiveTriggerEvent(reader);
                    break;
            }
        }
       private void _messageStream_OnClientDisconnected(IMessageStream socket)
        {
            if (OnFail != null)
                OnFail();
        }

        private void ReceiveTriggerEvent(MessageReader reader)
        {
            byte[] rawEvent = reader.ReadBytes(EventData.EVENT_DATA_SIZE);
            EventData e = EventData.ToEvent(rawEvent);

            switch (e.EventType)
            {
                case EventType.HandStatusChange:
                    JointType type = (JointType)BitConverter.ToInt32(e.Data, 0);
                    HandStatus status = (HandStatus)BitConverter.ToInt16(e.Data, 4);
                    if (OnHandStatusChanged != null)
                        OnHandStatusChanged(type, status);
                    break;
                case EventType.FaceDataChange:
                    if (OnFaceDataChanged != null)
                        OnFaceDataChanged(BitConverter.ToInt32(e.Data, 0),
                            BitConverter.ToInt32(e.Data, 4),
                            BitConverter.ToInt32(e.Data, 8),
                            BitConverter.ToInt32(e.Data, 12));
                    break;
            }
        }
        private void ResponseColorFrame(MessageReader reader)
        {
            int sensorId = reader.ReadInt();
            bool isCompleted = reader.ReadBool();

            if (isCompleted && _sensors.ContainsKey(sensorId))
                _sensors[sensorId].SetColorChannel(new ColorChannel(reader.ReadString(), _sensors[sensorId]));
            else
                throw new Exception("Color Frame를 열 수 없습니다.");
        }
        private void ResponseDepthFrame(MessageReader reader)
        {
            int sensorId = reader.ReadInt();
            bool isCompleted = reader.ReadBool();

            if (isCompleted && _sensors.ContainsKey(sensorId))
                _sensors[sensorId].SetDepthChannel(new DepthChannel(reader.ReadString(), _sensors[sensorId]));
            else
                throw new Exception("Depth Frame를 열 수 없습니다.");
        }
        private void ResponseBodyFrame(MessageReader reader)
        {
            int sensorId = reader.ReadInt();
            bool isCompleted = reader.ReadBool();

            if (isCompleted && _sensors.ContainsKey(sensorId))
                _sensors[sensorId].SetBodyChannel(new BodyChannel(reader.ReadString(), _sensors[sensorId]));
            else
                throw new Exception("Body Frame를 열 수 없습니다.");
        }
        private void ReceiveNewSensorInfo(MessageReader reader)
        {
            NuiSensor sensor = new NuiSensor(this,
                             reader.ReadInt(),
                             reader.ReadString(),
                             reader.ReadString(), (SensorState)reader.ReadInt(),
                             new ColorInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadInt()),
                             new DepthInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadInt()),
                             new BodyInfo(reader.ReadInt()));

            if (!_sensors.ContainsKey(sensor.SensorId))
            {
                sensor.ChangeStatus(SensorState.OPENED);

                if (OnSensorConnected != null)
                    OnSensorConnected.Invoke(sensor);

                _sensors.Add(sensor.SensorId, sensor);
            }
        }
        private void ReceiveChangedSensorInfo(MessageReader reader)
        {
            int sensorId = reader.ReadInt();
            if (_sensors.ContainsKey(sensorId))
            {
                _sensors[sensorId].ChangeStatus((SensorState)reader.ReadInt());
                if (OnSensorDisconnected != null)
                    OnSensorDisconnected.Invoke(_sensors[sensorId]);
                _sensors.Remove(sensorId);
            }
        }
        private void ReceiveAllSensorInfo(MessageReader reader)
        {
            foreach (NuiSensor sensor in _sensors.Values)
            {
                sensor.ChangeStatus(SensorState.UNKNOWN);
                if (OnSensorDisconnected != null)
                    OnSensorDisconnected.Invoke(sensor);
            }
            _sensors.Clear();

            int sensorCount = reader.ReadInt();
            for (int i = 0; i < sensorCount; i++)
            {
                NuiSensor sensor = new NuiSensor(this,
                    reader.ReadInt(),
                    reader.ReadString(),
                    reader.ReadString(), (SensorState)reader.ReadInt(),
                    new ColorInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadInt()),
                    new DepthInfo(reader.ReadInt(), reader.ReadInt(), reader.ReadInt()),
                    new BodyInfo(reader.ReadInt()));

                sensor.ChangeStatus(SensorState.OPENED);

                if (OnSensorConnected != null)
                    OnSensorConnected.Invoke(sensor);

                _sensors.Add(sensor.SensorId, sensor);
            }
        }

        internal void SendData(MessageWriter message)
        {
            _messageStream.Send(message);
        }
    }
}
