using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>
    /// One palette colour. Shows a ring when it is the loaded colour.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ColorSwatchButton : MonoBehaviour
    {
        [SerializeField] private Color color = Color.red;
        [SerializeField] private string colorName = "Red";
        [SerializeField] private Image swatchImage;
        [SerializeField] private GameObject selectedFrame;

        public event Action<Color> Clicked;
        public Color Color => color;
        public string ColorName => colorName;

        private void Awake()
        {
            swatchImage.color = color;
            GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke(color));
        }

        public void SetSelected(bool selected) => selectedFrame.SetActive(selected);
    }
}
