using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EmreeDev.SceneBootstrapper.Settings
{
    public class SceneBootstrapperSettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        private Editor _editor;

        public SceneBootstrapperSettingsProvider(string path, SettingsScope scopes,
            IEnumerable<string> keywords = null) : base(path, scopes, keywords) {}

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _editor = Editor.CreateEditor(SceneBootstrapperSettings.GetOrCreate());
            _serializedObject = SceneBootstrapperSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space();
            _editor.OnInspectorGUI();
        }

        public static bool IsSettingsAvailable()
        {
            return SceneBootstrapperSettings.TryLoadAsset(out SceneBootstrapperSettings _);
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            if (IsSettingsAvailable())
            {
                SceneBootstrapperSettingsProvider provider =
                    new SceneBootstrapperSettingsProvider("Project/Scene Bootstrapper", SettingsScope.Project);

                // Automatically extract all keywords from the Styles.
                provider.keywords =
                    GetSearchKeywordsFromSerializedObject(SceneBootstrapperSettings.GetSerializedSettings());
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }
    }
}