using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenNUI.CSharp.Library
{
    public class NuiSensor
    {
        private NuiApplication _app;
        public int SensorId { get; private set; }
        public string Name { get; private set; }
        public string Vendor { get; private set; }
        public SensorState State { get; private set; }

        private bool _colorOpend = false;
        private bool _depthOpend = false;
        private bool _bodyOpend = false;

        private Channel.ColorChannel _colorChannel;
        private Channel.DepthChannel _depthChannel;
        private Channel.BodyChannel _bodyChannel;

        private BodyData[] _lastSkeletonFrame;

        public ColorInfo ColorInfo { get; private set; }
        public DepthInfo DepthInfo { get; private set; }
        public BodyInfo BodyInfo { get; private set; }
        
        internal NuiSensor(NuiApplication app, int sensorId, string name, string vendor, SensorState State, ColorInfo color, DepthInfo depth, BodyInfo body)
        {
            _app = app;
            SensorId = sensorId;
            ColorInfo = color;
            DepthInfo = depth;
            BodyInfo = body;
        }
        public void ChangeStatus(SensorState status)
        {
            State = status;
        }

        public void OpenColorFrame()
        {
            MessageWriter message = new MessageWriter(CTSHeader.REQUEST_COLOR_FRAME);
            message.WriteInt(SensorId);
            _app.SendData(message);

            _colorOpend = true;
        }
        public void OpenDepthFrame()
        {
            MessageWriter message = new MessageWriter(CTSHeader.REQUEST_DEPTH_FRAME);
            message.WriteInt(SensorId);
            _app.SendData(message);

            _depthOpend = true;
        }
        public void OpenBodyFrame()
        {
            MessageWriter message = new MessageWriter(CTSHeader.REQUEST_BODY_FRAME);
            message.WriteInt(SensorId);
            _app.SendData(message);

            _bodyOpend = true;
        }

        public ImageData GetColorFrame()
        {
            byte[] data = new byte[0];

            if (!_colorOpend) // 열기 시도도 안함
                throw new Exception("GetColorFrame호출 전에 OpenColorFrame를 호출해주세요.");

            if (_colorOpend && _colorChannel == null) // 열기 시도했으나 열리지 않았음.
                return null;

            if (State == SensorState.UNKNOWN)
                throw new Exception("센서가 닫혀있습니다.");

            _colorChannel.Read(out data);

            if (data.Length > 0)
                return new ImageData(data, this.ColorInfo);

            return null;
        }
        public DepthData GetDepthFrame()
        {
            short[] data = new short[0];
            if (!_depthOpend) // 열기 시도도 안함
                throw new Exception("GetDepthFrame호출 전에 OpenDepthrame를 호출해주세요.");

            if (_depthOpend && _depthChannel == null) // 열기 시도했으나 열리지 않았음.
                return null;

            if (State == SensorState.UNKNOWN)
                throw new Exception("센서가 닫혀있습니다.");

            _depthChannel.Read(out data);

            if (data.Length > 0)
                return new DepthData(data, DepthInfo);

            return null;
        }
        public BodyData[] GetBodyFrame()
        {
            if (!_bodyOpend) // 열기 시도도 안함
                throw new Exception("GetBodyFrame호출 전에 GetBodyFrame를 호출해주세요.");

            if (_bodyOpend && _bodyChannel == null) // 열기 시도했으나 열리지 않았음.
                return null;

            if (State == SensorState.UNKNOWN)
                throw new Exception("센서가 닫혀있습니다.");

            byte[] data = null;

            if (_bodyChannel.Read(out data) == false) // 쉐어드메모리 읽기 실패
                return _lastSkeletonFrame;

            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int bodiesCount = reader.ReadInt32();

            BodyData[] result = new BodyData[bodiesCount];
            for (int i = 0; i < bodiesCount; i++)
            {
                Dictionary<JointType, Joint> joints = new Dictionary<JointType, Joint>();
                Dictionary<JointType, JointOrientation> orientations = new Dictionary<JointType, JointOrientation>();

                int bodyId = reader.ReadInt32();
                bool valid = reader.ReadBoolean();
                int jointCount = reader.ReadInt32();
                for (int j = 0; j < jointCount; j++)
                {
                    JointType type = (JointType)reader.ReadInt32();
                    TrackingState state = (TrackingState)reader.ReadInt32();
                    double x = reader.ReadDouble();
                    double y = reader.ReadDouble();
                    double z = reader.ReadDouble();

                    float ox = reader.ReadSingle();
                    float oy = reader.ReadSingle();
                    float oz = reader.ReadSingle();
                    float ow = reader.ReadSingle();

                    Joint joint = new Joint(x, y, z, type, state);
                    JointOrientation orientation = new JointOrientation(type, ox, oy, oz, ow);

                    if (!joints.ContainsKey(type))
                        joints.Add(type, joint);

                    if (!orientations.ContainsKey(type))
                        orientations.Add(type, orientation);
                }
                HandStatus leftHand = (HandStatus)reader.ReadInt16();
                HandStatus rightHand = (HandStatus)reader.ReadInt16();
                result[i] = new BodyData(joints, orientations, valid, bodyId, this, leftHand, rightHand);
            }
            reader.Close();
            stream.Close();

            _lastSkeletonFrame = result;

            return result;
        }

        internal void SetColorChannel(Channel.ColorChannel chnn)
        {
            _colorChannel = chnn;
        }
        internal void SetDepthChannel(Channel.DepthChannel chnn)
        {
            _depthChannel = chnn;
        }
        internal void SetBodyChannel(Channel.BodyChannel chnn)
        {
            _bodyChannel = chnn;
        }
    }
}
