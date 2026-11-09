using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UISystem
{

    public class ViewController : MonoBehaviour
    {
        public static ViewController Instance;
        Screen currentView;
        Screen previousView;
        [SerializeField] ScreenName initScreen;


        [SerializeField] List<ScreenView> screens = new List<ScreenView>();
        [SerializeField] List<PopupView> popups = new List<PopupView>();

        [SerializeField] NavBar navBar;
        [SerializeField] Popup toast;
        Stack<ScreenName> screenStack = new Stack<ScreenName>();
        private DeviceOrientation currentDeviceOrientation;
        [System.Serializable]
        public struct ScreenView
        {
            public Screen screen;
            public ScreenName screenName;
            public bool hasNavBar;
        }

        [System.Serializable]
        public struct PopupView
        {
            public Popup popup;
            public PopupName popupName;
        }
        void Awake()
        {
            Instance = this;
            // Application.targetFrameRate = 30;
            // QualitySettings.vSyncCount = 2;
            // Debug.unityLogger.logEnabled = false;
        }
        void Start() => Init();
        

        public void ShowPopup(PopupName popupName)
        {
            Debug.Log(popupName);
            popups[GetPopupIndex(popupName)].popup.Show();
        }
        
        public void HidePopup(PopupName popupName)
        {
            popups[GetPopupIndex(popupName)].popup.Hide();
        }
        public void ShowToast(string description, float delay = 3)
        {
            toast.Fill(description);
            toast.Show();
        }
        public void ChangeScreen(ScreenName screen)
        {
            if (currentView != null)
            {
                previousView = currentView;
                previousView.Hide();
                currentView = screens[GetScreenIndex(screen)].screen;
                //currentView.previousScreen = screens.Find(x => x.screen == previousView).screenName;
                currentView.Show();
            }
            else
            {
                currentView = screens[GetScreenIndex(screen)].screen;
                currentView.Show();
            }

            UnityEngine.Screen.orientation = currentView.preferredOrientation;
            currentView.OnOrientationChange(currentDeviceOrientation);
            
        }

        public void HideScreen(ScreenName screen)
        {
            currentView.Hide();

        }
        public void HideSelectedScreen(ScreenName screen)
        {
            currentView = screens[GetScreenIndex(screen)].screen;
            currentView.Hide();
        }
        void Update()
        {
            // if (Input.GetKeyDown(KeyCode.Escape))
            // {
            //     // for (int i = 0; i < popups.Count; i++)
            //     // {
            //     //     if (popups[i].popup.isActive)
            //     //     {
            //     //         popups[i].popup.Hide();
            //     //         return;
            //     //     }
            //     // }
            //     currentView.Back();
            //
            // }
            
            if (currentDeviceOrientation != Input.deviceOrientation)
            {
                currentDeviceOrientation = Input.deviceOrientation;
                currentView.OnOrientationChange(currentDeviceOrientation);
            }
        }

        int GetScreenIndex(ScreenName screen)
        {
            return screens.FindIndex(
            delegate (ScreenView screenView)
            {
                return screenView.screenName.Equals(screen);
            });
        }

        int GetPopupIndex(PopupName popup)
        {
            return popups.FindIndex(
            delegate (PopupView popupView)
            {
                return popupView.popupName.Equals(popup);
            });
        }

        public void RedrawView() => currentView.Redraw();

        private void Init()
        {
            for (int indexOfScreen = 0; indexOfScreen < screens.Count; indexOfScreen++)
            {
                screens[indexOfScreen].screen.Disable();
            }
            for (int indexOfpopup = 0; indexOfpopup < popups.Count; indexOfpopup++)
            {
                popups[indexOfpopup].popup.Disable();
            }

            if (initScreen != ScreenName.None)
            {
                ChangeScreen(initScreen);
            }

            currentDeviceOrientation = Input.deviceOrientation;
            // popups[GetPopupIndex(PopupName.LoadingPopup)].popup.Show();

        }

        // public void ShowPopup(string title, string description)
        // {
        //     toast.Show(title, description);
        // }

        // public void HidePopup()
        // {
        //     toast.Hide();
        // }

        // ViewManager.Instance.GetViewComponent<ViewHunting>().ToggleChipsPopup(true);
        // public T GetScreen<T>(ScreenName sName) => (T)screens[GetScreenIndex(sName)].screen.GetComponent<T>();
        // public T GetPopup<T>(PopupName sName) => (T)popups[GetPopupIndex(sName)].popup.GetComponent<T>();
        [ContextMenu("SetMaxLayerOfScreen")]
        public void SetMaxLayerOfScreen()
        {
            foreach(ScreenView view in screens)
            {
                view.screen.maxVisibleLayer=1;
            }
        }
       
    }
}