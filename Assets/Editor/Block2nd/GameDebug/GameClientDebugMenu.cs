using System.Collections;
using System.Collections.Generic;
using Block2nd.Client.GameDebug;
using UnityEditor;
using UnityEngine;

namespace Editor.Block2nd.GameDebug
{
    public class GameClientDebugMenu : MonoBehaviour
    {
        [MenuItem("---Block2nd Tools---/GameClientDebug/Show Player Chunk Height Map Bake")]
        public static void ShowChunkHeightMapBakeAction()
        {
            if (Application.isPlaying)
            {
                GameClientDebugger.Instance.ShowPlayerChunkHeightMapBake();
            }
        }
        
        [MenuItem("---Block2nd Tools---/GameClientDebug/Show Player Chunk Non Fill Light Block")]
        public static void ShowPlayerChunkNonFillLightBlockAction()
        {
            if (Application.isPlaying)
            {
                GameClientDebugger.Instance.ShowPlayerChunkNonFillLightBlock();
            }
        }

        [MenuItem("---Block2nd Tools---/GameClientDebug/Print Player Chunk Sky Light Map")]
        public static void PrintPlayerChunkSkyLightMapAction()
        {
            if (Application.isPlaying)
            {
                GameClientDebugger.Instance.PrintPlayerChunkSkyLightMap();
            }
        }
    }
}