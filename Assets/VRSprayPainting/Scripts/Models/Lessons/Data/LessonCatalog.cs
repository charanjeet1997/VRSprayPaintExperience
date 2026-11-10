using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// The course: every lesson in menu order. Adding a lesson = create the asset and add it here.
    /// Unlocking follows each lesson's prerequisites.
    /// </summary>
    [CreateAssetMenu(menuName = "VR Spray Painting/Lesson Catalog", fileName = "LessonCatalog")]
    public class LessonCatalog : ScriptableObject
    {
        [SerializeField] private List<LessonData> lessons = new List<LessonData>();

        public IReadOnlyList<LessonData> Lessons => lessons;

        /// <summary>Lessons for one sprayer, in menu order.</summary>
        public List<LessonData> LessonsFor(SprayerProfile sprayer) => lessons.FindAll(l => l != null && l.IsFor(sprayer));

        /// <summary>All prerequisites that exist for this sprayer are passed (a can-only lesson doesn't block the gun).</summary>
        public bool IsUnlocked(LessonData lesson, IProgressStore progress, SprayerProfile sprayer) =>
            FirstMissing(lesson, progress, sprayer) == null;

        /// <summary>First prerequisite still to pass, for the locked card ("Pass: ..."). Null when unlocked.</summary>
        public LessonData FirstMissing(LessonData lesson, IProgressStore progress, SprayerProfile sprayer)
        {
            foreach (LessonData required in lesson.prerequisites)
                if (required != null && required.IsFor(sprayer) && !progress.HasPassed(required.lessonId))
                    return required;
            return null;
        }
    }
}
