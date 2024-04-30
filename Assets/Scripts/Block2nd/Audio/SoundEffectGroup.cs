using System;

namespace Block2nd.Audio
{
    public class SoundEffectGroup
    {
        private Random random = new Random();
        
        private string resourcePath;
        private int count;
        
        public SoundEffectGroup(string resourcePath, int count = 1)
        {
            if (count < 1)
                count = 1;
            
            this.resourcePath = resourcePath;
            this.count = count;
        }

        public string GetPath()
        {
            if (count == 1)
            {
                return resourcePath;
            }

            return resourcePath + random.Next(1, count);
        } 
    }
}