using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRSprayPainting.EditorTools
{
    /// <summary>
    /// Inspector for [SerializeReference, SubclassSelector] fields and lists: a dropdown to pick the concrete type
    /// (lesson step, practice goal...), then that type's fields. New subclasses appear automatically.
    /// </summary>
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, Type[]> TypeCacheByField = new Dictionary<string, Type[]>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            object current = property.managedReferenceValue;
            string typeName = current != null ? ObjectNames.NicifyVariableName(current.GetType().Name) : "(None)";

            Rect button = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            if (EditorGUI.DropdownButton(button, new GUIContent(typeName), FocusType.Keyboard))
                ShowMenu(property);

            // Element label shows its type, e.g. "Step 2: Practice Step".
            var shown = new GUIContent(current != null ? $"{label.text}: {typeName}" : label.text);
            EditorGUI.PropertyField(position, property, shown, true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUI.GetPropertyHeight(property, label, true);

        private static void ShowMenu(SerializedProperty property)
        {
            var menu = new GenericMenu();
            SerializedObject owner = property.serializedObject;
            string path = property.propertyPath;

            menu.AddItem(new GUIContent("(None)"), property.managedReferenceValue == null, () => Assign(owner, path, null));
            foreach (Type type in ConcreteTypes(property.managedReferenceFieldTypename))
            {
                Type chosen = type;
                bool selected = property.managedReferenceValue?.GetType() == type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), selected,
                    () => Assign(owner, path, Activator.CreateInstance(chosen)));
            }
            menu.ShowAsContext();
        }

        private static void Assign(SerializedObject owner, string path, object value)
        {
            owner.Update();
            SerializedProperty property = owner.FindProperty(path);
            property.managedReferenceValue = value;
            owner.ApplyModifiedProperties();
        }

        // "Assembly Namespace.Type" -> concrete, default-constructible subclasses.
        private static IEnumerable<Type> ConcreteTypes(string fieldTypename)
        {
            if (TypeCacheByField.TryGetValue(fieldTypename, out Type[] cached))
                return cached;

            Type baseType = null;
            int split = fieldTypename.IndexOf(' ');
            if (split > 0)
                baseType = Type.GetType($"{fieldTypename.Substring(split + 1)}, {fieldTypename.Substring(0, split)}");

            Type[] types = baseType == null
                ? Array.Empty<Type>()
                : TypeCache.GetTypesDerivedFrom(baseType)
                    .Where(t => !t.IsAbstract && !t.IsGenericType && !typeof(UnityEngine.Object).IsAssignableFrom(t)
                                && t.GetConstructor(Type.EmptyTypes) != null)
                    .OrderBy(t => t.Name)
                    .ToArray();
            TypeCacheByField[fieldTypename] = types;
            return types;
        }
    }
}
