using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomConsole.Runtime.Console
{
    public class EntryUpdater : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textArea;
        [SerializeField] private Button clickableArea;
        private Action _buttonAction;

        private void Awake()
        {
            clickableArea.onClick.AddListener(() => _buttonAction());
        }

        public void UpdateEntryText(string text)
        {
            UpdateEntryText(text, Color.white);
        }

        public void UpdateEntryText(string text, Color textColor)
        {
            textArea.text = text;
            textArea.color = textColor;
        }

        public void UpdateClickableArea(Action clickAction, bool isClickable)
        {
            try
            {
                if (!isClickable)
                {
                    clickableArea.gameObject.SetActive(false);
                    _buttonAction = null;
                }
                else
                {
                    clickableArea.gameObject.SetActive(true);
                    _buttonAction = clickAction;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error initializing clickable area: {e.Message}");
            }
        }

        public void ShowEntry()
        {
            textArea.gameObject.SetActive(true);
        }

        public void HideEntry()
        {
            textArea.gameObject.SetActive(false);
        }
    }
}
