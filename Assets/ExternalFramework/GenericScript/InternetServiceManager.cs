using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using EventManagement;
using SimpleJSON;
using UnityEngine;

namespace Games.InternetService
{
    public enum InternetStatus
    {
        None,
        Reachable,
        NotReachable
    }
    public class InternetServiceManager : MonoBehaviour
    {
        #region EVENT&DELEGATES
        public delegate void InternetStatusChanged(InternetStatus status);
        public static event InternetStatusChanged onInternetStatusChanged;
        #endregion

        #region PUBLIC_VARS
        public static InternetServiceManager instance;
        public InternetStatus currentInternetStatus;
        public string connectionUrl;
        public bool manualConnection;
        public float internetCheckTimeInterval;
        #endregion

        #region PRIVATE_VARS
        WaitForSeconds ticks;
        
        #endregion
        
        #region UNITY_CALLBACKS
        private void Awake()
        {
            if (InternetServiceManager.instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ticks = new WaitForSeconds(internetCheckTimeInterval);
            currentInternetStatus = InternetStatus.None;
            StartCoroutine(CheckForInternetConnectivity());
        }

        #endregion

        #region PRIVATE_METHODS
        #endregion

        #region PUBLIC_METHODS
        #endregion
        
        #region InternetServicesRegion
        IEnumerator CheckForInternetConnectivity()
        {
            while (gameObject.activeInHierarchy)
            {
                if (manualConnection)
                {
                    onInternetStatusChanged?.Invoke(currentInternetStatus);
                }
                else
                {
                    StartCoroutine(GetService(connectionUrl));
                }
                yield return ticks;
            }
        }
        IEnumerator GetService(string getURL)
        {
            HTTPRequest request = new HTTPRequest(new Uri(getURL), HTTPMethods.Get, OnRequestFinished);
            request.ConnectTimeout = TimeSpan.FromSeconds(5);
            request.Send();
            yield return StartCoroutine(request);
        }

        void OnRequestFinished(BestHTTP.HTTPRequest request, BestHTTP.HTTPResponse response)
        {
            if (response != null)
            {
                if (response.StatusCode == 200 || response.StatusCode == 304)
                {
                    if(currentInternetStatus!=InternetStatus.Reachable)
                    {
                        currentInternetStatus = InternetStatus.Reachable;
                        onInternetStatusChanged?.Invoke(currentInternetStatus);
                    }
                }
                else
                {
                    if(currentInternetStatus!=InternetStatus.NotReachable)
                    {
                        currentInternetStatus = InternetStatus.NotReachable;
                        onInternetStatusChanged?.Invoke(currentInternetStatus);
                    }
                    
                }
            }
            else
            {
                if(currentInternetStatus!=InternetStatus.NotReachable)
                    {
                        currentInternetStatus = InternetStatus.NotReachable;
                        onInternetStatusChanged?.Invoke(currentInternetStatus);
                    }
            }
        }
        #endregion
    }
}

