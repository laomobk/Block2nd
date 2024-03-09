using Block2nd.Database;

namespace Block2nd.Behavior.Block
{
    public class WaterBlockBehavior : LiquidBlockBehavior
    {
        private int waterCode = -1;
        
        public override BlockBehavior CreateInstance()
        {
            return new WaterBlockBehavior();
        }

        protected override int GetSelfBlockCode()
        {
            if (waterCode == -1)
                waterCode = BlockMetaDatabase.GetBlockMetaById("b2nd:block/water").blockCode;
            return waterCode;
        }
    }
}