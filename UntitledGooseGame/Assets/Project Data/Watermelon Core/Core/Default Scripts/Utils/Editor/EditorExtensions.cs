﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    public static class EditorExtensions
    {
        /// <summary>
        /// Create SciptableObject from code
        /// </summary>
        public static T CreateItem<T>(Type type, string fullPath) where T : ScriptableObject
        {
            T item = (T)ScriptableObject.CreateInstance(type);

            string objectPath = fullPath + ".asset";

            if (AssetDatabase.LoadAssetAtPath<T>(objectPath) != null)
            {
                return AssetDatabase.LoadAssetAtPath<T>(objectPath);
            }

            AssetDatabase.CreateAsset(item, objectPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return item;
        }

        /// <summary>
        /// Add element to SerializedProperty array
        /// </summary>
        public static void AddToObjectArray<T>(this SerializedProperty arrayProperty, T elementToAdd) where T : Object
        {
            // If the SerializedProperty this is being called from is not an array, throw an exception.
            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " is not an array.");

            // Pull all the information from the target of the serializedObject.
            arrayProperty.serializedObject.Update();

            // Add a null array element to the end of the array then populate it with the object parameter.
            arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
            arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).objectReferenceValue = elementToAdd;

            // Push all the information on the serializedObject back to the target.
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Remove element from SerializedProperty array
        /// </summary>
        public static void RemoveFromObjectArrayAt(this SerializedProperty arrayProperty, int index)
        {
            // If the index is not appropriate or the serializedProperty this is being called from is not an array, throw an exception.
            if (index < 0)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " cannot have negative elements removed.");

            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " is not an array.");

            if (index > arrayProperty.arraySize - 1)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " has only " + arrayProperty.arraySize + " elements so element " + index + " cannot be removed.");

            // Pull all the information from the target of the serializedObject.
            arrayProperty.serializedObject.Update();

            // If there is a non-null element at the index, null it.
            if (arrayProperty.GetArrayElementAtIndex(index).objectReferenceValue)
                arrayProperty.DeleteArrayElementAtIndex(index);

            // Delete the null element from the array at the index.
            arrayProperty.DeleteArrayElementAtIndex(index);

            // Push all the information on the serializedObject back to the target.
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Use this to remove an object from an object array represented by a SerializedProperty.
        /// </summary>
        /// <typeparam name="T">Type of object to be removed.</typeparam>
        /// <param name="arrayProperty">Property that contains array.</param>
        /// <param name="elementToRemove">Element to be removed.</param>
        public static void RemoveFromObjectArray<T>(this SerializedProperty arrayProperty, T elementToRemove) where T : Object
        {
            // If either the serializedProperty doesn't represent an array or the element is null, throw an exception.
            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " is not an array.");

            if (!elementToRemove)
                throw new UnityException("Removing a null element is not supported using this method.");

            // Pull all the information from the target of the serializedObject.
            arrayProperty.serializedObject.Update();

            // Go through all the elements in the serializedProperty's array...
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty elementProperty = arrayProperty.GetArrayElementAtIndex(i);

                // ... until the element matches the parameter...
                if (elementProperty.objectReferenceValue == elementToRemove)
                {
                    // ... then remove it.
                    arrayProperty.RemoveFromObjectArrayAt(i);
                    return;
                }
            }

            throw new UnityException("Element " + elementToRemove.name + "was not found in property " + arrayProperty.name);
        }

        /// <summary>
        /// Use this to remove the object at an index from an object array represented by a SerializedProperty.
        /// </summary>
        public static void RemoveFromVariableArrayAt(this SerializedProperty arrayProperty, int index)
        {
            // If the index is not appropriate or the serializedProperty this is being called from is not an array, throw an exception.
            if (index < 0)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " cannot have negative elements removed.");

            if (!arrayProperty.isArray)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " is not an array.");

            if (index > arrayProperty.arraySize - 1)
                throw new UnityException("SerializedProperty " + arrayProperty.name + " has only " + arrayProperty.arraySize + " elements so element " + index + " cannot be removed.");

            // Pull all the information from the target of the serializedObject.
            arrayProperty.serializedObject.Update();

            // Delete the null element from the array at the index.
            arrayProperty.DeleteArrayElementAtIndex(index);

            // Push all the information on the serializedObject back to the target.
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Get object from serializedProperty
        /// </summary>
        public static object GetPropertyObject(SerializedProperty property)
        {
            string[] path = property.propertyPath.Split('.');
            object baseObject = property.serializedObject.targetObject;
            Type baseType = baseObject.GetType();

            for (int i = 0; i < path.Length; i++)
            {
                FieldInfo fieldInfo = baseType.GetField(path[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                baseType = fieldInfo.FieldType;

                baseObject = fieldInfo.GetValue(baseObject);
            }

            return baseObject;
        }

        /// <summary>
        /// Get full path to serializedProperty
        /// </summary>
        public static string GetPropertyPath(this SerializedProperty property)
        {
            return property.propertyPath.Split(new string[] { ".Array" }, StringSplitOptions.None)[0];
        }

        /// <summary>
        /// Get serializedProperty id from path
        /// </summary>
        public static int GetPropertyArrayIndex(this SerializedProperty property)
        {
            Match match = Regex.Match(property.propertyPath, @"\[([^)]*)\]");

            return int.Parse(match.Result("$1"));
        }

        public static bool AddObject<T>(this SerializedProperty property, T addedObject) where T : ScriptableObject
        {
            if (property.isArray)
            {
                if (addedObject != null)
                {
                    property.serializedObject.Update();

                    int index = property.arraySize;

                    property.arraySize++;
                    property.GetArrayElementAtIndex(index).objectReferenceValue = addedObject;

                    property.serializedObject.ApplyModifiedProperties();

                    return true;
                }
            }

            return false;
        }

        public static bool RemoveObject(this SerializedProperty property, int index, string title = "This object will be removed!", string content = "Are you sure?")
        {
            if (EditorUtility.DisplayDialog(title, content, "Remove", "Cancel"))
            {
                if (property.isArray)
                {
                    string assetPath = AssetDatabase.GetAssetPath(property.GetArrayElementAtIndex(index).objectReferenceValue);

                    property.RemoveFromObjectArrayAt(index);

                    if (File.Exists(EditorUtils.projectFolderPath + assetPath))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }

                    return true;
                }
            }

            return false;
        }

        public static void SelectSourceObject(this SerializedProperty property)
        {
            if (property.objectReferenceValue != null)
            {
                EditorUtility.FocusProjectWindow();

                EditorGUIUtility.PingObject(property.objectReferenceValue);
            }
        }

        public static MethodInfo PlayClipMethod()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            return audioUtilClass.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, null);
        }

        public static MethodInfo StopClipMethod()
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            return audioUtilClass.GetMethod("StopClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, null);
        }

        public static void PlayClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            MethodInfo method = audioUtilClass.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, null);

            method.Invoke(null, new object[] { clip });
        }

        public static void StopClip(AudioClip clip)
        {
            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");

            MethodInfo method = audioUtilClass.GetMethod("StopClip", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(AudioClip) }, null);

            method.Invoke(null, new object[] { clip });
        }

        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            property.NextVisible(true);
            while (true)
            {
                if ((SerializedProperty.EqualContents(property, nextElement)))
                {
                    yield break;
                }

                yield return property;

                bool hasNext = property.NextVisible(false);
                if (!hasNext)
                {
                    break;
                }
            }
        }

        public static IEnumerable<SerializedProperty> GetPropertiesByGroup(this SerializedObject serializedObject, string groupName)
        {
            Type targetType = serializedObject.targetObject.GetType();

            IEnumerable<FieldInfo> fieldInfos = targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(x => x.CompareAttributeName(groupName));
            foreach (var field in fieldInfos)
            {
                yield return serializedObject.FindProperty(field.Name);
            }
        }

        public static bool CompareAttributeName(this FieldInfo fieldInfo, string groupName)
        {
            Attribute attribute = fieldInfo.GetCustomAttribute(typeof(GroupAttribute), false);
            if (attribute != null)
            {
                GroupAttribute groupAttribute = (GroupAttribute)attribute;
                if (groupAttribute != null)
                {
                    return groupAttribute.Name == groupName;
                }
            }

            return false;
        }
    }
}