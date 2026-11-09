using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UISystem
{
    public class Popup : BaseUI
    {
        [HideInInspector]
        public GameObject overlay;
        [HideInInspector]
        public Screen parentScreen;

        public override void Awake()
        {
            content = transform.GetChild(1).gameObject;
            overlay = transform.GetChild(0).gameObject;

            canvas = GetComponent<Canvas>();
            canvasGroup = content.GetComponent<CanvasGroup>();

            uiAnimator = GetComponent<UIAnimator>();
            uiAnimation = GetComponent<UIAnimation>();
        }
        public virtual void Show(Screen screen)
        {
            parentScreen=screen;
            SetSortingOrder(parentScreen.GetSortingOrder()+1);
            parentScreen.HideUnncessoryBaseUI(this);
            base.Show();
        }
        public override void Hide()
        {
            //parentScreen.ShowNcessoryBaseUI(this);
            base.Hide();
        }
        public void Fill(string description)
        {
            content.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = description.ToUpper();
        }
    }
}