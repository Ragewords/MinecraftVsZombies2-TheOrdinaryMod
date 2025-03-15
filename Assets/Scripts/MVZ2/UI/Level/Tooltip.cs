﻿using System.Collections.Generic;
using MVZ2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVZ2.Level.UI
{
    public class Tooltip : MonoBehaviour
    {
        public void SetData(Transform target, Vector2 pivot, TooltipViewData viewData)
        {
            targetTransform = target;
            nameText.text = viewData.name;
            errorText.text = viewData.error;
            descriptionText.text = viewData.description;
            nameText.gameObject.SetActive(!string.IsNullOrEmpty(viewData.name));
            errorText.gameObject.SetActive(!string.IsNullOrEmpty(viewData.error));
            descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(viewData.description));

            var rectTransform = transform as RectTransform;
            rectTransform.pivot = pivot;
            rectTransform.position = targetTransform.position;
            var rootCanvas = rectTransform.GetRootCanvasNonAlloc(canvasListCache);
            if (rootCanvas)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                rectTransform.LimitInsideScreen(rootCanvas.transform, rootTransform.rect.size);
            }
        }
        private void Update()
        {
            if (!targetTransform || !targetTransform.gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }
            var rectTransform = transform as RectTransform;
            rectTransform.position = targetTransform.position;

            var rootCanvas = rectTransform.GetRootCanvasNonAlloc(canvasListCache);
            if (rootCanvas)
            {
                rectTransform.LimitInsideScreen(rootCanvas.transform, rootTransform.rect.size);
            }
        }
        [SerializeField]
        RectTransform rootTransform;
        [SerializeField]
        TextMeshProUGUI nameText;
        [SerializeField]
        TextMeshProUGUI errorText;
        [SerializeField]
        TextMeshProUGUI descriptionText;
        private Transform targetTransform;
        private List<Canvas> canvasListCache = new List<Canvas>();
    }
    public struct TooltipViewData
    {
        public string name;
        public string error;
        public string description;
    }
}
