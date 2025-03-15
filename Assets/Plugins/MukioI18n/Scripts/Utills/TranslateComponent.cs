using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

namespace MukioI18n
{
    public interface ITranslateComponent
    {
        string Key { get; }

        string Context { get; }

        IEnumerable<string> Keys { get; }

        string Comment { get; }

        string Path { get; }
    }

    public interface ITranslationProvider : ITranslator
    {
        [Obsolete]
        I18n i18n { get; }

        event Action OnLanguageChange;
    }

    public abstract class TranslateComponent<T> : MonoBehaviour, ITranslateComponent where T : MonoBehaviour
    {
        T _component = null;
        public T Component
        {
            get
            {
                if (_component == null)
                    _component = GetComponent<T>();
                return _component;
            }
        }

        ITranslationProvider _provider;

        public ITranslationProvider provider
        {
            get
            {
                // 在父物体或场景根寻找翻译提供者
                if (_provider == null)
                {
                    _provider = GetComponentInParent<ITranslationProvider>();
                    if (_provider == null)
                    {
                        var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                        foreach (var obj in rootObjs)
                        {
                            _provider = obj.GetComponent<ITranslationProvider>();
                            if (_provider != null) break;
                        }
                    }
                }
                return _provider;
            }
        }

        string _key = null;
        public string Key
        {
            get
            {
                if (_key == null)
                    _key = getKeyInner();
                return _key;
            }
        }

        public IEnumerable<string> Keys
        {
            get
            {
                return getKeysInner();
            }
        }

        abstract protected string getKeyInner();

        virtual protected IEnumerable<string> getKeysInner()
        {
            return null;
        }

        string _comment;
        public string Comment
        {
            get
            {
                if (_comment == null)
                    _comment = getCommentInner();
                return _comment;
            }
        }

        abstract protected string getCommentInner();

        string _context;
        public string Context
        {
            get
            {
                if (_context == null)
                    _context = getContextInner();
                return _context;
            }
        }

        abstract protected string getContextInner();

        public abstract void Translate();

        private void Awake()
        {
            _key = getKeyInner();
            provider.OnLanguageChange += tryTranslate;
        }

        void OnDestroy()
        {
            provider.OnLanguageChange -= tryTranslate;
        }

        void tryTranslate()
        {
            if (gameObject != null)
                Translate();
        }

        private void Start()
        {
            Translate();
        }

        public virtual string Path
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                Transform tr = transform;
                do
                {
                    sb.Insert(0, tr.name);
                    sb.Insert(0, "/");
                } while (tr = tr.parent);

                return sb.ToString();
            }
        }
    }
}
