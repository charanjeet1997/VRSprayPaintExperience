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
    }
}
