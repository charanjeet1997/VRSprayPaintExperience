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
    }
}
