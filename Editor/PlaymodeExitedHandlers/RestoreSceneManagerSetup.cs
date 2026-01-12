using System;
using EmreeDev.SceneBootstrapper.SceneLoadedHandlers;
using EmreeDev.SceneBootstrapper.Utilities;
using UnityEditor;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper.PlaymodeExitedHandlers
{
    [Serializable]
    public class RestoreSceneManagerSetup : IPlaymodeExitedHandler
    {
        public void OnPlaymodeExited(SceneBootstrapperData bootstrapperData)
        {
            // by not calling this we let Unity restore unsaved changes in the scene
            // EditorSceneManager.RestoreSceneManagerSetup(bootstrapperData.SceneSetups);
        }

        [CustomPropertyDrawer(typeof(RestoreSceneManagerSetup))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                "Default. Will try to restore hierarchy state (loaded scenes, selected & expanded objects) from before entering playmode.";
        }
    }
}