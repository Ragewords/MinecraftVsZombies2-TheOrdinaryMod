﻿using UnityEngine;

namespace MVZ2.UI
{
    public abstract class Dialog : MonoBehaviour
    {
        public void ResetPosition()
        {
            dialogTransform.anchoredPosition = Vector2.zero;
        }
        [SerializeField]
        private RectTransform dialogTransform;
    }
}
