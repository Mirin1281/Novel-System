using UnityEngine;
using UnityEditor;
using Novel.Command;

namespace Novel.Editor
{
    [CustomEditor(typeof(CommandData))]
    public class CommandDataEditor : UnityEditor.Editor
    {
        CommandData _target;

        public override void OnInspectorGUI()
        {
            if (_target == null)
            {
                _target = target as CommandData;
            }
            if (_target != null)
            {
                DrawBoxLayout(_target.GetCommandColor());
            }
            
            // enabledÇÃê›íË //
            var enabledProp = serializedObject.FindProperty("enabled");
            EditorGUILayout.PropertyField(enabledProp);

            using (new EditorGUI.DisabledScope(!enabledProp.boolValue))
            {
                // commandÇÃê›íË //
                var commandProp = serializedObject.FindProperty("command");
                EditorGUILayout.PropertyField(commandProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawBoxLayout(Color color)
        {
            var originalColor = GUI.color;

            // AlphaílÇè¨Ç≥Ç≠ÇµÇ»Ç¢Ç∆ï∂éöÇ™å©Ç¶Ç»Ç¢ÇÃÇ≈â∫Ç∞ÇÈ
            GUI.color = new Color(color.r, color.g, color.b, 0.3f);
            var style = new GUIStyle
            {
                normal =
            {
                background = Texture2D.whiteTexture
            }
            };
            GUI.Box(new Rect(0, 0, EditorGUIUtility.currentViewWidth, 43f), string.Empty, style);

            GUI.color = originalColor;
        }
    }
}