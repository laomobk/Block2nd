using System;
using UnityEngine;
using UnityEngine.UI;

namespace Block2nd.GUI
{
    public class ChatUI : MonoBehaviour
    {
        public InputField inputField;

        public string GetInputText()
        {
            return inputField.text;
        }

        public void Focus()
        {
            inputField.ActivateInputField();
        }

        public void Clear()
        {
            inputField.text = "";
        }

        public void Set(string text)
        {
            inputField.text = text;
        }
    }
}