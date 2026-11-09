using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace VRSprayPainting.EditorTools
{
    /// <summary>
    /// One-click project setup for the VR spray painting prototype.
    /// Menu: Tools > VR Spray Painting.
    /// </summary>
    public static class VRSprayPaintingSetup
    {
        private const string MenuRoot = "Tools/VR Spray Painting/";
        private const string RootFolder = "Assets/VRSprayPainting";
        // Minimal pipeline-only scene. The full PaintTest.unity (booth, gun, onboarding UI) is
        // authored in the editor and must not be overwritten by this tool.
        private const string ScenePath = RootFolder + "/Scenes/PaintTest_Minimal.unity";
        private const AndroidSdkVersions MinAndroidSdk = (AndroidSdkVersions)32;
        private const string XrOriginPrefabName = "XR Origin (XR Rig)";
        private const string PaintableMaterialPath = RootFolder + "/Materials/Painting/Paintable_Primer.mat";
        private const string StampShaderPath = RootFolder + "/Shaders/PaintStamp.shader";

        private static readonly string[] XrPackages =
        {
            "com.unity.xr.management",
            "com.unity.xr.openxr",
            "com.unity.xr.interaction.toolkit",
            "com.unity.xr.meta-openxr",
        };

        private static AddAndRemoveRequest packageRequest;

        [MenuItem(MenuRoot + "Run All", priority = 0)]
        public static void RunAll()
        {
            ConfigurePlayerSettings();
            // Last: package resolution triggers a domain reload.
            InstallXrPackages();
        }

        [MenuItem(MenuRoot + "1. Install XR Packages", priority = 20)]
        public static void InstallXrPackages()
        {
            if (packageRequest != null && !packageRequest.IsCompleted)
            {
                Debug.LogWarning("[VRSpraySetup] Package install already running.");
                return;
            }

            Debug.Log("[VRSpraySetup] Installing: " + string.Join(", ", XrPackages));
            packageRequest = Client.AddAndRemove(XrPackages);
            EditorApplication.update += PollPackageRequest;
        }

        private static void PollPackageRequest()
        {
            if (!packageRequest.IsCompleted)
                return;

            EditorApplication.update -= PollPackageRequest;

            if (packageRequest.Status == StatusCode.Success)
                Debug.Log("[VRSpraySetup] XR packages installed. Next: enable OpenXR + Meta Quest feature group in Project Settings > XR Plug-in Management (Android tab).");
            else
                Debug.LogError("[VRSpraySetup] Package install failed: " + packageRequest.Error?.message);

            packageRequest = null;
        }

        [MenuItem(MenuRoot + "2. Configure Player Settings (Quest)", priority = 21)]
        public static void ConfigurePlayerSettings()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = MinAndroidSdk;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });

            PlayerSettings.colorSpace = ColorSpace.Linear;

            AssetDatabase.SaveAssets();
            Debug.Log("[VRSpraySetup] Player settings configured for Quest (Android, IL2CPP, ARM64, API 32+, Vulkan, Linear).");
        }

        [MenuItem(MenuRoot + "3. Create Minimal Paint Test Scene", priority = 22)]
        public static void CreatePaintTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("VR Spray Painting", "PaintTest_Minimal.unity already exists. Overwrite?", "Overwrite", "Cancel"))
                return;

            EnsureFolder(RootFolder, "Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Camera cam = Camera.main;
            if (cam != null)
                cam.transform.SetPositionAndRotation(new Vector3(0f, 1.4f, -1.2f), Quaternion.Euler(10f, 0f, 0f));

            GameObject xrOriginPrefab = FindStarterAssetsPrefab(XrOriginPrefabName);
            if (xrOriginPrefab != null)
            {
                // The rig brings its own camera; keep the default one only as a fallback.
                if (cam != null)
                    Object.DestroyImmediate(cam.gameObject);

                var rig = (GameObject)PrefabUtility.InstantiatePrefab(xrOriginPrefab, scene);
                rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[VRSpraySetup] '" + XrOriginPrefabName + "' not found. Import XRI Starter Assets and re-run.");
            }

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";

            GameObject paintables = new GameObject("Paintables");

            // Quad already has a MeshCollider — required for RaycastHit.textureCoord.
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
            panel.name = "PaintablePanel";
            panel.transform.SetParent(paintables.transform);
            panel.transform.SetPositionAndRotation(new Vector3(-0.5f, 1.2f, 0.5f), Quaternion.identity);
            panel.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "PaintableCube";
            cube.transform.SetParent(paintables.transform);
            cube.transform.SetPositionAndRotation(new Vector3(0.5f, 1.1f, 0.5f), Quaternion.Euler(0f, 30f, 0f));
            cube.transform.localScale = Vector3.one * 0.4f;
            UseMeshCollider(cube);

            MakePaintable(panel);
            MakePaintable(cube);
            new GameObject("DebugMousePainter").AddComponent<DebugMousePainter>();

            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("[VRSpraySetup] Created " + ScenePath);
        }

        // XRI Starter Assets rig. Searched inside the sample folder because its path contains the XRI version.
        private static GameObject FindStarterAssetsPrefab(string prefabName)
        {
            foreach (string guid in AssetDatabase.FindAssets(prefabName + " t:Prefab", new[] { "Assets/Samples/XR Interaction Toolkit" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Starter Assets/") && System.IO.Path.GetFileNameWithoutExtension(path) == prefabName)
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return null;
        }

        private static void MakePaintable(GameObject go)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(PaintableMaterialPath);
            if (material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;

            var surface = go.AddComponent<PaintableSurface>();
            var serialized = new SerializedObject(surface);
            serialized.FindProperty("stampShader").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Shader>(StampShaderPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void UseMeshCollider(GameObject go)
        {
            Collider existing = go.GetComponent<Collider>();
            if (existing != null && !(existing is MeshCollider))
                Object.DestroyImmediate(existing);

            if (!go.TryGetComponent(out MeshCollider _))
                go.AddComponent<MeshCollider>();
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
