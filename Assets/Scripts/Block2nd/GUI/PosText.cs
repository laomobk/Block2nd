using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PosText : MonoBehaviour
{
    private Text text;
    private GameObject player;

    // Use this for initialization
    void Start()
    {
        this.text = GetComponent<Text>();
        this.player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = this.player.transform.position;
        this.text.text = "X: " + pos.x + " Y: " + pos.y + " Z: " + pos.z;
    }
}