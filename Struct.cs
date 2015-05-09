using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OpenNUI.CSharp.Library
{
    public enum EventType : int
    {
        HandStatusChange,
        HandPositionChange,
        FaceDataChange,
        PatternDataChange,
        GestureDetected,
        SensorCreated,
        SensorStatusChanged,

        UserCustom,
    }
    public enum SensorState : int
    {
        UNLOADED = 0,   //DLL은 물려있는데 센서가 열리지 않음.
        UNKNOWN,        //뽑힘 (제거)
        OPENED,         //정상
        SUSPENDED       //센서 정지.
    } 
    public enum HandStatus : short
    {
        Unknown = 0,
        NotTracked = 1,
        Open = 2,
        Closed = 3,
        Point = 4,
    }
    public enum TrackingState : int
    {
        NotTracked = 0,
        Inferred = 1,
        Tracked = 2,
    }
    public enum JointType : int
    {
        NULL = -1,
        SpineBase = 0,
        SpineMid = 1,
        Neck = 2,
        Head = 3,
        ShoulderLeft = 4,
        ElbowLeft = 5,
        WristLeft = 6,
        HandLeft = 7,
        ShoulderRight = 8,
        ElbowRight = 9,
        WristRight = 10,
        HandRight = 11,
        HipLeft = 12,
        KneeLeft = 13,
        AnkleLeft = 14,
        FootLeft = 15,
        HipRight = 16,
        KneeRight = 17,
        AnkleRight = 18,
        FootRight = 19,
        SpineShoulder = 20,
        HandTipLeft = 21,
        ThumbLeft = 22,
        HandTipRight = 23,
        ThumbRight = 24,
    }
    public static class SensorSupport
    {
        public const int DEPTH = 0x01;
        public const int COLOR = 0x02;
        public const int BODY = 0x04;
        public const int HAND = 0x08;
    }
    public struct Vector3
    {
        public double x;
        public double y;
        public double z;
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override string ToString()
        {
            return "(" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ")";
        }
    }
    public struct Joint
    {
        public readonly Vector3 position;
        public readonly JointType type;
        public readonly TrackingState state;
        internal Joint(double x, double y, double z, JointType type, TrackingState state)
        {
            position = new Vector3(x, y, z);
            this.type = type;
            this.state = state;
        }
    }
    public struct JointOrientation
    {
        public JointType type;
        public readonly float x;
        public readonly float y;
        public readonly float z;
        public readonly float w;

        internal JointOrientation(JointType type, float x, float y, float z, float w)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public override string ToString()
        {
            return "(" + x.ToString() + "," + y.ToString() + "," + z.ToString() + "," + w.ToString() + ")";
        }
    }
    public struct ColorInfo
    {
        public int Width;
        public int Height;
        public int BytePerPixel;
        public int Size { get { return Width * Height * BytePerPixel; } }
        public ColorInfo(int width, int height, int bpp)
        {
            Width = width;
            Height = height;
            BytePerPixel = bpp;
        }
    }
    public struct DepthInfo
    {
        public int Width;
        public int Height;
        public int BytePerPixel;
        public int Size { get { return Width * Height * BytePerPixel; } }

        public DepthInfo(int width, int height, int bpp)
        {
            Width = width;
            Height = height;
            BytePerPixel = bpp;
        }
    }
    public struct BodyInfo
    {
        public int MaxTrackingNumber;
        public BodyInfo(int maxTrackingNumber)
        {
            MaxTrackingNumber = maxTrackingNumber;
        }
    }
    public class ImageData
    {
        public byte[] FrameData { get; private set; }
        public ColorInfo Description { get; private set; }
        internal ImageData(byte[] data, ColorInfo info)
        {
            FrameData = data;
            Description = info;
        }
    }
    public class DepthData
    {
        public byte[] FrameData { get; private set; }
        public DepthInfo Description { get; private set; }
        internal DepthData(byte[] data, DepthInfo info)
        {
            this.FrameData = data;
            this.Description = info;
        }
    }
    public struct BodyData
    {
        public readonly Dictionary<JointType, Joint> Joints;
        public readonly Dictionary<JointType, JointOrientation> Orientations;
        public readonly int Id;
        public readonly bool Valid;
        public readonly HandStatus LeftHand;
        public readonly HandStatus RightHand;
        public readonly NuiSensor Sensor;
        internal BodyData(Dictionary<JointType, Joint> joints, Dictionary<JointType, JointOrientation> orientations, bool valid, int id, NuiSensor sensor, HandStatus leftHand, HandStatus rightHand)
        {
            this.Joints = joints;
            this.Orientations = orientations;
            this.Id = id;
            this.Valid = valid;
            this.Sensor = sensor;
            this.LeftHand = leftHand;
            this.RightHand = rightHand;
        }
    }
    public class EventData
    {
        public const int EVENT_DATA_SIZE = 260;
        public EventType EventType { get; private set; }
        public byte[] Data { get; set; }
        public EventData(EventType type)
        {
            EventType = type;
            Data = new byte[256];
        }


        // 쓰기 함수 만들어야돠ㅣㅁ
        public static EventData ToEvent(byte[] data, int offset = 0)
        {
            if (data.Length >= EventData.EVENT_DATA_SIZE + offset)
            {
                EventData e = new EventData((EventType)BitConverter.ToInt32(data, 0));
                Array.Copy(data, sizeof(Int32), e.Data, 0, 256);
                return e;
            }
            return null; // [에러임 이건]
        }
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[EVENT_DATA_SIZE];
            MemoryStream stream = new MemoryStream(buffer.Length);
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((int)EventType);
            writer.Write(Data);
            buffer = stream.GetBuffer();
            writer.Close();
            stream.Close();
            return buffer;
        }
    }

}
