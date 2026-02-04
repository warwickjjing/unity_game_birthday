using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 간단한 미니게임 기본 구현입니다.
    /// 다른 미니게임들의 기본 템플릿으로 사용할 수 있습니다.
    /// </summary>
    public class SimpleMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("게임 제한 시간 (초)")]
        [SerializeField] private float timeLimit = 10f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;

        private Action<bool> _onComplete;
        private float _remainingTime;
        private bool _isPlaying;
        private bool _isSuccess = false;

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _remainingTime = timeLimit;
            _isPlaying = false;
            _isSuccess = false;

            // UI 초기화
            if (timerText != null)
            {
                timerText.text = $"남은 시간: {_remainingTime:F1}초";
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (instructionsText != null)
            {
                instructionsText.gameObject.SetActive(true);
            }
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);
            Debug.Log($"[SimpleMiniGame] {gameObject.name} 게임 시작!");
        }

        public void EndGame(bool success)
        {
            _isPlaying = false;
            _isSuccess = success;

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
            if (resultText != null)
            {
                if (success)
                {
                    resultText.text = "성공!";
                    resultText.color = Color.green;
                }
                else
                {
                    resultText.text = "실패!";
                    resultText.color = Color.red;
                }
            }

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(!success);
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(success);
            }

            Debug.Log($"[SimpleMiniGame] {gameObject.name} 게임 종료 - {(success ? "성공" : "실패")}");

            // 성공 시 자동으로 콜백 호출
            if (success)
            {
                _onComplete?.Invoke(true);
            }
        }

        public void CleanUp()
        {
            _isPlaying = false;
            gameObject.SetActive(false);
            Debug.Log($"[SimpleMiniGame] {gameObject.name} 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 타이머 업데이트
            _remainingTime -= Time.deltaTime;

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

            // 시간 초과
            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                // 기본적으로 성공 처리 (자동 성공)
                EndGame(true);
            }
        }

        private void Awake()
        {
            // 버튼 이벤트 연결
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(OnRetryButtonClick);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinueButtonClick);
            }
        }

        private void OnRetryButtonClick()
        {
            Debug.Log($"[SimpleMiniGame] {gameObject.name} 다시 시도");
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log($"[SimpleMiniGame] {gameObject.name} 계속하기");
            MiniGameManager.Instance.EndMiniGame(true);
        }
    }
}

