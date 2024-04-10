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
            text = GetComponent<Text>();
            text.text = "Block2nd " + GameVersion.Subtitle + " " + GameVersion.Version + 
                        (ClientSharedData.zoz ? " for Suzumiya Haruhi" : "");
        }
    }

}