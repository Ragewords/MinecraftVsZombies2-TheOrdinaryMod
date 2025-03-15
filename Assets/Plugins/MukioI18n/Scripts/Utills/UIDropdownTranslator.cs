using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MukioI18n
{
    [RequireComponent(typeof(Dropdown))]
    public class UIDropdownTranslator : TranslateComponent<Dropdown>
    {
        [SerializeField]
        string comment;

        [SerializeField]
        string context;

        List<string> keys = new List<string>();
        public override void Translate()
        {
            if (Component == null)
                return;

            if (keys.Count == 0)
            {
                getKeysInner();
            }

            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            if (string.IsNullOrEmpty(Context))
            {
                foreach (var key in keys)
                {
                    options.Add(new Dropdown.OptionData(provider._(key)));
                }
            }
            else
            {
                foreach (var key in keys)
                {
                    options.Add(new Dropdown.OptionData(provider._p(Context, key)));
                }
            }

            Component.options = options;
            Component.RefreshShownValue();
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
            return null;
        }

        protected override IEnumerable<string> getKeysInner()
        {
            foreach (var option in Component.options)
            {
                keys.Add(option.text);
            }
            return keys;
        }
    }
}
