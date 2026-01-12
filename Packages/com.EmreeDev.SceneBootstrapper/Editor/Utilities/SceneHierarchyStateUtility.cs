using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EmreeDev.SceneBootstrapper.Utilities
{
    public static class SceneHierarchyStateUtility
    {
        /// <summary>
        ///     Starts EditorCoroutine that will restore previously selected and expanded GameObjects in the hierarchy.
        ///     Waits for scenes to load first.
        /// </summary>
        public static EditorCoroutine StartRestoreHierarchyStateCoroutine(SceneBootstrapperData bootstrapperData,
            SceneBootstrapperHierarchyData bootstrapperHierarchyData)
        {
            bool playmodeState = Application.isPlaying;
            return EditorCoroutineUtility.StartCoroutineOwnerless(RestoreHierarchyStateEnumerator(
                bootstrapperData, bootstrapperHierarchyData, playmodeState));
        }

        private static IEnumerator RestoreHierarchyStateEnumerator(SceneBootstrapperData bootstrapperData,
            SceneBootstrapperHierarchyData bootstrapperHierarchyData, bool playmodeState)
        {
            while (!IsAnySceneLoaded(bootstrapperData.SceneSetups))
            {
                yield return null;
                if (Application.isPlaying != playmodeState)
                {
                    Debug.Log("Playmode state was changed, stopped hierarchy state restore.");
                    yield break;
                }
            }

            yield return null;

            RestoreHierarchyStateImmediate(bootstrapperData, bootstrapperHierarchyData);
        }

        private static bool IsAnySceneLoaded(SceneSetup[] sceneSetups)
        {
            return sceneSetups
                .Any(s => SceneManager.GetSceneByPath(s.path).isLoaded);
        }

        /// <summary>
        ///     Immediately tries to restore previously selected and expanded GameObjects. If no scene was loaded will log error and return.
        /// </summary>
        /// <param name="bootstrapperData">The scene data loaded/to be loaded.</param>
        /// <param name="bootstrapperHierarchyData">The hierarchy state.</param>
        public static void RestoreHierarchyStateImmediate(SceneBootstrapperData bootstrapperData,
            SceneBootstrapperHierarchyData bootstrapperHierarchyData)
        {
            if (!IsAnySceneLoaded(bootstrapperData.SceneSetups))
            {
                Debug.LogError(
                    "Cannot restore hierarchy state because no scene was loaded when this method was called.");
                return;
            }

            SceneHierarchyUtility.SetScenesExpanded(bootstrapperHierarchyData.ExpandedScenes);

            var ids = bootstrapperHierarchyData.SelectedInHierarchyObjects;
            var selection = new List<GameObject>(ids.Length);
            bool isMissingObjects = false;
            for (int i = 0; i < ids.Length; i++)
            {
                GlobalObjectId id = ids[i];
                bool isPrefab = id.targetPrefabId != 0;
                if (isPrefab && Application.isPlaying)
                {
                    id = ConvertPrefabGidToUnpackedGid(id);
                }

                GameObject obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as GameObject;
                if (obj == null)
                {
                    isMissingObjects = true;
                    continue;
                }

                selection.Add(obj);
            }

            Selection.objects = selection.ToArray();

            for (int i = 0; i < bootstrapperHierarchyData.ExpandedInHierarchyObjects.Length; i++)
            {
                GlobalObjectId id = bootstrapperHierarchyData.ExpandedInHierarchyObjects[i];
                bool isPrefab = id.targetPrefabId != 0;
                if (isPrefab && Application.isPlaying)
                {
                    id = ConvertPrefabGidToUnpackedGid(id);
                }

                Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                if (obj == null || !(obj is GameObject))
                {
                    isMissingObjects = true;
                    continue;
                }

                SceneHierarchyUtility.SetExpanded(obj as GameObject, true);
            }

            if (isMissingObjects)
            {
                Debug.LogWarning(
                    "Some selected or expanded objects are missing. Most likely they are destroyed during Awake.");
            }
        }

        // how could I know this by myself... https://uninomicon.com/globalobjectid
        private static GlobalObjectId ConvertPrefabGidToUnpackedGid(GlobalObjectId id)
        {
            ulong fileId = (id.targetObjectId ^ id.targetPrefabId) & 0x7fffffffffffffff;
            bool success = GlobalObjectId.TryParse(
                $"GlobalObjectId_V1-{id.identifierType}-{id.assetGUID}-{fileId}-0",
                out GlobalObjectId unpackedGid);
            return unpackedGid;
        }
    }
}