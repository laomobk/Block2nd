using UnityEngine;

namespace Block2nd.Client
{
    public class WantsToQuit
    {
        static void QuitClient()
        {
            var client = GameObject.FindGameObjectWithTag("GameClient");
            if (client == null)
            {
                return;
            }
            
            var coroutine = client.GetComponent<GameClient>().SaveAndQuitCoroutine(false);
            while (coroutine.MoveNext()) ;
        }

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.quitting += QuitClient;
        }
    }
}