using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>Lesson results saved on the device with PlayerPrefs.</summary>
    public class PlayerPrefsProgressStore : IProgressStore
    {
        private const string BestKey = "VRSP.Lesson.{0}.Best";
        private const string PassedKey = "VRSP.Lesson.{0}.Passed";

        public float GetBest(string lessonId) => PlayerPrefs.GetFloat(string.Format(BestKey, lessonId), -1f);

        public bool HasPassed(string lessonId) => PlayerPrefs.GetInt(string.Format(PassedKey, lessonId), 0) == 1;

        public void Record(string lessonId, float score, bool passed)
        {
            if (score > GetBest(lessonId))
                PlayerPrefs.SetFloat(string.Format(BestKey, lessonId), score);
            if (passed)
                PlayerPrefs.SetInt(string.Format(PassedKey, lessonId), 1);
            PlayerPrefs.Save();
        }
    }
}
