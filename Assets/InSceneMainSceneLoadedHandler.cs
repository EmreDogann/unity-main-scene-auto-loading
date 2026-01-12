#if UNITY_EDITOR // this script should not be present in builds
using System.Collections;
using EmreeDev.SceneBootstrapper;
using EmreeDev.SceneBootstrapper.SceneLoadedHandlers;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InSceneMainSceneLoadedHandler : MonoBehaviour, ISceneLoadedHandler
{
    public void OnMainSceneLoaded(SceneBootstrapperData bootstrapperData)
    {
        Debug.Log("OnMainSceneLoaded! Now decide what to do with bootstrapperData.SceneSetups...");
        StartCoroutine(LoadDesiredScenes(bootstrapperData));
    }

    private IEnumerator LoadDesiredScenes(SceneBootstrapperData bootstrapperData)
    {
        yield return new WaitForSeconds(1f);
        foreach (SceneSetup sceneSetup in bootstrapperData.SceneSetups)
        {
            SceneManager.LoadScene(sceneSetup.path, LoadSceneMode.Additive);
        }
    }
}
#endif