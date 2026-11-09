using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EndPointData
{
    public EndPointName endPointName;
    public string endPointUrl;
    public bool usePrefix = true;
}
public enum ServerType
{
    Local,
    Live
}
public enum EndPointName
{
    CCTVData = 0,
    BroadcastAuth = 1,
    VTSVesselHydration = 2,
    Berth = 3,
    Vehicles = 4,
    Traffic = 5,
    VTSWeather = 6,
    VTSTide = 7,
    PlotDetail = 8,
    PortGPTChat = 9,
    TwinAlerts = 10,
    AnchorageNotifications = 11,

    // Twin authentication. APPEND ONLY — these values are serialized in ServerData.asset,
    // so inserting above this point would silently remap every existing endpoint.
    TwinAuthLogin = 12,
    TwinAuthTwoFactor = 13,
    TwinAuthMe = 14,
    TwinAuthLogout = 15,
    ContainerInventorySnapshot = 16
}
[Serializable]
public class ServerConfig
{
    public string url;
    public string secretKey;
    public string accessToken;
    public ServerType serverType;
}

[CreateAssetMenu(menuName = "Data/ServerData")]
public class ServerData : ScriptableObject
{
    public ServerConfig[] serverConfigs;
    public string apiPrefix = "/api/v1";
    
    [HideInInspector]
    public string currentUrl;
    public ServerType serverType;
    public List<EndPointData> endPointDatas;
    public void OnEnable()
    {
        Validata();
    }
    public void Validata()
    {
        ChangeUrl(serverType);
    }
    
    public void ChangeUrl(ServerType serverType)
    {
        ServerConfig config = Array.Find(serverConfigs, s => s.serverType == serverType);
        currentUrl = config.url;
    }

    public ServerConfig GetServerConfig()
    {
        return Array.Find(serverConfigs, s => s.serverType == serverType);
    }

    public string PrepareUrl(EndPointName endPointName)
    {
        return GetFullUrl(endPointName);
    }

    public string GetFullUrl(EndPointName name)
    {
        ChangeUrl(serverType);
        EndPointData data = endPointDatas != null ? endPointDatas.Find(e => e.endPointName == name) : null;
        if (data == null)
        {
            if (name == EndPointName.VTSWeather)
            {
                string normPrefix = string.IsNullOrEmpty(apiPrefix) ? "" : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);
                string baseWithSlash = currentUrl.EndsWith("/") ? currentUrl.Substring(0, currentUrl.Length - 1) : currentUrl;
                return $"{baseWithSlash}{normPrefix}/data-feed/vts/weather";
            }
            if (name == EndPointName.VTSTide)
            {
                string normPrefix = string.IsNullOrEmpty(apiPrefix) ? "" : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);
                string baseWithSlash = currentUrl.EndsWith("/") ? currentUrl.Substring(0, currentUrl.Length - 1) : currentUrl;
                return $"{baseWithSlash}{normPrefix}/data-feed/vts/tide";
            }
            if (name == EndPointName.PlotDetail)
            {
                string normPrefix = string.IsNullOrEmpty(apiPrefix) ? "" : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);
                string baseWithSlash = currentUrl.EndsWith("/") ? currentUrl.Substring(0, currentUrl.Length - 1) : currentUrl;
                return $"{baseWithSlash}{normPrefix}/data-feed/plot/detail";
            }
            if (name == EndPointName.AnchorageNotifications)
            {
                string normPrefix = string.IsNullOrEmpty(apiPrefix) ? "" : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);
                string baseWithSlash = currentUrl.EndsWith("/") ? currentUrl.Substring(0, currentUrl.Length - 1) : currentUrl;
                return $"{baseWithSlash}{normPrefix}/anchorage-notifications";
            }
            if (name == EndPointName.PortGPTChat)
            {
                string normPrefix = string.IsNullOrEmpty(apiPrefix) ? "" : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);
                string baseWithSlash = currentUrl.EndsWith("/") ? currentUrl.Substring(0, currentUrl.Length - 1) : currentUrl;
                return $"{baseWithSlash}{normPrefix}/port-gpt";
            }
            if (name == EndPointName.TwinAlerts)
            {
                string normPrefix = string.IsNullOrEmpty(apiPrefix) ? "" : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);
                string baseWithSlash = currentUrl.EndsWith("/") ? currentUrl.Substring(0, currentUrl.Length - 1) : currentUrl;
                return $"{baseWithSlash}{normPrefix}/twin/alerts";
            }
            // Twin auth endpoints. Fallbacks so authentication still works if the asset has not
            // been populated yet; adding them to endPointDatas overrides these.
            if (name == EndPointName.TwinAuthLogin) return BuildPrefixedUrl("/twin/auth/login");
            if (name == EndPointName.TwinAuthTwoFactor) return BuildPrefixedUrl("/twin/auth/two-factor");
            if (name == EndPointName.TwinAuthMe) return BuildPrefixedUrl("/twin/auth/me");
            if (name == EndPointName.TwinAuthLogout) return BuildPrefixedUrl("/twin/auth/logout");
            if (name == EndPointName.ContainerInventorySnapshot) return BuildPrefixedUrl("/container-inventory/snapshot");

            Debug.LogError($"Endpoint {name} not found in ServerData!");
            return string.Empty;
        }

        string prefix = data.usePrefix ? apiPrefix : "";
        return currentUrl + prefix + data.endPointUrl;
    }

    /// <summary>
    /// Joins the current base url, the api prefix and a relative path, normalising the slashes.
    /// Same composition the per-endpoint fallbacks above perform inline.
    /// </summary>
    private string BuildPrefixedUrl(string relativePath)
    {
        string normPrefix = string.IsNullOrEmpty(apiPrefix)
            ? string.Empty
            : (apiPrefix.StartsWith("/") ? apiPrefix : "/" + apiPrefix);

        string baseWithoutSlash = currentUrl.EndsWith("/")
            ? currentUrl.Substring(0, currentUrl.Length - 1)
            : currentUrl;

        return $"{baseWithoutSlash}{normPrefix}{relativePath}";
    }
}