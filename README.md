# Unity Editor Scene Bootstrapper
Editor-only tool intended to streamline your workflow when using Persistent/initial/loader/boot/bootstrap (you name it) scenes.
<br>

Features:
 + Load specificed scenes before any other when entering playmode.
 + Persist hierarchy state(selected and expanded objects) on entering/exiting playmode.
 + Convinient settings UI located in ProjectSettings window.
 + UPM support, simple install.
 + Highly extendable for your project.

Dependencies:
 + Unity 2019.4+ (uses [SerializeReference](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/SerializeReference.html) attribute)
 + [EditorCoroutines](https://docs.unity3d.com/Packages/com.unity.editorcoroutines@1.0/manual/index.html) package

Notes:
 - Does not yet support the new UITK-based hierarchy introduced in Unity 6.3 LTS.
 - This is **Editor-only**, will not build into a runtime build.

![image](https://user-images.githubusercontent.com/23558898/118852487-81cd5c00-b8db-11eb-8c40-de2e1ae2a458.png)

https://user-images.githubusercontent.com/23558898/118852692-ab868300-b8db-11eb-9f79-9c9e76c96f76.mp4

https://user-images.githubusercontent.com/23558898/118852714-ae817380-b8db-11eb-8217-cbb1f810175e.mp4

## Installation
Install via git url by adding this entry into your **manifest.json**

`"com.emreedev.scenebootstrapper": "https://github.com/EmreDogann/Unity-Editor-Scene-Bootstrapper.git#upm"`

Or using the PackageManager window:
1. copy this link `https://github.com/EmreDogann/Unity-Editor-Scene-Bootstrapper.git#upm`,
2. open PackageManager window,
3. click `+` button in top-left corner,
4. select `Add package from git URL`,
5. in appeared field paste the link you copied before (`https://github.com/EmreDogann/Unity-Editor-Scene-Bootstrapper.git#upm`).

## How it works
After installing this package it will create `Assets/SceneBootstrapperSettings.asset`. Plugin settings can be changed by selecting this asset or by using ProjectSettings window.

`Scene Bootstrapper` works upon playmode changes only:
- `SceneProvider` is the system that will determine which scene to return when the plugin asks for the "bootstrap" scene.
- `SceneLoadedHandler` decides what to do after the bootstrap scene has loaded.
- `PlaymodeExitHandler` decides what to do after exiting playmode.
- **On Playmode Enter:** `SceneProvider` asks for the bootstrap scene and sets  `EditorSceneManager.playModeStartScene`([docs](https://docs.unity3d.com/ScriptReference/SceneManagement.EditorSceneManager-playModeStartScene.html)) with specified scene. This will load the scene before any others.
  - When the bootstrap scene is loaded, `SceneLoadedHandler` is notified with additional information about preveiously opened scenes.
- **On Playmode Exit:** `SceneProvider` notifies `PlaymodeExitHandler`.
- You also have the option to save and restore hierarchy settings between playmode sessions.

### Available Defaults
Scene Providers:
- `First Scene In Build Settings`: Will always bootstrap the scene listed as first (index 0) in the build settings options.
- `Specified Scene Asset`: Allows you to pick from the project browser any scene asset.
- Or, a custom provider (see [Extending](#Extending) section below).

Scene Loaded Handlers:
- `Load All Loaded Scenes (Additive)`: Loads all scenes that were loaded in the hierarchy before entering playmode.
- `Load All Loaded Scenes`: Similar to `Load All Loaded Scenes (Additive)`, but will load them non-additively. Loads the active scene first.
- `Load Active Scene`: Loads only the active scene in the hierarchy.
- `Load Active Scene (Additive)`: Similar to `Load Active Scene`, but additively loads only the active scene in the hierarchy.
- `Delegate To In Scene Implementations`: Will find and call all scene loaded handlers in the active scene.

Playmode Exit Handlers:
- `Restore Scene Manager Setup`: Does nothing. Intended as a blank slate you can use to extend functionality from here.

## Extending
For now, there are 3 ways to extend:
 + SceneProvider
 + SceneLoadedHandler
 + IPlaymodeExitHandler

**Main thing to remember - all extended scripts should be in editor assemblies! e.g. under `Editor` folder or within editor .asmdef**

### ISceneProvider
Responsible for providing the bootstrap scene, that will be loaded first when entering playmode:
```c#
public interface ISceneProvider
{
    SceneAsset Get();
}
```

An example of implementing your own is shown in the sample code ([Sample_SceneProvider.cs](https://github.com/EmreDogann/Unity-Editor-Scene-Bootstrapper/blob/main/Assets/Sample_SceneProvider.cs)).

After that you could find your option in settings's dropdown:
![image](https://user-images.githubusercontent.com/23558898/118925561-9a735b80-b947-11eb-915e-74811f5f99a9.png)

You could also write a custom property drawer if you'd like:
```c#
[CustomPropertyDrawer(typeof(Sample_SceneProvider))]
public class Sample_SceneProviderPropertyDrawer : PropertyDrawer
{
    private const int FieldsCount = 4;
    private const int FieldHeightSelf = 18;
    private const int FieldHeightTotal = 20;


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return FieldHeightTotal * FieldsCount;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.indentLevel++;

        position.height = FieldHeightSelf;
        GUI.enabled = false;
        EditorGUI.LabelField(position, "Custom description");
        GUI.enabled = true;
        position.y += FieldHeightTotal;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_setting1"));
        position.y += FieldHeightTotal;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_setting2"));
        position.y += FieldHeightTotal;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("_setting3"));

        EditorGUI.indentLevel--;
    }
}
```
![image](https://user-images.githubusercontent.com/23558898/118938973-e0382000-b957-11eb-8b88-aa9bbfc0de7e.png)

### ISceneLoadedHandler

The same as above, just implement this interface and you are good to go.

However, you could also create an in-scnene handler and combine with the `Delegate To In Scene Implementations` scene provider.
```c#
// This script should not be present in builds!
#if UNITY_EDITOR
using System.Collections;
using EmreeDev.SceneBootstrapper;
using EmreeDev.SceneBootstrapper.SceneLoadedHandlers;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameSceneLoadedHandler : MonoBehaviour, ISceneLoadedHandler
{
    public void OnSceneLoaded(SceneBootstrapperData bootstrapperData)
    {
        Debug.Log("OnSceneLoaded! Now decide what to do with bootstrapperData.SceneSetups...");
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

```

### IPlaymodeExitedHandler
Look at the default empty implementation [RestoreSceneManagerSetup](https://github.com/EmreDogann/Unity-Editor-Scene-Bootstrapper/blob/main/Packages/com.EmreeDev.SceneBootstrapper/Editor/PlaymodeExitHandlers/RestoreSceneManagerSetup.cs).
