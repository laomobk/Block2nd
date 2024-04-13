namespace Block2nd.Behavior.Block
{
    public class PlantBehavior: BlockBehavior
    {
        public override BlockBehavior CreateInstance()
        {
            return this;
        }

        public override bool CanCollide()
        {
            return false;
        }
        
    }
}