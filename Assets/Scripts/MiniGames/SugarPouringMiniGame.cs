using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 설탕 따르기 미니게임입니다.
    /// 마우스를 누르고 있으면 게이지가 채워지고, 목표 범위 내에 멈추면 성공합니다.
    /// </summary>
    public class SugarPouringMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("설탕이 따라지는 속도 (초당)")]
        [SerializeField] private float pouringSpeed = 0.3f;

        [Tooltip("목표 최소값 (0-1 범위)")]
        [SerializeField] private float targetMin = 0.8f;

        [Tooltip("목표 최대값 (0-1 범위)")]
        [SerializeField] private float targetMax = 1.0f;

        [Tooltip("제한 시간 (초)")]
        [SerializeField] private float timeLimit = 10f;

        [Header("UI References")]
        [SerializeField] private Image sugarFillImage;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Image targetZoneImage;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip pouringSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioSource audioSource;

        private Action<bool> _onComplete;
        private float _currentAmount;
        private float _remainingTime;
        private bool _isPlaying;
        private bool _isPouring;

        private void Awake()
        {
            // 버튼 이벤트 연결
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryButtonClick);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueButtonClick);
            }

            // AudioSource 자동 찾기
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _currentAmount = 0f;
            _remainingTime = timeLimit;
            _isPlaying = false;
            _isPouring = false;

            // UI 초기화
            UpdateUI();

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (instructionsText != null)
            {
                instructionsText.text = "마우스를 눌러 설탕을 따르세요!";
                instructionsText.gameObject.SetActive(true);
            }

            // 목표 영역 표시 업데이트
            UpdateTargetZone();

            Debug.Log("[SugarPouringMiniGame] 초기화 완료");
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);

            Debug.Log("[SugarPouringMiniGame] 게임 시작!");
        }

        public void EndGame(bool success)
        {
            _isPlaying = false;

            // 결과 패널 표시
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (instructionsText != null)
            {
                instructionsText.gameObject.SetActive(false);
            }

            // 결과 텍스트 및 버튼 설정
            if (success)
            {
                if (resultText != null)
                {
                    resultText.text = "성공! 완벽한 계량입니다!";
                    resultText.color = Color.green;
                }

                if (retryButton != null)
                {
                    retryButton.gameObject.SetActive(false);
                }

                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(true);
                }

                PlaySound(successSound);
            }
            else
            {
                if (resultText != null)
                {
                    string reason = _remainingTime <= 0 ? "시간 초과!" : 
                                   _currentAmount > targetMax ? "너무 많습니다!" : 
                                   "부족합니다!";
                    resultText.text = $"실패! {reason}";
                    resultText.color = Color.red;
                }

                if (retryButton != null)
                {
                    retryButton.gameObject.SetActive(true);
                }

                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(false);
                }

                PlaySound(failSound);
            }

            Debug.Log($"[SugarPouringMiniGame] 게임 종료 - {(success ? "성공" : "실패")}");

            // 성공 시 자동으로 콜백 호출
            if (success)
            {
                _onComplete?.Invoke(true);
            }
        }

        public void CleanUp()
        {
            _isPlaying = false;
            _isPouring = false;
            gameObject.SetActive(false);

            Debug.Log("[SugarPouringMiniGame] 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 입력 처리
            HandleInput();

            // 설탕 따르기
            if (_isPouring)
            {
                PourSugar();
            }

            // 타이머 업데이트
            UpdateTimer();

            // UI 업데이트
            UpdateUI();

            // 자동 종료 조건 체크
            CheckAutoEndConditions();
        }

        private void HandleInput()
        {
            // 마우스 왼쪽 버튼 또는 스페이스바로 따르기
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                if (!_isPouring)
                {
                    _isPouring = true;
                    PlaySound(pouringSound);
                }
            }
            else
            {
                if (_isPouring)
                {
                    _isPouring = false;
                    StopSound();
                }
            }

            // 사용자가 수동으로 Enter를 눌러 확인
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckSuccess();
            }
        }

        private void PourSugar()
        {
            if (_currentAmount >= 1f)
            {
                _currentAmount = 1f;
                _isPouring = false;
                StopSound();
                return;
            }

            _currentAmount += pouringSpeed * Time.deltaTime;
            _currentAmount = Mathf.Clamp01(_currentAmount);
        }

        private void UpdateTimer()
        {
            _remainingTime -= Time.deltaTime;

            if (_remainingTime < 0)
            {
                _remainingTime = 0;
            }
        }

        private void UpdateUI()
        {
            // Fill 이미지 업데이트
            if (sugarFillImage != null)
            {
                sugarFillImage.fillAmount = _currentAmount;

                // 색상 변경 (목표 범위에 따라)
                if (_currentAmount >= targetMin && _currentAmount <= targetMax)
                {
                    sugarFillImage.color = Color.green;
                }
                else if (_currentAmount > targetMax)
                {
                    sugarFillImage.color = Color.red;
                }
                else
                {
                    sugarFillImage.color = Color.white;
                }
            }

            // 타이머 텍스트 업데이트
            if (timerText != null)
            {
                timerText.text = $"남은 시간: {_remainingTime:F1}초";

                // 시간이 얼마 안 남으면 빨간색
                if (_remainingTime < 3f)
                {
                    timerText.color = Color.red;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }
        }

        private void UpdateTargetZone()
        {
            if (targetZoneImage != null)
            {
                // 목표 영역 표시 위치 및 크기 설정
                RectTransform rectTransform = targetZoneImage.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // 부모의 높이를 기준으로 목표 영역 설정
                    float parentHeight = (transform as RectTransform).rect.height;
                    float zoneHeight = parentHeight * (targetMax - targetMin);
                    float zoneY = parentHeight * (targetMin + (targetMax - targetMin) / 2f) - parentHeight / 2f;

                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, zoneHeight);
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, zoneY);
                }
            }
        }

        private void CheckAutoEndConditions()
        {
            // 시간 초과
            if (_remainingTime <= 0)
            {
                CheckSuccess();
            }

            // 100%를 초과한 경우 자동 실패
            if (_currentAmount >= 1f && !_isPouring)
            {
                CheckSuccess();
            }
        }

        private void CheckSuccess()
        {
            if (!_isPlaying)
                return;

            bool success = _currentAmount >= targetMin && _currentAmount <= targetMax;
            EndGame(success);
        }

        private void OnRetryButtonClick()
        {
            Debug.Log("[SugarPouringMiniGame] 다시 시도");

            // 게임 재시작
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log("[SugarPouringMiniGame] 계속하기");

            // 매니저에게 종료 알림
            MiniGameManager.Instance.EndMiniGame(true);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                if (clip == pouringSound)
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.clip = clip;
                        audioSource.loop = true;
                        audioSource.Play();
                    }
                }
                else
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }

        private void StopSound()
        {
            if (audioSource != null && audioSource.isPlaying && audioSource.clip == pouringSound)
            {
                audioSource.Stop();
            }
        }
    }
}

