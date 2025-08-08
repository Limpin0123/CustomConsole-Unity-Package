using System;
using UnityEditor;
using UnityEngine;

namespace CustomConsole.Runtime
{
    [ExecuteAlways]
    public class BottomToolResizer : MonoBehaviour
    {
        [SerializeField] private RectTransform sizeReference;
        
        [SerializeField] private RectTransform bottomToolRectTransform;
        [SerializeField] private RectTransform persistentFunctionRectTransform;
        //[SerializeField] private RectTransform helperRectTransform;
        [SerializeField, Range(0.01f,1)] private float heightRatio = 0.1f;
        [SerializeField] private float minWidth = 300;
        private Vector2 _lastSize;

#if UNITY_EDITOR
        private void OnEnable()
        {
            EditorApplication.update += OnReferenceSizeChanging;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnReferenceSizeChanging;
        }

        private void OnReferenceSizeChanging()
        {
            if (sizeReference == null) return;

            if (sizeReference.rect.size != _lastSize)
            {
                _lastSize = sizeReference.rect.size;
                Resizing();
            }
        }
#endif
        private void Resizing()
        {
            Vector2 newSize = sizeReference.rect.size;
            newSize.y = Mathf.Clamp(sizeReference.rect.height * heightRatio, 20, 50);
            Vector2 newPos = new Vector2(0, 0);
            if (newSize.x < minWidth)
            {
                newPos.x = (minWidth - newSize.x) * 0.5f;
                newSize.x = minWidth;
            }
            newPos.y = -newSize.y * 0.5f - (newSize.y * 0.05f);
            bottomToolRectTransform.sizeDelta = newSize;
            bottomToolRectTransform.anchoredPosition = newPos;
            
            persistentFunctionRectTransform.sizeDelta = new Vector2(newSize.y - 2, newSize.y - 2);
            persistentFunctionRectTransform.anchoredPosition = new Vector2(-newSize.y * 0.5f -2, 0);
            
            //helperRectTransform
        }
    }
}
