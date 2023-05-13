using UnityEditor;

namespace Ems.MainSceneAutoLoading.MainSceneProviders
{
    public interface IMainSceneProvider
    {
        SceneAsset Get();
    }
}