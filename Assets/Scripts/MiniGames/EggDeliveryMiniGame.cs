using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 계란 배달 미니게임입니다.
    /// 계란을 조심스럽게 운반하여 목표 지점까지 배달합니다.
    /// 너무 빨리 움직이거나 장애물에 부딪히면 실패합니다.
    /// </summary>
    public class EggDeliveryMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("게임 제한 시간 (초)")]
        [SerializeField] private float timeLimit = 20f;

        [Tooltip("최대 속도 (너무 빨리 움직이면 깨짐)")]
        [SerializeField] private float maxSpeed = 150f;

        [Tooltip("균형 게이지 감소 속도")]
        [SerializeField] private float balanceDecreaseRate = 10f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private Image balanceGauge; // 균형 게이지
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Image eggImage; // 계란 이미지
        [SerializeField] private Image targetImage; // 목표 지점

        private Action<bool> _onComplete;
        private float _remainingTime;
        private float _balance = 100f; // 균형 게이지 (0-100)
        private Vector2 _eggPosition;
        private Vector2 _targetPosition;
        private bool _isPlaying;
        private bool _reachedTarget = false;

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _remainingTime = timeLimit;
            _balance = 100f;
            _isPlaying = false;
            _reachedTarget = false;

            // 계란 초기 위치 (왼쪽)
            _eggPosition = new Vector2(-400, 0);
            if (eggImage != null)
            {
                RectTransform eggRect = eggImage.GetComponent<RectTransform>();
                eggRect.anchoredPosition = _eggPosition;
            }

            // 목표 지점 위치 (오른쪽)
            _targetPosition = new Vector2(400, 0);
            if (targetImage != null)
            {
                RectTransform targetRect = targetImage.GetComponent<RectTransform>();
                targetRect.anchoredPosition = _targetPosition;
            }

            // UI 초기화
            UpdateUI();

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (instructionsText != null)
            {
                instructionsText.text = "화살표 키로 조심스럽게 이동하세요! 너무 빨리 움직이면 깨집니다!";
                instructionsText.gameObject.SetActive(true);
            }
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);
            Debug.Log("[EggDeliveryMiniGame] 게임 시작!");
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
                    resultText.text = "성공! 계란을 안전하게 배달했습니다!";
                    resultText.color = Color.green;
                }
                else
                {
                    string reason = _balance <= 0 ? "계란이 깨졌습니다!" : 
                                   _remainingTime <= 0 ? "시간 초과!" : "목표에 도달하지 못했습니다!";
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

            Debug.Log($"[EggDeliveryMiniGame] 게임 종료 - {(success ? "성공" : "실패")}");

            if (success)
            {
                _onComplete?.Invoke(true);
            }
        }

        public void CleanUp()
        {
            _isPlaying = false;
            gameObject.SetActive(false);
            Debug.Log("[EggDeliveryMiniGame] 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 타이머 업데이트
            _remainingTime -= Time.deltaTime;

            // 입력 처리
            Vector2 moveInput = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                moveInput.x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                moveInput.x += 1f;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                moveInput.y += 1f;
            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                moveInput.y -= 1f;

            // 계란 이동
            float moveSpeed = 100f; // 기본 이동 속도
            Vector2 moveDelta = moveInput.normalized * moveSpeed * Time.deltaTime;
            float actualSpeed = moveDelta.magnitude / Time.deltaTime;

            // 속도가 너무 빠르면 균형 감소
            if (actualSpeed > maxSpeed)
            {
                _balance -= balanceDecreaseRate * Time.deltaTime;
            }
            else
            {
                // 천천히 움직이면 균형 회복
                _balance = Mathf.Min(100f, _balance + 5f * Time.deltaTime);
            }

            _balance = Mathf.Clamp(_balance, 0f, 100f);

            _eggPosition += moveDelta;
            _eggPosition.x = Mathf.Clamp(_eggPosition.x, -450f, 450f);
            _eggPosition.y = Mathf.Clamp(_eggPosition.y, -200f, 200f);

            if (eggImage != null)
            {
                RectTransform eggRect = eggImage.GetComponent<RectTransform>();
                eggRect.anchoredPosition = _eggPosition;
            }

            // 목표 지점 도달 체크
            if (Vector2.Distance(_eggPosition, _targetPosition) < 50f)
            {
                _reachedTarget = true;
            }

            // UI 업데이트
            UpdateUI();

            // 게임 종료 조건 체크
            if (_balance <= 0f)
            {
                EndGame(false);
            }
            else if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                EndGame(_reachedTarget);
            }
            else if (_reachedTarget)
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

            if (balanceGauge != null)
            {
                balanceGauge.fillAmount = _balance / 100f;

                if (_balance < 30f)
                {
                    balanceGauge.color = Color.red;
                }
                else if (_balance < 60f)
                {
                    balanceGauge.color = Color.yellow;
                }
                else
                {
                    balanceGauge.color = Color.green;
                }
            }
        }

        private void Awake()
        {
            // 계란 이미지가 없으면 생성
            if (eggImage == null)
            {
                GameObject eggObj = new GameObject("Egg");
                eggObj.transform.SetParent(transform, false);
                RectTransform eggRect = eggObj.AddComponent<RectTransform>();
                eggRect.anchoredPosition = new Vector2(-400, 0);
                eggRect.sizeDelta = new Vector2(60, 80);
                eggImage = eggObj.AddComponent<Image>();
                eggImage.color = new Color(1f, 1f, 0.9f); // 계란 색상
                MiniGameUIFactory.SetDefaultSprite(eggImage);
            }

            // 목표 지점 이미지가 없으면 생성
            if (targetImage == null)
            {
                GameObject targetObj = new GameObject("Target");
                targetObj.transform.SetParent(transform, false);
                RectTransform targetRect = targetObj.AddComponent<RectTransform>();
                targetRect.anchoredPosition = new Vector2(400, 0);
                targetRect.sizeDelta = new Vector2(100, 100);
                targetImage = targetObj.AddComponent<Image>();
                targetImage.color = new Color(0, 1, 0, 0.5f); // 초록색 반투명
                MiniGameUIFactory.SetDefaultSprite(targetImage);
            }

            // 균형 게이지가 없으면 생성
            if (balanceGauge == null)
            {
                GameObject gaugeObj = new GameObject("BalanceGauge");
                gaugeObj.transform.SetParent(transform, false);
                RectTransform gaugeRect = gaugeObj.AddComponent<RectTransform>();
                gaugeRect.anchoredPosition = new Vector2(0, 250);
                gaugeRect.sizeDelta = new Vector2(300, 30);
                balanceGauge = gaugeObj.AddComponent<Image>();
                balanceGauge.color = Color.green;
                balanceGauge.type = Image.Type.Filled;
                balanceGauge.fillMethod = Image.FillMethod.Horizontal;
                balanceGauge.fillAmount = 1f;
                MiniGameUIFactory.SetDefaultSprite(balanceGauge);
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
            Debug.Log("[EggDeliveryMiniGame] 다시 시도");
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log("[EggDeliveryMiniGame] 계속하기");
            MiniGameManager.Instance.EndMiniGame(true);
        }
    }
}

