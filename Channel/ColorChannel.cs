using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;
using System.IO;

using System.IO.MemoryMappedFiles;

namespace OpenNUI.CSharp.Library.Channel
{
    public unsafe class ColorChannel : IChannel
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static unsafe extern void* CopyMemory(void* dest, void* src, ulong count);

        public const int BlockCount = 3;

        int zero = 0;
        private MemoryMappedFile _mappedFile = null;
        private MemoryMappedViewAccessor mappedFileAccessor = null;
        private byte* MappedPointer;
        private int*[] lockDatas;
        private NuiSensor _sensor;
        private long _stamp;
        public ColorChannel(string mappedName, NuiSensor sensor)
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
            bool result = false;
            data = new byte[_sensor.ColorInfo.Size];
            for (int i = 0; i < BlockCount; i++)
            {
                if (Interlocked.CompareExchange(ref *lockDatas[i], 1, 0) != 0)
                    continue;

                long stamp = *((long*)(MappedPointer + sizeof(int) * BlockCount + (_sensor.ColorInfo.Size + sizeof(long)) * i));
                if (_stamp < stamp)
                {
                    fixed (byte* dest = &data[0])
                    {
                        CopyMemory(dest, MappedPointer + sizeof(int) * BlockCount + (_sensor.ColorInfo.Size + sizeof(long)) * i + sizeof(long),
                             (ulong)_sensor.ColorInfo.Size
                            );
                    }
                    _stamp = stamp;
                    result = true;
                }
                Interlocked.Exchange(ref *lockDatas[i], 0);
                return result;
            }
            return result;
        }
    }
}