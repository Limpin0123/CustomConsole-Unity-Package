using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CustomConsole.Editor
{
    public class CreatingCC
    {
        [MenuItem("GameObject/CustomConsole/Custom Console", false, 10)]
        public static void CreatePrefab()
        {
            EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
            GameObject eventSystemGO = (eventSystem != null) ? eventSystem.gameObject : null;
            GameObject selected = Selection.activeGameObject;
            bool isCanvaSelected = selected != null && selected.GetComponent<Canvas>() != null && selected.scene == SceneManager.GetActiveScene();
            string messageString = "Will be created :\n• A Custom Console";
            if (!isCanvaSelected) messageString += "\n• A parent Canva";
            if(eventSystem == null) messageString += "\n• An event system";
            
            if(!EditorUtility.DisplayDialog("Custom Console", messageString, "Proced", "Cancel"))
            {
                return;
            }
            
            string prefabPath = "Packages/com.limpin.customconsole/Prefabs/Custom Console.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at: {prefabPath}");
                return;
            }
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (isCanvaSelected)
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

            if (eventSystemGO == null)
            {
                eventSystemGO = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
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