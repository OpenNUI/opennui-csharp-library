using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OpenNUI.CSharp.Library
{ 
    class MessageWriter
    {
        private readonly BinaryWriter _binWriter;
        private MemoryStream _buffer;
        public byte[] ToArray()
        {
            return _buffer.GetBuffer();
        }

        public MessageWriter()
        {
            _buffer = new MemoryStream(0);
            _binWriter = new BinaryWriter(_buffer, Encoding.Default);
        }

        public MessageWriter(STCHeader pHeader)
        {
            _buffer = new MemoryStream(0);
            _binWriter = new BinaryWriter(_buffer, Encoding.Default);
            WriteShort((short)pHeader);
        }

        public MessageWriter(CTSHeader pHeader)
        {
            _buffer = new MemoryStream(0);
            _binWriter = new BinaryWriter(_buffer, Encoding.Default);
            WriteShort((short)pHeader);
        }

        public void WriteShort(short @short)
        {
            _binWriter.Write(@short);
        }

        public void WriteByte(byte @byte)
        {
            _binWriter.Write(@byte);
        }

        public void WriteBytes(byte[] @bytes)
        {
            _binWriter.Write(@bytes);
        }

        public void WriteBool(bool @bool)
        {
            _binWriter.Write(@bool);
        }

        public void WriteUShort(ushort @ushort)
        {
            _binWriter.Write(@ushort);
        }

        public void WriteInt(int @int)
        {
            _binWriter.Write(@int);
        }

        public void WriteUInt(uint @uint)
        {
            _binWriter.Write(@uint);
        }

        public void WriteLong(long @long)
        {
            _binWriter.Write(@long);
        }

        public void WriteULong(ulong @ulong)
        {
            _binWriter.Write(@ulong);
        }

        public void WriteString(string @string)
        {
            WriteInt(Encoding.Default.GetByteCount(@string));
            _binWriter.Write(Encoding.Default.GetBytes(@string));
        }
        public void WriteFloat(float @float)
        {
            _binWriter.Write(@float);
        }
    }
}
