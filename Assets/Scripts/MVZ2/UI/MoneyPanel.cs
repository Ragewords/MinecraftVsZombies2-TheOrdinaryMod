﻿using TMPro;
using UnityEngine;

namespace MVZ2.UI
{
    public class MoneyPanel : MonoBehaviour
    {
        public void SetMoney(string money)
        {
            moneyText.text = money;
        }
        public void SetFade(bool fade)
        {
            willFade = fade;
        }
        public void Hide()
        {
            fadeTimeout = 0;
        }
        public void ResetTimeout()
        {
            fadeTimeout = maxFadeTimeout;
        }
        private void Update()
        {
            if (willFade)
            {
                fadeTimeout -= Time.deltaTime;
                if (fadeTimeout <= 0)
                {
                    fadeTimeout = 0;
                }
            }
            else
            {
                fadeTimeout = maxFadeTimeout;
            }
            canvasGroup.alpha = Mathf.Clamp01(fadeTimeout / 0.5f);
        }
        [SerializeField]
        private CanvasGroup canvasGroup;
        [SerializeField]
        private TextMeshProUGUI moneyText;
        [SerializeField]
        private bool willFade;
        [SerializeField]
        private float maxFadeTimeout;
        private float fadeTimeout;
    }
}
