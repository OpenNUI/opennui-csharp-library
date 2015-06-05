using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace OpenNUI.CSharp.Library.Channel
{
    public unsafe class DepthChannel : IChannel
    {
        private const int BlockCount = 3;

        private int _zero = 0;
        private MemoryMappedFile _mappedFile = null;
        private MemoryMappedViewAccessor _mappedFileAccessor = null;
        private byte* _mappedPointer;
        private int*[] lockDatas;
        private NuiSensor _sensor;
        public DepthChannel(string mappedName, NuiSensor sensor)
        {
            _sensor = sensor;
            _mappedFile = MemoryMappedFile.OpenExisting(mappedName);
            _mappedFileAccessor = _mappedFile.CreateViewAccessor();
            _mappedFileAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _mappedPointer);

            lockDatas = new int*[BlockCount];
            for (int i = 0; i < BlockCount; i++)
                lockDatas[i] = (int*)(sizeof(int) * i + _mappedPointer);
        }
        public void Close()
        {
            _mappedFileAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
            _mappedFile.Dispose();
        }
        internal bool Read(out short[] data)
        {
            data = new short[0];
            for (int i = 0; i < BlockCount; i++)
            {
                if (Interlocked.CompareExchange(ref *lockDatas[i], 1, 0) == 0)
                {
                    int size = 0;
                    _mappedFileAccessor.Read<int>(sizeof(int) * BlockCount + (_sensor.DepthInfo.Size + sizeof(int)) * i, out size);

                    if (size > 0)
                    {
                        data = new short[size / sizeof(short)];
                        _mappedFileAccessor.Write<int>(sizeof(int) * BlockCount + (_sensor.DepthInfo.Size + sizeof(int)) * i, ref _zero);
                        Marshal.Copy((IntPtr)(_mappedPointer + sizeof(int) * BlockCount + 
                            (_sensor.DepthInfo.Size + sizeof(int)) * i + sizeof(int)), data, 0, _sensor.DepthInfo.Size / sizeof(short));
                    }
                    Interlocked.Exchange(ref *lockDatas[i], 0);
                    break;
                }
            }
            return data.Length > 0;
        }
    }
}