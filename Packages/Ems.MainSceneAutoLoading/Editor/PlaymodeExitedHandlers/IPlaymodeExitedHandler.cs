﻿namespace Ems.MainSceneAutoLoading.PlaymodeExitedHandlers
{
    public interface IPlaymodeExitedHandler
    {
        void OnPlaymodeExited(LoadMainSceneArgs args);
    }
}