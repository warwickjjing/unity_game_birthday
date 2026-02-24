using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BirthdayCakeQuest.Managers;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 편지와 가사를 표시하는 크레딧 플레이어입니다.
    /// 편지는 ScrollView의 Content에, 가사는 하단에 별도로 표시합니다.
    /// </summary>
    public sealed class CreditsLetterPlayer : MonoBehaviour
    {
        [Header("Music")]
        [Tooltip("엔딩 음악 (재생 시간 기준으로 편지/가사 동기화)")]
        [SerializeField] private AudioClip endingMusic;
        
        [Tooltip("음악 재생 시작 지연 시간 (초)")]
        [SerializeField] private float musicStartDelay = 0f;

        [Header("Letter (ScrollView Content)")]
        [Tooltip("편지 텍스트 (Content 내부의 TextMeshProUGUI)")]
        [SerializeField] private TextMeshProUGUI letterText;
        
        [Tooltip("Content RectTransform (높이 자동 조정용)")]
        [SerializeField] private RectTransform letterContent;
        
        [Tooltip("편지 슬라이드 목록 (Inspector에서 설정)")]
        [SerializeField] private List<CreditSlide> letterSlides = new List<CreditSlide>();

        [Header("Lyrics (Bottom)")]
        [Tooltip("가사 표시용 TextMeshProUGUI (하단)")]
        [SerializeField] private TextMeshProUGUI lyricsText;
        
        [Tooltip("가사 영역 배경 (선택)")]
        [SerializeField] private GameObject lyricsBackground;
        
        [Tooltip("가사 슬라이드 목록 (Inspector에서 설정)")]
        [SerializeField] private List<CreditSlide> lyricsSlides = new List<CreditSlide>();

        [Header("UI References - Common")]
        [Tooltip("종료 버튼 (크레딧 후 표시)")]
        [SerializeField] private GameObject returnButton;

        [Header("Settings")]
        [Tooltip("편지 기본 폰트 크기")]
        [SerializeField] private float defaultLetterFontSize = 42f;
        
        [Tooltip("가사 기본 폰트 크기")]
        [SerializeField] private float defaultLyricsFontSize = 36f;

        [Tooltip("편지 페이드 인 시간 (초)")]
        [SerializeField] private float letterFadeInDuration = 1f;

        [Tooltip("편지 페이드 아웃 시간 (초)")]
        [SerializeField] private float letterFadeOutDuration = 1f;

        private AudioSource _musicSource;
        private float _musicStartTime;
        private int _currentLetterIndex = -1;
        private int _currentLyricsIndex = -1;
        private bool _isPlaying = false;
        private Coroutine _slideRoutine;
        private ContentSizeFitter _contentSizeFitter;

        private void Awake()
        {
            if (returnButton != null)
            {
                returnButton.SetActive(false);
            }

            // 초기 상태: 모든 텍스트 숨김
            if (letterText != null)
            {
                letterText.gameObject.SetActive(false);
            }
            if (lyricsText != null)
            {
                lyricsText.gameObject.SetActive(false);
            }

            // ContentSizeFitter 확인 및 추가
            if (letterContent != null)
            {
                _contentSizeFitter = letterContent.GetComponent<ContentSizeFitter>();
                if (_contentSizeFitter == null)
                {
                    _contentSizeFitter = letterContent.gameObject.AddComponent<ContentSizeFitter>();
                    _contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    Debug.Log("[CreditsLetterPlayer] ContentSizeFitter 추가됨");
                }
            }

            // LetterText에 ContentSizeFitter 추가 (없으면)
            if (letterText != null)
            {
                var textSizeFitter = letterText.GetComponent<ContentSizeFitter>();
                if (textSizeFitter == null)
                {
                    textSizeFitter = letterText.gameObject.AddComponent<ContentSizeFitter>();
                    textSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    textSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    Debug.Log("[CreditsLetterPlayer] LetterText에 ContentSizeFitter 추가됨");
                }
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
            int letterCount = letterSlides != null ? letterSlides.Count : 0;
            int lyricsCount = lyricsSlides != null ? lyricsSlides.Count : 0;
            return letterCount > 0 || lyricsCount > 0;
        }

        /// <summary>
        /// 슬라이드 개수를 반환합니다.
        /// </summary>
        public void GetSlideCounts(out int letterCount, out int lyricsCount)
        {
            letterCount = letterSlides != null ? letterSlides.Count : 0;
            lyricsCount = lyricsSlides != null ? lyricsSlides.Count : 0;
        }

        /// <summary>
        /// 편지와 가사 재생을 시작합니다.
        /// </summary>
        public void StartCredits()
        {
            if (_isPlaying)
            {
                Debug.LogWarning("[CreditsLetterPlayer] Already playing!");
                return;
            }

            if ((letterSlides == null || letterSlides.Count == 0) && 
                (lyricsSlides == null || lyricsSlides.Count == 0))
            {
                Debug.LogWarning("[CreditsLetterPlayer] No slides to play!");
                return;
            }

            _isPlaying = true;
            _currentLetterIndex = -1;
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
            _slideRoutine = StartCoroutine(PlayCreditsRoutine());
        }

        private IEnumerator PlayCreditsRoutine()
        {
            // 편지와 가사 슬라이드 정렬 (시작 시간 순)
            List<CreditSlide> allSlides = new List<CreditSlide>();
            if (letterSlides != null) allSlides.AddRange(letterSlides);
            if (lyricsSlides != null) allSlides.AddRange(lyricsSlides);
            allSlides.Sort((a, b) => a.startTime.CompareTo(b.startTime));

            float musicElapsed = 0f;
            float totalDuration = endingMusic != null ? endingMusic.length : 
                (allSlides.Count > 0 ? allSlides[allSlides.Count - 1].endTime : 10f);

            while (musicElapsed < totalDuration)
            {
                musicElapsed = Time.time - _musicStartTime;

                // 현재 시간에 맞는 편지 슬라이드 찾기
                int nextLetterIndex = -1;
                for (int i = 0; i < letterSlides.Count; i++)
                {
                    var slide = letterSlides[i];
                    if (musicElapsed >= slide.startTime && musicElapsed < slide.endTime)
                    {
                        nextLetterIndex = i;
                        break;
                    }
                }

                // 현재 시간에 맞는 가사 슬라이드 찾기
                int nextLyricsIndex = -1;
                for (int i = 0; i < lyricsSlides.Count; i++)
                {
                    var slide = lyricsSlides[i];
                    if (musicElapsed >= slide.startTime && musicElapsed < slide.endTime)
                    {
                        nextLyricsIndex = i;
                        break;
                    }
                }

                // 편지 슬라이드 전환
                if (nextLetterIndex != _currentLetterIndex)
                {
                    if (_currentLetterIndex >= 0)
                    {
                        yield return StartCoroutine(FadeOutLetter(letterSlides[_currentLetterIndex]));
                    }

                    _currentLetterIndex = nextLetterIndex;

                    if (_currentLetterIndex >= 0)
                    {
                        yield return StartCoroutine(FadeInLetter(letterSlides[_currentLetterIndex]));
                    }
                }

                // 가사 슬라이드 전환
                if (nextLyricsIndex != _currentLyricsIndex)
                {
                    if (_currentLyricsIndex >= 0)
                    {
                        yield return StartCoroutine(FadeOutLyrics(lyricsSlides[_currentLyricsIndex]));
                    }

                    _currentLyricsIndex = nextLyricsIndex;

                    if (_currentLyricsIndex >= 0)
                    {
                        yield return StartCoroutine(FadeInLyrics(lyricsSlides[_currentLyricsIndex]));
                    }
                }

                yield return null;
            }

            // 마지막 슬라이드 페이드 아웃
            if (_currentLetterIndex >= 0 && _currentLetterIndex < letterSlides.Count)
            {
                yield return StartCoroutine(FadeOutLetter(letterSlides[_currentLetterIndex]));
            }
            if (_currentLyricsIndex >= 0 && _currentLyricsIndex < lyricsSlides.Count)
            {
                yield return StartCoroutine(FadeOutLyrics(lyricsSlides[_currentLyricsIndex]));
            }

            // 종료 버튼 표시
            if (returnButton != null)
            {
                returnButton.SetActive(true);
            }

            _isPlaying = false;
            Debug.Log("[CreditsLetterPlayer] Credits complete!");
        }

        private IEnumerator FadeInLetter(CreditSlide slide)
        {
            if (letterText == null) yield break;

            // 텍스트 설정
            letterText.text = slide.text;
            float fontSize = slide.fontSize > 0 ? slide.fontSize : defaultLetterFontSize;
            letterText.fontSize = fontSize;
            
            // 페이드 인 시간이 0이면 즉시 표시
            float fadeDuration = slide.fadeInDuration > 0 ? slide.fadeInDuration : letterFadeInDuration;
            
            // 텍스트 활성화
            letterText.gameObject.SetActive(true);
            
            // Content 높이 자동 조정을 위해 여러 프레임 대기
            // ContentSizeFitter가 레이아웃을 업데이트할 시간을 줌
            yield return null; // 첫 프레임: 레이아웃 시스템이 텍스트 크기 계산
            yield return null; // 두 번째 프레임: ContentSizeFitter가 높이 업데이트
            
            // ContentSizeFitter 강제 업데이트
            if (_contentSizeFitter != null)
            {
                _contentSizeFitter.SetLayoutVertical();
                _contentSizeFitter.SetLayoutHorizontal();
            }
            
            // Content 높이가 여전히 0이면 수동으로 계산
            if (letterContent != null && letterContent.sizeDelta.y <= 0.1f)
            {
                // LetterText의 preferredHeight 사용
                float preferredHeight = letterText.preferredHeight;
                if (preferredHeight > 0)
                {
                    letterContent.sizeDelta = new Vector2(letterContent.sizeDelta.x, preferredHeight);
                    Debug.Log($"[CreditsLetterPlayer] Content 높이 수동 설정: {preferredHeight}");
                }
            }
            
            yield return null; // 한 프레임 더 대기
            
            if (fadeDuration <= 0f)
            {
                // 즉시 표시
                letterText.color = slide.textColor;
            }
            else
            {
                // 페이드 인
                letterText.color = new Color(slide.textColor.r, slide.textColor.g, slide.textColor.b, 0f);
                
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    letterText.color = new Color(slide.textColor.r, slide.textColor.g, slide.textColor.b, alpha);
                    yield return null;
                }
                
                letterText.color = slide.textColor;
            }
        }

        private IEnumerator FadeOutLetter(CreditSlide slide)
        {
            if (letterText == null) yield break;

            float startAlpha = letterText.color.a;
            float fadeDuration = slide.fadeOutDuration > 0 ? slide.fadeOutDuration : letterFadeOutDuration;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                Color currentColor = letterText.color;
                letterText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                yield return null;
            }

            letterText.gameObject.SetActive(false);
        }

        private IEnumerator FadeInLyrics(CreditSlide slide)
        {
            if (lyricsText == null) yield break;

            // 텍스트 설정
            lyricsText.text = slide.text;
            float fontSize = slide.fontSize > 0 ? slide.fontSize : defaultLyricsFontSize;
            lyricsText.fontSize = fontSize;
            
            // 페이드 인 시간이 0이면 즉시 표시
            float fadeDuration = slide.fadeInDuration > 0 ? slide.fadeInDuration : 0.5f; // 기본값
            
            if (fadeDuration <= 0f)
            {
                // 즉시 표시
                lyricsText.color = slide.textColor;
                lyricsText.gameObject.SetActive(true);
                
                // 배경 표시 (있는 경우)
                if (lyricsBackground != null)
                {
                    lyricsBackground.SetActive(true);
                }
            }
            else
            {
                // 페이드 인
                lyricsText.color = new Color(slide.textColor.r, slide.textColor.g, slide.textColor.b, 0f);
                lyricsText.gameObject.SetActive(true);
                
                // 배경 표시 (있는 경우)
                if (lyricsBackground != null)
                {
                    lyricsBackground.SetActive(true);
                }
                
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                    lyricsText.color = new Color(slide.textColor.r, slide.textColor.g, slide.textColor.b, alpha);
                    yield return null;
                }
                
                lyricsText.color = slide.textColor;
            }
        }

        private IEnumerator FadeOutLyrics(CreditSlide slide)
        {
            if (lyricsText == null) yield break;

            float startAlpha = lyricsText.color.a;
            float elapsed = 0f;

            while (elapsed < slide.fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / slide.fadeOutDuration);
                Color currentColor = lyricsText.color;
                lyricsText.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                yield return null;
            }

            lyricsText.gameObject.SetActive(false);

            // 배경 숨김 (있는 경우)
            if (lyricsBackground != null)
            {
                lyricsBackground.SetActive(false);
            }
        }

        /// <summary>
        /// 크레딧을 정지합니다.
        /// </summary>
        public void StopCredits()
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

            if (letterText != null)
            {
                letterText.gameObject.SetActive(false);
            }
            if (lyricsText != null)
            {
                lyricsText.gameObject.SetActive(false);
            }

            _isPlaying = false;
        }

        /// <summary>
        /// 크레딧을 리셋합니다.
        /// </summary>
        public void ResetCredits()
        {
            StopCredits();
            _currentLetterIndex = -1;
            _currentLyricsIndex = -1;

            if (letterText != null)
            {
                letterText.gameObject.SetActive(false);
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

