using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BirthdayCakeQuest.Managers;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 슬라이드 데이터 (메시지 또는 가사)
    /// </summary>
    [System.Serializable]
    public class CreditSlide
    {
        [Tooltip("슬라이드 시작 시간 (초)")]
        public float startTime;
        
        [Tooltip("슬라이드 종료 시간 (초)")]
        public float endTime;
        
        [Tooltip("표시할 텍스트")]
        [TextArea(3, 10)]
        public string text;
        
        [Tooltip("텍스트 색상")]
        public Color textColor = Color.white;
        
        [Tooltip("폰트 크기 (0이면 기본값 사용)")]
        public float fontSize = 0f;
        
        [Tooltip("페이드 인 시간 (초)")]
        public float fadeInDuration = 0.5f;
        
        [Tooltip("페이드 아웃 시간 (초)")]
        public float fadeOutDuration = 0.5f;
        
        [Tooltip("슬라이드 타입 (메시지 또는 가사)")]
        public SlideType slideType = SlideType.Message;
    }

    public enum SlideType
    {
        Message,  // 메시지 (중앙/상단 표시)
        Lyrics    // 가사 (하단 표시)
    }

    /// <summary>
    /// 음악에 맞춰 슬라이드 형식으로 크레딧을 표시합니다.
    /// 메시지와 가사를 별도 영역에 표시할 수 있습니다.
    /// </summary>
    public sealed class CreditsSlidePlayer : MonoBehaviour
    {
        [Header("Music")]
        [Tooltip("엔딩 음악 (재생 시간 기준으로 슬라이드 동기화)")]
        [SerializeField] private AudioClip endingMusic;
        
        [Tooltip("음악 재생 시작 지연 시간 (초)")]
        [SerializeField] private float musicStartDelay = 0f;

        [Header("Slides")]
        [Tooltip("슬라이드 목록 (Inspector에서 설정)")]
        [SerializeField] private List<CreditSlide> slides = new List<CreditSlide>();

        [Header("UI References - Message Area")]
        [Tooltip("메시지 표시용 TextMeshPro (중앙/상단)")]
        [SerializeField] private TextMeshProUGUI messageText;
        
        [Tooltip("메시지 영역 배경 (선택)")]
        [SerializeField] private GameObject messageBackground;

        [Header("UI References - Lyrics Area")]
        [Tooltip("가사 표시용 TextMeshPro (하단)")]
        [SerializeField] private TextMeshProUGUI lyricsText;
        
        [Tooltip("가사 영역 배경 (선택)")]
        [SerializeField] private GameObject lyricsBackground;

        [Header("UI References - Common")]
        [Tooltip("종료 버튼 (크레딧 후 표시)")]
        [SerializeField] private GameObject returnButton;

        [Header("Settings")]
        [Tooltip("메시지 기본 폰트 크기")]
        [SerializeField] private float defaultMessageFontSize = 60f;
        
        [Tooltip("가사 기본 폰트 크기")]
        [SerializeField] private float defaultLyricsFontSize = 36f;

        private AudioSource _musicSource;
        private float _musicStartTime;
        private int _currentMessageIndex = -1;
        private int _currentLyricsIndex = -1;
        private bool _isPlaying = false;
        private Coroutine _slideRoutine;

        private void Awake()
        {
            if (returnButton != null)
            {
                returnButton.SetActive(false);
            }

            // 초기 상태: 모든 텍스트 숨김
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
            if (lyricsText != null)
            {
                lyricsText.gameObject.SetActive(false);
            }

            // AudioSource 찾기 또는 생성
            _musicSource = GetComponent<AudioSource>();
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }
            _musicSource.playOnAwake = false;
            _musicSource.loop = false;
        }

        /// <summary>
        /// 슬라이드가 있는지 확인합니다.
        /// </summary>
        public bool HasSlides()
        {
            return slides != null && slides.Count > 0;
        }

        /// <summary>
        /// 슬라이드 개수를 반환합니다.
        /// </summary>
        public int GetSlideCount()
        {
            return slides != null ? slides.Count : 0;
        }

        /// <summary>
        /// 슬라이드 재생을 시작합니다.
        /// </summary>
        public void StartSlides()
        {
            if (_isPlaying)
            {
                Debug.LogWarning("[CreditsSlidePlayer] Already playing!");
                return;
            }

            if (slides == null || slides.Count == 0)
            {
                Debug.LogWarning("[CreditsSlidePlayer] No slides to play!");
                return;
            }

            _isPlaying = true;
            _currentMessageIndex = -1;
            _currentLyricsIndex = -1;

            // 음악 재생
            if (endingMusic != null)
            {
                _musicSource.clip = endingMusic;
                _musicSource.PlayDelayed(musicStartDelay);
                _musicStartTime = Time.time + musicStartDelay;
            }
            else
            {
                _musicStartTime = Time.time;
            }

            // 슬라이드 재생 시작
            _slideRoutine = StartCoroutine(PlaySlidesRoutine());
        }

        private IEnumerator PlaySlidesRoutine()
        {
            // 슬라이드 정렬 (시작 시간 순)
            List<CreditSlide> sortedSlides = new List<CreditSlide>(slides);
            sortedSlides.Sort((a, b) => a.startTime.CompareTo(b.startTime));

            float musicElapsed = 0f;
            float totalDuration = endingMusic != null ? endingMusic.length : 
                (sortedSlides.Count > 0 ? sortedSlides[sortedSlides.Count - 1].endTime : 10f);

            while (musicElapsed < totalDuration)
            {
                musicElapsed = Time.time - _musicStartTime;

                // 현재 시간에 맞는 메시지 슬라이드 찾기
                int nextMessageIndex = -1;
                int nextLyricsIndex = -1;

                for (int i = 0; i < sortedSlides.Count; i++)
                {
                    var slide = sortedSlides[i];
                    if (musicElapsed >= slide.startTime && musicElapsed < slide.endTime)
                    {
                        if (slide.slideType == SlideType.Message)
                        {
                            nextMessageIndex = i;
                        }
                        else if (slide.slideType == SlideType.Lyrics)
                        {
                            nextLyricsIndex = i;
                        }
                    }
                }

                // 메시지 슬라이드 전환
                if (nextMessageIndex != _currentMessageIndex)
                {
                    if (_currentMessageIndex >= 0)
                    {
                        yield return StartCoroutine(FadeOutSlide(
                            sortedSlides[_currentMessageIndex], 
                            SlideType.Message
                        ));
                    }

                    _currentMessageIndex = nextMessageIndex;

                    if (_currentMessageIndex >= 0)
                    {
                        yield return StartCoroutine(FadeInSlide(
                            sortedSlides[_currentMessageIndex], 
                            SlideType.Message
                        ));
                    }
                }

                // 가사 슬라이드 전환
                if (nextLyricsIndex != _currentLyricsIndex)
                {
                    if (_currentLyricsIndex >= 0)
                    {
                        yield return StartCoroutine(FadeOutSlide(
                            sortedSlides[_currentLyricsIndex], 
                            SlideType.Lyrics
                        ));
                    }

                    _currentLyricsIndex = nextLyricsIndex;

                    if (_currentLyricsIndex >= 0)
                    {
                        yield return StartCoroutine(FadeInSlide(
                            sortedSlides[_currentLyricsIndex], 
                            SlideType.Lyrics
                        ));
                    }
                }

                yield return null;
            }

            // 마지막 슬라이드 페이드 아웃
            if (_currentMessageIndex >= 0)
            {
                yield return StartCoroutine(FadeOutSlide(
                    sortedSlides[_currentMessageIndex], 
                    SlideType.Message
                ));
            }
            if (_currentLyricsIndex >= 0)
            {
                yield return StartCoroutine(FadeOutSlide(
                    sortedSlides[_currentLyricsIndex], 
                    SlideType.Lyrics
                ));
            }

            // 종료 버튼 표시
            if (returnButton != null)
            {
                returnButton.SetActive(true);
            }

            _isPlaying = false;
            Debug.Log("[CreditsSlidePlayer] Slides complete!");
        }

        private IEnumerator FadeInSlide(CreditSlide slide, SlideType type)
        {
            TextMeshProUGUI targetText = type == SlideType.Message ? messageText : lyricsText;
            if (targetText == null) yield break;

            // 텍스트 설정
            targetText.text = slide.text;
            float fontSize = slide.fontSize > 0 ? slide.fontSize : 
                (type == SlideType.Message ? defaultMessageFontSize : defaultLyricsFontSize);
            targetText.fontSize = fontSize;
            targetText.color = new Color(slide.textColor.r, slide.textColor.g, slide.textColor.b, 0f);
            targetText.gameObject.SetActive(true);

            // 배경 표시 (있는 경우)
            GameObject background = type == SlideType.Message ? messageBackground : lyricsBackground;
            if (background != null)
            {
                background.SetActive(true);
            }

            // 페이드 인
            float elapsed = 0f;
            while (elapsed < slide.fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / slide.fadeInDuration);
                targetText.color = new Color(slide.textColor.r, slide.textColor.g, slide.textColor.b, alpha);
                yield return null;
            }

            targetText.color = slide.textColor;
        }

        private IEnumerator FadeOutSlide(CreditSlide slide, SlideType type)
        {
            TextMeshProUGUI targetText = type == SlideType.Message ? messageText : lyricsText;
            if (targetText == null) yield break;

            float startAlpha = targetText.color.a;
            float elapsed = 0f;

            while (elapsed < slide.fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / slide.fadeOutDuration);
                Color currentColor = targetText.color;
                targetText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                yield return null;
            }

            targetText.gameObject.SetActive(false);

            // 배경 숨김 (있는 경우)
            GameObject background = type == SlideType.Message ? messageBackground : lyricsBackground;
            if (background != null)
            {
                background.SetActive(false);
            }
        }

        /// <summary>
        /// 슬라이드를 정지합니다.
        /// </summary>
        public void StopSlides()
        {
            if (_slideRoutine != null)
            {
                StopCoroutine(_slideRoutine);
                _slideRoutine = null;
            }

            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.Stop();
            }

            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
            if (lyricsText != null)
            {
                lyricsText.gameObject.SetActive(false);
            }

            _isPlaying = false;
        }

        /// <summary>
        /// 슬라이드를 리셋합니다.
        /// </summary>
        public void ResetSlides()
        {
            StopSlides();
            _currentMessageIndex = -1;
            _currentLyricsIndex = -1;

            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
            if (lyricsText != null)
            {
                lyricsText.gameObject.SetActive(false);
            }

            if (returnButton != null)
            {
                returnButton.SetActive(false);
            }
        }
    }
}

