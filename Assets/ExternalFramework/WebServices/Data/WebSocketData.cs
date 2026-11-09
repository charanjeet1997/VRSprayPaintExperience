using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for a specific WebSocket channel.
/// </summary>
[Serializable]
public class WebSocketChannelData
{
    public WebSocketChannelName channelName;
    public string channelPath; // e.g. "private-vts.vessel"
}

/// <summary>
/// Predefined channel names for the VTS system.
/// </summary>
public enum WebSocketChannelName
{
    Vessel,
    Tide,
    Weather,
    Camera,
    Berth,
    Traffic,
    Plot,
    PortGPT,

    // APPEND ONLY — these values are serialized in WebSocketData.asset.
    TwinAlerts,
    ContainerInventory
}

[Serializable]
public class WebSocketConfigData
{
    public string hostURl;
    public string pusherAppKey;
    public string pusherCluster;
    public ServerType serverType;
    public bool useWss;
}

/// <summary>
/// ScriptableObject to manage WebSocket connection settings and channels.
/// Mirrors the pattern seen in ServerData.
/// </summary>
[CreateAssetMenu(menuName = "Data/WebSocketData", fileName = "WebSocketData")]
public class WebSocketData : ScriptableObject
{
    
    
    [Header("Host Settings")]
    public WebSocketConfigData[] webSocketConfig;
    
    
    [Header("Runtime Configuration")]
    public ServerType serverType;
    
    [HideInInspector]
    public string currentUrl;

    public string currentHost;
    
    [Header("Available Channels")]
    public List<WebSocketChannelData> channels;

    private void OnEnable()
    {
        ValidateData();
    }

    /// <summary>
    /// Refreshes the current URL based on the selected server type.
    /// </summary>
    public void ValidateData()
    {
        ChangeUrl(serverType);
    }

    public WebSocketConfigData GetWebSocketConfigData()
    {
        return Array.Find(webSocketConfig,x=> x.serverType == serverType);
    }
    /// <summary>
    /// Changes the target URL based on server type.
    /// </summary>
    public void ChangeUrl(ServerType serverType)
    {
        this.serverType = serverType;
        string scheme =  "wss";
        WebSocketConfigData webSocketConfigData = Array.Find(webSocketConfig, data => data.serverType == serverType);
        string host = webSocketConfigData.hostURl;
        
        // Construct the base WebSocket URL
        currentUrl = $"{scheme}://{host}";
        currentHost = $"{host}";
    }

    /// <summary>
    /// Returns the fully formatted WebSocket base URL.
    /// </summary>
    public string GetFullUrl()
    {
        ValidateData();
        return currentUrl;
    }
}
