using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Block2nd.World;

namespace Block2nd.UnsafeStructure
{
    public sealed unsafe class FastBuffer<T> where T : unmanaged
    {
        private int capacity;
        private int length = 0;
        private int blockSize = 12;
        private byte cellSize;

        private T* data;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => length;
        }
        
        public FastBuffer()
        {
            cellSize = (byte) sizeof(T);
            capacity = blockSize;
            data = (T*) Marshal.AllocHGlobal(cellSize * capacity);
            
            GC.AddMemoryPressure(cellSize * capacity);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => data[index];
        }

        public void Push(T val)
        {
            length++;
            CheckCapacity(ref length);
            data[length] = val;
        }

        private void CheckCapacity(ref int index)
        {
            if (index < capacity)
                return;
            int increase = ((index - capacity) / blockSize + 1) * blockSize;
            GC.AddMemoryPressure(increase * cellSize);
            capacity += increase;

            ReAlloc();
        }

        private void ReAlloc()
        {
            data = (T*) Marshal.ReAllocHGlobal((IntPtr) data, new IntPtr(capacity * cellSize));
        }

        public void CopyTo(ref T[] target)
        {
            long size = length * cellSize;
            fixed (T* tp = target)
            {
                Buffer.MemoryCopy(data, tp, size, size);
            }
        }

        public T[] ToArray()
        {
            var target = new T[length];
            CopyTo(ref target);
            return target;
        }

        public void ResetIndex()
        {
            length = 0;
        }

        ~FastBuffer()
        {
            Marshal.FreeHGlobal((IntPtr)data);
        }
    }
}