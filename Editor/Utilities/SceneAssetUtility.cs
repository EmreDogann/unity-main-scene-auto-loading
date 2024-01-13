using UnityEditor;

namespace Ems.MainSceneAutoLoading.Utilities
{
    public static class SceneAssetUtility
    {
        public static string ConvertSceneAssetToString(SceneAsset sceneAsset)
        {
            return AssetDatabase.GetAssetOrScenePath(sceneAsset);
        }

        public static SceneAsset ConvertPathToSceneAsset(string scenePath)
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }
    }
}