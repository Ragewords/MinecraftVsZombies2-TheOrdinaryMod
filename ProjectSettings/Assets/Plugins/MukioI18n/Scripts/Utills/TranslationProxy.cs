using System;
using UnityEngine;

namespace MukioI18n
{
    /// <summary>
    /// 简易的翻译代理
    /// </summary>
    public class TranslationProxy : MonoBehaviour, ITranslationProvider
    {
        [SerializeField]
        ITranslationProvider provider;
        
        [Obsolete]
        I18n ITranslationProvider.i18n => provider.i18n;

        #region Translation
        public string _(string text)
        {
            return provider._(text);
        }

        public string _(string text, params object[] args)
        {
            return provider._(text, args);
        }

        public string _n(string text, string pluralText, long n)
        {
            return provider._n(text, pluralText, n);
        }

        public string _n(string text, string pluralText, long n, params object[] args)
        {
            return provider._n(text, pluralText, n, args);
        }

        public string _p(string context, string text)
        {
            return provider._p(context, text);
        }

        public string _p(string context, string text, params object[] args)
        {
            return provider._p(context, text, args);
        }

        public string _pn(string context, string text, string pluralText, long n)
        {
            return provider._pn(context, text, pluralText, n);
        }

        public string _pn(string context, string text, string pluralText, long n, params object[] args)
        {
            return provider._pn(context, text, pluralText, n, args);
        }

        public event Action OnLanguageChange
        {
            add
            {
                provider.OnLanguageChange += value;
            }
            remove
            {
                provider.OnLanguageChange -= value;
            }
        }
        #endregion
    }
}
