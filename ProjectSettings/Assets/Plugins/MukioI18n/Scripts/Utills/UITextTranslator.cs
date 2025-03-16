﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MukioI18n
{
    [RequireComponent(typeof(Text))]
    public class UITextTranslator : TranslateComponent<Text>
    {
        [SerializeField]
        string comment;

        [SerializeField]
        string context;

        public override void Translate()
        {
            if (string.IsNullOrEmpty(Context))
            {
                Component.text = provider._(Key);
            }
            else
            {
                Component.text = provider._p(Context, Key);
            }
        }

        protected override string getCommentInner()
        {
            return comment;
        }

        protected override string getContextInner()
        {
            return context;
        }

        protected override string getKeyInner()
        {
            return Component.text;
        }
    }
}
