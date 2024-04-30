using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class SystemInfoText : MonoBehaviour
    {
        private Text text;

        private void Awake()
        {
            text = GetComponent<Text>();
        }

        private void Start()
        {
            text.text =
                $"Device: {SystemInfo.deviceModel} | {SystemInfo.operatingSystem}\n" +
                $"CPU: {SystemInfo.processorCount}x {SystemInfo.processorType}\n" +
                $"Display: {Screen.width}x{Screen.height} ({SystemInfo.graphicsDeviceVendor})\n" +
                $"GPU: {SystemInfo.graphicsDeviceName}\n" +
                $"Graphics API: {SystemInfo.graphicsDeviceVersion}";
        }
    }
}