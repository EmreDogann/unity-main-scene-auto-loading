using System;
using EmreeDev.SceneBootstrapper.SceneProviders;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class Sample_MainSceneProvider : ISceneProvider
{
    [SerializeField]
    private bool _setting1;

    [SerializeField]
    private int _setting2;

    [SerializeField]
    private Object _setting3;

    public SceneAsset Get()
    {
        // implementation
        throw new NotImplementedException();
    }

    [CustomPropertyDrawer(typeof(Sample_MainSceneProvider))]
    public class Sample_MainSceneProviderPropertyDrawer : PropertyDrawer
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
}