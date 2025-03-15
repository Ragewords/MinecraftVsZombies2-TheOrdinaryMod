using System;
using MVZ2.Models;
using MVZ2.UI;
using UnityEngine;

namespace MVZ2.Almanacs
{
    public class AlmanacUI : MonoBehaviour
    {
        public void DisplayPage(AlmanacPage page)
        {
            indexUI.SetActive(page == AlmanacPage.Index);
            standaloneContraptions.SetActive(page == AlmanacPage.ContraptionsStandalone);
            mobileContraptions.SetActive(page == AlmanacPage.ContraptionsMobile);
            enemies.SetActive(page == AlmanacPage.Enemies);
            artifacts.SetActive(page == AlmanacPage.Artifacts);
            miscs.SetActive(page == AlmanacPage.Miscs);
        }
        public void SetIndexArtifactVisible(bool visible)
        {
            indexUI.SetArtifactVisible(visible);
        }
        public void SetContraptionEntries(ChoosingBlueprintViewData[] entries, bool commandBlockVisible)
        {
            standaloneContraptions.SetEntries(entries, commandBlockVisible);
            mobileContraptions.SetEntries(entries, commandBlockVisible);
        }
        public void SetEnemyEntries(AlmanacEntryViewData[] entries)
        {
            enemies.SetEntries(entries);
        }
        public void SetArtifactEntries(AlmanacEntryViewData[] entries)
        {
            artifacts.SetEntries(entries);
        }
        public void SetMiscGroups(AlmanacEntryGroupViewData[] groups)
        {
            miscs.SetGroups(groups);
        }
        public void SetActiveContraptionEntry(Model prefab, Camera camera, string name, string description, string cost, string recharge)
        {
            standaloneContraptions.SetActiveEntry(prefab, camera, name, description, cost, recharge);
            mobileContraptions.SetActiveEntry(prefab, camera, name, description, cost, recharge);
        }
        public void SetActiveEnemyEntry(Model prefab, Camera camera, string name, string description)
        {
            enemies.SetActiveEntry(prefab, camera, name, description);
        }
        public void SetActiveArtifactEntry(Sprite sprite, string name, string description)
        {
            artifacts.SetActiveEntry(sprite, name, description);
        }
        public void SetActiveMiscEntry(Sprite sprite, string name, string description)
        {
            miscs.SetActiveEntry(sprite, name, description);
        }
        public void SetActiveMiscEntry(Model prefab, Camera camera, string name, string description)
        {
            miscs.SetActiveEntry(prefab, camera, name, description);
        }
        private void Awake()
        {
            indexUI.OnButtonClick += type => OnIndexButtonClick?.Invoke(type);
            standaloneContraptions.OnEntryClick += index => OnContraptionEntryClick?.Invoke(index);
            mobileContraptions.OnEntryClick += index => OnContraptionEntryClick?.Invoke(index);
            enemies.OnEntryClick += index => OnEnemyEntryClick?.Invoke(index);
            artifacts.OnEntryClick += index => OnArtifactEntryClick?.Invoke(index);
            miscs.OnGroupEntryClick += (groupIndex, entryIndex) => OnMiscGroupEntryClick?.Invoke(groupIndex, entryIndex);

            indexUI.OnReturnClick += () => OnIndexReturnClick?.Invoke();
            standaloneContraptions.OnReturnClick += () => OnPageReturnClick?.Invoke();
            mobileContraptions.OnReturnClick += () => OnPageReturnClick?.Invoke();
            enemies.OnReturnClick += () => OnPageReturnClick?.Invoke();
            artifacts.OnReturnClick += () => OnPageReturnClick?.Invoke();
            miscs.OnReturnClick += () => OnPageReturnClick?.Invoke();
        }
        public event Action OnIndexReturnClick;
        public event Action OnPageReturnClick;
        public event Action<IndexAlmanacPage.ButtonType> OnIndexButtonClick;
        public event Action<int> OnContraptionEntryClick;
        public event Action<int> OnEnemyEntryClick;
        public event Action<int> OnArtifactEntryClick;
        public event Action<int, int> OnMiscGroupEntryClick;
        [SerializeField]
        private IndexAlmanacPage indexUI;
        [SerializeField]
        private ContraptionAlmanacPage standaloneContraptions;
        [SerializeField]
        private ContraptionAlmanacPage mobileContraptions;
        [SerializeField]
        private MiscAlmanacPage enemies;
        [SerializeField]
        private MiscAlmanacPage artifacts;
        [SerializeField]
        private MiscAlmanacPage miscs;
        public enum AlmanacPage
        {
            Index,
            ContraptionsStandalone,
            ContraptionsMobile,
            Enemies,
            Artifacts,
            Miscs
        }
    }
}
