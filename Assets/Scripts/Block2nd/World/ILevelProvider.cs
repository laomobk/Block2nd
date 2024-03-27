using Block2nd.Database;
using UnityEngine;

namespace Block2nd.World
{
    public interface ILevelProvider
    {
        Level ProvideLevel(GameObject levelPrefab, WorldSettings worldSettings);
    }
}