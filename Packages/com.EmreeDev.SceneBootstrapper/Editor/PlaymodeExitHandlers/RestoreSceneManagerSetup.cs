using System;
using EmreeDev.SceneBootstrapper.SceneLoadedHandlers;
using EmreeDev.SceneBootstrapper.Utilities;
using UnityEditor;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper.PlaymodeExitHandlers
{
    [Serializable]
    public class RestoreSceneManagerSetup : IPlaymodeExitHandler
    {
        public void OnPlaymodeExit(SceneBootstrapperData bootstrapperData)
        {
            // By not calling this we let Unity restore unsaved changes in the scene
            // EditorSceneManager.RestoreSceneManagerSetup(bootstrapperData.SceneSetups);
        }

        [CustomPropertyDrawer(typeof(RestoreSceneManagerSetup))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Does nothing. Intended as a template to extend from.";
        }
    }
}