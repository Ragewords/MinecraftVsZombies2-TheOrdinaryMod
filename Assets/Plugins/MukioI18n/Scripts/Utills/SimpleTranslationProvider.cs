using UnityEngine;

using System.IO;
using System.Globalization;
using System;

namespace MukioI18n
{
    /// <summary>
    /// 简易的翻译提供器
    /// </summary>
    public class SimpleTranslationProvider : MonoBehaviour, ITranslationProvider
    {
        /// <summary>
        /// 默认载入路径
        /// </summary>
        [SerializeField]
        string folderPath = "i18n";

        /// <summary>
        /// 载入的语言
        /// </summary>
        [SerializeField]
        string[] langs = { "zh-CN", "en-US" };

        /// <summary>
        /// 默认语言
        /// </summary>
        [SerializeField]
        string defaultLang = "zh-CN";

        public I18n i18n { get; } = new I18n();

        void Awake()
        {
            foreach (var lang in langs)
            {
                i18n.Load(folderPath, new CultureInfo(lang));
            }
            i18n.SetLang(new CultureInfo(defaultLang));
        }

        #region Translation
        public string _(string text)
        {
            return i18n._(text);
        }

        public string _(string text, params object[] args)
        {
            return i18n._(text, args);
        }

        public string _n(string text, string pluralText, long n)
        {
            return i18n._n(text, pluralText, n);
        }

        public string _n(string text, string pluralText, long n, params object[] args)
        {
            return i18n._n(text, pluralText, n, args);
        }

        public string _p(string context, string text)
        {
            return i18n._p(context, text);
        }

        public string _p(string context, string text, params object[] args)
        {
            return i18n._p(context, text, args);
        }

        public string _pn(string context, string text, string pluralText, long n)
        {
            return i18n._pn(context, text, pluralText, n);
        }

        public string _pn(string context, string text, string pluralText, long n, params object[] args)
        {
            return i18n._pn(context, text, pluralText, n, args);
        }

        public event Action OnLanguageChange
        {
            add
            {
                i18n.OnLanguageChange += value;
            }
            remove
            {
                i18n.OnLanguageChange -= value;
            }
        }
        #endregion
    }
}
