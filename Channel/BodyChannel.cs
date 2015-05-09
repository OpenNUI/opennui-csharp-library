using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.MemoryMappedFiles;

namespace OpenNUI.CSharp.Library.Channel
{
    public unsafe class BodyChannel : IChannel
    {
        private int BlockCount = 3;
        int zero = 0;
        private MemoryMappedFile _mappedFile = null;
        private MemoryMappedViewAccessor mappedFileAccessor = null;
        private byte* MappedPointer;
        private int*[] lockDatas;
        private NuiSensor _sensor;
        public BodyChannel(string mappedName, NuiSensor sensor)
        {
            _sensor = sensor;
            _mappedFile = MemoryMappedFile.OpenExisting(mappedName);
            mappedFileAccessor = _mappedFile.CreateViewAccessor();
            mappedFileAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref MappedPointer);

            lockDatas = new int*[BlockCount];
            for (int i = 0; i < BlockCount; i++)
                lockDatas[i] = (int*)(sizeof(int) * i + MappedPointer);

        }

        public void Close()
        {
            mappedFileAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
            _mappedFile.Dispose();
        }
        public bool Read(out byte[] data)
        {
            data = new byte[0];
            for (int i = 0; i < BlockCount; i++)
            {
                if (Interlocked.CompareExchange(ref *lockDatas[i], 1, 0) == 0)
                {
                    int size = 0;
                    mappedFileAccessor.Read<int>(sizeof(int) * BlockCount + (_sensor.BodyInfo.MaxTrackingNumber * 2048 + sizeof(int)) * i, out size);

                    if (size > 0)
                    {
                        data = new byte[size];
                        mappedFileAccessor.Write<int>(sizeof(int) * BlockCount + (_sensor.BodyInfo.MaxTrackingNumber * 2048 + sizeof(int)) * i, ref zero);
                        Marshal.Copy((IntPtr)(MappedPointer + sizeof(int) * BlockCount + (_sensor.BodyInfo.MaxTrackingNumber * 2048 + sizeof(int)) * i + sizeof(int)), data, 0, _sensor.BodyInfo.MaxTrackingNumber * 2048);
                    }
                    Interlocked.Exchange(ref *lockDatas[i], 0);
                    break;
                }
            }
            return data.Length > 0;
        }
    }
}

