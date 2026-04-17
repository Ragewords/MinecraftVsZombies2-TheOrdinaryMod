#nullable enable

using System;
using System.Collections.Generic;

namespace PVZEngine.Modifiers
{
    public class ModifierLibrary
    {
        public ModifierLibrary()
        {
        }
        #region ÊôÐÔ
        public IEnumerable<IPropertyKey> GetModifyPropertyKeys()
        {
            return modifierCachesForProperty.Keys;
        }
        public void GetModifierItemsForProperty(IPropertyKey name, List<ModifierSourceItem> results)
        {
            if (!modifierCachesForProperty.TryGetValue(name, out var list))
                return;
            noStackModifierBuffer.Clear();
            foreach (var element in list)
            {
                var modifier = element.modifier;
                if (modifier.NoStack)
                {
                    if (noStackModifierBuffer.Contains(modifier))
                    {
                        continue;
                    }
                    else
                    {
                        noStackModifierBuffer.Add(modifier);
                    }
                }
                results.Add(element);
            }
        }
        #endregion

        #region ÐÞ¸ÄÆ÷»º´æ
        public void AddModifierCaches(IEnumerable<ModifierSourceItem> modifiers)
        {
            foreach (var item in modifiers)
            {
                var modifier = item.modifier;
                var modifyName = modifier.PropertyName;
                var usingName = modifier.UsingContainerPropertyName;
                if (!modifierCachesForProperty.TryGetValue(modifyName, out var list))
                {
                    list = new List<ModifierSourceItem>();
                    modifierCachesForProperty.Add(modifyName, list);
                }
                list.Add(item);

                if (!modifierCachesUsingProperty.TryGetValue(usingName, out var usingList))
                {
                    usingList = new List<ModifierSourceItem>();
                    modifierCachesUsingProperty.Add(usingName, usingList);
                }
                usingList.Add(item);

                CallModifiedPropertyChanged(modifyName);
            }
        }
        public void RemoveModifierCaches(IEnumerable<ModifierSourceItem> modifiers)
        {
            foreach (var item in modifiers)
            {
                var modifier = item.modifier;
                var modifyName = modifier.PropertyName;
                if (modifierCachesForProperty.TryGetValue(modifyName, out var list))
                {
                    list.Remove(item);
                }

                var usingName = modifier.UsingContainerPropertyName;
                if (modifierCachesUsingProperty.TryGetValue(modifyName, out var usingList))
                {
                    usingList.Remove(item);
                }

                CallModifiedPropertyChanged(modifyName);
            }
        }
        public void ClearModifierCaches()
        {
            foreach (var pair in modifierCachesForProperty)
            {
                var modifyName = pair.Key;
                if (modifierCachesForProperty.TryGetValue(modifyName, out var list))
                {
                    list.Clear();
                }
                CallModifiedPropertyChanged(modifyName);
            }
            foreach (var pair in modifierCachesUsingProperty)
            {
                var usingName = pair.Key;
                if (modifierCachesUsingProperty.TryGetValue(usingName, out var list))
                {
                    list.Clear();
                }
            }
            modifierCachesForProperty.Clear();
            modifierCachesUsingProperty.Clear();
        }
        #endregion

        public void CallPropertyChanged(IModifierSource source, IPropertyKey key)
        {
            if (!modifierCachesUsingProperty.TryGetValue(key, out var list))
                return;
            foreach (var item in list)
            {
                if (item.container != source)
                    continue;
                var modifier = item.modifier;
                if (key.Equals(modifier.UsingContainerPropertyName))
                {
                    CallModifiedPropertyChanged(modifier.PropertyName);
                }
            }
        }
        public void CallModifiedPropertyChanged(IPropertyKey key)
        {
            OnModifiedPropertyNeedsUpdate?.Invoke(key);
        }
        public event Action<IPropertyKey>? OnModifiedPropertyNeedsUpdate;

        #region ÊôÐÔ×Ö¶Î
        private Dictionary<IPropertyKey, List<ModifierSourceItem>> modifierCachesForProperty = new Dictionary<IPropertyKey, List<ModifierSourceItem>>(new PropertyKeyComparer());
        private Dictionary<IPropertyKey, List<ModifierSourceItem>> modifierCachesUsingProperty = new Dictionary<IPropertyKey, List<ModifierSourceItem>>(new PropertyKeyComparer());
        private static HashSet<PropertyModifier> noStackModifierBuffer = new HashSet<PropertyModifier>();
        #endregion
    }
}