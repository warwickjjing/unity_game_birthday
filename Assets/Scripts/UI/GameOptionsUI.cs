using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BirthdayCakeQuest.Managers;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 옵션(설정) 패널: BGM/효과음 볼륨, 전체화면. 타이틀 또는 일시정지 메뉴에서 열 수 있습니다.
    /// </summary>
    public class GameOptionsUI : MonoBehaviour
    {
        [Header("Volume")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TextMeshProUGUI bgmLabel;
        [SerializeField] private TextMeshProUGUI sfxLabel;

        [Header("Display")]
        [SerializeField] private Toggle fullscreenToggle;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        private const string PrefsBGMVolume = "BGMVolume";
        private const string PrefsSFXVolume = "SFXVolume";
        private const string PrefsFullscreen = "Fullscreen";

        private void OnEnable()
        {
            LoadAndApplyOptions();

            // BGM이 아예 안 들리면 슬라이더로 변화를 못 느끼므로, 옵션 열 때 재생 중이 아니면 한 번 재생
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.EnsureBGMPlayingForOptionsPreview();

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveAllListeners();
                bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
            }
            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void OnDisable()
        {
            if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
        }

        /// <summary>
        /// PlayerPrefs에서 값을 읽어 UI에 반영하고 SceneLoader에 적용합니다.
        /// </summary>
        public void LoadAndApplyOptions()
        {
            float bgm = 0.5f;
            if (PlayerPrefs.HasKey(PrefsBGMVolume))
                bgm = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsBGMVolume));

            if (bgmSlider != null)
            {
                bgmSlider.SetValueWithoutNotify(bgm);
                UpdateLabel(bgmLabel, "BGM", bgm);
            }

            if (SceneLoader.Instance != null)
                SceneLoader.Instance.SetMusicVolume(bgm);

            if (sfxSlider != null)
            {
                float sfx = 0.5f;
                if (PlayerPrefs.HasKey(PrefsSFXVolume))
                    sfx = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefsSFXVolume));
                sfxSlider.SetValueWithoutNotify(sfx);
                UpdateLabel(sfxLabel, "효과음", sfx);
            }

            if (fullscreenToggle != null)
            {
                bool full = Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
                if (PlayerPrefs.HasKey(PrefsFullscreen))
                    full = PlayerPrefs.GetInt(PrefsFullscreen, full ? 1 : 0) != 0;
                fullscreenToggle.SetIsOnWithoutNotify(full);
            }
        }

        private void OnBGMVolumeChanged(float value)
        {
            value = Mathf.Clamp01(value);
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.SetMusicVolume(value);
            PlayerPrefs.SetFloat(PrefsBGMVolume, value);
            PlayerPrefs.Save();
            UpdateLabel(bgmLabel, "BGM", value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            value = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(PrefsSFXVolume, value);
            PlayerPrefs.Save();
            UpdateLabel(sfxLabel, "효과음", value);
            // 효과음은 추후 AudioMixer 또는 전역 SFX 볼륨 적용 시 사용
        }

        private void OnFullscreenChanged(bool isFull)
        {
            Screen.fullScreen = isFull;
            PlayerPrefs.SetInt(PrefsFullscreen, isFull ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void UpdateLabel(TextMeshProUGUI label, string prefix, float value)
        {
            if (label != null)
                label.text = prefix + ": " + Mathf.RoundToInt(value * 100f) + "%";
        }

        /// <summary>
        /// 패널을 표시합니다.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 패널을 숨깁니다.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
