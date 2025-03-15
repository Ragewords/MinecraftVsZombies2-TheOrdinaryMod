using System;
using System.Linq;
using MVZ2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVZ2.MusicRoom
{
    public class MusicRoomUI : MonoBehaviour
    {
        public void UpdateList(string[] items)
        {
            itemList.updateList(items.Length, (i, obj) =>
            {
                var item = obj.GetComponent<MusicRoomListItem>();
                item.UpdateName(items[i]);
            },
            obj =>
            {
                var item = obj.GetComponent<MusicRoomListItem>();
                item.OnClick += OnItemClickCallback;
            },
            obj =>
            {
                var item = obj.GetComponent<MusicRoomListItem>();
                item.OnClick -= OnItemClickCallback;
            });
        }
        public void UpdateInformation(string name, string info, string description)
        {
            nameText.text = name;
            informationText.text = info;
            descriptionText.text = description;
        }
        public void SetPlaying(bool playing) 
        {
            playButtonObj.SetActive(!playing);
            pauseButtonObj.SetActive(playing);
        }
        public void SetMusicTime(float time)
        {
            barSlider.SetValueWithoutNotify(time);
        }
        public void SetSelectedItem(int index)
        {
            var count = itemList.Count;
            for (int i = 0; i < count; i++)
            {
                var item = itemList.getElement<MusicRoomListItem>(i);
                item.SetSelected(index == i);
            }
        }
        private void Awake()
        {
            returnButton.onClick.AddListener(() => OnReturnClick?.Invoke());
            playButton.onClick.AddListener(() => OnPlayButtonClick?.Invoke());
            pauseButton.onClick.AddListener(() => OnPauseButtonClick?.Invoke());
            barSlider.onValueChanged.AddListener(value => OnMusicBarDrag?.Invoke(value));
        }
        private void OnItemClickCallback(MusicRoomListItem item)
        {
            OnMusicItemClick?.Invoke(itemList.indexOf(item));
        }
        public event Action OnReturnClick;
        public event Action OnPlayButtonClick;
        public event Action OnPauseButtonClick;
        public event Action<int> OnMusicItemClick;
        public event Action<float> OnMusicBarDrag;

        [SerializeField]
        private ElementList itemList;
        [SerializeField]
        private Button returnButton;
        [SerializeField]
        private TextMeshProUGUI nameText;
        [SerializeField]
        private TextMeshProUGUI informationText;
        [SerializeField]
        private TextMeshProUGUI descriptionText;
        [SerializeField]
        private Button playButton;
        [SerializeField]
        private Button pauseButton;
        [SerializeField]
        private GameObject playButtonObj;
        [SerializeField]
        private GameObject pauseButtonObj;
        [SerializeField]
        private Slider barSlider;
    }
}
