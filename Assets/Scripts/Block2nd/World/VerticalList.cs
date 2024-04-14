using System.Collections.Generic;

namespace Block2nd.World
{
    public class VerticalList<T>
    {
        private readonly List<T[,,]> arrays;

        private int length;
        
        private int width;
        private int depth;
        private int height;

        public int Length => length;

        public VerticalList(int length, int width, int depth, int height)
        {
            arrays = new List<T[,,]>();

            for (int i = 0; i < length; ++i)
            {
                arrays.Add(new T[width, height, depth]);
            }

            this.length = length;
            this.width = width;
            this.depth = depth;
            this.height = height;
        }

        public void GetRange(int minY, int maxY, out int minIdx, out int maxIdx)
        {
            minIdx = minY / height;
            maxIdx = maxY / height + 1;
        }

        public T[,,] Get(int idx)
        {
            if (idx < 0 || idx >= arrays.Count)
                return null;

            return arrays[idx];
        }
    }
}