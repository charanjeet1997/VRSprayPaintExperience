using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>One sprayer choice button on the main menu (segmented toggle).</summary>
    public class SprayerOptionView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color selectedColor = new Color(0.98f, 0.55f, 0.1f);
        [SerializeField] private Color idleColor = new Color(1f, 1f, 1f, 0.08f);
        [SerializeField] private Color selectedText = new Color(0.08f, 0.08f, 0.1f);
        [SerializeField] private Color idleText = new Color(0.72f, 0.75f, 0.8f);

        private int index;

        public event Action<int> Clicked;

        private void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(index));

        public void Show(SprayerOptionInfo info)
        {
            gameObject.SetActive(true);
            index = info.index;
            label.text = info.name;
            background.color = info.selected ? selectedColor : idleColor;
            label.color = info.selected ? selectedText : idleText;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
