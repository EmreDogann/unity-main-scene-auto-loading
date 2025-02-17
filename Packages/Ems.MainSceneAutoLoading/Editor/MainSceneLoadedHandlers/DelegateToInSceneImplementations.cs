﻿using Ems.MainSceneAutoLoading.Utilities;
using UnityEditor;

namespace Ems.MainSceneAutoLoading.MainSceneLoadedHandlers
{
    public class DelegateToInSceneImplementations : IMainSceneLoadedHandler
    {
        public void OnMainSceneLoaded(LoadMainSceneArgs args)
        {
            var handlers = ObjectUtility.FindInterfacesOfType<IMainSceneLoadedHandler>();
            foreach (IMainSceneLoadedHandler handler in handlers)
            {
                handler.OnMainSceneLoaded(args);
            }
        }

        [CustomPropertyDrawer(typeof(DelegateToInSceneImplementations))]
        public sealed class Drawer : BasePropertyDrawer
        {
            public override string Description =>
                $"Finds all implementations of {nameof(IMainSceneLoadedHandler)} " +
                $"in the main scene and calls '{nameof(IMainSceneLoadedHandler.OnMainSceneLoaded)}()' on them.";
        }
    }
}