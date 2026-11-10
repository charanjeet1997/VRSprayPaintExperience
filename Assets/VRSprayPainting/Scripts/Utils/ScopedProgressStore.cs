namespace VRSprayPainting
{
    /// <summary>
    /// Keeps results separately per sprayer: wraps another store and prefixes every lesson key with the current
    /// scope (the sprayer's progressScope). Lessons and menus use IProgressStore and never see the scope.
    /// </summary>
    public class ScopedProgressStore : IProgressStore
    {
        private readonly IProgressStore inner;

        public ScopedProgressStore(IProgressStore inner) => this.inner = inner;

        /// <summary>Empty = unprefixed keys.</summary>
        public string Scope { get; set; } = "";

        public float GetBest(string lessonId) => inner.GetBest(Key(lessonId));
        public bool HasPassed(string lessonId) => inner.HasPassed(Key(lessonId));
        public void Record(string lessonId, float score, bool passed) => inner.Record(Key(lessonId), score, passed);

        private string Key(string lessonId) => string.IsNullOrEmpty(Scope) ? lessonId : $"{Scope}/{lessonId}";
    }
}
