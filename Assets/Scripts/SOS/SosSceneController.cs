using System;
using System.Collections;
using Block2nd.Client;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SOS
{
    public class SosSceneController: MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(PlayerWaitCoroutine());
        }

        private IEnumerator PlayerWaitCoroutine()
        {
            yield return new WaitForSeconds(68);
            ClientSharedData.zoz = true;
            SceneManager.LoadScene("Title");
        }
    }
}