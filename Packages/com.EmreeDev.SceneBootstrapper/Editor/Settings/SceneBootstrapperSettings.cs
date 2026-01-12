using System.Linq;
using EmreeDev.SceneBootstrapper.SceneLoadedHandlers;
using EmreeDev.SceneBootstrapper.SceneProviders;
using EmreeDev.SceneBootstrapper.PlaymodeExitedHandlers;
using UnityEditor;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper.Settings
{
    public sealed class SceneBootstrapperSettings : ScriptableObject
    {
        private const string DefaultAssetPath = "Assets/SceneBootstrapperSettings.asset";
        private static string AssetPathKey => $"{Application.dataPath}_SceneBootstrapperSettingsPath";

        public bool Enabled = true;
        public bool KeepActiveSceneAsActive = true;
        [Tooltip("Preserve the state of the hierarchy (expanded scenes & gameobjects, selected gameobjects, etc.) on playmode change." +
                 "\n\nIf your scene is very large and playmode changing is slow, it might be worth turning this off to see if it improves things." +
                 "\n\nNote: Does not work with new UI Toolkit-based hierarchy introduced in Unity 6.3.")]
        [SerializeField]
        public bool PreserveHierarchyState = true;

        [SerializeReference]
        internal ISceneProvider sceneProvider = new FirstSceneInBuildSettings();

        [SerializeReference]
        internal ISceneLoadedHandler sceneLoadedHandler = new LoadAllLoadedScenes();

        [SerializeReference]
        internal IPlaymodeExitedHandler _playmodeExitedHandler = new RestoreSceneManagerSetup();

        internal ISceneProvider GetMainSceneProvider()
        {
            return sceneProvider;
        }

        internal ISceneLoadedHandler GetLoadMainSceneHandler()
        {
            return sceneLoadedHandler;
        }

        internal IPlaymodeExitedHandler GetPlaymodeExitedHandler()
        {
            return _playmodeExitedHandler;
        }

        internal static SceneBootstrapperSettings GetOrCreate()
        {
            if (TryLoadAsset(out SceneBootstrapperSettings settings))
            {
                return settings;
            }

            settings = CreateInstance<SceneBootstrapperSettings>();
            settings.Enabled = true;
            settings.KeepActiveSceneAsActive = true;
            settings.sceneProvider = new FirstSceneInBuildSettings();
            settings.sceneLoadedHandler = new LoadAllLoadedScenes();
            settings._playmodeExitedHandler = new RestoreSceneManagerSetup();
            AssetDatabase.CreateAsset(settings, DefaultAssetPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreate());
        }

        internal static bool TryLoadAsset(out SceneBootstrapperSettings settings)
        {
            string assetPath = EditorPrefs.GetString(AssetPathKey, DefaultAssetPath);
            // try to load at the saved or default path
            settings = AssetDatabase.LoadAssetAtPath<SceneBootstrapperSettings>(assetPath);
            if (settings != null)
            {
                return true;
            }

            // if no asset at path try to find it in project's assets
            string assetGuid = AssetDatabase.FindAssets($"t:{typeof(SceneBootstrapperSettings)}").FirstOrDefault();
            if (string.IsNullOrEmpty(assetGuid))
            {
                return false;
            }

            assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            settings = AssetDatabase.LoadAssetAtPath<SceneBootstrapperSettings>(assetPath);

            if (settings == null)
            {
                return false;
            }

            EditorPrefs.SetString(AssetPathKey, assetPath);
            return true;
        }
    }
}