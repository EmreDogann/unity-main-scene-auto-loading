﻿using System;
using System.Linq;
using Ems.MainSceneAutoLoading.MainSceneLoadedHandlers;
using Ems.MainSceneAutoLoading.MainSceneProviders;
using Ems.MainSceneAutoLoading.PlaymodeExitedHandlers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ems.MainSceneAutoLoading.Settings
{
    [CustomEditor(typeof(MainSceneAutoLoadingSettings))]
    public class MainSceneAutoLoadingSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings.Enabled)));

            DrawRealization(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings._mainSceneProvider)),
                typeof(IMainSceneProvider));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings.KeepActiveSceneAsActive)));
            DrawRealization(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings._mainSceneLoadedHandler)),
                typeof(IMainSceneLoadedHandler));

            DrawRealization(serializedObject.FindProperty(nameof(MainSceneAutoLoadingSettings._playmodeExitedHandler)),
                typeof(IPlaymodeExitedHandler));

            EditorGUIUtility.labelWidth = labelWidth;
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRealization(SerializedProperty serializedProperty, Type addType)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(serializedProperty.displayName));
            string typeName = serializedProperty.managedReferenceFullTypename.Split('.', ' ').Last();
            typeName = ObjectNames.NicifyVariableName(typeName);
            if (EditorGUILayout.DropdownButton(new GUIContent(typeName), FocusType.Keyboard, GUILayout.MinWidth(10)))
            {
                GenericMenu menu = new GenericMenu();

                TypeCache.TypeCollection foundTypes = TypeCache.GetTypesDerivedFrom(addType);
                for (int i = 0; i < foundTypes.Count; ++i)
                {
                    Type type = foundTypes[i];

                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    if (type.IsSubclassOf(typeof(Object)))
                    {
                        continue;
                    }

                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                    {
                        serializedProperty.managedReferenceValue = Activator.CreateInstance(type);
                        serializedProperty.serializedObject.ApplyModifiedProperties();
                    });
                }

                if (menu.GetItemCount() == 0)
                {
                    menu.AddDisabledItem(new GUIContent($"No implementations of {addType}"));
                    Debug.LogError($"Something went really wrong. Can't find any implementation of type {addType}");
                }

                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedProperty, GUIContent.none, true);
        }
    }
}