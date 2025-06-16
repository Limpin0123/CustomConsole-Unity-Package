using System.IO;
using UnityEditor;
using UnityEngine;
using CustomConsole.Runtime.Console;
#if UNITY_EDITOR

namespace CustomConsole.Editor
{
    [CustomEditor(typeof(CustomConsolePathExtractor))]
    public class PathTool : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle warningStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle titleStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle normalStyle = new GUIStyle
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            warningStyle.normal.textColor = new Color(1, 1, 0.2f);
            titleStyle.normal.textColor = Color.white;
            normalStyle.normal.textColor = Color.white;

            CustomConsolePathExtractor script = (CustomConsolePathExtractor)target;

            bool pathIsOk = false;
            bool lineIsOk = false;

            GUILayout.Label("Path Opener Tool", titleStyle);
            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.Label("Path to file", normalStyle);
            script.path = EditorGUILayout.TextField(script.path);
            if (script.path == "") EditorGUILayout.LabelField("File Path is empty", warningStyle);
            else if (!File.Exists(script.path)) EditorGUILayout.LabelField("File path doesn't exist", warningStyle);
            else pathIsOk = true;
            GUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.5f, 0.5f, 0.5f));
            GUILayout.Space(10);

            GUILayout.BeginVertical();
            GUILayout.Label("Line in file", normalStyle);
            script.line = EditorGUILayout.IntField(script.line);
            if (script.line < 1) EditorGUILayout.LabelField("Line Number is empty", warningStyle);
            else if (pathIsOk && script.line <= File.ReadAllLines(script.path).Length) lineIsOk = true;
            else EditorGUILayout.LabelField("Line Number is too big or file isn't correct", warningStyle);
            GUILayout.EndVertical();

            GUILayout.Space(10);
            if (GUILayout.Button("Open File") && pathIsOk && lineIsOk)
            {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(script.path, script.line);
            }
        }
    }
}
#endif
