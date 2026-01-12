// Partially from: https://github.com/sandolkakos/unity-utilities/blob/main/Scripts/Editor/SceneHierarchyUtility.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper.Utilities
{
    /// <summary>
    ///     Editor functionalities from internal SceneHierarchyWindow and SceneHierarchy classes.
    ///     For that we are using reflection.
    /// </summary>
    public static class SceneHierarchyUtility
    {
        private static Func<object, object> _sceneHierarchyGetter;
        private static Func<object> _lastActiveHierarchyWindowGetter;
        private static Func<object, List<GameObject>> _expandedGameObjectGetter;
        private static Func<object, List<string>> _expandedSceneNamesGetter;
#if UNITY_6000_2_OR_NEWER
        private static Action<object, EntityId, bool> _setExpandedMethod;
#else
        private static Action<object, int, bool> _setExpandedMethod;
#endif
        private static Action<object, List<string>> _setScenesExpandedMethod;
        private static Type _hierarchyWindowType;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Type sceneHierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchy");
            _hierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            _sceneHierarchyGetter = ReflectionUtility.BuildPropertyGetter<object, object>(_hierarchyWindowType,
                "sceneHierarchy", BindingFlags.Public | BindingFlags.Instance);
            _lastActiveHierarchyWindowGetter = ReflectionUtility.BuildPropertyGetter<object>(
                _hierarchyWindowType, "lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);

            _expandedGameObjectGetter = ReflectionUtility.BuildMethodInvoker<object, List<GameObject>>(
                sceneHierarchyType, "GetExpandedGameObjects", BindingFlags.Public | BindingFlags.Instance);

            _expandedSceneNamesGetter = ReflectionUtility.BuildMethodInvoker<object, List<string>>(
                sceneHierarchyType, "GetExpandedSceneNames", BindingFlags.NonPublic | BindingFlags.Instance);

#if UNITY_6000_2_OR_NEWER
            _setExpandedMethod = ReflectionUtility.BuildMethodInvoker_VoidReturn<object, EntityId, bool>(
                sceneHierarchyType, "ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);
#else
            _setExpandedMethod = ReflectionUtility.BuildMethodInvoker_VoidReturn<object, int, bool>(
                sceneHierarchyType, "ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

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
#if UNITY_6000_2_OR_NEWER
                _setExpandedMethod(sceneHierarchy, go.GetEntityId(), expand);
#else
                _setExpandedMethod(sceneHierarchy, go.GetInstanceID(), expand);
#endif
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

        private static EditorWindow GetHierarchyWindow()
        {
            return _lastActiveHierarchyWindowGetter() as EditorWindow;
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