using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using CustomConsole.Runtime.Logger;

namespace CustomConsole.Runtime.Console
{
    public class CustomConsolePathExtractor : MonoBehaviour
    {
        [HideInInspector] public bool copyFullPath = false;
        [HideInInspector] public string path = "";
        [HideInInspector] public int line = -1;

        public bool TryToGetPathAndLine(string stackTrace, out string path, out int line)
        {
            path = "";
            line = -1;
            
            MatchCollection matches = Regex.Matches(stackTrace, @"^(?!.*CustomLogger).*\(at ([^<>""\\|?*@:]+:?[^<>""\\|?*@:]+\.cs):(\d+)\)", RegexOptions.Multiline);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    path = match.Groups[1].Value;
                    if (int.TryParse(match.Groups[2].Value,
                            out line)) //try to transform the string in an int
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void OpenFile(string filePath, int line)
        {
#if UNITY_EDITOR
            if(!File.Exists(filePath)) CustomLogger.CCErrorLog($"file path doesn't exis {filePath}t");
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