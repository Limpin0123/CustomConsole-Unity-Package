using UnityEditor;
using UnityEngine;

namespace CustomConsole.Editor
{
    public class CreatingCC
    {
        [MenuItem("GameObject/CustomConsole/Custom Console", false, 10)]
        public static void CreatePrefab()
        {
            string prefabPath = "Packages/com.limpin.customconsole/CustomConsole/Prefabs/CustomConsole.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at: {prefabPath}");
                return;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance != null)
            {
                Selection.activeObject = instance;
            }
            else
            {
                Debug.LogError("Error while creating prefab.");
            }
        }
    }
}
