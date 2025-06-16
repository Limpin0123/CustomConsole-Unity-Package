using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CustomConsole.Editor
{
    public class CreatingCC
    {
        [MenuItem("GameObject/CustomConsole/Custom Console", false, 10)]
        public static void CreatePrefab()
        {
            string prefabPath = "Packages/com.limpin.customconsole/Prefabs/Custom Console.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at: {prefabPath}");
                return;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Canvas>() != null)
            {
                instance.transform.SetParent(Selection.activeGameObject.transform);
                instance.transform.localPosition = Vector3.zero;
            }
            else
            {
                GameObject canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                
                Undo.RegisterCreatedObjectUndo(canvas, "Create Canvas");
                instance.transform.SetParent(canvas.transform, false);
            }
            
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