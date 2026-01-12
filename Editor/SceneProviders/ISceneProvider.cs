using UnityEditor;

namespace EmreeDev.SceneBootstrapper.SceneProviders
{
    public interface ISceneProvider
    {
        SceneAsset Get();
    }
}