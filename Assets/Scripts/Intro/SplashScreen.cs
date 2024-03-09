using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
    public Image mask;

    private void Start()
    {
        StartCoroutine(SphlashScreenCoroutine());
    }

    private IEnumerator SphlashScreenCoroutine()
    {
        
        var color = mask.color;

        color.a = 1;
        
        mask.color = color;
        
        for (; color.a >= -0.2; color.a -= Time.deltaTime)
        {
            mask.color = color;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);

        for (; color.a <= 1.2; color.a += Time.deltaTime)
        {
            mask.color = color;
            yield return null;
        }

        SceneManager.LoadScene("Game");
    }
}
