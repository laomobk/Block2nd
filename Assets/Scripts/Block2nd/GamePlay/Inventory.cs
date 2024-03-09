namespace Block2nd.GamePlay
{
    public class Inventory
    {
        public int[] blockCodes;
        public int selectIdx = 0;

        public Inventory()
        {
            blockCodes = new [] {1, 2, 3, 4, 5, 6, 7, 8, 9};
        }

        public void SetSlot(int idx, int code)
        {
            if (idx >= 9 || idx < 0)
                return;
            blockCodes[idx] = code;
        }
        
        public int GetSlot(int idx, int code)
        {
            if (idx >= 9 || idx < 0)
                return 0;
            return blockCodes[idx] = code;
        }

        public int Next()
        {
            selectIdx = (selectIdx + 1) % blockCodes.Length;
            return selectIdx;
        }

        public int Prev()
        {
            selectIdx--;
            if (selectIdx < 0)
                selectIdx = blockCodes.Length - 1;
            return selectIdx;
        }
    }
}