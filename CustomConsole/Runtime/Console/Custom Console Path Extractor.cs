using UnityEngine;

namespace CustomConsole.Runtime.Console
{
    public class CustomConsolePathExtractor : MonoBehaviour
    {
        [HideInInspector] public bool copyFullPath = false;
        [HideInInspector] public string path = "";
        [HideInInspector] public int line = -1;

        public bool TryToGetPathAndLine(string stactTrace, out string path, out int line)
        {
            path = "";
            line = -1;

            string[] lines = stactTrace.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("(at ") && !lines[i].Contains("CustomLogger:"))
                {
                    int startIndex = lines[i].IndexOf("(at ") + 4;
                    int endIndex = lines[i].IndexOf(")", startIndex);

                    string subString = lines[i].Substring(startIndex, endIndex - startIndex);

                    int lastColonIndex = subString.LastIndexOf(':');
                    path = subString.Substring(0, lastColonIndex);

                    if (int.TryParse(subString.Substring(lastColonIndex + 1),
                            out line)) //try to transform the string in an int
                    {
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public void OpenFile(string filePath, int line)
        {
#if UNITY_EDITOR
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, line);
#else
        string path = "";
        if(copyFullPath)
        {
            path = filePath;
        }
        else
        {
            int pathBeginning = filePath.IndexOf("Assets");
            path = filePath.Substring(pathBeginning);
        }
        GUIUtility.systemCopyBuffer = path;
        Debug.Log($"<size=60%>{path}</size> \nPath copied to clipboard \n<u>Error at line : {line}</u>");
#endif
        }
    }
}