using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EmreeDev.SceneBootstrapper.Settings;
using EmreeDev.SceneBootstrapper.Utilities;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper
{
    public static class SceneBootstrapper
    {
        private static string Key => $"{Application.productName}.SceneBootstrapperData";
        private static SceneBootstrapperData _currentBootstrapperData;
        private static SceneBootstrapperHierarchyData _currentBootstrapperHierarchyData;

        public static SceneBootstrapperData CurrentBootstrapperData
        {
            get
            {
                if (_currentBootstrapperData == null && EditorPrefs.HasKey(Key))
                {
                    string json = EditorPrefs.GetString(Key);
                    _currentBootstrapperData = SceneBootstrapperData.Deserialize(json);
                }

                return _currentBootstrapperData;
            }
            private set
            {
                _currentBootstrapperData = value;
                if (_currentBootstrapperData == null)
                {
                    EditorPrefs.DeleteKey(Key);
                    return;
                }

                EditorPrefs.SetString(Key, value.Serialize());
            }
        }

        public static SceneBootstrapperHierarchyData CurrentBootstrapperHierarchyData
        {
            get
            {
                if (_currentBootstrapperHierarchyData == null && EditorPrefs.HasKey(Key))
                {
                    string json = EditorPrefs.GetString(Key);
                    _currentBootstrapperHierarchyData = SceneBootstrapperHierarchyData.Deserialize(json);
                }

                return _currentBootstrapperHierarchyData;
            }
            private set
            {
                _currentBootstrapperHierarchyData = value;
                if (_currentBootstrapperHierarchyData == null)
                {
                    EditorPrefs.DeleteKey(Key);
                    return;
                }

                EditorPrefs.SetString(Key, value.Serialize());
            }
        }

        public static SceneBootstrapperSettings Settings => SceneBootstrapperSettings.GetOrCreate();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // no need for delay for not first load of the project
            if (SceneBootstrapperSettings.TryLoadAsset(out _))
            {
                InitializeInternal();
            }

            // delayed initialization is needed.
            // In the case of the first load of the project (when "Library" folder does not exist) AssetDatabase won't be able to track settings asset in the project
            // and if we try to load the settings asset now, it won't be able to load and will be overridden with default values
            EditorCoroutineUtility.StartCoroutine(DelayedInitialize(), null);
        }

        private static IEnumerator DelayedInitialize()
        {
            yield return new WaitForEndOfFrame();
            InitializeInternal();
        }

        private static void InitializeInternal()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.pauseStateChanged += PauseStateChanged;

            if (!Settings.Enabled)
            {
                SetPlayModeStartScene(null);
                return;
            }

            SceneAsset scene = Settings.GetMainSceneProvider().Get();
            SetPlayModeStartScene(scene);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnPlaymodeExited();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    OnEnteringPlayMode();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static void PauseStateChanged(PauseState state)
        {
            switch (state)
            {
                case PauseState.Paused:
                    if (Application.isPlaying)
                    {
                        if (Settings.PreserveHierarchyState)
                        {
                            SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(CurrentBootstrapperData, CurrentBootstrapperHierarchyData);
                        }
                    }

                    break;
                case PauseState.Unpaused:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private static void OnEnteringPlayMode()
        {
            CurrentBootstrapperData = null;
            if (!Settings.Enabled)
            {
                SetPlayModeStartScene(null);
                return;
            }

            SceneAsset mainScene = Settings.GetMainSceneProvider().Get();
            SetPlayModeStartScene(mainScene);

            var loadedScenes = EditorSceneManager.GetSceneManagerSetup();
            CurrentBootstrapperData = new SceneBootstrapperData(loadedScenes);
            CurrentBootstrapperHierarchyData = null;

            if (Settings.PreserveHierarchyState)
            {
                var selectedGameObjects = Selection.gameObjects;
                var selectedGameObjectsIds = new GlobalObjectId[selectedGameObjects.Length];
                GlobalObjectId.GetGlobalObjectIdsSlow(selectedGameObjects, selectedGameObjectsIds);

                var expandedInHierarchyObjects = SceneHierarchyUtility.GetExpandedGameObjects()
                    .Select(go => GlobalObjectId.GetGlobalObjectIdSlow(go)).ToArray();
                var expandedScenes = SceneHierarchyUtility.GetExpandedSceneNames();
                expandedScenes.Add(mainScene.name);

                CurrentBootstrapperHierarchyData = new SceneBootstrapperHierarchyData(selectedGameObjectsIds, expandedInHierarchyObjects,
                    expandedScenes);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnMainSceneLoaded()
        {
            if (CurrentBootstrapperData != null)
            {
                if (Settings.PreserveHierarchyState)
                {
                    SceneHierarchyUtility.SetScenesExpanded(new List<string>
                        { EditorSceneManager.playModeStartScene.name });
                }

                Settings.GetLoadMainSceneHandler().OnMainSceneLoaded(CurrentBootstrapperData);

                if (Settings.PreserveHierarchyState)
                {
                    SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(CurrentBootstrapperData, CurrentBootstrapperHierarchyData);
                }
            }
        }

        private static void OnPlaymodeExited()
        {
            if (CurrentBootstrapperData != null)
            {
                Settings.GetPlaymodeExitedHandler().OnPlaymodeExited(CurrentBootstrapperData);
            }

            if (Settings.PreserveHierarchyState)
            {
                SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(CurrentBootstrapperData,
                    CurrentBootstrapperHierarchyData);
            }

            CurrentBootstrapperData = null;
            CurrentBootstrapperHierarchyData = null;
        }

        /// <summary>
        ///     Returns true if playModeStartScene was changed, otherwise, false.
        /// </summary>
        /// <param name="sceneAsset">Scene asset</param>
        /// <returns>true if playModeStartScene was changed, otherwise, false</returns>
        public static bool SetPlayModeStartScene(SceneAsset sceneAsset)
        {
            if (EditorSceneManager.playModeStartScene != sceneAsset)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
                return true;
            }

            return false;
        }
    }
}