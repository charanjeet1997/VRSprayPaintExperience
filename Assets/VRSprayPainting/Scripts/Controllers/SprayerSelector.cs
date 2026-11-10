using System.Collections.Generic;
using DataBindingFramework;
using ServiceLocatorFramework;
using UnityEngine;

namespace VRSprayPainting
{
    /// <summary>
    /// Which sprayer the user trains with (spray gun, spray can...). Applies the profile to the held sprayer,
    /// puts it on its resting spot, keeps lesson progress separate per sprayer and remembers the choice.
    /// Publishes Sprayer.Current and the menu's sprayer options. Registers the progress store.
    /// </summary>
    public class SprayerSelector : MonoBehaviour
    {
        private const string SavedKey = "VRSP.Sprayer";

        [SerializeField] private List<SprayerProfile> profiles = new List<SprayerProfile>();
        [SerializeField] private SprayGun sprayer;
        [SerializeField] private ReturnHome returnHome;
        [SerializeField] private SprayDemoController demo;

        private ScopedProgressStore progress;
        private IPropertyManager propertyManagerCache;
        private IPropertyManager propertyManager => propertyManagerCache ??= ServiceLocator.Current.Get<IPropertyManager>();

        public IReadOnlyList<SprayerProfile> Profiles => profiles;
        public SprayerProfile Current { get; private set; }

        private void Awake()
        {
            progress = new ScopedProgressStore(new PlayerPrefsProgressStore());
            ServiceLocator.Current.Register<IProgressStore>(progress);
        }

        private void Start()
        {
            string saved = PlayerPrefs.GetString(SavedKey, "");
            int index = profiles.FindIndex(p => p != null && p.sprayerId == saved);
            Select(index >= 0 ? index : 0);
        }

        public void Select(int index)
        {
            if (index < 0 || index >= profiles.Count || profiles[index] == null)
                return;

            Current = profiles[index];
            progress.Scope = Current.progressScope;
            sprayer.ApplyProfile(Current);

            // Each sprayer has its own resting pose on the stool (a can stands upright).
            Transform rest = sprayer.ActiveModel != null ? sprayer.ActiveModel.RestPose : null;
            if (rest != null)
            {
                returnHome.SetHome(rest.position, rest.rotation);
                demo.SetHome(rest.position, rest.rotation);
                if (!sprayer.IsAutopilot)
                    returnHome.SendHome();
            }

            PlayerPrefs.SetString(SavedKey, Current.sprayerId);
            PlayerPrefs.Save();

            propertyManager.GetOrCreateProperty<SprayerProfile>(PropertyNameConstants.SprayerCurrent).Value = Current;
            propertyManager.GetOrCreateProperty<List<SprayerOptionInfo>>(PropertyNameConstants.MenuSprayers).Value = BuildOptions();
        }

        private List<SprayerOptionInfo> BuildOptions()
        {
            var options = new List<SprayerOptionInfo>();
            for (int i = 0; i < profiles.Count; i++)
                if (profiles[i] != null)
                    options.Add(new SprayerOptionInfo { index = i, name = profiles[i].displayName, selected = profiles[i] == Current });
            return options;
        }
    }
}
