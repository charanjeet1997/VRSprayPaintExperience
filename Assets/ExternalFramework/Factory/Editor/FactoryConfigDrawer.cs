using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Game.Factories
{
    [CustomPropertyDrawer(typeof(FactoryConfig<>))]
    public class FactoryConfigDrawer : PropertyDrawer
    {
        private List<string> names = new List<string>()
        {
            "StartPoint", "EndPoint",
            "BombTrap", "SpikeTrap", "SentryTrap", // Traps
            "ArmourBoost", "HealthBoost", "SneakBoost", "StaminaBoost", "ShieldBoost", "DamageBoost", "AmmoBoost", // Boosts
            "Melee", "Shooter", "Zombie", "GunnerShooter", "SniperShooter", "ChaserZombie", "ReviverZombie", // Enemies
            "SelectionGizmo", // Gizmos
            "Bomb", "BombDropZone",
            "Coin",// Collectibles
            "CapturableFlag", "CapturableFlagDropZone",
            "CaptureZone",
            "PS_ArmourBoost", "PS_HealthBoost", "PS_SneakBoost", "PS_StaminaBoost", "PS_ShieldBoost", "PS_AmmoBoost", // Boosts effects
            "PS_Hit", // Hit effects
            "SentryBullet",
            "SFX","UISFX","BackgroundMusic", // Sounds
            "ScenaryObject",
        };
 
        private const float Padding = 5f; // Padding space
        private bool isFoldout = true; // Foldout state
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
{
    EditorGUI.BeginProperty(position, label, property);
 
    // Apply padding to the top
    position.y += Padding;
 
    // Draw the foldout
    isFoldout = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), isFoldout, label);
    if (isFoldout)
    {
        position.y += EditorGUIUtility.singleLineHeight + 2;
 
        // Get the prefabs property (assumes the field is named "prefabs")
        SerializedProperty prefabsProperty = property.FindPropertyRelative("prefab");
        // Debug.Log("Prefab is null "+ (prefabsProperty != null));
        // Debug.Log("Prefab is array "+ prefabsProperty.isArray);
 
        if (prefabsProperty != null && prefabsProperty.isArray)
        {
            // Draw label for the prefab list
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "Prefabs");
            position.y += EditorGUIUtility.singleLineHeight + 2;
 
            // Loop through the prefab list
            for (int i = 0; i < prefabsProperty.arraySize; i++)
            {
                SerializedProperty element = prefabsProperty.GetArrayElementAtIndex(i);
                EditorGUI.PropertyField(
                    new Rect(position.x + 10, position.y, position.width - 10, EditorGUIUtility.singleLineHeight),
                    element,
                    GUIContent.none
                );
                position.y += EditorGUIUtility.singleLineHeight + 2;
            }
 
            // Buttons to add/remove elements
            if (GUI.Button(new Rect(position.x, position.y, position.width * 0.5f - 5, EditorGUIUtility.singleLineHeight), "Add Prefab"))
            {
                prefabsProperty.arraySize++;
            }
 
            if (GUI.Button(new Rect(position.x + position.width * 0.5f + 5, position.y, position.width * 0.5f - 5, EditorGUIUtility.singleLineHeight), "Remove Prefab"))
            {
                if (prefabsProperty.arraySize > 0)
                    prefabsProperty.arraySize--;
            }
 
            position.y += EditorGUIUtility.singleLineHeight + 2;
        }
        else
        {
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), "No prefab list found.");
            position.y += EditorGUIUtility.singleLineHeight + 2;
        }
 
        // Draw other properties
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.FindPropertyRelative("name"), new GUIContent("Name"));
        position.y += EditorGUIUtility.singleLineHeight + 2;
 
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.FindPropertyRelative("startImmediately"), new GUIContent("WorldStart Immediately"));
        position.y += EditorGUIUtility.singleLineHeight + 2;
 
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.FindPropertyRelative("numberOfObjectsToCreate"), new GUIContent("Number of Objects"));
        position.y += EditorGUIUtility.singleLineHeight + 2;
 
        EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.FindPropertyRelative("delayBetweenInstances"), new GUIContent("Delay Between Instances"));
    }
 
    EditorGUI.EndProperty();
}
 
 
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
{
    if (isFoldout)
    {
        SerializedProperty prefabsProperty = property.FindPropertyRelative("prefab");
        int prefabCount = prefabsProperty != null && prefabsProperty.isArray ? prefabsProperty.arraySize : 0;
 
        // Base height for foldout + other fields
        float height = (5 * (EditorGUIUtility.singleLineHeight + 2)) + (2 * Padding);
 
        // Add height for each prefab in the list
        height += prefabCount * (EditorGUIUtility.singleLineHeight + 2);
 
        // Add height for Add/Remove buttons
        height += EditorGUIUtility.singleLineHeight + 2;
 
        return height;
    }
    else
    {
        return EditorGUIUtility.singleLineHeight + 2 * Padding;
    }
}
 
 
    }
}
