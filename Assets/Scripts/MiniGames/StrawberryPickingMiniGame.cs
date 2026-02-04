using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 딸기 따기 타이밍 미니게임입니다.
    /// 좋은 딸기를 클릭하여 수집하고, 상한 딸기는 피해야 합니다.
    /// </summary>
    public class StrawberryPickingMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("게임 제한 시간 (초)")]
        [SerializeField] private float timeLimit = 30f;

        [Tooltip("목표 수집 개수")]
        [SerializeField] private int targetCount = 5;

        [Tooltip("딸기 생성 간격 (초)")]
        [SerializeField] private float spawnInterval = 1.5f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Transform strawberryContainer; // 딸기들이 생성될 컨테이너

        private Action<bool> _onComplete;
        private float _remainingTime;
        private int _collectedCount = 0;
        private bool _isPlaying;
        private float _spawnTimer = 0f;

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _remainingTime = timeLimit;
            _collectedCount = 0;
            _isPlaying = false;
            _spawnTimer = 0f;

            // 기존 딸기 제거
            if (strawberryContainer != null)
            {
                foreach (Transform child in strawberryContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // UI 초기화
            UpdateUI();

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (instructionsText != null)
            {
                instructionsText.text = "좋은 딸기를 클릭하세요! 상한 딸기는 피하세요!";
                instructionsText.gameObject.SetActive(true);
            }
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);
            Debug.Log("[StrawberryPickingMiniGame] 게임 시작!");
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
            if (resultText != null)
            {
                if (success)
                {
                    resultText.text = $"성공! {_collectedCount}개의 딸기를 수집했습니다!";
                    resultText.color = Color.green;
                }
                else
                {
                    string reason = _remainingTime <= 0 ? "시간 초과!" : "목표 개수에 도달하지 못했습니다!";
                    resultText.text = $"실패! {reason}";
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

            Debug.Log($"[StrawberryPickingMiniGame] 게임 종료 - {(success ? "성공" : "실패")}");

            // 성공 시 자동으로 콜백 호출
            if (success)
            {
                _onComplete?.Invoke(true);
            }
        }

        public void CleanUp()
        {
            _isPlaying = false;
            
            // 모든 딸기 제거
            if (strawberryContainer != null)
            {
                foreach (Transform child in strawberryContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            gameObject.SetActive(false);
            Debug.Log("[StrawberryPickingMiniGame] 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 타이머 업데이트
            _remainingTime -= Time.deltaTime;

            // 딸기 생성
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnStrawberry();
            }

            // UI 업데이트
            UpdateUI();

            // 시간 초과 또는 목표 달성 체크
            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                bool success = _collectedCount >= targetCount;
                EndGame(success);
            }
            else if (_collectedCount >= targetCount)
            {
                EndGame(true);
            }
        }

        private void UpdateUI()
        {
            if (timerText != null)
            {
                timerText.text = $"남은 시간: {_remainingTime:F1}초";

                if (_remainingTime < 5f)
                {
                    timerText.color = Color.red;
                }
                else
                {
                    timerText.color = Color.white;
                }
            }

            if (countText != null)
            {
                countText.text = $"수집: {_collectedCount} / {targetCount}";
            }
        }

        private void SpawnStrawberry()
        {
            if (strawberryContainer == null)
                return;

            // 딸기 버튼 생성
            GameObject strawberryObj = CreateButton($"Strawberry_{Time.time}", strawberryContainer);
            RectTransform rect = strawberryObj.GetComponent<RectTransform>();
            
            // 랜덤 위치
            float x = UnityEngine.Random.Range(-400f, 400f);
            float y = UnityEngine.Random.Range(-200f, 200f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(80, 80);

            // 좋은 딸기 또는 상한 딸기 (70% 확률로 좋은 딸기)
            bool isGood = UnityEngine.Random.value > 0.3f;
            
            Image buttonImage = strawberryObj.GetComponent<Image>();
            buttonImage.color = isGood ? new Color(1f, 0.3f, 0.3f) : new Color(0.5f, 0.5f, 0.5f); // 빨간색 또는 회색

            TextMeshProUGUI buttonText = strawberryObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isGood ? "🍓" : "💀";
                buttonText.fontSize = 40;
            }

            Button button = strawberryObj.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnStrawberryClicked(strawberryObj, isGood));

            // 3초 후 자동 제거
            Destroy(strawberryObj, 3f);
        }

        private void OnStrawberryClicked(GameObject strawberry, bool isGood)
        {
            if (!_isPlaying)
                return;

            if (isGood)
            {
                _collectedCount++;
                Debug.Log($"[StrawberryPickingMiniGame] 좋은 딸기 수집! ({_collectedCount}/{targetCount})");
            }
            else
            {
                // 상한 딸기를 클릭하면 실패
                Debug.Log("[StrawberryPickingMiniGame] 상한 딸기를 클릭했습니다!");
                EndGame(false);
                return;
            }

            Destroy(strawberry);
        }

        private GameObject CreateButton(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(80, 80);

            Image buttonImage = obj.AddComponent<Image>();
            buttonImage.color = new Color(1f, 0.3f, 0.3f);
            MiniGameUIFactory.SetDefaultSprite(buttonImage);

            Button button = obj.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Button Text 생성
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "🍓";
            buttonText.fontSize = 40;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            if (TMP_Settings.defaultFontAsset != null)
            {
                buttonText.font = TMP_Settings.defaultFontAsset;
            }

            return obj;
        }

        private void Awake()
        {
            // Container가 없으면 생성
            if (strawberryContainer == null)
            {
                GameObject containerObj = new GameObject("StrawberryContainer");
                containerObj.transform.SetParent(transform, false);
                RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.sizeDelta = Vector2.zero;
                containerRect.anchoredPosition = Vector2.zero;
                strawberryContainer = containerObj.transform;
            }

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
            Debug.Log("[StrawberryPickingMiniGame] 다시 시도");
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log("[StrawberryPickingMiniGame] 계속하기");
            MiniGameManager.Instance.EndMiniGame(true);
        }
    }
}

