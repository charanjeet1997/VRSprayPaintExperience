using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// What completes an onboarding step.
    /// </summary>
    public enum OnboardingStepType
    {
        Welcome,    // primary button
        Demo,       // intro demo finished (primary button skips it)
        Grab,       // spray gun grabbed
        Spray,      // first stamp lands on a surface
        BuildUp,    // N stamps landed
        PickColor,  // palette colour chosen
        Reset,      // paint cleared
        Done,       // primary button hides the guide
    }

    [Serializable]
    public struct OnboardingStepData
    {
        public OnboardingStepType type;
        public string title;
        [TextArea(2, 4)] public string body;
        [Tooltip("Shown briefly when the step is completed.")]
        public string successText;
        [Tooltip("Primary button text. Empty hides the button (step completes by action).")]
        public string primaryLabel;
        [Tooltip("Controller input for this step, shown as a chip (e.g. GRIP). Empty hides it.")]
        public string inputHint;

        public static List<OnboardingStepData> DefaultSteps() => new List<OnboardingStepData>
        {
            new OnboardingStepData
            {
                type = OnboardingStepType.Welcome,
                title = "Welcome to the paint booth",
                body = "You'll learn to spray paint in a few quick steps.\nPoint at a button with your controller and pull the trigger to press it.",
                primaryLabel = "Watch demo",
                inputHint = "TRIGGER  to press",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.Demo,
                title = "Watch the spray gun",
                body = "The spray gun will show you how it's done.",
                primaryLabel = "Skip demo",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.Grab,
                title = "Pick up the spray gun",
                body = "The spray gun is on the stool to your right.\nReach out and hold the GRIP button to pick it up.",
                successText = "Got it!",
                inputHint = "Hold GRIP",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.Spray,
                title = "Spray the panel",
                body = "Aim the nozzle at the grey panel or the cube and pull the TRIGGER.",
                successText = "Nice, that's paint!",
                inputHint = "Pull TRIGGER",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.BuildUp,
                title = "Build up the coat",
                body = "Spray in smooth passes, gun straight on. Keep the ring GREEN (15-25 cm).\nToo close = heavy, too far = dry and speckled. Thumbstick up/down = tighter/wider spray.",
                successText = "Great coverage!",
                inputHint = "Hold TRIGGER  +  Stick",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.PickColor,
                title = "Change colour",
                body = "Use the palette on your right. Point at a colour and pull the trigger.",
                successText = "New colour loaded.",
                inputHint = "Point + TRIGGER",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.Reset,
                title = "Start over",
                body = "Press B or Y on your controller, or 'Clear paint' on the palette, to wipe every surface clean.",
                successText = "All clean.",
                inputHint = "B  or  Y",
            },
            new OnboardingStepData
            {
                type = OnboardingStepType.Done,
                title = "You're ready!",
                body = "Paint freely. Dropped the gun? It returns to the stool by itself.\nOpen this tour again from the Main menu any time.",
                primaryLabel = "Hide guide",
            },
        };
    }
}
