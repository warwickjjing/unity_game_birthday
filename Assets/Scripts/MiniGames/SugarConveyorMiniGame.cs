using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 설탕 컨베이어 벨트 타이밍 미니게임입니다.
    /// 설탕 봉지가 노란 구역을 통과할 때 클릭하여 수집합니다.
    /// 5개를 성공하면 클리어합니다.
    /// </summary>
    public class SugarConveyorMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("설탕 봉지 이동 속도")]
        [SerializeField] private float conveyorSpeed = 200f;

        [Tooltip("설탕 봉지 생성 간격 (초)")]
        [SerializeField] private float spawnInterval = 2f;

        [Tooltip("목표 수집 개수")]
        [SerializeField] private int targetCount = 5;

        [Tooltip("노란 구역의 폭 (픽셀)")]
        [SerializeField] private float targetZoneWidth = 100f;

        [Tooltip("Perfect 판정 범위 (노란 구역 중앙 기준)")]
        [SerializeField] private float perfectRange = 30f;

        [Tooltip("Good 판정 범위 (노란 구역 전체)")]
        [SerializeField] private float goodRange = 50f;

        [Header("UI References")]
        [SerializeField] private Image conveyorBeltImage;
        [SerializeField] private Image targetZoneImage; // 노란 구역
        [SerializeField] private Transform bagContainer; // 설탕 봉지들이 생성될 컨테이너
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI feedbackText; // Good/Perfect 표시
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip perfectSound;
        [SerializeField] private AudioClip goodSound;
        [SerializeField] private AudioClip missSound;
        [SerializeField] private AudioSource audioSource;

        private Action<bool> _onComplete;
        private int _collectedCount = 0;
        private bool _isPlaying;
        private float _spawnTimer = 0f;
        private float _feedbackTimer = 0f;
        private const float FEEDBACK_DURATION = 1f;

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _collectedCount = 0;
            _isPlaying = false;
            _spawnTimer = 0f;
            _feedbackTimer = 0f;

            // 기존 봉지 제거
            if (bagContainer != null)
            {
                foreach (Transform child in bagContainer)
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
                instructionsText.text = "설탕 봉지가 노란 구역을 통과할 때 클릭하세요!";
                instructionsText.gameObject.SetActive(true);
            }

            if (feedbackText != null)
            {
                feedbackText.text = "";
                feedbackText.gameObject.SetActive(false);
            }

            // 노란 구역 설정
            SetupTargetZone();

            Debug.Log("[SugarConveyorMiniGame] 초기화 완료");
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);
            Debug.Log("[SugarConveyorMiniGame] 게임 시작!");
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
                    resultText.text = $"성공! {_collectedCount}개의 설탕 봉지를 수집했습니다!";
                    resultText.color = Color.green;
                }
                else
                {
                    resultText.text = $"실패! 목표 개수에 도달하지 못했습니다! ({_collectedCount}/{targetCount})";
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

            Debug.Log($"[SugarConveyorMiniGame] 게임 종료 - {(success ? "성공" : "실패")}");

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
            Debug.Log("[SugarConveyorMiniGame] 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 설탕 봉지 생성
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= spawnInterval)
            {
                _spawnTimer = 0f;
                SpawnSugarBag();
            }

            // 설탕 봉지 이동
            UpdateBags();

            // 피드백 텍스트 업데이트
            if (_feedbackTimer > 0f)
            {
                _feedbackTimer -= Time.deltaTime;
                if (_feedbackTimer <= 0f)
                {
                    if (feedbackText != null)
                    {
                        feedbackText.gameObject.SetActive(false);
                    }
                }
            }

            // 클릭 입력 처리
            if (Input.GetMouseButtonDown(0))
            {
                OnClick();
            }

            // UI 업데이트
            UpdateUI();

            // 목표 달성 체크
            if (_collectedCount >= targetCount)
            {
                EndGame(true);
            }
        }

        private void SetupTargetZone()
        {
            if (targetZoneImage != null)
            {
                RectTransform targetRect = targetZoneImage.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    // 화면 중앙에 노란 구역 배치
                    targetRect.anchoredPosition = new Vector2(0, 0);
                    targetRect.sizeDelta = new Vector2(targetZoneWidth, 60f);
                    targetZoneImage.color = new Color(1f, 1f, 0f, 0.5f); // 반투명 노란색
                }
            }
        }

        private void SpawnSugarBag()
        {
            if (bagContainer == null)
                return;

            GameObject bagObj = new GameObject($"SugarBag_{Time.time}");
            bagObj.transform.SetParent(bagContainer, false);

            RectTransform rect = bagObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-500, 0); // 왼쪽에서 시작
            rect.sizeDelta = new Vector2(60, 60);

            Image bagImage = bagObj.AddComponent<Image>();
            bagImage.color = new Color(1f, 1f, 0.9f); // 흰색/노란색
            MiniGameUIFactory.SetDefaultSprite(bagImage);

            // 봉지 데이터 저장
            SugarBagData bagData = bagObj.AddComponent<SugarBagData>();
            bagData.speed = conveyorSpeed;
            bagData.hasBeenClicked = false;
        }

        private void UpdateBags()
        {
            if (bagContainer == null)
                return;

            foreach (Transform child in bagContainer)
            {
                SugarBagData bagData = child.GetComponent<SugarBagData>();
                if (bagData == null)
                    continue;

                RectTransform rect = child.GetComponent<RectTransform>();
                rect.anchoredPosition += Vector2.right * bagData.speed * Time.deltaTime;

                // 화면 밖으로 나가면 제거
                if (rect.anchoredPosition.x > 600)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnClick()
        {
            if (bagContainer == null)
                return;

            // 가장 가까운 봉지 찾기
            Transform closestBag = null;
            float closestDistance = float.MaxValue;

            foreach (Transform child in bagContainer)
            {
                SugarBagData bagData = child.GetComponent<SugarBagData>();
                if (bagData == null || bagData.hasBeenClicked)
                    continue;

                RectTransform rect = child.GetComponent<RectTransform>();
                float distance = Mathf.Abs(rect.anchoredPosition.x - 0); // 중앙(노란 구역)까지의 거리

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBag = child;
                }
            }

            if (closestBag == null)
                return;

            SugarBagData clickedBag = closestBag.GetComponent<SugarBagData>();
            RectTransform clickedRect = closestBag.GetComponent<RectTransform>();

            // 판정
            string feedback = "";
            Color feedbackColor = Color.white;
            bool isSuccess = false;

            if (closestDistance <= perfectRange)
            {
                feedback = "Perfect!";
                feedbackColor = Color.yellow;
                isSuccess = true;
                PlaySound(perfectSound);
            }
            else if (closestDistance <= goodRange)
            {
                feedback = "Good!";
                feedbackColor = Color.green;
                isSuccess = true;
                PlaySound(goodSound);
            }
            else
            {
                feedback = "Miss!";
                feedbackColor = Color.red;
                PlaySound(missSound);
            }

            // 피드백 표시
            if (feedbackText != null)
            {
                feedbackText.text = feedback;
                feedbackText.color = feedbackColor;
                feedbackText.gameObject.SetActive(true);
                _feedbackTimer = FEEDBACK_DURATION;
            }

            if (isSuccess)
            {
                _collectedCount++;
                clickedBag.hasBeenClicked = true;
                Destroy(closestBag.gameObject);
                Debug.Log($"[SugarConveyorMiniGame] 설탕 봉지 수집! ({_collectedCount}/{targetCount})");
            }
            else
            {
                PlaySound(clickSound);
            }
        }

        private void UpdateUI()
        {
            if (countText != null)
            {
                countText.text = $"수집: {_collectedCount} / {targetCount}";
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
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

            // AudioSource 자동 찾기
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        private void OnRetryButtonClick()
        {
            Debug.Log("[SugarConveyorMiniGame] 다시 시도");
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log("[SugarConveyorMiniGame] 계속하기");
            MiniGameManager.Instance.EndMiniGame(true);
        }

        // 설탕 봉지 데이터를 저장하는 헬퍼 클래스
        private class SugarBagData : MonoBehaviour
        {
            public float speed;
            public bool hasBeenClicked;
        }
    }
}

