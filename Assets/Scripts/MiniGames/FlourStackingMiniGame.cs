using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 포대 쌓기 미니게임입니다.
    /// 떨어지는 밀가루 포대를 좌우로 움직여 받아내고 쌓습니다.
    /// </summary>
    public class FlourStackingMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("게임 제한 시간 (초)")]
        [SerializeField] private float timeLimit = 30f;

        [Tooltip("목표 쌓기 개수")]
        [SerializeField] private int targetCount = 10;

        [Tooltip("포대 생성 간격 (초)")]
        [SerializeField] private float spawnInterval = 2f;

        [Tooltip("포대 낙하 속도")]
        [SerializeField] private float fallSpeed = 200f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Image catcherImage; // 포대를 받는 캐처
        [SerializeField] private Transform bagContainer; // 포대들이 생성될 컨테이너

        private Action<bool> _onComplete;
        private float _remainingTime;
        private int _stackedCount = 0;
        private bool _isPlaying;
        private float _spawnTimer = 0f;
        private float _catcherPosition = 0f; // 캐처 X 위치 (-400 ~ 400)

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _remainingTime = timeLimit;
            _stackedCount = 0;
            _isPlaying = false;
            _spawnTimer = 0f;
            _catcherPosition = 0f;

            // 기존 포대 제거
            if (bagContainer != null)
            {
                foreach (Transform child in bagContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // 캐처 초기 위치
            if (catcherImage != null)
            {
                RectTransform catcherRect = catcherImage.GetComponent<RectTransform>();
                catcherRect.anchoredPosition = new Vector2(0, -300);
            }

            // UI 초기화
            UpdateUI();

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (instructionsText != null)
            {
                instructionsText.text = "좌우 화살표 키로 포대를 받아내세요!";
                instructionsText.gameObject.SetActive(true);
            }
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);
            Debug.Log("[FlourStackingMiniGame] 게임 시작!");
        }

        public void EndGame(bool success)
        {
            _isPlaying = false;

            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (instructionsText != null)
            {
                instructionsText.gameObject.SetActive(false);
            }

            if (resultText != null)
            {
                if (success)
                {
                    resultText.text = $"성공! {_stackedCount}개의 포대를 쌓았습니다!";
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

            Debug.Log($"[FlourStackingMiniGame] 게임 종료 - {(success ? "성공" : "실패")}");

            if (success)
            {
                _onComplete?.Invoke(true);
            }
        }

        public void CleanUp()
        {
            _isPlaying = false;

            if (bagContainer != null)
            {
                foreach (Transform child in bagContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            gameObject.SetActive(false);
            Debug.Log("[FlourStackingMiniGame] 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 타이머 업데이트
            _remainingTime -= Time.deltaTime;

            // 캐처 이동 (좌우 화살표 키)
            float moveSpeed = 300f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                _catcherPosition -= moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                _catcherPosition += moveSpeed * Time.deltaTime;
            }
            _catcherPosition = Mathf.Clamp(_catcherPosition, -400f, 400f);

            if (catcherImage != null)
            {
                RectTransform catcherRect = catcherImage.GetComponent<RectTransform>();
                catcherRect.anchoredPosition = new Vector2(_catcherPosition, -300);
            }

            // 포대 생성
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnBag();
            }

            // 포대 낙하 처리
            UpdateBags();

            // UI 업데이트
            UpdateUI();

            // 시간 초과 또는 목표 달성 체크
            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                bool success = _stackedCount >= targetCount;
                EndGame(success);
            }
            else if (_stackedCount >= targetCount)
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
                countText.text = $"쌓은 포대: {_stackedCount} / {targetCount}";
            }
        }

        private void SpawnBag()
        {
            if (bagContainer == null)
                return;

            GameObject bagObj = new GameObject($"Bag_{Time.time}");
            bagObj.transform.SetParent(bagContainer, false);

            RectTransform rect = bagObj.AddComponent<RectTransform>();
            float x = UnityEngine.Random.Range(-400f, 400f);
            rect.anchoredPosition = new Vector2(x, 400);
            rect.sizeDelta = new Vector2(100, 100);

            Image bagImage = bagObj.AddComponent<Image>();
            bagImage.color = new Color(0.9f, 0.9f, 0.8f); // 밀가루 색상
            MiniGameUIFactory.SetDefaultSprite(bagImage);

            // 포대 데이터 저장
            BagData bagData = bagObj.AddComponent<BagData>();
            bagData.fallSpeed = fallSpeed;
        }

        private void UpdateBags()
        {
            if (bagContainer == null)
                return;

            foreach (Transform child in bagContainer)
            {
                BagData bagData = child.GetComponent<BagData>();
                if (bagData == null)
                    continue;

                RectTransform rect = child.GetComponent<RectTransform>();
                rect.anchoredPosition += Vector2.down * bagData.fallSpeed * Time.deltaTime;

                // 캐처와 충돌 체크
                if (catcherImage != null)
                {
                    RectTransform catcherRect = catcherImage.GetComponent<RectTransform>();
                    float catcherX = catcherRect.anchoredPosition.x;
                    float catcherWidth = catcherRect.sizeDelta.x;

                    if (rect.anchoredPosition.y <= -300 && rect.anchoredPosition.y >= -350)
                    {
                        if (Mathf.Abs(rect.anchoredPosition.x - catcherX) < catcherWidth / 2 + 50)
                        {
                            // 포대를 받았음
                            _stackedCount++;
                            Debug.Log($"[FlourStackingMiniGame] 포대 수집! ({_stackedCount}/{targetCount})");
                            Destroy(child.gameObject);
                            continue;
                        }
                    }
                }

                // 바닥에 떨어졌으면 제거
                if (rect.anchoredPosition.y < -400)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void Awake()
        {
            // Container가 없으면 생성
            if (bagContainer == null)
            {
                GameObject containerObj = new GameObject("BagContainer");
                containerObj.transform.SetParent(transform, false);
                RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.sizeDelta = Vector2.zero;
                containerRect.anchoredPosition = Vector2.zero;
                bagContainer = containerObj.transform;
            }

            // Catcher가 없으면 생성
            if (catcherImage == null)
            {
                GameObject catcherObj = new GameObject("Catcher");
                catcherObj.transform.SetParent(transform, false);
                RectTransform catcherRect = catcherObj.AddComponent<RectTransform>();
                catcherRect.anchoredPosition = new Vector2(0, -300);
                catcherRect.sizeDelta = new Vector2(150, 30);
                catcherImage = catcherObj.AddComponent<Image>();
                catcherImage.color = new Color(0.5f, 0.3f, 0.2f); // 갈색
                MiniGameUIFactory.SetDefaultSprite(catcherImage);
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
            Debug.Log("[FlourStackingMiniGame] 다시 시도");
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log("[FlourStackingMiniGame] 계속하기");
            MiniGameManager.Instance.EndMiniGame(true);
        }

        // 포대 데이터를 저장하는 헬퍼 클래스
        private class BagData : MonoBehaviour
        {
            public float fallSpeed;
        }
    }
}

