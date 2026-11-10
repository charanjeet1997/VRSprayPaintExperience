using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// One lesson: its card on the menu, what must be passed first, and an ordered list of steps
    /// (explain, demo, practice, assess... in any order, any number). Edited in the Inspector; no code per lesson.
    /// </summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Lesson", fileName = "Lesson")]
    public class LessonData : ScriptableObject
    {
        [Tooltip("Stable key for saved progress. Don't change after release.")]
        public string lessonId = "lesson";
        public string title = "Lesson";
        [Tooltip("One line on the menu card.")]
        public string summary;
        [Tooltip("Lessons that must be passed before this one opens. Empty = always open.")]
        public List<LessonData> prerequisites = new List<LessonData>();
        [Tooltip("Sprayers this lesson is for (e.g. a can-only shaking lesson). Empty = every sprayer.")]
        public List<SprayerProfile> sprayers = new List<SprayerProfile>();

        public bool IsFor(SprayerProfile sprayer) => sprayers.Count == 0 || sprayer == null || sprayers.Contains(sprayer);

        [SerializeReference, SubclassSelector]
        public List<LessonStep> steps = new List<LessonStep>();
    }
}
