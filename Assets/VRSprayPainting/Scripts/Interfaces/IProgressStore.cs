namespace VRSprayPainting
{
    /// <summary>
    /// Where lesson results are kept. PlayerPrefsProgressStore today; a file, cloud or LMS store can replace it
    /// without touching the lessons.
    /// </summary>
    public interface IProgressStore
    {
        /// <summary>Best score 0..1, or -1 when never attempted.</summary>
        float GetBest(string lessonId);
        bool HasPassed(string lessonId);
        void Record(string lessonId, float score, bool passed);
    }
}
