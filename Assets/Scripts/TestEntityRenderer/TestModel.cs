using Block2nd.Model;
using UnityEngine;

namespace TestEntityRenderer
{
    public class TestModel: ModelBase
    {
        public TestModel(string textureId) : base(textureId)
        {
            AddBox(Vector3.zero, Vector3.zero, 
                1f, 1f, 2f, 0f, 0f, 0f, 0f, 0f)
                .AddBox(Vector3.zero, Vector3.one, 2f, 2f, 2f, 
                    0f, 0f, 0f, 0f, 0f);
        }
    }
}