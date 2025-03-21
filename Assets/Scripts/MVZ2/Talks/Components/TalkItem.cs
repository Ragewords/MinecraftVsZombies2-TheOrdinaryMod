﻿using UnityEngine;
using UnityEngine.UI;

namespace MVZ2.Talk
{
    public class TalkItem : MonoBehaviour
    {
        public void SetShowing(bool showing)
        {
            animator.SetBool("Showing", showing);
        }
        public void ForceShow()
        {
            animator.SetTrigger("Show");
        }
        public void SetSprite(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private Image iconImage;
    }
}
