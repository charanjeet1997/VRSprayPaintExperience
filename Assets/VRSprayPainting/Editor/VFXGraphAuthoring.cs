using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace VRSprayPainting.EditorTools
{
    /// <summary>
    /// Builds the spray gun VFX Graph assets from the "Simple Loop" template.
    /// The VFX Graph model API is internal, so everything goes through reflection.
    /// Menu: Tools > VR Spray Painting > Build Spray VFX.
    /// </summary>
    public static class VFXGraphAuthoring
    {
        public const string DropletsPath = "Assets/VRSprayPainting/VFX/Spray_Droplets.vfx";
        public const string MistPath = "Assets/VRSprayPainting/VFX/Spray_Mist.vfx";
        public const string ColorProperty = "SprayColor";
        public const string BlendProperty = "SprayBlend";

        private const string TemplatePath = "Packages/com.unity.visualeffectgraph/Editor/Templates/02_Simple_Loop.vfx";
        private const string MistTexturePath = "Assets/VRSprayPainting/Sprites/SprayMist.png";
        private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static Assembly EditorAssembly =>
            AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Unity.VisualEffectGraph.Editor");

        private static Type VfxType(string fullName) =>
            EditorAssembly.GetType(fullName) ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName)).First(t => t != null);

        [MenuItem("Tools/VR Spray Painting/Build Spray VFX", priority = 40)]
        public static void BuildAll()
        {
            Build(DropletsPath, mist: false);
            Build(MistPath, mist: true);
            RelinkGunPrefab();
            Debug.Log("[VRSpraySetup] Built " + DropletsPath + " and " + MistPath);
        }

        private static void Build(string path, bool mist)
        {
            if (!AssetDatabase.IsValidFolder("Assets/VRSprayPainting/VFX"))
                AssetDatabase.CreateFolder("Assets/VRSprayPainting", "VFX");
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CopyAsset(TemplatePath, path);
            AssetDatabase.ImportAsset(path);

            object resource = GetResource(path);
            object graph = Call(VfxType("UnityEditor.VFX.VisualEffectResourceExtensions"), null, "GetOrCreateGraph", resource);

            object spawner = Find(graph, "VFXBasicSpawner");
            object init = Find(graph, "VFXBasicInitialize");
            object update = Find(graph, "VFXBasicUpdate");
            object output = Find(graph, "VFXPlanarPrimitiveOutput");

            // Spawner: emits only between Play() and Stop() (OnPlay/OnStop events from SprayGun).
            SetSlot(Find(spawner, "VFXSpawnerConstantRate"), "Rate", mist ? 900f : 2000f);

            // Initialize. Template capacity is 68 — far below rate × lifetime, which made the spray tiny.
            object data = Call(init.GetType(), init, "GetData");
            Call(data.GetType(), data, "SetSettingValue", "capacity", (uint)(mist ? 1500 : 1500));
            SetBounds(init, new Vector3(0f, 0f, mist ? 0.45f : 0.4f), new Vector3(1f, 1f, 1.2f));
            object lifetime = Find(init, "SetAttribute");
            SetSlot(lifetime, "A", mist ? 0.35f : 0.15f);
            SetSlot(lifetime, "B", mist ? 0.75f : 0.3f);

            object shape = Find(init, "PositionShape");
            SetSphereRadius(shape, 0.003f);

            object velocity = Find(init, "VelocityDirection");
            SetSlotValue(SubSlot(InputSlot(velocity, "Direction"), "direction"), Vector3.forward);
            SetSlot(velocity, "MinSpeed", mist ? 1.6f : 2.5f);
            SetSlot(velocity, "MaxSpeed", mist ? 2.2f : 3.5f);
            // Blend toward the sphere normal gives the cone spread (~8° droplets, ~10° mist).
            SetSlot(velocity, "DirectionBlend", mist ? 0.84f : 0.87f);
            // Exposed so SprayGun can widen/tighten the cone with the precision control.
            object blendParam = CreateExposed(graph, BlendProperty, typeof(float), mist ? 0.84f : 0.87f);
            Link(OutputSlot(blendParam), InputSlot(velocity, "DirectionBlend"));

            object color = CreateSetAttribute("color", random: false);
            AddChild(init, color);
            object colorParam = CreateExposedColor(graph, ColorProperty, new Color(0.85f, 0.1f, 0.1f));
            Link(OutputSlot(colorParam), InputSlot(color, 0));

            object alpha = CreateSetAttribute("alpha", random: false);
            AddChild(init, alpha);
            SetSlotValue(InputSlot(alpha, 0), 1f);

            // Update
            SetSlotValue(SubSlot(InputSlot(Find(update, "Gravity"), "Force"), "vector"), mist ? new Vector3(0f, 0.03f, 0f) : new Vector3(0f, -0.4f, 0f));
            SetSlot(Find(update, "Drag"), "dragCoefficient", mist ? 2.5f : 0.6f);

            // Output
            Texture2D texture = mist
                ? AssetDatabase.LoadAssetAtPath<Texture2D>(MistTexturePath)
                : AssetDatabase.GetBuiltinExtraResource<Texture2D>("Default-Particle.psd");
            if (texture != null)
                SetSlot(output, "mainTexture", texture);
            SetSetting(output, "sort", Enum.Parse(SettingType(output, "sort"), "On")); // back-to-front for soft mist
            object curves = Children(output).Cast<object>().Where(c => c.GetType().Name == "AttributeFromCurve").ToArray();
            var curveBlocks = (object[])curves;

            object sizeBlock = curveBlocks[0];
            SetSetting(sizeBlock, "Composition", Enum.ToObject(SettingType(sizeBlock, "Composition"), 0)); // Overwrite
            SetSlot(sizeBlock, "Size", mist
                ? new AnimationCurve(new Keyframe(0f, 0.015f), new Keyframe(0.4f, 0.1f), new Keyframe(1f, 0.22f))
                : new AnimationCurve(new Keyframe(0f, 0.004f), new Keyframe(1f, 0.006f)));

            object colorBlock = curveBlocks[1];
            var gradient = new Gradient();
            gradient.SetKeys(
                mist
                    ? new[] { new GradientColorKey(new Color(1f, 0.85f, 0.85f), 0f), new GradientColorKey(Color.white, 1f) }
                    : new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                mist
                    ? new[] { new GradientAlphaKey(0.27f, 0f), new GradientAlphaKey(0.3f, 0.2f), new GradientAlphaKey(0.13f, 0.65f), new GradientAlphaKey(0f, 1f) }
                    : new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.6f), new GradientAlphaKey(0f, 1f) });
            SetSlot(colorBlock, "Color", gradient);

            Save(graph, resource, path);
        }

        // Rebuilding recreates the assets (new GUIDs), so point the gun's effects at them again.
        private static void RelinkGunPrefab()
        {
            const string prefabPath = "Assets/VRSprayPainting/Prefabs/SprayGun.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            Transform nozzle = root.transform.Find("Nozzle");
            if (nozzle != null)
            {
                Relink(nozzle.Find("SprayDroplets"), DropletsPath);
                Relink(nozzle.Find("SprayMist"), MistPath);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void Relink(Transform target, string assetPath)
        {
            if (target != null && target.TryGetComponent(out UnityEngine.VFX.VisualEffect effect))
                effect.visualEffectAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.VFX.VisualEffectAsset>(assetPath);
        }

        // ---------- model helpers ----------

        private static object GetResource(string path) =>
            Call(VfxType("UnityEditor.VFX.VisualEffectResource"), null, "GetResourceAtPath", path);

        public static IEnumerable Children(object model) => (IEnumerable)Prop(model.GetType(), "children").GetValue(model);

        private static object Find(object parent, string typeName) =>
            Children(parent).Cast<object>().First(c => c.GetType().Name == typeName);

        private static PropertyInfo Prop(Type t, string name) =>
            t.GetProperties(All).Where(p => p.Name == name).OrderBy(p => p.DeclaringType == t ? 0 : 1).FirstOrDefault();

        private static object Call(Type type, object target, string method, params object[] args)
        {
            MethodInfo m = type.GetMethods(All)
                .Where(x => x.Name == method && x.GetParameters().Length >= args.Length &&
                            x.GetParameters().Skip(args.Length).All(p => p.IsOptional))
                .OrderBy(x => x.GetParameters().Length)
                .First(x => x.GetParameters().Take(args.Length).Select((p, i) => args[i] == null || p.ParameterType.IsInstanceOfType(args[i]) || p.ParameterType.IsAssignableFrom(args[i].GetType())).All(ok => ok));
            object[] full = args.Concat(m.GetParameters().Skip(args.Length).Select(p => p.DefaultValue)).ToArray();
            return m.Invoke(target, full);
        }

        private static object InputSlot(object model, int index) =>
            ((IEnumerable)Prop(model.GetType(), "inputSlots").GetValue(model)).Cast<object>().ElementAt(index);

        private static object InputSlot(object model, string name) =>
            ((IEnumerable)Prop(model.GetType(), "inputSlots").GetValue(model)).Cast<object>()
                .First(s => (string)Prop(s.GetType(), "name").GetValue(s) == name);

        private static object OutputSlot(object model) =>
            ((IEnumerable)Prop(model.GetType(), "outputSlots").GetValue(model)).Cast<object>().First();

        private static void SetSlot(object model, string slotName, object value)
        {
            object slot = InputSlot(model, slotName);
            Prop(slot.GetType(), "value").SetValue(slot, value);
        }

        private static object SubSlot(object slot, string name) =>
            Children(slot).Cast<object>().First(s => (string)Prop(s.GetType(), "name").GetValue(s) == name);

        private static void SetSlotValue(object slot, object value) => Prop(slot.GetType(), "value").SetValue(slot, value);

        private static Type SettingType(object model, string name) => Field(model.GetType(), name).FieldType;

        private static FieldInfo Field(Type t, string name)
        {
            for (; t != null; t = t.BaseType)
            {
                FieldInfo f = t.GetField(name, All);
                if (f != null) return f;
            }
            return null;
        }

        private static void SetSetting(object model, string name, object value) =>
            Call(model.GetType(), model, "SetSettingValue", name, value);

        private static void AddChild(object parent, object child) => Call(parent.GetType(), parent, "AddChild", child);

        private static void Link(object from, object to) => Call(from.GetType(), from, "Link", to);

        private static void SetBounds(object init, Vector3 center, Vector3 size)
        {
            object bounds = InputSlot(init, "bounds");
            SetSlotValue(SubSlot(bounds, "center"), center);
            SetSlotValue(SubSlot(bounds, "size"), size);
        }

        private static void SetSphereRadius(object shapeBlock, float radius)
        {
            object arc = InputSlot(shapeBlock, "arcSphere");
            object sphere = SubSlot(arc, "sphere");
            SetSlotValue(SubSlot(sphere, "radius"), radius);
        }

        private static object CreateSetAttribute(string attribute, bool random)
        {
            Type t = VfxType("UnityEditor.VFX.Block.SetAttribute");
            object block = ScriptableObject.CreateInstance(t);
            SetSetting(block, "attribute", attribute);
            SetSetting(block, "Random", Enum.ToObject(SettingType(block, "Random"), random ? 2 : 0));
            return block;
        }

        private static object CreateExposedColor(object graph, string name, Color value) =>
            CreateExposed(graph, name, typeof(Color), value);

        private static object CreateExposed(object graph, string name, Type valueType, object value)
        {
            Type t = VfxType("UnityEditor.VFX.VFXParameter");
            object param = ScriptableObject.CreateInstance(t);
            Call(t, param, "Init", valueType);
            SetSetting(param, "m_Exposed", true);
            SetSetting(param, "m_ExposedName", name);
            AddChild(graph, param);
            object slot = OutputSlot(param);
            SetSlotValue(slot, value);
            Prop(t, "value")?.SetValue(param, value);
            return param;
        }

        private static void Save(object graph, object resource, string path)
        {
            Type g = graph.GetType();
            if (g.GetMethods(All).Any(m => m.Name == "SetExpressionGraphDirty"))
                Call(g, graph, "SetExpressionGraphDirty");
            Call(g, graph, "RecompileIfNeeded");
            Call(resource.GetType(), resource, "WriteAsset");
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        // ---------- debug ----------

        public static string Dump(string assetPath)
        {
            object graph = Call(VfxType("UnityEditor.VFX.VisualEffectResourceExtensions"), null, "GetOrCreateGraph", GetResource(assetPath));
            var sb = new StringBuilder();
            DumpModel(graph, 0, sb);
            return sb.ToString();
        }

        private static void DumpModel(object model, int depth, StringBuilder sb)
        {
            Type t = model.GetType();
            sb.Append(new string(' ', depth * 2)).Append(t.Name);
            PropertyInfo inputs = Prop(t, "inputSlots");
            if (inputs != null)
                foreach (object slot in (IEnumerable)inputs.GetValue(model))
                {
                    object val = null;
                    try { val = Prop(slot.GetType(), "value").GetValue(slot); } catch { }
                    bool linked = false;
                    try { linked = (bool)Call(slot.GetType(), slot, "HasLink"); } catch { }
                    sb.Append(" [").Append(Prop(slot.GetType(), "name").GetValue(slot)).Append('=').Append(val).Append(linked ? "*" : "").Append(']');
                }
            foreach (FieldInfo f in t.GetFields(All).Where(f => f.GetCustomAttributes(true).Any(a => a.GetType().Name == "VFXSettingAttribute")))
                if (f.Name is "attribute" or "Composition" or "sort" or "m_ExposedName")
                    sb.Append(" {").Append(f.Name).Append('=').Append(f.GetValue(model)).Append('}');
            sb.AppendLine();

            PropertyInfo children = Prop(t, "children");
            if (children != null && !(t.Name.EndsWith("Slot") || t.Name.StartsWith("VFXSlot")))
                foreach (object c in (IEnumerable)children.GetValue(model))
                    DumpModel(c, depth + 1, sb);
        }
    }
}
