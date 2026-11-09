using System;
using System.Collections;
using System.Collections.Generic;
using ServiceLocatorFramework;
using UnityEngine;

public class WebServiceManager : MonoBehaviour
{
    public ServerData webServiceData;

    private void OnEnable()
    {
        ServiceLocator.Current.Register<WebServiceManager>(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Current.Unregister<WebServiceManager>();
    }

    public string PrepareUrl(EndPointName endPointName)
    {
        if (webServiceData == null)
        {
            Debug.LogError("[WebServiceManager] ServerData not assigned!");
            return string.Empty;
        }
        return webServiceData.PrepareUrl(endPointName);
    }
    
}