using UnityEditor;

namespace Watermelon
{
    [CustomEditor(typeof(GameSettings))]
    public class GameSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("SETTINGS");

            serializedObject.Update();
            EditorGUILayoutCustom.DrawAllProperties(serializedObject, true);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
        }
    }
}
