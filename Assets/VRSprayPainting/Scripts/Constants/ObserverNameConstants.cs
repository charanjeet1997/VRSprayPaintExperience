namespace VRSprayPainting
{
    /// <summary>
    /// Keys for IObserverManager events between gameplay and UI.
    /// </summary>
    public static class ObserverNameConstants
    {
        // Onboarding panel buttons
        public const string OnboardingPrimaryPressed = "Onboarding.PrimaryPressed";
        public const string OnboardingSkipPressed = "Onboarding.SkipPressed";
        public const string OnboardingRestartPressed = "Onboarding.RestartPressed";

        // Paint
        public const string PaintColorSelected = "Paint.ColorSelected";   // Color
        public const string PaintResetRequested = "Paint.ResetRequested";
        public const string PaintResetPerformed = "Paint.ResetPerformed";

        // Assessment
        public const string AssessmentRequested = "Assessment.Requested";
        public const string AssessmentClosed = "Assessment.Closed";
        public const string AssessmentThicknessToggled = "Assessment.ThicknessToggled";

        // App flow
        public const string MenuRequested = "App.MenuRequested";
        public const string MenuLessonSelected = "Menu.LessonSelected";     // int lesson index
        public const string MenuFreePaintSelected = "Menu.FreePaintSelected";
        public const string MenuTourSelected = "Menu.TourSelected";
        public const string MenuSprayerSelected = "Menu.SprayerSelected"; // int profile index
        public const string MenuHowToSelected = "Menu.HowToSelected";
        public const string MenuHistorySelected = "Menu.HistorySelected";
        public const string MenuSettingsSelected = "Menu.SettingsSelected";
        public const string HowToClosed = "HowTo.Closed";

        // Lesson panel buttons
        public const string LessonPrimaryPressed = "Lesson.PrimaryPressed";
        public const string LessonSecondaryPressed = "Lesson.SecondaryPressed";
    }
}
