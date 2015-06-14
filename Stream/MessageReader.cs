using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OpenNUI.CSharp.Library
{
    class MessageReader
    {
        private MemoryStream _buffer;
        private readonly BinaryReader _binReader;

        public byte[] ToArray()
        {
            return _buffer.GetBuffer();
        }

        public int Length
        {
            get { return (int)_buffer.Length; }
        }

        public MessageReader(byte[] arrayOfBytes)
        {
            _buffer = new MemoryStream(arrayOfBytes, false);
            _binReader = new BinaryReader(_buffer, Encoding.Default);
        }

        public byte ReadByte()
        {
            return _binReader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return _binReader.ReadBytes(count);
        }

        public bool ReadBool()
        {
            return _binReader.ReadBoolean();
        }

        public short ReadShort()
        {
            return _binReader.ReadInt16();
        }

        public ushort ReadUShort()
        {
            return _binReader.ReadUInt16();
        }

        public int ReadInt()
        {
            return _binReader.ReadInt32();
        }

        public uint ReadUInt()
        {
            return _binReader.ReadUInt32();
        }

        public long ReadLong()
        {
            return _binReader.ReadInt64();
        }

        public ulong ReadULong()
        {
            return _binReader.ReadUInt64();
        }

        public string ReadString()
        {
            return Encoding.Default.GetString(ReadBytes(ReadInt()));
        }

        public float ReadFloat()
        {
            return _binReader.ReadSingle();
        }
    }
}