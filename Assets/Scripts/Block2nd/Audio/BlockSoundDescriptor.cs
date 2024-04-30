namespace Block2nd.Audio
{
    public class BlockSoundDescriptor
    {
        public SoundEffectGroup breakSoundGroup;
        public SoundEffectGroup placeSoundGroup;
        public SoundEffectGroup stepSoundGroup;

        public BlockSoundDescriptor(SoundEffectGroup breakSoundGroup, 
                                    SoundEffectGroup placeSoundGroup = null,
                                    SoundEffectGroup stepSoundGroup = null)
        {
            this.breakSoundGroup = breakSoundGroup;
            this.placeSoundGroup = placeSoundGroup;
            this.stepSoundGroup = stepSoundGroup;
            
            if (placeSoundGroup == null)
            {
                this.placeSoundGroup = breakSoundGroup;
            }

            if (stepSoundGroup == null)
            {
                this.stepSoundGroup = breakSoundGroup;
            }
        }

        public static BlockSoundDescriptor NewWithGroup(string resPath, int count)
        {
            return new BlockSoundDescriptor(new SoundEffectGroup(resPath, count));
        }

        public static BlockSoundDescriptor NewWithGroup(string resPath, int count, string stepResPath, int stepCount)
        {
            return new BlockSoundDescriptor(
                new SoundEffectGroup(resPath, count),
                stepSoundGroup: new SoundEffectGroup(stepResPath, stepCount));
        }
        
        public static readonly BlockSoundDescriptor SoundDigGrass = NewWithGroup(
            "sound3/dig/grass", 4, "sound3/step/grass", 6);
        public static readonly BlockSoundDescriptor SoundDigStone = NewWithGroup("sound3/dig/stone", 4);
        public static readonly BlockSoundDescriptor SoundDigCloth = NewWithGroup("sound3/dig/cloth", 4);
        public static readonly BlockSoundDescriptor SoundDigSand = NewWithGroup("sound3/dig/sand", 4);
        public static readonly BlockSoundDescriptor SoundDigWood = NewWithGroup("sound3/dig/wood", 4);
        public static readonly BlockSoundDescriptor SoundDigGravel = NewWithGroup("sound3/dig/gravel", 4);
    }
}