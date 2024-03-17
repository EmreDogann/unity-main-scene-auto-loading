// Partially from: https://github.com/sandolkakos/unity-utilities/blob/main/Scripts/Editor/SceneHierarchyUtility.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ems.MainSceneAutoLoading.Utilities
{
    /// <summary>
    ///     Editor functionalities from internal SceneHierarchyWindow and SceneHierarchy classes.
    ///     For that we are using reflection.
    /// </summary>
    public static class SceneHierarchyUtility
    {
        private static Func<object, object> _sceneHierarchyGetter;
        private static Func<object, List<GameObject>> _expandedGameObjectGetter;
        private static Func<object, List<string>> _expandedSceneNamesGetter;
        private static Action<object, int, bool> _setExpandedMethod;
        private static Action<object, int, bool> _setExpandedRecursiveMethod;
        private static Action<object, List<string>> _setScenesExpandedMethod;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Type sceneHierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchy");
            Type sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

            _sceneHierarchyGetter = ReflectionUtility.BuildPropertyGetter<object, object>(sceneHierarchyWindowType,
                "sceneHierarchy", BindingFlags.Public | BindingFlags.Instance);

            _expandedGameObjectGetter = ReflectionUtility.BuildMethodInvoker<object, List<GameObject>>(
                sceneHierarchyType, "GetExpandedGameObjects", BindingFlags.Public | BindingFlags.Instance);

            _expandedSceneNamesGetter = ReflectionUtility.BuildMethodInvoker<object, List<string>>(
                sceneHierarchyType, "GetExpandedSceneNames", BindingFlags.NonPublic | BindingFlags.Instance);

            _setExpandedMethod = ReflectionUtility.BuildMethodInvoker_VoidReturn<object, int, bool>(
                sceneHierarchyType, "ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);

            _setExpandedRecursiveMethod = ReflectionUtility.BuildMethodInvoker_VoidReturn<object, int, bool>(
                sceneHierarchyType, "SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);

            _setScenesExpandedMethod = ReflectionUtility.BuildMethodInvoker_VoidReturn<object, List<string>>(
                sceneHierarchyType, "SetScenesExpanded", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        ///     Check if the target GameObject is expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static bool IsExpanded(GameObject go)
        {
            return GetExpandedGameObjects().Contains(go);
        }

        /// <summary>
        ///     Get a list of all GameObjects which are expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static List<GameObject> GetExpandedGameObjects()
        {
            object sceneHierarchy = GetSceneHierarchy();
            if (sceneHierarchy != null)
            {
                var result = _expandedGameObjectGetter(sceneHierarchy);

                return result;
            }

            return new List<GameObject>();
        }

        /// <summary>
        ///     Set the target GameObject as expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static void SetExpanded(GameObject go, bool expand)
        {
            object sceneHierarchy = GetSceneHierarchy();
            if (sceneHierarchy != null)
            {
                _setExpandedMethod(sceneHierarchy, go.GetInstanceID(), expand);
            }
        }

        /// <summary>
        ///     Set the target Scene as expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static void SetExpanded(Scene scene, bool expand)
        {
            object sceneHierarchy = GetSceneHierarchy();
            if (sceneHierarchy != null)
            {
                _setExpandedMethod(sceneHierarchy, scene.handle, expand);
            }
        }

        /// <summary>
        ///     Set the target GameObject and all children as expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static void SetExpandedRecursive(GameObject go, bool expand)
        {
            object sceneHierarchy = GetSceneHierarchy();
            if (sceneHierarchy != null)
            {
                _setExpandedRecursiveMethod(sceneHierarchy, go.GetInstanceID(), expand);
            }
        }

        private static object GetSceneHierarchy()
        {
            EditorWindow window = GetHierarchyWindow();
            if (window)
            {
                object sceneHierarchy = _sceneHierarchyGetter(window);
                return sceneHierarchy;
            }

            return null;
        }

        public static bool HasOpenInstances(Type t)
        {
            var objectsOfTypeAll = Resources.FindObjectsOfTypeAll(t);
            return objectsOfTypeAll != null && objectsOfTypeAll.Length != 0;
        }

        private static EditorWindow GetHierarchyWindow()
        {
            if (HasOpenInstances(typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow")))
            {
                // For it to open, so that it the current focused window.
                EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
                return EditorWindow.focusedWindow;
            }

            return null;
            // Type windowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            // return EditorWindow.GetWindow(windowType);
        }

        public static List<string> GetExpandedSceneNames()
        {
            object sceneHierarchy = GetSceneHierarchy();
            if (sceneHierarchy != null)
            {
                var result = _expandedSceneNamesGetter(sceneHierarchy);

                return result;
            }

            return new List<string>();
        }

        public static void SetScenesExpanded(List<string> sceneNames)
        {
            object sceneHierarchy = GetSceneHierarchy();
            if (sceneHierarchy != null)
            {
                _setScenesExpandedMethod(sceneHierarchy, sceneNames);
            }
        }
    }
}