using System.Collections;
using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRSprayPainting
{
    /// <summary>
    /// Step-by-step guide. Each step completes on the real action (grab, spray, pick colour, reset),
    /// publishes its text through IPropertyManager and toggles the world hints for that step.
    /// Opened from the main menu as the getting-started tour (OnboardingRestartPressed); not shown on launch.
    /// </summary>
    public class OnboardingController : MonoBehaviour
    {
        [SerializeField] private XRGrabInteractable gunGrab;
        [SerializeField] private SprayPainter sprayPainter;
        [SerializeField] private SprayDemoController demo;

        [Header("World hints")]
        [SerializeField] private GameObject gunHint;
        [SerializeField] private GameObject targetHint;
        [SerializeField] private GameObject paletteHint;

        [SerializeField] private int buildUpStamps = 45;
        [SerializeField] private float successDuration = 1.2f;
        [SerializeField] private List<OnboardingStepData> steps = OnboardingStepData.DefaultSteps();

        private IPropertyManager propertyManager;
        private IObserverManager observerManager;

        private int stepIndex = -1;
        private int stampCount;
        private bool isAdvancing;

        private OnboardingStepData CurrentStep => steps[stepIndex];
        private bool IsRunning => stepIndex >= 0;

        private void Start()
        {
            propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            observerManager = ServiceLocator.Current.Get<IObserverManager>();

            observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingPrimaryPressed).Bind(this, OnPrimaryPressed);
            observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingSkipPressed).Bind(this, Finish);
            observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingRestartPressed).Bind(this, Restart);
            observerManager.GetOrCreateObserver<Color>(ObserverNameConstants.PaintColorSelected).Bind(this, OnColorSelected);
            observerManager.GetOrCreateObserver(ObserverNameConstants.PaintResetPerformed).Bind(this, OnPaintReset);

            gunGrab.selectEntered.AddListener(OnGunGrabbed);
            sprayPainter.Painted += OnPainted;
            demo.CaptionChanged += OnDemoCaption;
            demo.Finished += OnDemoFinished;
        }

        private void OnDestroy()
        {
            if (gunGrab != null)
                gunGrab.selectEntered.RemoveListener(OnGunGrabbed);
            if (sprayPainter != null)
                sprayPainter.Painted -= OnPainted;
            if (demo != null)
            {
                demo.CaptionChanged -= OnDemoCaption;
                demo.Finished -= OnDemoFinished;
            }
        }

        private void ShowStep(int index)
        {
            stepIndex = index;
            stampCount = 0;
            OnboardingStepData step = CurrentStep;

            Set(PropertyNameConstants.OnboardingStepLabel, $"STEP {index + 1} OF {steps.Count}");
            Set(PropertyNameConstants.OnboardingStepCount, steps.Count);
            Set(PropertyNameConstants.OnboardingStepIndex, index);
            Set(PropertyNameConstants.OnboardingInputHint, step.inputHint);
            Set(PropertyNameConstants.OnboardingTitle, step.title);
            Set(PropertyNameConstants.OnboardingBody, step.body);
            Set(PropertyNameConstants.OnboardingPrimaryLabel, step.primaryLabel);
            Set(PropertyNameConstants.OnboardingProgress, step.type == OnboardingStepType.BuildUp ? 0f : -1f);
            Set(PropertyNameConstants.OnboardingSucceeded, false);
            Set(PropertyNameConstants.OnboardingVisible, true);

            UpdateHints(step.type);

            // Already holding the gun when the grab step appears: don't make the user re-grab.
            if (step.type == OnboardingStepType.Grab && gunGrab.isSelected)
                Complete(OnboardingStepType.Grab);

            // Demo can't fly a gun that is in a hand: go straight on.
            if (step.type == OnboardingStepType.Demo && !demo.Play())
                Complete(OnboardingStepType.Demo);
        }

        private void Complete(OnboardingStepType type)
        {
            if (!IsRunning || isAdvancing || CurrentStep.type != type)
                return;

            StartCoroutine(AdvanceAfterSuccess());
        }

        private IEnumerator AdvanceAfterSuccess()
        {
            isAdvancing = true;
            OnboardingStepData step = CurrentStep;

            if (!string.IsNullOrEmpty(step.successText))
            {
                Set(PropertyNameConstants.OnboardingBody, step.successText);
                Set(PropertyNameConstants.OnboardingSucceeded, true);
                Set(PropertyNameConstants.OnboardingPrimaryLabel, string.Empty);
                Set(PropertyNameConstants.OnboardingInputHint, string.Empty);
                Set(PropertyNameConstants.OnboardingProgress, -1f);
                UpdateHints(null);
                yield return new WaitForSeconds(successDuration);
            }

            isAdvancing = false;

            if (stepIndex + 1 < steps.Count)
                ShowStep(stepIndex + 1);
            else
                Finish();
        }

        private void Finish()
        {
            StopDemo();
            StopAllCoroutines();
            isAdvancing = false;
            stepIndex = -1;
            UpdateHints(null);
            Set(PropertyNameConstants.OnboardingVisible, false);
        }

        private void Restart()
        {
            StopDemo();
            StopAllCoroutines();
            isAdvancing = false;
            ShowStep(0);
        }

        private void UpdateHints(OnboardingStepType? type)
        {
            SetActive(gunHint, type == OnboardingStepType.Grab);
            SetActive(targetHint, type == OnboardingStepType.Spray || type == OnboardingStepType.BuildUp);
            SetActive(paletteHint, type == OnboardingStepType.PickColor || type == OnboardingStepType.Reset);
        }

        private void OnPrimaryPressed()
        {
            if (!IsRunning)
                return;

            if (CurrentStep.type == OnboardingStepType.Done)
                Finish();
            else if (CurrentStep.type == OnboardingStepType.Demo)
                StopDemo(); // Finished -> completes the step
            else
                Complete(CurrentStep.type);
        }

        private void OnGunGrabbed(SelectEnterEventArgs args) => Complete(OnboardingStepType.Grab);

        private void OnPainted()
        {
            if (!IsRunning || isAdvancing)
                return;

            if (CurrentStep.type == OnboardingStepType.Spray)
            {
                Complete(OnboardingStepType.Spray);
            }
            else if (CurrentStep.type == OnboardingStepType.BuildUp)
            {
                stampCount++;
                Set(PropertyNameConstants.OnboardingProgress, Mathf.Clamp01((float)stampCount / buildUpStamps));
                if (stampCount >= buildUpStamps)
                    Complete(OnboardingStepType.BuildUp);
            }
        }

        private void OnDemoCaption(string caption)
        {
            if (IsRunning && CurrentStep.type == OnboardingStepType.Demo)
                Set(PropertyNameConstants.OnboardingBody, caption);
        }

        private void OnDemoFinished() => Complete(OnboardingStepType.Demo);

        private void StopDemo()
        {
            if (demo.IsPlaying)
                demo.Stop();
        }

        private void OnColorSelected(Color color) => Complete(OnboardingStepType.PickColor);
        private void OnPaintReset() => Complete(OnboardingStepType.Reset);

        private void Set<T>(string key, T value) => propertyManager.GetOrCreateProperty<T>(key).Value = value;

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
                target.SetActive(active);
        }
    }
}
