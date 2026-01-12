using UnityEditor;
using UnityEngine;

namespace EmreeDev.SceneBootstrapper.SceneLoadedHandlers
{
    public abstract class BasePropertyDrawer : PropertyDrawer
    {
        public abstract string Description { get; }
        private float _textHeight = EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUIStyle style = EditorStyles.wordWrappedLabel;

            EditorGUI.indentLevel++;
            Rect indentedRect = EditorGUI.IndentedRect(position);
            EditorGUI.indentLevel--;

            _textHeight = style.CalcHeight(new GUIContent(Description, Description), indentedRect.width);
            indentedRect.height = _textHeight;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.LabelField(indentedRect, Description, style);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _textHeight;
        }
    }
}