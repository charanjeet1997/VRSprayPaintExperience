using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>One instructor setting: name, current value and ◀ ▶ buttons to change it.</summary>
    public class SettingRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;

        public event Action<int> Stepped;

        private void Awake()
        {
            previousButton.onClick.AddListener(() => Stepped?.Invoke(-1));
            nextButton.onClick.AddListener(() => Stepped?.Invoke(1));
        }

        public void Show(string label, string value, string hint)
        {
            labelText.text = label;
            valueText.text = value;
            if (hintText != null)
                hintText.text = hint;
        }
    }
}
