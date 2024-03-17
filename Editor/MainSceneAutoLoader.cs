using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ems.MainSceneAutoLoading.Settings;
using Ems.MainSceneAutoLoading.Utilities;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ems.MainSceneAutoLoading
{
    public static class MainSceneAutoLoader
    {
        private static string Key => $"{Application.productName}.LoadMainSceneArgs";
        private static LoadMainSceneArgs _currentArgs;

        public static LoadMainSceneArgs CurrentArgs
        {
            get
            {
                if (_currentArgs == null && EditorPrefs.HasKey(Key))
                {
                    string json = EditorPrefs.GetString(Key);
                    _currentArgs = LoadMainSceneArgs.Deserialize(json);
                }

                return _currentArgs;
            }
            private set
            {
                _currentArgs = value;
                if (_currentArgs == null)
                {
                    EditorPrefs.DeleteKey(Key);
                    return;
                }

                EditorPrefs.SetString(Key, value.Serialize());
            }
        }

        public static MainSceneAutoLoadingSettings Settings => MainSceneAutoLoadingSettings.GetOrCreate();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // no need for delay for not first load of the project
            if (MainSceneAutoLoadingSettings.TryLoadAsset(out _))
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
                        SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(CurrentArgs);
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
            CurrentArgs = null;
            if (!Settings.Enabled)
            {
                SetPlayModeStartScene(null);
                return;
            }

            SceneAsset mainScene = Settings.GetMainSceneProvider().Get();
            // SceneAsset currentLoadedScene = SceneAssetUtility.ConvertPathToSceneAsset(SceneManager.GetActiveScene().path);
            // if (mainScene == currentLoadedScene)
            // {
            //     return;
            // }

            SetPlayModeStartScene(mainScene);

            var loadedScenes = EditorSceneManager.GetSceneManagerSetup();
            var selectedGameObjects = Selection.gameObjects;
            var selectedGameObjectsIds = new GlobalObjectId[selectedGameObjects.Length];
            GlobalObjectId.GetGlobalObjectIdsSlow(selectedGameObjects, selectedGameObjectsIds);

            var expandedInHierarchyObjects = SceneHierarchyUtility.GetExpandedGameObjects()
                .Select(go => GlobalObjectId.GetGlobalObjectIdSlow(go)).ToArray();
            var expandedScenes = SceneHierarchyUtility.GetExpandedSceneNames();
            expandedScenes.Add(mainScene.name);

            CurrentArgs = new LoadMainSceneArgs(loadedScenes, selectedGameObjectsIds, expandedInHierarchyObjects,
                expandedScenes);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnMainSceneLoaded()
        {
            if (CurrentArgs != null)
            {
                SceneHierarchyUtility.SetScenesExpanded(new List<string>
                    { EditorSceneManager.playModeStartScene.name });

                bool originalCrossSceneReferenceValue = EditorSceneManager.preventCrossSceneReferences;
                if (Settings.enableCrossSceneReferenceSupport)
                {
                    EditorSceneManager.preventCrossSceneReferences = false;
                }

                Settings.GetLoadMainSceneHandler().OnMainSceneLoaded(CurrentArgs);

                EditorSceneManager.preventCrossSceneReferences = originalCrossSceneReferenceValue;
            }
        }

        private static void OnPlaymodeExited()
        {
            if (CurrentArgs != null)
            {
                Settings.GetPlaymodeExitedHandler().OnPlaymodeExited(CurrentArgs);
            }

            CurrentArgs = null;
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

        [MenuItem("Tools/MainSceneAutoLoading/Reset")]
        public static void Reset()
        {
            EditorSceneManager.playModeStartScene = null;
        }
    }
}