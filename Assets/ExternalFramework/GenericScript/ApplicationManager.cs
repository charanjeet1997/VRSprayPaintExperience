using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    #region PUBLIC_VARS

    [Range(0, 120)] public int targetFramerate;
    [Range(0, 4)] public int vSyncCount;

    public bool logEnabled;
    [Tooltip("Keep updating (sockets, data, rendering) when the window loses focus. Must stay ON for an always-on display.")]
    public bool runInBackground = true;
    [Tooltip("Windows: stop the display and PC from sleeping / screen-saving while the app runs.")]
    public bool keepDisplayAwake = true;

    #endregion

    #region PRIVATE_VARS

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);
#endif

    #endregion

    #region UNITY_CALLBACKS

    private IEnumerator Start()
    {
        yield return null;
        Application.targetFrameRate = targetFramerate;
        Application.runInBackground = runInBackground;
        QualitySettings.vSyncCount = vSyncCount;
        Debug.unityLogger.logEnabled = logEnabled;
        SetKeepAwake(keepDisplayAwake);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        Debug.Log($"[ApplicationManager] Focus {(hasFocus ? "gained" : "lost")} at {DateTime.Now:HH:mm:ss}");
        if (hasFocus) SetKeepAwake(keepDisplayAwake);
    }

    private void OnDestroy()
    {
        SetKeepAwake(false);
    }

    #endregion

    #region PRIVATE_METHODS

    private static void SetKeepAwake(bool keepAwake)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        SetThreadExecutionState(keepAwake
            ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED
            : ES_CONTINUOUS);
#endif
    }

    #endregion
}
