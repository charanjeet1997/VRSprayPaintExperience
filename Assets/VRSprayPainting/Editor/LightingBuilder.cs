using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRSprayPainting.EditorTools
{
    /// <summary>
    /// Booth lighting where the light really comes from the fixtures:
    /// - a baked rectangular area light inside every ceiling strip and angled wall diffuser (soft, even booth light),
    /// - one mixed spot light inside the centre fixture for live shadows under the gun and hands,
    /// - light probes so moving objects and the paintable surfaces get the baked light,
    /// - a soft additive glow on each fixture (no HDR/bloom needed on Quest).
    /// Baked lighting costs almost nothing per frame. Re-run after changing the room (menu or EnvironmentBuilder).
    /// </summary>
    public static class LightingBuilder
    {
        private const string ModelDir = "Assets/VRSprayPainting/Models/";
        private const string MatDir = "Assets/VRSprayPainting/Materials/Environment/";
        private const string TexDir = "Assets/VRSprayPainting/Materials/Environment/Textures/";
        private const string SettingsPath = "Assets/VRSprayPainting/Scenes/PaintTest_LightingSettings.lighting";

        private static readonly string[] CeilingStrips = { "LightStrip_0", "LightStrip_1", "LightStrip_2" };
        private static readonly string[] WallDiffusers = { "Diffuser_L", "Diffuser_R" };
        private static readonly Color FixtureColor = new Color(1f, 0.97f, 0.92f);

        [MenuItem("Tools/VR Spray Painting/Build Lighting (bake)")]
        public static void Build()
        {
            Transform env = GameObject.Find("Environment").transform;
            Transform lighting = env.Find("Lighting");

            RemoveOldLights(env, lighting);
            PrepareStaticGeometry(env);

            Transform fixtures = Recreate(lighting, "FixtureLights");
            foreach (string name in CeilingStrips)
                AddCeilingLight(fixtures, GameObject.Find(name).transform);
            foreach (string name in WallDiffusers)
                AddWallLight(fixtures, GameObject.Find(name).transform);
            AddShadowLight(fixtures, GameObject.Find("LightStrip_1").transform);

            BuildGlow(lighting);
            BuildLightProbes(lighting);
            ConfigureSettings();

            EditorSceneManager.MarkSceneDirty(env.gameObject.scene);
            EditorSceneManager.SaveScene(env.gameObject.scene);

            Lightmapping.Bake();
            RebakeReflectionProbe(lighting);

            EditorSceneManager.MarkSceneDirty(env.gameObject.scene);
            EditorSceneManager.SaveScene(env.gameObject.scene);
        }

        // ---------- Lights ----------

        private static void RemoveOldLights(Transform env, Transform lighting)
        {
            var doomed = new List<GameObject>();
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (l.name is "KeyLight" or "FillLight" or "SideLight_L" or "SideLight_R")
                    doomed.Add(l.gameObject);
            foreach (GameObject go in doomed)
                Object.DestroyImmediate(go);

            // The room is closed: no sun inside the booth.
            GameObject sun = GameObject.Find("Directional Light");
            if (sun != null)
                sun.SetActive(false);
        }

        private static void AddCeilingLight(Transform parent, Transform strip)
        {
            Light light = NewLight(parent, "Area_" + strip.name, LightType.Rectangle, LightmapBakeType.Baked);
            // Emits along +Z: point it straight down from just under the diffuser.
            light.transform.SetPositionAndRotation(strip.position - Vector3.up * 0.012f, Quaternion.LookRotation(Vector3.down, Vector3.forward));
            light.areaSize = new Vector2(strip.lossyScale.x, strip.lossyScale.z);
            light.intensity = 11f;
        }

        private static void AddWallLight(Transform parent, Transform diffuser)
        {
            Light light = NewLight(parent, "Area_" + diffuser.name, LightType.Rectangle, LightmapBakeType.Baked);
            // Diffuser's lit face points along its local -Y (down and into the room); its long side runs along Z.
            Vector3 normal = -diffuser.up;
            light.transform.SetPositionAndRotation(diffuser.position + normal * 0.012f, Quaternion.LookRotation(normal, diffuser.forward));
            light.areaSize = new Vector2(diffuser.lossyScale.x, diffuser.lossyScale.z);
            light.intensity = 8f;
        }

        // The only realtime light: sits inside the centre fixture so live shadows come from where the light is.
        private static void AddShadowLight(Transform parent, Transform strip)
        {
            Light light = NewLight(parent, "ShadowLight_" + strip.name, LightType.Spot, LightmapBakeType.Mixed);
            light.transform.SetPositionAndRotation(strip.position - Vector3.up * 0.03f, Quaternion.LookRotation(Vector3.down, Vector3.forward));
            light.spotAngle = 130f;
            light.innerSpotAngle = 70f;
            light.range = 4f;
            light.intensity = 1.6f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.75f;
        }

        private static Light NewLight(Transform parent, string name, LightType type, LightmapBakeType mode)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Light light = go.AddComponent<Light>();
            light.type = type;
            light.lightmapBakeType = mode;
            light.color = FixtureColor;
            light.shadows = LightShadows.Soft;
            return light;
        }

        // ---------- Geometry ----------

        // Everything in the room is static and contributes to GI. Built-in Quad/Cylinder have no lightmap UVs,
        // so static ones get copies with generated UV2. Paintables and decals take their light from probes.
        private static void PrepareStaticGeometry(Transform env)
        {
            Directory.CreateDirectory(ModelDir);
            Mesh quad = LightmapReady("Quad");
            Mesh cylinder = LightmapReady("Cylinder");

            foreach (MeshRenderer r in env.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.GetComponent<TMP_Text>() != null)
                    continue;

                bool decal = r.sharedMaterial != null && (r.sharedMaterial.name.Contains("ContactShadow") || r.sharedMaterial.name.Contains("Glow"));
                if (decal)
                {
                    GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic);
                    r.receiveGI = ReceiveGI.LightProbes;
                    continue;
                }

                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    if (mf.sharedMesh.name == "Quad") mf.sharedMesh = quad;
                    else if (mf.sharedMesh.name == "Cylinder") mf.sharedMesh = cylinder;
                }

                GameObjectUtility.SetStaticEditorFlags(r.gameObject,
                    StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic);
                r.receiveGI = ReceiveGI.Lightmaps;
                r.shadowCastingMode = ShadowCastingMode.On;
            }

            // Paint surfaces are lightmapped like the room (PaintedSurface reads lightmaps). The built-in quad gets the
            // lightmap-ready copy (same UV0, so paint and raycast UVs are unchanged); the paintable cube's atlas is its UV2.
            foreach (PaintableSurface p in Object.FindObjectsByType<PaintableSurface>(FindObjectsSortMode.None))
            {
                MeshFilter mf = p.GetComponent<MeshFilter>();
                if (mf.sharedMesh != null && mf.sharedMesh.name == "Quad")
                {
                    mf.sharedMesh = quad;
                    if (p.TryGetComponent(out MeshCollider mc))
                        mc.sharedMesh = quad;
                }

                GameObjectUtility.SetStaticEditorFlags(p.gameObject, StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);
                p.GetComponent<MeshRenderer>().receiveGI = ReceiveGI.Lightmaps;
            }
        }

        private static Mesh LightmapReady(string builtin)
        {
            string path = ModelDir + builtin + "_Lightmapped.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                return existing;

            Mesh copy = Object.Instantiate(Resources.GetBuiltinResource<Mesh>(builtin + ".fbx"));
            copy.name = builtin + "_Lightmapped";
            Unwrapping.GenerateSecondaryUVSet(copy);
            AssetDatabase.CreateAsset(copy, path);
            return copy;
        }

        // ---------- Glow ----------

        private static void BuildGlow(Transform lighting)
        {
            Material glow = GlowMaterial();
            Transform g = Recreate(lighting, "FixtureGlow");

            foreach (string name in CeilingStrips)
            {
                Transform strip = GameObject.Find(name).transform;
                GlowQuad(g, strip.position - Vector3.up * 0.006f, Quaternion.LookRotation(Vector3.up, Vector3.forward),
                    new Vector3(strip.lossyScale.x * 1.5f, 1.1f, 1f), glow);
            }

            foreach (string name in WallDiffusers)
            {
                Transform d = GameObject.Find(name).transform;
                Vector3 normal = -d.up;
                GlowQuad(g, d.position + normal * 0.006f, Quaternion.LookRotation(-normal, d.forward),
                    new Vector3(1.1f, d.lossyScale.z * 1.15f, 1f), glow);
            }

            // Fixture faces are the light source: always full brightness, independent of scene lighting.
            // (Lit + emission gets its keyword stripped by URP material validation; unlit is exact and cheaper.)
            Material strip0 = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_LightStrip.mat");
            strip0.shader = Shader.Find("Universal Render Pipeline/Unlit");
            strip0.SetTexture("_BaseMap", null);
            strip0.SetColor("_BaseColor", FixtureColor);
            strip0.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; // the area lights carry the light
            EditorUtility.SetDirty(strip0);
        }

        private static void GlowQuad(Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, Material mat)
        {
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = "Glow";
            Object.DestroyImmediate(q.GetComponent<Collider>());
            q.transform.SetParent(parent, false);
            q.transform.SetPositionAndRotation(pos, rot);
            q.transform.localScale = scale;
            MeshRenderer r = q.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.receiveGI = ReceiveGI.LightProbes;
            GameObjectUtility.SetStaticEditorFlags(q, StaticEditorFlags.BatchingStatic);
        }

        // Additive soft-edged rectangle: bright core fading out, so the fixture reads as emitting light.
        private static Material GlowMaterial()
        {
            Directory.CreateDirectory(TexDir);
            string texPath = TexDir + "T_FixtureGlow.png";
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f, v = (y + 0.5f) / size * 2f - 1f;
                    // Rounded-rectangle falloff from the centre to the edge.
                    float d = Mathf.Pow(Mathf.Pow(Mathf.Abs(u), 4f) + Mathf.Pow(Mathf.Abs(v), 4f), 0.25f);
                    float a = Mathf.Clamp01(1f - d);
                    a = Mathf.Pow(a, 1.8f) * 0.75f;
                    tex.SetPixel(x, y, new Color(a, a * 0.98f, a * 0.94f, 1f));
                }
            tex.Apply();
            File.WriteAllBytes(texPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            string matPath = MatDir + "Env_FixtureGlow.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(m, matPath);
            }
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texPath));
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 2f); // additive
            m.SetFloat("_SrcBlend", (float)BlendMode.One);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 3000;
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------- Probes and settings ----------

        private static void BuildLightProbes(Transform lighting)
        {
            Transform t = Recreate(lighting, "LightProbes");
            LightProbeGroup group = t.gameObject.AddComponent<LightProbeGroup>();
            var positions = new List<Vector3>();
            for (float x = -2.7f; x <= 2.71f; x += 0.9f)
                for (float z = -2.3f; z <= 3.31f; z += 0.8f)
                    foreach (float y in new[] { 0.2f, 0.9f, 1.6f, 2.5f })
                        positions.Add(new Vector3(x, y, z));
            // Denser around the work area where the paint surfaces and the gun are.
            for (float x = -1f; x <= 1.01f; x += 0.5f)
                for (float z = -0.2f; z <= 1.01f; z += 0.4f)
                    foreach (float y in new[] { 0.8f, 1.2f, 1.5f })
                        positions.Add(new Vector3(x, y, z));
            group.probePositions = positions.ToArray();
        }

        private static void ConfigureSettings()
        {
            LightingSettings ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(SettingsPath);
            if (ls == null)
            {
                ls = new LightingSettings { name = "PaintTest_LightingSettings" };
                AssetDatabase.CreateAsset(ls, SettingsPath);
            }
            ls.bakedGI = true;
            ls.realtimeGI = false;
            ls.mixedBakeMode = MixedLightingMode.IndirectOnly;
            ls.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            ls.lightmapResolution = 16f;
            ls.lightmapPadding = 2;
            ls.lightmapMaxSize = 1024;
            ls.directSampleCount = 32;
            ls.indirectSampleCount = 256;
            ls.environmentSampleCount = 64;
            ls.maxBounces = 2;
            ls.ao = true;
            ls.aoMaxDistance = 0.5f;
            ls.aoExponentIndirect = 1f;
            ls.aoExponentDirect = 0.3f;
            ls.lightmapCompression = LightmapCompression.NormalQuality;
            ls.directionalityMode = LightmapsMode.NonDirectional;
            ls.filteringMode = LightingSettings.FilterMode.Auto;
            EditorUtility.SetDirty(ls);
            Lightmapping.lightingSettings = ls;

            // Bounce light from the white booth replaces the old flat ambient.
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.08f, 0.08f, 0.085f);
        }

        private static void RebakeReflectionProbe(Transform lighting)
        {
            Transform probeT = lighting.Find("ReflectionProbe");
            if (probeT == null || !probeT.TryGetComponent(out ReflectionProbe probe))
                return;
            string path = TexDir + "BoothReflection.exr";
            Lightmapping.BakeReflectionProbe(probe, path);
            AssetDatabase.ImportAsset(path);
            probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
        }

        private static Transform Recreate(Transform parent, string name)
        {
            Transform old = parent.Find(name);
            if (old != null)
                Object.DestroyImmediate(old.gameObject);
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }
    }
}
