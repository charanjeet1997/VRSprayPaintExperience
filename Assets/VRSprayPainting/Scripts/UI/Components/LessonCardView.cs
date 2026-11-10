using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VRSprayPainting
{
    /// <summary>One lesson on the main menu: number, title, summary and status (best score / passed / locked).</summary>
    public class LessonCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Image numberBadge;
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Color openColor = new Color(0.98f, 0.55f, 0.1f);
        [SerializeField] private Color passedColor = new Color(0.3f, 0.8f, 0.4f);
        [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.25f);

        private int index;

        public event Action<int> Clicked;

        private void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(index));

        public void Show(LessonCardInfo info)
        {
            gameObject.SetActive(true);
            index = info.index;
            numberText.text = (info.index + 1).ToString();
            titleText.text = info.title;
            summaryText.text = info.summary;

            button.interactable = info.unlocked;
            group.alpha = info.unlocked ? 1f : 0.45f;
            numberBadge.color = !info.unlocked ? lockedColor : info.passed ? passedColor : openColor;

            if (!info.unlocked)
                statusText.text = string.IsNullOrEmpty(info.lockedBy) ? "Locked" : $"Locked\nPass \"{info.lockedBy}\" first";
            else if (info.passed)
                statusText.text = $"Passed\nBest {Mathf.RoundToInt(info.bestScore * 100f)}";
            else if (info.bestScore >= 0f)
                statusText.text = $"Best {Mathf.RoundToInt(info.bestScore * 100f)}\nTry again";
            else
                statusText.text = "Start";
            statusText.color = !info.unlocked ? new Color(1f, 1f, 1f, 0.6f) : info.passed ? passedColor : openColor;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
