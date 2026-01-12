using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper
{
    public class SceneBootstrapperData
    {
        public readonly SceneSetup[] SceneSetups;

        public SceneBootstrapperData(SceneSetup[] sceneSetups)
        {
            SceneSetups = sceneSetups;
        }

        [Serializable]
        private class SaveData
        {
            public SceneSetup[] SceneSetups;
        }

        public string Serialize()
        {
            SaveData saveData = new SaveData
            {
                SceneSetups = SceneSetups
            };

            string json = JsonUtility.ToJson(saveData);
            return json;
        }

        public static SceneBootstrapperData Deserialize(string json)
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            SceneBootstrapperData bootstrapperData = new SceneBootstrapperData(
                saveData.SceneSetups
            );

            return bootstrapperData;
        }
    }

    public class SceneBootstrapperHierarchyData
    {
        public readonly GlobalObjectId[] SelectedInHierarchyObjects;
        public readonly GlobalObjectId[] ExpandedInHierarchyObjects;
        public readonly List<string> ExpandedScenes;

        public SceneBootstrapperHierarchyData(
            GlobalObjectId[] selectedInHierarchyObjects,
            GlobalObjectId[] expandedInHierarchyObjects,
            List<string> expandedScenes)
        {
            SelectedInHierarchyObjects = selectedInHierarchyObjects;
            ExpandedInHierarchyObjects = expandedInHierarchyObjects;
            ExpandedScenes = expandedScenes;
        }

        [Serializable]
        private class SaveData
        {
            public string[] SelectedInHierarchyObjects;
            public string[] ExpandedInHierarchyObjects;
            public List<string> ExpandedScenes;
        }

        public string Serialize()
        {
            SaveData saveData = new SaveData
            {
                SelectedInHierarchyObjects = SelectedInHierarchyObjects.Select(x => x.ToString()).ToArray(),
                ExpandedInHierarchyObjects = ExpandedInHierarchyObjects.Select(x => x.ToString()).ToArray(),
                ExpandedScenes = ExpandedScenes
            };

            string json = JsonUtility.ToJson(saveData);
            return json;
        }

        public static SceneBootstrapperHierarchyData Deserialize(string json)
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            SceneBootstrapperHierarchyData bootstrapperData = new SceneBootstrapperHierarchyData(
                ParseGlobalObjectIds(saveData.SelectedInHierarchyObjects),
                ParseGlobalObjectIds(saveData.ExpandedInHierarchyObjects),
                saveData.ExpandedScenes
            );

            return bootstrapperData;
        }

        private static GlobalObjectId[] ParseGlobalObjectIds(string[] stringIds)
        {
            var ids = new List<GlobalObjectId>(stringIds.Length);
            foreach (string stringId in stringIds)
            {
                if (GlobalObjectId.TryParse(stringId, out GlobalObjectId id))
                {
                    ids.Add(id);
                }
            }

            return ids.ToArray();
        }
    }
}