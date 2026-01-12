using EmreeDev.SceneBootstrapper.Utilities;
using UnityEditor;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    public class DelegateToInSceneImplementations : ISceneLoadedHandler
    {
        public void OnMainSceneLoaded(SceneBootstrapperData bootstrapperData)
        {
            var handlers = ObjectUtility.FindInterfacesOfType<ISceneLoadedHandler>();
            foreach (ISceneLoadedHandler handler in handlers)
            {
                handler.OnMainSceneLoaded(bootstrapperData);
            }
        }

        [CustomPropertyDrawer(typeof(DelegateToInSceneImplementations))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                $"Finds all implementations of {nameof(ISceneLoadedHandler)} " +
                $"in the main scene and calls '{nameof(ISceneLoadedHandler.OnMainSceneLoaded)}()' on them.";
        }
    }
}