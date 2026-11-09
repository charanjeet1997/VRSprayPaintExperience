using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRSprayPainting.EditorTools
{
    /// <summary>
    /// Dresses the paint booth: closes the room (front wall + door), generates procedural textures
    /// (concrete, wall panels, stained filters, perforated ceiling, drop cloth, pegboard, grate, contact shadows),
    /// adds props and signage, and sets up booth lighting. Re-runnable: the "Dressing" group is rebuilt each time.
    /// Kept light for Quest 2: unlit shadows/decals, one shadowed light, no post-processing.
    /// </summary>
    public static class EnvironmentBuilder
    {
        private const string MatDir = "Assets/VRSprayPainting/Materials/Environment/";
        private const string TexDir = "Assets/VRSprayPainting/Materials/Environment/Textures/";

        [MenuItem("Tools/VR Spray Painting/Build Environment")]
        public static void Build()
        {
            Directory.CreateDirectory(TexDir);

            Texture2D concrete = SaveTexture("T_Concrete", 512, ConcretePixel, true);
            Texture2D wall = SaveTexture("T_WallPanels", 512, WallPixel, false);
            Texture2D ceiling = SaveTexture("T_CeilingPerforated", 512, CeilingPixel, false);
            Texture2D filterA = SaveTexture("T_FilterA", 256, (u, v) => FilterPixel(u, v, 0), false);
            Texture2D filterB = SaveTexture("T_FilterB", 256, (u, v) => FilterPixel(u, v, 1), false);
            Texture2D cloth = SaveTexture("T_DropCloth", 1024, ClothPixel, false);
            Texture2D pegboard = SaveTexture("T_Pegboard", 512, PegboardPixel, false);
            Texture2D grate = SaveTexture("T_FloorGrate", 512, GratePixel, false);
            Texture2D worktop = SaveTexture("T_Worktop", 512, WorktopPixel, false);
            Texture2D shadow = SaveTexture("T_ContactShadow", 128, ShadowPixel, false, TextureWrapMode.Clamp);

            // Existing booth materials get textures and tuned values.
            Lit("Env_Concrete", new Color(0.78f, 0.78f, 0.8f), 0.35f, concrete, new Vector2(4f, 4f));
            Lit("Env_BoothWall", new Color(0.96f, 0.97f, 0.98f), 0.2f, wall, new Vector2(5f, 2.5f));
            Lit("Env_WallTrim", new Color(0.95f, 0.96f, 0.97f), 0.1f, ceiling, new Vector2(5f, 5f));
            Lit("Env_FilterPanel", Color.white, 0.02f, filterA, Vector2.one);
            Material filterMatB = Lit("Env_FilterPanel_B", Color.white, 0.02f, filterB, Vector2.one);
            Lit("Env_Workbench", new Color(0.75f, 0.76f, 0.78f), 0.45f, worktop, Vector2.one, metallic: 0.6f);
            Lit("Env_DarkSteel", new Color(0.17f, 0.18f, 0.2f), 0.45f, null, Vector2.one, metallic: 0.5f);
            Lit("Env_Steel", new Color(0.6f, 0.62f, 0.65f), 0.55f, null, Vector2.one, metallic: 0.8f);
            Material lightStrip = Lit("Env_LightStrip", Color.white, 0.5f, null, Vector2.one);
            lightStrip.EnableKeyword("_EMISSION");
            lightStrip.SetColor("_EmissionColor", new Color(1f, 0.98f, 0.94f) * 1.4f);
            lightStrip.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

            Material clothMat = Lit("Env_DropCloth", Color.white, 0.1f, cloth, Vector2.one);
            Material pegMat = Lit("Env_Pegboard", Color.white, 0.15f, pegboard, new Vector2(2f, 1.2f));
            Material grateMat = Lit("Env_FloorGrate", new Color(0.55f, 0.56f, 0.58f), 0.5f, grate, new Vector2(10f, 1f), metallic: 0.7f);
            Material doorMat = Lit("Env_DoorPaint", new Color(0.22f, 0.32f, 0.42f), 0.45f, null, Vector2.one);
            Material glassMat = Lit("Env_DarkGlass", new Color(0.08f, 0.1f, 0.12f), 0.92f, null, Vector2.one);
            Material redMat = Lit("Env_ExtinguisherRed", new Color(0.75f, 0.05f, 0.04f), 0.7f, null, Vector2.one);
            Material blackMat = Lit("Env_RubberBlack", new Color(0.05f, 0.05f, 0.055f), 0.2f, null, Vector2.one);
            Material tapeMat = Lit("Env_MaskingTape", new Color(0.86f, 0.78f, 0.55f), 0.15f, null, Vector2.one);
            Material signBlue = Lit("Env_SignBlue", new Color(0.05f, 0.25f, 0.6f), 0.4f, null, Vector2.one);
            Material signYellow = Lit("Env_SignYellow", new Color(0.98f, 0.8f, 0.05f), 0.4f, null, Vector2.one);
            Material exitGreen = Lit("Env_ExitGreen", new Color(0.05f, 0.55f, 0.25f), 0.4f, null, Vector2.one);
            exitGreen.EnableKeyword("_EMISSION");
            exitGreen.SetColor("_EmissionColor", new Color(0.05f, 0.6f, 0.25f));
            Material shadowMat = ContactShadowMaterial(shadow);

            GameObject env = GameObject.Find("Environment");
            Transform old = env.transform.Find("Dressing");
            if (old != null)
                Object.DestroyImmediate(old.gameObject);
            Transform root = new GameObject("Dressing").transform;
            root.SetParent(env.transform, false);

            // Filters: alternate the stain pattern so the wall doesn't repeat.
            Transform filters = env.transform.Find("PaintBooth/ExtractionFilters");
            int fi = 0;
            foreach (Transform f in filters)
                if (f.name.StartsWith("Filter_"))
                    f.GetComponent<Renderer>().sharedMaterial = fi++ % 2 == 0
                        ? AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_FilterPanel.mat")
                        : filterMatB;

            Material darkSteel = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_DarkSteel.mat");
            Material steel = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_Steel.mat");
            Material boothWall = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_BoothWall.mat");
            Material trim = AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_WallTrim.mat");

            BuildFrontWall(root, boothWall, darkSteel, doorMat, glassMat, steel, exitGreen);
            BuildFloor(root, clothMat, grateMat, shadowMat);
            BuildSideLights(root, darkSteel, lightStrip);
            BuildProps(root, darkSteel, steel, redMat, blackMat, tapeMat, pegMat);
            BuildSigns(root, signBlue, signYellow, redMat);
            ConfigureLighting(env.transform);

            // Ceiling uses the perforated diffuser; skirting stays dark.
            env.transform.Find("PaintBooth/Ceiling").GetComponent<Renderer>().sharedMaterial = trim;
            foreach (string s in new[] { "Skirting_Back", "Skirting_Left", "Skirting_Right" })
                env.transform.Find("PaintBooth/" + s).GetComponent<Renderer>().sharedMaterial = darkSteel;

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(env.scene);
            EditorSceneManager.SaveScene(env.scene);

            LightingBuilder.Build();
        }

        // ---------- Geometry ----------

        private static void BuildFrontWall(Transform root, Material wall, Material frame, Material door, Material glass, Material steel, Material exit)
        {
            Transform g = Group(root, "FrontWall");
            // Wall with a door opening at x 1.35..2.35, height 2.15.
            Box(g, "Wall_Left", new Vector3(-0.825f, 1.5f, -2.55f), new Vector3(4.35f, 3f, 0.1f), wall);
            Box(g, "Wall_Right", new Vector3(2.675f, 1.5f, -2.55f), new Vector3(0.65f, 3f, 0.1f), wall);
            Box(g, "Wall_Lintel", new Vector3(1.85f, 2.575f, -2.55f), new Vector3(1f, 0.85f, 0.1f), wall);
            Box(g, "Skirting", new Vector3(-0.825f, 0.08f, -2.49f), new Vector3(4.35f, 0.16f, 0.02f), frame);

            Box(g, "DoorFrame_L", new Vector3(1.33f, 1.075f, -2.52f), new Vector3(0.05f, 2.15f, 0.14f), frame);
            Box(g, "DoorFrame_R", new Vector3(2.37f, 1.075f, -2.52f), new Vector3(0.05f, 2.15f, 0.14f), frame);
            Box(g, "DoorFrame_T", new Vector3(1.85f, 2.17f, -2.52f), new Vector3(1.09f, 0.05f, 0.14f), frame);
            Box(g, "Door", new Vector3(1.85f, 1.075f, -2.56f), new Vector3(1.0f, 2.15f, 0.05f), door);
            // Corridor wall behind the door so gaps never show the void.
            Box(g, "Threshold", new Vector3(1.85f, -0.045f, -2.6f), new Vector3(1.1f, 0.1f, 0.3f), frame);
            Box(g, "Corridor", new Vector3(1.85f, 1.2f, -2.75f), new Vector3(1.4f, 2.6f, 0.05f), frame);
            Box(g, "Door_Window", new Vector3(1.85f, 1.55f, -2.532f), new Vector3(0.36f, 0.5f, 0.01f), glass);
            Box(g, "Door_WindowFrame", new Vector3(1.85f, 1.55f, -2.534f), new Vector3(0.42f, 0.56f, 0.008f), frame);
            Box(g, "Door_PushBar", new Vector3(1.85f, 1.0f, -2.5f), new Vector3(0.78f, 0.05f, 0.04f), steel);
            Box(g, "Door_KickPlate", new Vector3(1.85f, 0.15f, -2.532f), new Vector3(0.94f, 0.25f, 0.006f), steel);

            Box(g, "ExitSign", new Vector3(1.85f, 2.42f, -2.49f), new Vector3(0.36f, 0.14f, 0.03f), exit);
            Label(g, "ExitText", "EXIT", new Vector3(1.85f, 2.42f, -2.473f), Quaternion.Euler(0f, 180f, 0f), 0.08f, Color.white, 0.3f, 0.1f);
        }

        private static void BuildFloor(Transform root, Material cloth, Material grate, Material shadow)
        {
            Transform g = Group(root, "Floor");
            // Drop cloth under the work area, slightly creased by a tiny tilt.
            Quad(g, "DropCloth", new Vector3(0f, 0.003f, 0.55f), Quaternion.Euler(90f, 0f, 0f), new Vector3(2.5f, 1.7f, 1f), cloth);

            // Exhaust grate along the back wall (downdraft pit).
            Box(g, "Grate_Frame", new Vector3(0f, 0.004f, 3.18f), new Vector3(5.8f, 0.008f, 0.56f), AssetDatabase.LoadAssetAtPath<Material>(MatDir + "Env_DarkSteel.mat"));
            Quad(g, "Grate", new Vector3(0f, 0.009f, 3.18f), Quaternion.Euler(90f, 0f, 0f), new Vector3(5.7f, 0.48f, 1f), grate);

            // Soft contact shadows that ground every object.
            Transform s = Group(g, "ContactShadows");
            Shadow(s, new Vector3(0.5f, 0.006f, 0.5f), new Vector2(0.75f, 0.75f), shadow);
            Shadow(s, new Vector3(-0.5f, 0.006f, 0.56f), new Vector2(0.95f, 0.7f), shadow);
            Shadow(s, new Vector3(0.45f, 0.006f, 0.05f), new Vector2(0.6f, 0.6f), shadow);
            Shadow(s, new Vector3(2.2f, 0.004f, 0.6f), new Vector2(1.9f, 1.15f), shadow);
            Shadow(s, new Vector3(-2.7f, 0.004f, 1.5f), new Vector2(0.65f, 1.5f), shadow);
            Shadow(s, new Vector3(1.1f, 0.004f, -2.33f), new Vector2(0.35f, 0.35f), shadow);
            Shadow(s, new Vector3(-2.55f, 0.004f, -1.9f), new Vector2(0.55f, 0.55f), shadow);
        }

        // Real booths light from angled fixtures along the upper wall edges so there are no shadows on the work.
        private static void BuildSideLights(Transform root, Material housing, Material strip)
        {
            Transform g = Group(root, "SideLights");
            foreach (int side in new[] { -1, 1 })
            {
                var rot = Quaternion.Euler(0f, 0f, -side * 45f); // diffuser faces down and into the room
                Vector3 c = new Vector3(side * 2.82f, 2.8f, 0.5f);
                Transform h = Box(g, side < 0 ? "Fixture_L" : "Fixture_R", c, new Vector3(0.36f, 0.08f, 4.6f), housing).transform;
                h.rotation = rot;
                Transform s = Box(g, side < 0 ? "Diffuser_L" : "Diffuser_R", c + rot * new Vector3(0f, -0.045f, 0f), new Vector3(0.3f, 0.01f, 4.5f), strip).transform;
                s.rotation = rot;
                Object.DestroyImmediate(s.GetComponent<Collider>());

            }
        }

        private static void BuildProps(Transform root, Material darkSteel, Material steel, Material red, Material black, Material tape, Material peg)
        {
            Transform g = Group(root, "Props");

            // Fire extinguisher by the door, on a wall bracket.
            Transform ext = Group(g, "FireExtinguisher");
            Cyl(ext, "Body", new Vector3(1.1f, 0.33f, -2.33f), 0.15f, 0.27f, red);
            Cyl(ext, "Shoulder", new Vector3(1.1f, 0.62f, -2.33f), 0.12f, 0.03f, red);
            Cyl(ext, "Valve", new Vector3(1.1f, 0.68f, -2.33f), 0.04f, 0.04f, steel);
            Box(ext, "Handle", new Vector3(1.1f, 0.73f, -2.33f), new Vector3(0.12f, 0.015f, 0.03f), black);
            Box(ext, "Hose", new Vector3(1.17f, 0.5f, -2.33f), new Vector3(0.02f, 0.3f, 0.02f), black);
            Box(ext, "Bracket", new Vector3(1.1f, 0.5f, -2.48f), new Vector3(0.12f, 0.12f, 0.02f), darkSteel);

            // Waste bin for used masking and rags.
            Transform bin = Group(g, "Bin");
            Cyl(bin, "Body", new Vector3(-2.55f, 0.3f, -1.9f), 0.42f, 0.3f, Lit("Env_BinYellow", new Color(0.9f, 0.7f, 0.05f), 0.35f, null, Vector2.one));
            Cyl(bin, "Lid", new Vector3(-2.55f, 0.61f, -1.9f), 0.44f, 0.012f, darkSteel);

            // Pegboard with tools above the workbench.
            Transform pb = Group(g, "Pegboard");
            Quad(pb, "Board", new Vector3(2.935f, 1.45f, 0.6f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.4f, 0.8f, 1f), peg);
            Box(pb, "Scraper", new Vector3(2.92f, 1.55f, 0.15f), new Vector3(0.015f, 0.2f, 0.06f), Lit("Env_ToolOrange", new Color(0.95f, 0.45f, 0.08f), 0.5f, null, Vector2.one));
            Box(pb, "Ruler", new Vector3(2.92f, 1.35f, 0.5f), new Vector3(0.01f, 0.04f, 0.5f), steel);
            Box(pb, "SandingBlock", new Vector3(2.9f, 1.6f, 0.85f), new Vector3(0.05f, 0.07f, 0.12f), black);
            Cyl(pb, "TapeHook_0", new Vector3(2.9f, 1.62f, 1.1f), 0.11f, 0.025f, tape).transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Cyl(pb, "TapeHook_1", new Vector3(2.9f, 1.4f, 1.1f), 0.09f, 0.025f, Lit("Env_TapeBlue", new Color(0.2f, 0.45f, 0.85f), 0.2f, null, Vector2.one)).transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            // Bench clutter.
            Cyl(g, "Tape_0", new Vector3(2.55f, 0.89f, 0.35f), 0.1f, 0.025f, tape);
            Cyl(g, "Tape_1", new Vector3(2.7f, 0.89f, 0.5f), 0.08f, 0.02f, tape);
            Box(g, "Rag", new Vector3(2.6f, 0.876f, 0.8f), new Vector3(0.22f, 0.004f, 0.16f), Lit("Env_Rag", new Color(0.75f, 0.72f, 0.68f), 0.05f, null, Vector2.one)).transform.rotation = Quaternion.Euler(0f, 18f, 0f);
            Box(g, "Clipboard", new Vector3(2.45f, 0.876f, 0.95f), new Vector3(0.23f, 0.006f, 0.32f), Lit("Env_Clipboard", new Color(0.4f, 0.27f, 0.15f), 0.3f, null, Vector2.one)).transform.rotation = Quaternion.Euler(0f, -8f, 0f);

            // Air hose reel on the left wall.
            Transform reel = Group(g, "HoseReel");
            Cyl(reel, "Drum", new Vector3(-2.88f, 1.7f, -0.6f), 0.36f, 0.06f, red).transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Cyl(reel, "Hub", new Vector3(-2.83f, 1.7f, -0.6f), 0.12f, 0.02f, steel).transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Box(reel, "Mount", new Vector3(-2.94f, 1.7f, -0.6f), new Vector3(0.02f, 0.3f, 0.3f), darkSteel);
            Box(reel, "HoseDrop", new Vector3(-2.86f, 1.2f, -0.43f), new Vector3(0.025f, 0.9f, 0.025f), black);

            // Wall-mounted pressure gauge / air regulator next to the reel.
            Cyl(reel, "Gauge", new Vector3(-2.93f, 1.45f, -1.05f), 0.11f, 0.02f, steel).transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Cyl(reel, "GaugeFace", new Vector3(-2.91f, 1.45f, -1.05f), 0.09f, 0.004f, Lit("Env_GaugeFace", new Color(0.95f, 0.95f, 0.92f), 0.6f, null, Vector2.one)).transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        private static void BuildSigns(Transform root, Material blue, Material yellow, Material red)
        {
            Transform g = Group(root, "Signs");
            // TMP text reads correctly when its +Z points away from the viewer.
            Quaternion onLeftWall = Quaternion.Euler(0f, -90f, 0f);
            Quaternion onBackWall = Quaternion.identity;
            Quaternion onFrontWall = Quaternion.Euler(0f, 180f, 0f);

            Box(g, "PPE_Plate", new Vector3(-2.94f, 1.75f, 0.3f), new Vector3(0.01f, 0.45f, 0.74f), blue);
            Label(g, "PPE_Text", "<b>PPE REQUIRED</b>\n<size=42%>RESPIRATOR · GLOVES · GOGGLES</size>", new Vector3(-2.932f, 1.75f, 0.3f), onLeftWall, 0.065f, Color.white, 0.68f, 0.38f);

            Box(g, "Booth_Plate", new Vector3(0f, 2.62f, 3.44f), new Vector3(2.1f, 0.24f, 0.01f), yellow);
            Label(g, "Booth_Text", "<b>SPRAY BOOTH 01</b>  ·  EXTRACTION ON", new Vector3(0f, 2.62f, 3.433f), onBackWall, 0.09f, new Color(0.08f, 0.08f, 0.08f), 1.9f, 0.18f);

            Box(g, "Flame_Plate", new Vector3(-0.6f, 1.9f, -2.49f), new Vector3(0.5f, 0.34f, 0.01f), red);
            Label(g, "Flame_Text", "<b>NO NAKED\nFLAMES</b>", new Vector3(-0.6f, 1.9f, -2.483f), onFrontWall, 0.08f, Color.white, 0.44f, 0.28f);

            Box(g, "Fire_Plate", new Vector3(1.1f, 0.95f, -2.49f), new Vector3(0.2f, 0.12f, 0.01f), red);
            Label(g, "Fire_Text", "<b>FIRE</b>", new Vector3(1.1f, 0.95f, -2.483f), onFrontWall, 0.06f, Color.white, 0.17f, 0.09f);
        }

        private static void ConfigureLighting(Transform env)
        {
            // Enclosed booth: no sky. Ambient is set by LightingBuilder (bounce light comes from the bake).
            RenderSettings.skybox = null;
            RenderSettings.fog = false;

            foreach (Camera cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.2f, 0.21f, 0.23f);
            }

            // Lights live in the fixtures and are baked by LightingBuilder (called at the end of Build).
            Transform lighting = env.Find("Lighting");

            Transform probeT = lighting.Find("ReflectionProbe");
            if (probeT != null && probeT.TryGetComponent(out ReflectionProbe probe))
            {
                probe.size = new Vector3(6f, 3.1f, 6.2f);
                probe.center = new Vector3(0f, 0f, -0.05f);
                probe.boxProjection = true;
                probe.mode = ReflectionProbeMode.Custom;
                string path = TexDir + "BoothReflection.exr";
                Lightmapping.BakeReflectionProbe(probe, path);
                AssetDatabase.ImportAsset(path);
                probe.customBakedTexture = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            }
        }

        // ---------- Pixel functions (u, v in 0..1) ----------

        private static Color ConcretePixel(float u, float v)
        {
            float n = Fbm(u * 8f, v * 8f, 5, 1);
            float fine = Hash(Mathf.Floor(u * 512f), Mathf.Floor(v * 512f), 3);
            float g = 0.55f + (n - 0.5f) * 0.25f + (fine - 0.5f) * 0.06f;
            // Sealed epoxy: a few darker scuffs and old overspray dots.
            float scuff = Smooth(0.62f, 0.75f, Fbm(u * 3f + 7f, v * 3f, 4, 2));
            g -= scuff * 0.08f;
            Color c = new Color(g, g, g * 1.02f);
            float dot = Hash(Mathf.Floor(u * 256f), Mathf.Floor(v * 256f), 9);
            if (dot > 0.9985f)
                c = Color.Lerp(c, PaintColor(dot * 977f), 0.6f);
            c.a = 0.35f + n * 0.3f; // smoothness variation
            return c;
        }

        private static Color WallPixel(float u, float v)
        {
            // One tile = one 1.2 m wall panel with a recessed seam and a faint overspray haze.
            float seam = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
            float s = Smooth(0f, 0.012f, seam);
            float n = Fbm(u * 5f, v * 5f, 4, 4);
            float g = Mathf.Lerp(0.55f, 1f, s) - (n - 0.5f) * 0.05f;
            Color c = new Color(g, g, g);
            // Rivets near panel corners.
            foreach (Vector2 r in new[] { new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.04f), new Vector2(0.04f, 0.96f), new Vector2(0.96f, 0.96f) })
                if (Vector2.Distance(new Vector2(u, v), r) < 0.008f)
                    c *= 0.75f;
            float dot = Hash(Mathf.Floor(u * 512f), Mathf.Floor(v * 512f), 5);
            if (dot > 0.9993f)
                c = Color.Lerp(c, PaintColor(dot * 313f), 0.35f);
            c.a = 0.2f;
            return c;
        }

        private static Color CeilingPixel(float u, float v)
        {
            float seam = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
            float s = Smooth(0f, 0.01f, seam);
            float gx = Mathf.Repeat(u * 40f, 1f) - 0.5f, gy = Mathf.Repeat(v * 40f, 1f) - 0.5f;
            float hole = Smooth(0.18f, 0.24f, Mathf.Sqrt(gx * gx + gy * gy));
            float g = Mathf.Lerp(0.6f, 0.97f, s) * Mathf.Lerp(0.55f, 1f, hole);
            return new Color(g, g, g, 0.1f);
        }

        private static Color FilterPixel(float u, float v, int variant)
        {
            // Fibrous filter media, cream coloured, with layers of captured overspray toward the middle.
            float fibre = Fbm(u * 30f, v * 6f, 4, 10 + variant) * 0.6f + Fbm(u * 6f, v * 30f, 4, 20 + variant) * 0.4f;
            float g = 0.78f + (fibre - 0.5f) * 0.25f;
            Color c = new Color(g, g * 0.97f, g * 0.9f);
            float centre = 1f - Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.55f)) * 1.7f);
            for (int i = 0; i < 3; i++)
            {
                float m = Fbm(u * 3f + i * 5.3f, v * 3f + variant * 2.1f, 4, 40 + i + variant * 7);
                float amount = Smooth(0.45f, 0.75f, m) * (0.25f + centre * 0.6f);
                c = Color.Lerp(c, PaintColor(i * 3 + variant * 5 + 1), amount * 0.55f);
            }
            float grime = Fbm(u * 2f, v * 2f, 3, 60 + variant);
            c *= Mathf.Lerp(1f, 0.82f, grime * centre);
            c.a = 0.02f;
            return c;
        }

        private static Color ClothPixel(float u, float v)
        {
            // Canvas drop cloth: woven texture, folds, and accumulated paint splatter around the work area.
            float weave = (Mathf.Sin(u * 1024f * Mathf.PI) * Mathf.Sin(v * 1024f * Mathf.PI)) * 0.03f;
            float fold = Mathf.Sin((u * 2.2f + Fbm(u * 2f, v * 2f, 3, 70) * 0.6f) * Mathf.PI * 2f) * 0.035f;
            float n = Fbm(u * 10f, v * 10f, 4, 71);
            float g = 0.74f + weave + fold + (n - 0.5f) * 0.08f;
            Color c = new Color(g, g * 0.95f, g * 0.86f);

            // Overspray clouds under the easel (u 0.1..0.5) and the pedestal (u 0.5..0.9).
            for (int i = 0; i < 6; i++)
            {
                float m = Fbm(u * 4f + i * 3.1f, v * 4f, 4, 80 + i);
                Vector2 hub = i % 2 == 0 ? new Vector2(0.3f, 0.55f) : new Vector2(0.7f, 0.55f);
                float near = 1f - Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), hub) * 2.6f);
                float speck = Hash(Mathf.Floor(u * 1024f), Mathf.Floor(v * 1024f), 90 + i);
                if (speck > 1f - near * near * 0.06f * (0.5f + m))
                    c = Color.Lerp(c, PaintColor(i), 0.85f);
                c = Color.Lerp(c, PaintColor(i), Smooth(0.62f, 0.8f, m) * near * 0.35f);
            }

            // A few drips/splats.
            for (int i = 0; i < 14; i++)
            {
                Vector2 p = new Vector2(Hash(i, 1, 99), Hash(i, 2, 99));
                float r = 0.004f + Hash(i, 3, 99) * 0.012f;
                float d = Vector2.Distance(new Vector2(u, v), p) + (Fbm(u * 60f, v * 60f, 2, 100 + i) - 0.5f) * r;
                if (d < r)
                    c = Color.Lerp(c, PaintColor(i + 2), 0.9f);
            }
            c.a = 0.1f;
            return c;
        }

        private static Color PegboardPixel(float u, float v)
        {
            float gx = Mathf.Repeat(u * 16f, 1f) - 0.5f, gy = Mathf.Repeat(v * 16f, 1f) - 0.5f;
            float hole = Smooth(0.1f, 0.13f, Mathf.Sqrt(gx * gx + gy * gy));
            float n = Fbm(u * 20f, v * 4f, 3, 110);
            Color board = new Color(0.82f, 0.66f, 0.45f) * (0.92f + n * 0.16f);
            Color c = Color.Lerp(new Color(0.08f, 0.06f, 0.05f), board, hole);
            c.a = 0.15f;
            return c;
        }

        private static Color GratePixel(float u, float v)
        {
            float bx = Mathf.Repeat(u * 12f, 1f), by = Mathf.Repeat(v * 6f, 1f);
            float bar = Mathf.Max(Smooth(0.82f, 0.86f, bx), Smooth(0.8f, 0.86f, by));
            float n = Fbm(u * 20f, v * 20f, 3, 120);
            Color metal = new Color(0.62f, 0.63f, 0.65f) * (0.85f + n * 0.3f);
            Color c = Color.Lerp(new Color(0.03f, 0.03f, 0.035f), metal, bar);
            c.a = 0.5f * bar;
            return c;
        }

        private static Color WorktopPixel(float u, float v)
        {
            float n = Fbm(u * 6f, v * 6f, 4, 130);
            float scratch = Mathf.Pow(Mathf.Abs(Mathf.Sin((u * 0.3f + v) * 400f + n * 12f)), 60f) * 0.15f;
            float g = 0.7f + (n - 0.5f) * 0.12f + scratch;
            Color c = new Color(g, g, g * 1.01f);
            for (int i = 0; i < 8; i++)
            {
                Vector2 p = new Vector2(Hash(i, 1, 140), Hash(i, 2, 140));
                float r = 0.01f + Hash(i, 3, 140) * 0.03f;
                if (Vector2.Distance(new Vector2(u, v), p) + (Fbm(u * 40f, v * 40f, 2, 150 + i) - 0.5f) * r < r)
                    c = Color.Lerp(c, PaintColor(i + 4), 0.8f);
            }
            c.a = 0.45f + n * 0.2f;
            return c;
        }

        private static Color ShadowPixel(float u, float v)
        {
            float dx = (u - 0.5f) * 2f, dy = (v - 0.5f) * 2f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.Pow(Mathf.Clamp01(1f - d), 1.6f) * 0.55f;
            return new Color(0f, 0f, 0f, a);
        }

        private static Color PaintColor(float seed)
        {
            Color[] palette =
            {
                new Color(0.8f, 0.12f, 0.1f), new Color(0.12f, 0.3f, 0.75f), new Color(0.95f, 0.75f, 0.1f),
                new Color(0.15f, 0.55f, 0.2f), new Color(0.9f, 0.45f, 0.08f), new Color(0.5f, 0.2f, 0.6f),
            };
            int i = Mathf.Abs(Mathf.FloorToInt(seed)) % palette.Length;
            return palette[i];
        }

        // ---------- Noise ----------

        private static float Hash(float x, float y, int seed)
        {
            float h = Mathf.Sin(x * 127.1f + y * 311.7f + seed * 74.7f) * 43758.5453f;
            return h - Mathf.Floor(h);
        }

        // Tileable value noise: lattice wraps at `period` so textures repeat seamlessly.
        private static float ValueNoise(float x, float y, int seed, int period)
        {
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float H(int ix, int iy) => Hash(Mod(ix, period), Mod(iy, period), seed);
            float a = Mathf.Lerp(H(x0, y0), H(x0 + 1, y0), fx);
            float b = Mathf.Lerp(H(x0, y0 + 1), H(x0 + 1, y0 + 1), fx);
            return Mathf.Lerp(a, b, fy);
        }

        private static float Fbm(float x, float y, int octaves, int seed)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += ValueNoise(x, y, seed + i * 13, 4096) * amp;
                norm += amp;
                amp *= 0.5f;
                x *= 2f;
                y *= 2f;
            }
            return sum / norm;
        }

        private static int Mod(int a, int m) => ((a % m) + m) % m;

        // Shader-style smoothstep (Mathf.SmoothStep interpolates between its first two arguments instead).
        private static float Smooth(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        // ---------- Helpers ----------

        private delegate Color PixelFunc(float u, float v);

        private static Texture2D SaveTexture(string name, int size, PixelFunc f, bool tileable, TextureWrapMode wrap = TextureWrapMode.Repeat)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size, v = (y + 0.5f) / size;
                    Color c = f(u, v);
                    if (tileable)
                    {
                        // Blend with offset copies near the edges so noise-based tiles repeat without a seam.
                        float wx = Mathf.Clamp01(Mathf.Min(u, 1f - u) * 8f), wy = Mathf.Clamp01(Mathf.Min(v, 1f - v) * 8f);
                        if (wx < 1f) c = Color.Lerp(f(Mathf.Repeat(u + 0.5f, 1f), v), c, wx);
                        if (wy < 1f) c = Color.Lerp(f(u, Mathf.Repeat(v + 0.5f, 1f)), c, wy);
                    }
                    pixels[y * size + x] = c;
                }
            tex.SetPixels(pixels);
            tex.Apply();
            string path = TexDir + name + ".png";
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.wrapMode = wrap;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 4;
            importer.alphaIsTransparency = name == "T_ContactShadow";
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material Lit(string name, Color color, float smoothness, Texture2D tex, Vector2 tiling, float metallic = 0f)
        {
            string path = MatDir + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", color);
            m.SetTexture("_BaseMap", tex);
            m.SetTextureScale("_BaseMap", tiling);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            // Alpha channel of the generated textures carries smoothness variation.
            bool alphaSmooth = tex != null;
            m.SetFloat("_SmoothnessTextureChannel", alphaSmooth ? 1f : 0f);
            if (alphaSmooth) m.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            else m.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            if (alphaSmooth) m.SetFloat("_Smoothness", 1f); // texture alpha holds absolute smoothness
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Material ContactShadowMaterial(Texture2D tex)
        {
            string path = MatDir + "Env_ContactShadow.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = 2990;
            m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(m);
            return m;
        }

        private static Transform Group(Transform parent, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        private static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.isStatic = true;
            return go;
        }

        private static GameObject Cyl(Transform parent, string name, Vector3 pos, float diameter, float halfHeight, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(diameter, halfHeight, diameter);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.isStatic = true;
            return go;
        }

        private static GameObject Quad(Transform parent, string name, Vector3 pos, Quaternion rot, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, rot);
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.isStatic = true;
            return go;
        }

        private static void Shadow(Transform parent, Vector3 pos, Vector2 size, Material mat)
        {
            GameObject go = Quad(parent, "Shadow", pos, Quaternion.Euler(90f, 0f, 0f), new Vector3(size.x, size.y, 1f), mat);
            Renderer r = go.GetComponent<Renderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        private static void Label(Transform parent, string name, string text, Vector3 pos, Quaternion rot, float size, Color color, float width = 0.4f, float height = 0.12f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(pos, rot);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = size * 13f; // ~0.05 m per character per font unit with the default TMP font
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(width, height);

            // Shrink until the text fits inside its plate (width/height are the plate's inner area).
            for (int i = 0; i < 40; i++)
            {
                Vector2 preferred = tmp.GetPreferredValues(text);
                if (preferred.x <= width && preferred.y <= height)
                    break;
                tmp.fontSize *= 0.93f;
            }
        }
    }
}
