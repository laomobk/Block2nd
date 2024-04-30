using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class SystemInfoText : MonoBehaviour
    {
        private string staticInfo1, staticInfo2;
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Start()
        {
            staticInfo1 =
                $"Device: {SystemInfo.deviceModel} | {SystemInfo.operatingSystem}\n" +
                $"CPU: {SystemInfo.processorCount}x {SystemInfo.processorType}\n";
            staticInfo2 = 
                $"GPU: {SystemInfo.graphicsDeviceName}\n" +
                $"Graphics API: {SystemInfo.graphicsDeviceVersion}";
            text.text =
                staticInfo1 +
                $"Display: {Screen.width}x{Screen.height} ({SystemInfo.graphicsDeviceVendor})\n" +
                staticInfo2;
        }

        private void Update()
        {
            text.text =
                staticInfo1 +
                $"Display: {Screen.width}x{Screen.height} ({SystemInfo.graphicsDeviceVendor})\n" +
                staticInfo2; 
        }
    }
}