namespace UISystem
{
    using UnityEngine;
    using System.Collections.Generic;

    public static class CanvasUtility
    {
        public static void ShowPopups(Screen screen, List<PopupView> popups, int popupName)
        {
            PopupView view = popups.Find(x => x.popupName.Equals((PopupName)popupName));
            if (view != null)
            {
                view.popup.parentScreen = screen;
                view.popup.SetSortingOrder(screen.GetSortingOrder() + 1);
                screen.AddPopup(view.popup);
                view.popup.Show();
            }
            else
            {
                Debug.Log((PopupName)popupName + " Could not be found in given list");
            }
        }
        public static void ShowPopups(Screen screen, List<PopupView> popups, PopupName popupName)
        {
            PopupView view = popups.Find(x => x.popupName.Equals(popupName));
            if (view != null)
            {
                view.popup.parentScreen = screen;
                view.popup.SetSortingOrder(screen.GetSortingOrder() + 1);
                screen.HideUnncessoryBaseUI(view.popup);
                view.popup.Show();
            }
            else
            {
                Debug.Log((PopupName)popupName + " Could not be found in given list");
            }
        }
        public static void HideAllPopup(List<PopupView> popupViews)
        {
            for (int indexOfPopup = 0; indexOfPopup < popupViews.Count; indexOfPopup++)
            {
                popupViews[indexOfPopup].popup.Disable();
            }
        }
        public static void DisablePopup(List<PopupView> popups)
        {
            for (int indexOfPopup = 0; indexOfPopup < popups.Count; indexOfPopup++)
            {
                popups[indexOfPopup].popup.Disable();
            }
        }


    }
    [System.Serializable]
    public class PopupView
    {
        public Popup popup;
        public PopupName popupName;
    }
}