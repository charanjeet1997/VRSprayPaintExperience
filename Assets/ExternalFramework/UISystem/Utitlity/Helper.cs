// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// // using SimpleJSON;
// // using BestHTTP.SocketIO;
// using System.IO;
// using System.Linq;
// using System.Text;
// using System.Text.RegularExpressions;
// public static class Helper {
//     
//     public static Coroutine ExecuteScrollBarSnap (this MonoBehaviour monoBehaviour,Scrollbar scrollbar,float delay,float snapTime) {
//         return monoBehaviour.StartCoroutine(ScrollBarSnap(scrollbar,delay,snapTime));
//     }
//     static IEnumerator ScrollBarSnap(Scrollbar scroll,float delay,float snapTime)
//     {
//         yield return new WaitForSeconds(delay);
//         float currentime=0;
//         float initialValue=scroll.value;
//         float targetValue=1;
//         snapTime = snapTime*(1-initialValue);
//         Debug.Log(snapTime);
//         while(currentime<snapTime)
//         {
//             currentime+=Time.deltaTime;
//             scroll.value=Mathf.Lerp(initialValue,targetValue,currentime/snapTime);
//             Debug.Log(currentime+":"+scroll.value);
//             yield return null;
//         } 
//     }
//     public static Coroutine Execute (this MonoBehaviour monoBehaviour, Action action, float time) {
//         return monoBehaviour.StartCoroutine (DelayedAction (action, time));
//     }
//     static IEnumerator DelayedAction (Action action, float time) {
//         yield return new WaitForSecondsRealtime (time);
//         action ();
//     }
//     public static Coroutine WaitUntil (this MonoBehaviour monoBehaviour, System.Func<bool> condition, Action action) {
//         return monoBehaviour.StartCoroutine (DelayedAction (condition, action));
//     }
//     static IEnumerator DelayedAction (System.Func<bool> condition, Action action) {
//         if (condition == null)
//             yield break;
//         yield return new WaitUntil (condition);
//         action ();
//     }
//
//     public static void AnimationToggle (Animation animation, AnimationClip clip1, AnimationClip clip2) {
//         if (animation.clip == clip1) {
//             animation.clip = clip2;
//         } else {
//             animation.clip = clip1;
//         }
//         animation.Play ();
//     }
//     public static Action[] ParamsToArray (params Action[] actions) {
//         return actions;
//     }
//     public static Texture2D CaptureScreenShot () {
//         Texture2D screenshot;
//         screenshot = new Texture2D (Screen.width, Screen.height, TextureFormat.ARGB32, false);
//         screenshot.ReadPixels (new Rect (0.5f, 0.5f, Screen.width, Screen.height), 0, 0, false);
//         screenshot.Apply ();
//         return screenshot;
//     }
//     //---------- Save Screenshot ----------//
//     public static void Save_Screenshot (Texture2D screenshot, string postFixName, string extension) {
//         string screenShotPath = Application.persistentDataPath + "/" + DateTime.Now.ToString ("dd-MM-yyyy_HH:mm:ss") + "_" + postFixName + extension;
//         File.WriteAllBytes (screenShotPath, screenshot.EncodeToPNG ());
//         Debug.Log (screenShotPath);
//     }
//
//     public static Sprite CreateSpriteOutOfTexture (Texture2D texture) {
//         Sprite tst = Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0f, 0f), 100f);
//         return tst;
//     }
//
//     public static T GetRandom<T> (this T[] array) {
//         return array[UnityEngine.Random.Range (0, array.Length)];
//     }
//
//     public static T GetRandom<T> (this List<T> list) {
//         return list[UnityEngine.Random.Range (0, list.Count)];
//     }
//
//     public static T ToEnum<T> (this string value) {
//         return (T) Enum.Parse (typeof (T), value, true);
//     }
//     public static Bounds GetRenderBounds (GameObject go) {
//         var totalBounds = new Bounds ();
//         totalBounds.SetMinMax (Vector3.one * Mathf.Infinity, -Vector3.one * Mathf.Infinity);
//         foreach (var renderer in go.GetComponentsInChildren<Renderer> ()) {
//             var bounds = renderer.bounds;
//             var totalMin = totalBounds.min;
//             totalMin.x = Mathf.Min (totalMin.x, bounds.min.x);
//             totalMin.y = Mathf.Min (totalMin.y, bounds.min.y);
//             totalMin.z = Mathf.Min (totalMin.z, bounds.min.z);
//
//             var totalMax = totalBounds.max;
//             totalMax.x = Mathf.Max (totalMax.x, bounds.max.x);
//             totalMax.y = Mathf.Max (totalMax.y, bounds.max.y);
//             totalMax.z = Mathf.Max (totalMax.z, bounds.max.z);
//
//             totalBounds.SetMinMax (totalMin, totalMax);
//         }
//         return totalBounds;
//     }
//     public static T RandomEnumValue<T> () {
//         var v = Enum.GetValues (typeof (T));
//
//         return (T) v.GetValue (UnityEngine.Random.Range (0, v.Length));
//     }
//
//     public static T RandomEnumValueSkipFirst<T> () {
//         var v = Enum.GetValues (typeof (T));
//
//         return (T) v.GetValue (UnityEngine.Random.Range (1, v.Length));
//     }
//
//     public static T RandomEnumValueSkipLast<T> () {
//         var v = Enum.GetValues (typeof (T));
//
//         return (T) v.GetValue (UnityEngine.Random.Range (0, v.Length - 1));
//     }
//
//     public static float MapEvaluate (this AnimationCurve curve, float point, float start = 0, float end = 1) {
//         return curve.Evaluate (Helper.Map (start, end, 0f, 1f, point));
//     }
//
//     /// <summary>
//     /// Extension method to return an enum value of type T for the given int.
//     /// </summary>
//     /// <typeparam name="T"></typeparam>
//     /// <param name="value"></param>
//     /// <returns></returns>
//     public static T ToEnum<T> (this int value) {
//         var name = Enum.GetName (typeof (T), value);
//         return name.ToEnum<T> ();
//     }
//
//     /// <summary>
//     /// Invokes an action after waiting for a specified number of seconds
//     /// </summary>
//     /// <param name="delay">The number of seconds to wait before invoking</param>
//     /// <param name="myAction">The action to invoke after the time has passed</param>
//     public static IEnumerator InvokeInSeconds (float delay, Action myAction) {
//         yield return new WaitForSeconds (delay);
//         myAction ();
//     }
//
//     //	StartCoroutine(Helper.Schedule(
//     //		new Action[]
//     //		{
//     //			()=>print("2 seconds"),
//     //			()=>print("0 seconds"),
//     //			()=>print("5 seconds")
//     //		},
//     //
//     //		new float[]
//     //		{
//     //			2,0,5
//     //		}
//     //		));
//     /// <summary>
//     /// Schedule the specified actions.
//     /// </summary>
//     /// <param name="myActions">Array of actions to be performed.</param>
//     /// <param name="delay">The delay betweek two consecutive actions.</param>
//     public static IEnumerator Schedule (Action[] myActions, float[] delay, bool isContinouse) {
//         do {
//             for (int i = 0; i < myActions.Length; i++) {
//                 yield return new WaitForSeconds (delay[i]);
//                 myActions[i] ();
//             }
//
//         } while (isContinouse);
//     }
//     public static IEnumerator DoActionAfterSometime (float waitTime, Action action) {
//         while (true) {
//             yield return new WaitForSeconds (waitTime);
//             action ();
//         }
//     }
//     /// <summary>
//     /// Map the specified current1, current2, target1, target2 and val.
//     /// </summary>
//     /// <param name="current1">Lower bound of the value's current range.</param>
//     /// <param name="current2">Upper bound of the value's current range.</param>
//     /// <param name="target1">Lower bound of the value's target range.</param>
//     /// <param name="target2">Upper bound of the value's target range.</param>
//     /// <param name="val">Value to be scaled or mapped.</param>
//     public static float Map (float current1, float current2, float target1, float target2, float val) {
//         //third parameter is the interpolant between the current range which, in turn, is used in the linear interpolation of the target range. 
//         return Mathf.Lerp (target1, target2, Mathf.InverseLerp (current1, current2, val));
//     }
//
//     //unity already has this called LerpUnclamped
//     static float ULerp (float from, float to, float val) {
//         return (1 - val) * from + val * to;
//     }
//
//     //NOVELTY BITCH!
//     static float UInverseLerp (float from, float to, float val) {
//         return (val - from) / (to - from);
//     }
//
//     //unclamped map
//     public static float UMap (float current1, float current2, float target1, float target2, float val) {
//         return ULerp (target1, target2, UInverseLerp (current1, current2, val));
//     }
//
//     /*Mathf.Lerp(a,b,Time.deltatime) is not mathematically correct and is not framerate independent. Exponential Decay is here to help
//     http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
//     TL;DR
//     Use this in the future if you want exponential smoothing that's frame rate independent. */
//     // Smoothing rate dictates the proportion of source remaining after one second
//     public static float Damp (float source, float target, float smoothing, float dt) {
//         return Mathf.Lerp (source, target, 1 - Mathf.Pow (smoothing, dt));
//     }
//
//     public static void SafeDestroy (this GameObject go) {
//         if (go != null) {
//             MonoBehaviour.Destroy (go);
//         } else {
//             Debug.Log ("The object that you're trying to destroy doesn't exist!");
//         }
//     }
//
//     public static void Deactivate (this GameObject go) {
//         if (go != null) {
//             go.SetActive (false);
//         } else {
//             Debug.Log ("The object that you're trying to deactivate doesn't exist!");
//         }
//     }
//
//     public static void Activate (this GameObject go) {
//         if (go != null) {
//             go.SetActive (true);
//         } else {
//             Debug.Log ("The object that you're trying to activate doesn't exist!");
//         }
//     }
//     public static Vector2 PlaceRectAtWorldPosition (Canvas myCanvas, Camera camera, Vector2 targetPosition, RectTransform myRect) {
//         targetPosition = WorldToScreenWithScale (myCanvas, camera, targetPosition);
//
//         //get half of screen width and height, taking scaling into account
//         RectTransform canvasRect = myCanvas.GetComponent<RectTransform> ();
//         int screenWidthFromCenter = (int) (Screen.width / myCanvas.scaleFactor) / 2;
//         int screenHeightFromCenter = (int) (Screen.height / myCanvas.scaleFactor) / 2;
//
//         //"convert" this value to bottom left of parent (i.e. pretend that anchor = 0,0)
//         Vector2 bottomLeftOfParent = new Vector2 ((-myRect.anchorMax.x * 2) * screenWidthFromCenter, (-myRect.anchorMax.y * 2) * screenHeightFromCenter);
//
//         //move the "converted" value to targetPosition
//         return bottomLeftOfParent + targetPosition;
//     }
//
//     /// <summary>
//     /// Converts letter to its ASCII equivalent.
//     /// </summary>
//     /// <returns>ASCII value of the letter.</returns>
//     /// <param name="str">String sent.</param>
//     /// <param name="index">Index of char in the string to convert, 0 by default.</param>
//     public static int LetterToASCII (this string str, int index = 0) {
//         char i = str[index];
//         int j = i;
//         return j;
//     }
//
//     //if(!SanePrefs.isBonusMiniGame) helper
//     public enum Axis { X, Y, Z, XY, XZ, YZ, ALL }
//     /// <summary>
//     /// Transform to apply if(!SanePrefs.isBonusMiniGame) to.
//     /// </summary>
//     /// <param name="transform">Transform of the gameObject.</param>
//     /// <param name="duration">Duration.</param>
//     /// <param name="magnitude">Magnitude.</param>
//     /// <param name="axis">Axis.</param>
//     /// <param name="usePause">If set to <c>true</c> use pause.</param>
//     public static IEnumerator Shake (this Transform transform, float duration, float magnitude, Axis axis, bool usePause = true) {
//         for (float time = 0; time < duration; time += usePause ? Time.deltaTime : Time.unscaledDeltaTime) {
//             if (!usePause || Time.timeScale != 0) {
//                 Vector3 offset = new Vector3 (
//                     axis == Axis.X || axis == Axis.XY || axis == Axis.XZ || axis == Axis.ALL ? UnityEngine.Random.Range (-magnitude, magnitude) : 0,
//                     axis == Axis.Y || axis == Axis.XY || axis == Axis.YZ || axis == Axis.ALL ? UnityEngine.Random.Range (-magnitude, magnitude) : 0,
//                     axis == Axis.Z || axis == Axis.XZ || axis == Axis.YZ || axis == Axis.ALL ? UnityEngine.Random.Range (-magnitude, magnitude) : 0);
//                 transform.position += offset;
//                 yield return new WaitForEndOfFrame ();
//                 transform.position -= offset;
//             }
//         }
//     }
//
//     /// <summary>
//     /// Smooth moves from current position to the target position.
//     /// </summary>
//     /// <returns>The move to.</returns>
//     /// <param name="transform">Transform to be moved.</param>
//     /// <param name="fromPos">Initial position of the transform.</param>
//     /// <param name="toPos">Final position of the transform.</param>
//     /// <param name="duration">Lerp duration.</param>
//     /// <param name="myActions">List of actions to be executed at the end of the coroutine. Send a null if no actions are to be sent.</param>
//     public static IEnumerator SmoothMove (Transform transform, Vector3 fromPos, Vector3 toPos, float duration, List<Action> myActions = null) {
//         float elapsed = 0;
//         while (elapsed < duration && transform) {
//             transform.position = Vector3.Lerp (fromPos, toPos, (elapsed / duration));
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//         transform.position = toPos;
//         if (myActions != null) {
//             foreach (var Action in myActions) {
//                 Action ();
//             }
//         }
//         yield return null;
//     }
//
//     public static IEnumerator CountTo (Text text, int from, int to, float duration, string prefix = null, string postFix = null) {
//         int currentCount = 0;
//         for (float timer = 0; timer < duration; timer += Time.deltaTime) {
//             float progress = timer / duration;
//             currentCount = (int) Mathf.Lerp (from, to, progress);
//             text.text = prefix + currentCount.ToString () + postFix;
//             yield return null;
//         }
//         text.text = prefix + to + postFix;
//
//         yield return null;
//     }
//
//     public static IEnumerator SmoothMove (RectTransform transform, Vector3 fromPos, Vector3 toPos, float duration, List<Action> myActions = null) {
//         float elapsed = 0;
//         while (elapsed < duration && transform) {
//             transform.anchoredPosition = Vector3.Lerp (fromPos, toPos, (elapsed / duration));
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//         transform.anchoredPosition = toPos;
//         if (myActions != null) {
//             foreach (var Action in myActions) {
//                 Action ();
//             }
//         }
//         yield return null;
//     }
//
//     private static GUIStyle guiStyle = new GUIStyle ();
//     public static void DebugText (string text, Vector3 pos, Color color = default (Color), int size = 15) {
//         //check if default
//         if (color.a == 0.000f) {
//             color = Color.white;
//         }
//         guiStyle.fontSize = size;
//         guiStyle.normal.textColor = color;
//         Vector3 screenPos = Camera.main.WorldToScreenPoint (pos);
//         Vector2 textSize = GUI.skin.label.CalcSize (new GUIContent (text));
//         // GUI.color = color;
//         GUI.Label (new Rect (screenPos.x - (textSize.x / 2f) - (size), UnityEngine.Screen.height - screenPos.y, textSize.x, textSize.y), text, guiStyle);
//     }
//
//     public static IEnumerator SmoothScale (RectTransform transform, Vector3 fromScale, Vector3 toScale, float duration, List<Action> myActions = null) {
//         float elapsed = 0;
//         while (elapsed < duration && transform) {
//             transform.localScale = Vector3.Lerp (fromScale, toScale, (elapsed / duration));
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
//         transform.localScale = toScale;
//         if (myActions != null) {
//             foreach (var Action in myActions) {
//                 Action ();
//             }
//         }
//         yield return null;
//     }
//
//     public static IEnumerator SmoothFade (GameObject go, Color toColor, float duration, List<Action> myActions = null) {
//         float elapsed = 0;
//         if (go.GetComponent<Image> ()) {
//             Image image = go.GetComponent<Image> ();
//             Color startColor = image.color;
//             while (elapsed < duration && go) {
//                 image.color = Color.Lerp (startColor, toColor, (elapsed / duration));
//                 elapsed += Time.deltaTime;
//                 yield return null;
//             }
//             image.color = toColor;
//         } else if (go.GetComponent<SpriteRenderer> ()) {
//             SpriteRenderer sr = go.GetComponent<SpriteRenderer> ();
//             Color startColor = sr.color;
//             while (elapsed < duration && go) {
//                 sr.color = Color.Lerp (startColor, toColor, (elapsed / duration));
//                 elapsed += Time.deltaTime;
//                 yield return null;
//             }
//             sr.color = toColor;
//         } else if (go.GetComponent<TextMesh> ()) {
//             TextMesh tm = go.GetComponent<TextMesh> ();
//             Color startColor = tm.color;
//             while (elapsed < duration && go) {
//                 tm.color = Color.Lerp (startColor, toColor, (elapsed / duration));
//                 elapsed += Time.deltaTime;
//                 yield return null;
//             }
//             tm.color = toColor;
//         } else {
//             Debug.Log ("Image/Sprite Rendere not found! Fade won't work.");
//         }
//         if (myActions != null) {
//             foreach (var Action in myActions) {
//                 Action ();
//             }
//         }
//         yield return null;
//     }
//     public static Vector2 WorldToScreenWithScale (Canvas myCanvas, Camera camera, Vector2 targetPosition) {
//         Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint (camera, targetPosition);
//         screenPoint = screenPoint / myCanvas.scaleFactor; //apply scaling
//         return screenPoint;
//     }
//
//     //------------------------------------------------------------------------------------------------------------------------------LIST------------------------------------------------------------------------------------------------------------------------------//
//     private static System.Random rng = new System.Random ();
//     /// <summary>
//     /// Shuffle the specified list.
//     /// </summary>
//     /// <param name="list">List.</param>
//     /// <typeparam name="T">The 1st type parameter.</typeparam>
//     public static void Shuffle<T> (this IList<T> list) {
//         int n = list.Count;
//         while (n > 1) {
//             n--;
//             int k = rng.Next (n + 1);
//             T value = list[k];
//             list[k] = list[n];
//             list[n] = value;
//         }
//     }
//
//     /// <summary>
//     /// Better than regular shuffle.
//     /// </summary>
//     /// <param name="a">The alpha component.</param>
//     /// <typeparam name="T">The 1st type parameter.</typeparam>
//     public static void FYShuffle<T> (this IList<T> a) {
//         // Loops through array
//         for (int i = a.Count - 1; i > 0; i--) {
//             // Randomize a number between 0 and i (so that the range decreases each time)
//             int rnd = UnityEngine.Random.Range (0, i);
//             // Save the value of the current i, otherwise it'll overwrite when we swap the values
//             T temp = a[i];
//             // Swap the new and old values
//             a[i] = a[rnd];
//             a[rnd] = temp;
//         }
//     }
//
//     public static List<int> GenerateRandom (int count, int min, int max) {
//         if (max <= min || count < 0 ||
//             (count > max - min && max - min > 0)) {
//             throw new ArgumentOutOfRangeException ("Range " + min + " to " + max +
//                 " (" + ((Int64) max - (Int64) min) + " values), or count " + count + " is illegal");
//         }
//
//         HashSet<int> candidates = new HashSet<int> ();
//
//         for (int top = max - count; top < max; top++) {
//             if (!candidates.Add (rng.Next (min, top + 1))) {
//                 candidates.Add (top);
//             }
//         }
//
//         List<int> result = candidates.ToList ();
//
//         for (int i = result.Count - 1; i > 0; i--) {
//             int k = rng.Next (i + 1);
//             int tmp = result[k];
//             result[k] = result[i];
//             result[i] = tmp;
//         }
//         return result;
//     }
//
//     /// <summary>
//     /// Sorts the number list (int).
//     /// </summary>
//     /// <param name="list">List to sort.</param>
//     /// <param name="ascending">If set to true; sort in ascending order, else sort in descending order.</param>
//     public static void SortNumberList (List<int> list, bool ascending = true) {
//         int temp;
//         for (int i = 0; i < list.Count - 1; i++) {
//             for (int j = i + 1; j < list.Count; j++) {
//                 if (ascending) {
//                     if (list[i] > list[j]) {
//                         temp = list[i];
//                         list[i] = list[j];
//                         list[j] = temp;
//                     }
//                 } else {
//                     if (list[i] < list[j]) {
//                         temp = list[i];
//                         list[i] = list[j];
//                         list[j] = temp;
//                     }
//                 }
//             }
//         }
//     }
//
//     /// <summary>
//     /// Sorts the number list (float).
//     /// </summary>
//     /// <param name="list">List to sort.</param>
//     /// <param name="ascending">If set to true; sort in ascending order, else sort in descending order.</param>
//     public static void SortNumberList (List<float> list, bool ascending = true) {
//         float temp;
//         for (int i = 0; i < list.Count - 1; i++) {
//             for (int j = i + 1; j < list.Count; j++) {
//                 if (ascending) {
//                     if (list[i] > list[j]) {
//                         temp = list[i];
//                         list[i] = list[j];
//                         list[j] = temp;
//                     }
//                 } else {
//                     if (list[i] < list[j]) {
//                         temp = list[i];
//                         list[i] = list[j];
//                         list[j] = temp;
//                     }
//                 }
//             }
//         }
//     }
//
//     //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
//     //------------------------------------------------------------------------------------------------------------------------------COLOR------------------------------------------------------------------------------------------------------------------------------//
//
//     /// <summary>
//     /// Returns the color with the desired alpha level
//     /// </summary>
//     /// <returns>Alpha value</returns>
//     /// <param name="color">Color whose alpha is to be changed.</param>
//     /// <param name="alpha">Alpha value between 0 and 1.</param>
//     public static Color WithAlpha (this Color color, float alpha) {
//         return new Color (color.r, color.g, color.b, alpha);
//     }
//
//     //the 4 methods below are taken from http://www.quickfingers.net/quick-bites-01-color-extensions/
//     /// 
//     /// Output a hex string from a color
//     /// 
//     ///
//     /// Set to true to include a # character at the start
//     /// 
//     public static string HexFromColor (this Color color, bool includeHash = false) {
//         string red = Mathf.FloorToInt (color.r * 255).ToString ("X2");
//         string green = Mathf.FloorToInt (color.g * 255).ToString ("X2");
//         string blue = Mathf.FloorToInt (color.b * 255).ToString ("X2");
//         return (includeHash ? "#" : "") + red + green + blue;
//     }
//
//     /// 
//     /// Create a Color object from a Hex string (It's not important if you have a # character at
//     /// the start or not)
//     /// 
//     /// The hex string to convert
//     /// A Color object
//     public static Color ColorFromHex (string color) {
//         // remove the # character if there is one.
//         color = color.TrimStart ('#');
//         float red = (HexToInt (color[1]) + HexToInt (color[0]) * 16f) / 255f;
//         float green = (HexToInt (color[3]) + HexToInt (color[2]) * 16f) / 255f;
//         float blue = (HexToInt (color[5]) + HexToInt (color[4]) * 16f) / 255f;
//         Color finalColor = new Color { r = red, g = green, b = blue, a = 1 };
//         return finalColor;
//     }
//
//     /// 
//     /// Create a color object from integer R G B (A) components
//     /// 
//     ///The red component
//     ///The green component
//     ///The blue component
//     ///The alpha component (Defaults to 255, or fully opaque)
//     /// A Color object
//     public static Color ColorFromInt (int r, int g, int b, int a = 255) {
//         return new Color (r / 255f, g / 255f, b / 255f, a / 255f);
//     }
//
//     private static int HexToInt (char hexValue) {
//         return int.Parse (hexValue.ToString (), System.Globalization.NumberStyles.HexNumber);
//     }
//
//     public static Texture2D MakeTex (int width, int height, Color col) {
//         Color[] pix = new Color[width * height];
//         for (int i = 0; i < pix.Length; ++i) {
//             pix[i] = col;
//         }
//         Texture2D result = new Texture2D (width, height);
//         result.SetPixels (pix);
//         result.Apply ();
//         return result;
//     }
//
//     public static Color ChangeColorBrightness (Color color, float correctionFactor) {
//         float red = (float) color.r;
//         float green = (float) color.g;
//         float blue = (float) color.b;
//         if (correctionFactor < 0) {
//             correctionFactor = 1 + correctionFactor;
//             red *= correctionFactor;
//             green *= correctionFactor;
//             blue *= correctionFactor;
//         } else {
//             red = (255 - red) * correctionFactor + red;
//             green = (255 - green) * correctionFactor + green;
//             blue = (255 - blue) * correctionFactor + blue;
//         }
//         return FromArgb ((int) color.a, (int) red, (int) green, (int) blue);
//     }
//
//     public static Color FromArgb (int alpha, int red, int green, int blue) {
//         //      float fa = ((float)alpha) / 255.0f;
//         float fa = 255.0f;
//         float fr = ((float) red) / 255.0f;
//         float fg = ((float) green) / 255.0f;
//         float fb = ((float) blue) / 255.0f;
//         return new Color (fr, fg, fb, fa);
//     }
//     //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
//
//     //------------------------------------------------------------------------------------------------------------------------------VECTOR------------------------------------------------------------------------------------------------------------------------------//
//
//     public static Vector2 DirToPoint (Vector2 v, float degrees) {
//         float radians = degrees * Mathf.Deg2Rad;
//         float sin = Mathf.Sin (radians);
//         float cos = Mathf.Cos (radians);
//         float tx = v.x;
//         float ty = v.y;
//         return new Vector2 (cos * tx - sin * ty, sin * tx + cos * ty);
//     }
//
//     public static bool IsBetween (this float x, float x1, float x2) {
//         if (x1 < x2) {
//             return (x > x1 && x <= x2);
//         } else {
//             return (x > x2 && x <= x1);
//
//         }
//     }
//
//     public static bool IsXBetween (this Vector3 p, Vector3 p1, Vector3 p2) {
//         if (p1.x < p2.x) {
//             return (p.x > p1.x && p.x <= p2.x);
//         } else {
//             return (p.x > p2.x && p.x <= p1.x);
//
//         }
//     }
//
//     public static bool IsXBetween (this Vector3 p, Transform p1, Transform p2) {
//         if (p1.position.x < p2.position.x) {
//             return (p.x > p1.position.x && p.x <= p2.position.x);
//         } else {
//             return (p.x > p2.position.x && p.x <= p1.position.x);
//
//         }
//     }
//
//     public static bool IsZBetween (this Vector3 p, Vector3 p1, Vector3 p2) {
//         if (p1.z < p2.z) {
//             return (p.z > p1.z && p.z < p2.z);
//         } else {
//             return (p.z > p2.z && p.z < p1.z);
//         }
//     }
//
//     public static bool IsZBetween (this Vector3 p, Transform p1, Transform p2) {
//         if (p1.position.z < p2.position.z) {
//             return (p.z > p1.position.z && p.z < p2.position.z);
//         } else {
//             return (p.z > p2.position.z && p.z < p1.position.z);
//         }
//     }
//
//     public static List<Vector3> GeneratePointsBetween (Vector3 from, Vector3 to, int chunkAmount) {
//         //divider must be between 0 and 1
//         float divider = 1f / chunkAmount;
//         float linear = 0f;
//         List<Vector3> result = new List<Vector3> ();
//         if (chunkAmount == 0) {
//             Debug.LogError ("chunkAmount Distance must be > 0 instead of " + chunkAmount);
//             return null;
//         }
//
//         if (chunkAmount == 1) {
//             result.Add (Vector3.Lerp (from, to, 0.5f)); //Return half/middle point
//         }
//
//         for (int i = 0; i < chunkAmount; i++) {
//             if (i == 0) {
//                 linear = divider / 2;
//             } else {
//                 linear += divider; //Add the divider to it to get the next distance
//             }
//             result.Add (Vector3.Lerp (from, to, linear));
//         }
//
//         return result;
//     }
//
//     public static float GetDefenderSeparationDistance (Vector3 from, Vector3 to, int chunkAmount) {
//         //divider must be between 0 and 1
//         float divider = 1f / chunkAmount;
//         float linear = 0f;
//         List<Vector3> result = new List<Vector3> ();
//         if (chunkAmount == 0) {
//             Debug.LogError ("chunkAmount Distance must be > 0 instead of " + chunkAmount);
//             return 0;
//         }
//
//         if (chunkAmount == 1) {
//             result.Add (Vector3.Lerp (from, to, 0.5f)); //Return half/middle point
//         }
//
//         for (int i = 0; i < chunkAmount; i++) {
//             if (i == 0) {
//                 linear = divider / 2;
//             } else {
//                 linear += divider; //Add the divider to it to get the next distance
//             }
//             result.Add (Vector3.Lerp (from, to, linear));
//         }
//
//         return Mathf.Abs (result[0].x - result[1].x);
//     }
//     /// <summary>
//     /// Truncates the Vector3 (z value is removed)
//     /// </summary>
//     /// <returns>Vector2.</returns>
//     /// <param name="vec3">Vector3 to truncate.</param>
//     public static Vector2 ToVec2 (this Vector3 vec3) {
//         return new Vector2 (vec3.x, vec3.y);
//     }
//
//     /// <summary>
//     /// Converts a Vector2 to Vector3 with the required z value
//     /// </summary>
//     /// <returns>Vector3.</returns>
//     /// <param name="vec2">Vector2 to convert to Vector 3.</param>
//     /// <param name="zValue">z	 value.</param>
//     public static Vector2 ToVec3 (this Vector2 vec2, float zValue) {
//         return new Vector3 (vec2.x, vec2.y, zValue);
//     }
//
//     /// <summary>
//     /// Replaces the z value of the of the Vector3
//     /// </summary>
//     /// <returns>Vector3 whose z value is to be changed.</returns>
//     /// <param name="vec3">Vector3.</param>
//     /// <param name="zVal">z value.</param>
//     public static Vector3 ToVec3 (this Vector3 vec3, float zVal) {
//         return new Vector3 (vec3.x, vec3.y, zVal);
//     }
//
//     public static Vector3 WithY (this Vector3 vec3, float yVal) {
//         return new Vector3 (vec3.x, yVal, vec3.z);
//     }
//
//     public static float GetAngleWithXAxis (Vector3 t1, Vector3 t2) {
//         Vector3 distance = t2 - t1;
//         float angle = Mathf.Atan2 (distance.y, distance.x) * Mathf.Rad2Deg;
//         if (angle < 0) {
//             angle += 360;
//         }
//
//         return angle;
//     }
//
//     public static float GetAngle (Vector2 p1, Vector2 pMid, Vector2 p2) {
//         Vector2 vBA = (pMid - p1);
//         Vector2 vCA = (pMid - p2);
//         return Vector2.Angle (vBA, vCA);
//     }
//
//     ///Interpolates between a and b by t without clamping
//     ///the interpolant and makes sure the values interpolate
//     ///correctly when they wrap aroud 360 degrees
//     public static float LerpAngleUnclamped (float a, float b, float t) {
//         float delta = Mathf.Repeat ((b - a), 360.0f);
//         if (delta > 180.0f) {
//             delta -= 360.0f;
//         }
//
//         return a + delta * t;
//     }
//
//     //------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
//
//     public static void Width (this Transform tr, float widthMultiplier) {
//         float screenHeight = Camera.main.orthographicSize * 2;
//         float screenWidth = (screenHeight) / UnityEngine.Screen.height * UnityEngine.Screen.width;
//         if (tr.GetComponent<SpriteRenderer> ()) {
//             SpriteRenderer sr = tr.GetComponent<SpriteRenderer> ();
//             tr.transform.localScale = new Vector3 ((screenWidth / sr.sprite.bounds.size.x) * widthMultiplier, tr.transform.localScale.y, 1);
//         }
//         if (tr.GetComponent<MeshRenderer> ()) {
//             MeshRenderer mr = tr.GetComponent<MeshRenderer> ();
//             tr.transform.localScale = new Vector3 ((screenWidth / mr.bounds.size.x) * widthMultiplier, tr.transform.localScale.y, 1);
//         }
//
//     }
//
//     public static void Height (this Transform tr, float heightMultiplier) {
//         float screenHeight = Camera.main.orthographicSize * 2;
//         // float screenWidth = (screenHeight) / UnityEngine.Screen.height * UnityEngine.Screen.width;
//         if (tr.GetComponent<SpriteRenderer> ()) {
//             SpriteRenderer sr = tr.GetComponent<SpriteRenderer> ();
//             tr.transform.localScale = new Vector3 (tr.transform.localScale.x, (screenHeight / sr.sprite.bounds.size.y) * heightMultiplier, 1);
//
//         }
//         if (tr.GetComponent<MeshRenderer> ()) {
//             MeshRenderer mr = tr.GetComponent<MeshRenderer> ();
//             tr.transform.localScale = new Vector3 (tr.transform.localScale.x, (screenHeight / mr.bounds.size.y) * heightMultiplier, 1);
//         }
//     }
//
//     /// <summary>
//     /// Regular expression, which is used to validate an E-Mail address.
//     /// </summary>
//     public const string MatchEmailPattern =
//         @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@" +
//         @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\." +
//         @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|" +
//         @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
//
//     /// <summary>
//     /// Checks whether the given Email-Parameter is a valid E-Mail address.
//     /// </summary>
//     /// <param name="email">Parameter-string that contains an E-Mail address.</param>
//     /// <returns>True, wenn Parameter-string is not null and contains a valid E-Mail address;
//     /// otherwise false.</returns>
//     public static bool IsEmail (string email) {
//         if (email != null) return Regex.IsMatch (email, MatchEmailPattern);
//         else return false;
//     }
//
//     //____________
//
//     public static bool ValidateEmail (string email) {
//         Regex regexEmail = new Regex (@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z");
//         Match emailMatch = regexEmail.Match (email);
//         if (emailMatch.Success) {
//             return true;
//         } else {
//             return false;
//         }
//     }
//
//     public static bool ValidateUsername (string _username) {
//         Regex username = new Regex ("^[a-zA-Z0-9 #!@_]+$");
//         Match usernameMatch = username.Match (_username);
//         if (usernameMatch.Success) {
//             return true;
//         } else {
//             return false;
//         }
//     }
//     public static bool ValidatePassword (string _password) {
//         Regex password = new Regex ("^[a-zA-Z0-9]+$");
//         Match passwordMatch = password.Match (_password);
//         if (passwordMatch.Success) {
//             return true;
//         } else {
//             return false;
//         }
//     }
//     public static bool ValidateAge (string _age) {
//         Regex regexAge = new Regex ("[0-9]");
//         Match ageMatch = regexAge.Match (_age);
//         if (ageMatch.Success) {
//             return true;
//         } else {
//             return false;
//         }
//     }
// }