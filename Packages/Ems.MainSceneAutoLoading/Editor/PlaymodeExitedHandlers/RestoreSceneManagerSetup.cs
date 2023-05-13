using Ems.MainSceneAutoLoading.MainSceneLoadedHandlers;
using Ems.MainSceneAutoLoading.Utilities;
using UnityEditor;

namespace Ems.MainSceneAutoLoading.PlaymodeExitedHandlers
{
    public class RestoreSceneManagerSetup : IPlaymodeExitedHandler
    {
        public void OnPlaymodeExited(LoadMainSceneArgs args)
        {
            // by not calling this we let Unity restore unsaved changes in the scene
            // EditorSceneManager.RestoreSceneManagerSetup(args.SceneSetups);
            SceneHierarchyStateUtility.StartRestoreHierarchyStateCoroutine(args);
        }
        
        [CustomPropertyDrawer(typeof(RestoreSceneManagerSetup))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                $"Default. Will try to restore hierarchy state(loaded scenes, selected objects, objects` expanding) that was before entering playmode.";
        }
    }
}