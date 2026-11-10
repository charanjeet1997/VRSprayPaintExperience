namespace VRSprayPainting
{
    /// <summary>
    /// Keys for IPropertyManager (data binding) values shown by the UI.
    /// </summary>
    public static class PropertyNameConstants
    {
        // Onboarding panel
        public const string OnboardingVisible = "Onboarding.Visible";
        public const string OnboardingStepLabel = "Onboarding.StepLabel";
        public const string OnboardingTitle = "Onboarding.Title";
        public const string OnboardingBody = "Onboarding.Body";
        public const string OnboardingProgress = "Onboarding.Progress";
        public const string OnboardingPrimaryLabel = "Onboarding.PrimaryLabel";
        public const string OnboardingSucceeded = "Onboarding.Succeeded";
        public const string OnboardingStepIndex = "Onboarding.StepIndex";   // int, 0-based
        public const string OnboardingStepCount = "Onboarding.StepCount";   // int
        public const string OnboardingInputHint = "Onboarding.InputHint";   // string, empty hides the chip

        // Paint
        public const string PaintColor = "Paint.Color";

        // Assessment
        public const string AssessmentResult = "Assessment.Result";             // AssessmentResult
        public const string AssessmentVisible = "Assessment.Visible";           // bool
        public const string AssessmentThicknessView = "Assessment.ThicknessView"; // bool

        // App flow
        public const string AppMode = "App.Mode";                   // AppMode
        public const string MenuVisible = "Menu.Visible";           // bool
        public const string MenuCards = "Menu.Cards";               // List<LessonCardInfo>
        public const string MenuSprayers = "Menu.Sprayers";         // List<SprayerOptionInfo>
        public const string SprayerCurrent = "Sprayer.Current";     // SprayerProfile
        public const string HowToVisible = "HowTo.Visible";         // bool
        public const string HistoryVisible = "History.Visible";     // bool
        public const string SettingsVisible = "Settings.Visible";   // bool
        /// <summary>bool: false while a lesson step hides the training cues (e.g. the final test).</summary>
        public const string CuesAllowed = "Cues.Allowed";

        // Lesson panel
        public const string LessonVisible = "Lesson.Visible";       // bool
        public const string LessonStepLabel = "Lesson.StepLabel";
        public const string LessonTitle = "Lesson.Title";
        public const string LessonBody = "Lesson.Body";
        public const string LessonProgress = "Lesson.Progress";     // float, negative hides the bar
        public const string LessonPrimaryLabel = "Lesson.PrimaryLabel";     // empty hides the button
        public const string LessonSecondaryLabel = "Lesson.SecondaryLabel"; // empty hides the button
        public const string LessonSucceeded = "Lesson.Succeeded";   // bool, success accent colour
        public const string CoachReading = "Lesson.CoachReading";   // CoachReading shown on the sprayer, null = none
    }
}
