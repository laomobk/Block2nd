using System.Collections;
using System.Collections.Generic;
using Block2nd.Client;
using UnityEngine;
using UnityEngine.UI;

namespace TitlePage
{
    public class VersionText : MonoBehaviour
    {
        private Text text;
        
        void Start()
        {
            var game = "Block2nd ";
            text = GetComponent<Text>();
            #if !UNITY_64

            game = "Block2nd(32bit) ";
            
            #endif
            text.text = game + GameVersion.Subtitle + " " + GameVersion.Version + 
                        (ClientSharedData.zoz ? " for Suzumiya Haruhi" : "");
        }
    }

}