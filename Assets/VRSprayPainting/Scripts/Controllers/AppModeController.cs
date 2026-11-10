using System.Collections;
using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// App flow: starts on the main menu; from there the user picks a lesson (locked until its prerequisites are
    /// passed), Free Paint, or the getting-started tour. "Main menu" from anywhere comes back here.
    /// The sprayer (gun / can) is chosen here too; lessons and progress follow the chosen sprayer.
    /// </summary>
    public class AppModeController : MonoBehaviour
    {
        [SerializeField] private LessonController lessonController;
        [SerializeField] private AssessmentController assessment;
        [SerializeField] private PaintResetInput paintReset;
        [SerializeField] private SprayerSelector sprayerSelector;

        private IPropertyManager propertyManager;
        private IObserverManager observerManager;

        private IEnumerator Start()
        {
            propertyManager = ServiceLocator.Current.Get<IPropertyManager>();
            observerManager = ServiceLocator.Current.Get<IObserverManager>();
            observerManager.GetOrCreateObserver(ObserverNameConstants.MenuRequested).Bind(this, ShowMenu);
            observerManager.GetOrCreateObserver<int>(ObserverNameConstants.MenuLessonSelected).Bind(this, StartLesson);
            observerManager.GetOrCreateObserver(ObserverNameConstants.MenuFreePaintSelected).Bind(this, StartFreePaint);
            observerManager.GetOrCreateObserver(ObserverNameConstants.MenuTourSelected).Bind(this, StartTour);
            observerManager.GetOrCreateObserver<int>(ObserverNameConstants.MenuSprayerSelected).Bind(this, SelectSprayer);
            observerManager.GetOrCreateObserver(ObserverNameConstants.MenuHowToSelected).Bind(this, () => ShowPage(PropertyNameConstants.HowToVisible));
            observerManager.GetOrCreateObserver(ObserverNameConstants.MenuHistorySelected).Bind(this, () => ShowPage(PropertyNameConstants.HistoryVisible));
            observerManager.GetOrCreateObserver(ObserverNameConstants.MenuSettingsSelected).Bind(this, () => ShowPage(PropertyNameConstants.SettingsVisible));
            observerManager.GetOrCreateObserver(ObserverNameConstants.HowToClosed).Bind(this, () =>
            {
                PlayerPrefs.SetInt(HowToSeenKey, 1);
                PlayerPrefs.Save();
                ShowMenu();
            });

            // Every other Start (screens, lesson, assessment, onboarding) has run by the next frame, whatever
            // order Unity picked; opening the menu earlier can hit controllers that aren't set up yet.
            yield return null;
            ShowMenu();
            // First launch: how to use VR before anything else.
            if (PlayerPrefs.GetInt(HowToSeenKey, 0) == 0)
                ShowPage(PropertyNameConstants.HowToVisible);
        }

        public void ShowMenu()
        {
            lessonController.Stop();
            assessment.Close();
            // Hides the guide (and stops its demo) if the tour was open.
            observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingSkipPressed).Notify();

            SetMode(AppMode.Menu);
            HidePages();
            Set(PropertyNameConstants.MenuCards, BuildCards());
            Set(PropertyNameConstants.MenuVisible, true);
        }

        private const string HowToSeenKey = "VRSP.HowToSeen";

        // How to use / History / Settings replace the menu panel; Back returns to the menu.
        private void ShowPage(string visibleKey)
        {
            Set(PropertyNameConstants.MenuVisible, false);
            HidePages();
            Set(visibleKey, true);
        }

        private void HidePages()
        {
            Set(PropertyNameConstants.HowToVisible, false);
            Set(PropertyNameConstants.HistoryVisible, false);
            Set(PropertyNameConstants.SettingsVisible, false);
        }

        private void StartLesson(int index)
        {
            List<LessonData> lessons = CurrentLessons;
            if (index < 0 || index >= lessons.Count || !Catalog.IsUnlocked(lessons[index], Progress, sprayerSelector.Current))
                return;

            Set(PropertyNameConstants.MenuVisible, false);
            SetMode(AppMode.Lesson);
            lessonController.Begin(lessons[index]);
        }

        private void StartFreePaint()
        {
            Set(PropertyNameConstants.MenuVisible, false);
            SetMode(AppMode.FreePaint);
            paintReset.ResetAll();
        }

        private void StartTour()
        {
            StartFreePaint();
            observerManager.GetOrCreateObserver(ObserverNameConstants.OnboardingRestartPressed).Notify();
        }

        private List<LessonCardInfo> BuildCards()
        {
            var cards = new List<LessonCardInfo>();
            List<LessonData> lessons = CurrentLessons;
            for (int i = 0; i < lessons.Count; i++)
            {
                LessonData lesson = lessons[i];
                LessonData missing = Catalog.FirstMissing(lesson, Progress, sprayerSelector.Current);
                cards.Add(new LessonCardInfo
                {
                    index = i,
                    title = lesson.title,
                    summary = lessonController.Format(lesson.summary),
                    unlocked = missing == null,
                    lockedBy = missing != null ? missing.title : null,
                    passed = Progress.HasPassed(lesson.lessonId),
                    bestScore = Progress.GetBest(lesson.lessonId),
                });
            }
            return cards;
        }

        // Switching sprayer: different model, range, lessons and saved results; refresh the cards.
        private void SelectSprayer(int index)
        {
            sprayerSelector.Select(index);
            Set(PropertyNameConstants.MenuCards, BuildCards());
        }

        private LessonCatalog Catalog => lessonController.Catalog;
        private List<LessonData> CurrentLessons => Catalog.LessonsFor(sprayerSelector.Current);
        private IProgressStore Progress => ServiceLocator.Current.Get<IProgressStore>();

        private void SetMode(AppMode mode) => Set(PropertyNameConstants.AppMode, mode);

        private void Set<T>(string key, T value) => propertyManager.GetOrCreateProperty<T>(key).Value = value;
    }
}
