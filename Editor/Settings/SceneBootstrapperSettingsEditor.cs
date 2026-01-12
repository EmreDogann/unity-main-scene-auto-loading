using System;
using System.Linq;
using EmreeDev.SceneBootstrapper.SceneLoadedHandlers;
using EmreeDev.SceneBootstrapper.SceneProviders;
using EmreeDev.SceneBootstrapper.PlaymodeExitedHandlers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EmreeDev.SceneBootstrapper.Settings
{
    [CustomEditor(typeof(SceneBootstrapperSettings))]
    public class SceneBootstrapperSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUILayout.VerticalScope(EditorStyles.inspectorDefaultMargins))
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 200;

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SceneBootstrapperSettings.Enabled)));

                DrawRealization(serializedObject.FindProperty(nameof(SceneBootstrapperSettings.sceneProvider)),
                    typeof(ISceneProvider));

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(nameof(SceneBootstrapperSettings.KeepActiveSceneAsActive)));
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty(nameof(SceneBootstrapperSettings.PreserveHierarchyState)));
                DrawRealization(serializedObject.FindProperty(nameof(SceneBootstrapperSettings.sceneLoadedHandler)),
                    typeof(ISceneLoadedHandler));

                DrawRealization(serializedObject.FindProperty(nameof(SceneBootstrapperSettings._playmodeExitedHandler)),
                    typeof(IPlaymodeExitedHandler));

                EditorGUIUtility.labelWidth = labelWidth;
            }

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