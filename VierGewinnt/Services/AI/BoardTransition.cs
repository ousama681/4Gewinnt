namespace VierGewinnt.Services.AI
{
    public class BoardTransition
    {
        private const int BOARD_SIZE = 1 << 20;
        private const int BOARD_CANVAS = BOARD_SIZE - 1;

        private ulong[] keys = new ulong[BOARD_SIZE];
        private int[] values = new int[BOARD_SIZE];

        public int Conflicts { get; private set; }


        public int GetKeyValue(ulong key)
        {
            int index = (int)(key & BOARD_CANVAS);
            if (keys[index] == key)
            {
                Conflicts++;
                return values[index];
            }
            else return 0;
        }

        public void AddEntry(ulong key, int value)
        {
            int index = (int)(key & BOARD_CANVAS);
            keys[index] = key;
            values[index] = value;
        }
    }
}
