using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 체 흔들기 미니게임입니다.
    /// 마우스를 드래그하여 체를 흔들고, 일정량 체질하면 성공합니다.
    /// </summary>
    public class FlourSiftingMiniGame : MonoBehaviour, IMiniGame
    {
        [Header("Game Settings")]
        [Tooltip("체질 진행도 목표 (0-1)")]
        [SerializeField] private float targetProgress = 1.0f;

        [Tooltip("마우스 이동 거리당 진행도 증가량")]
        [SerializeField] private float progressPerDistance = 0.001f;

        [Tooltip("최소 흔들기 속도 (초당 픽셀)")]
        [SerializeField] private float minShakeSpeed = 50f;

        [Header("UI References")]
        [SerializeField] private Image sieveImage; // 체 이미지
        [SerializeField] private Image progressBarFill; // 진행도 게이지
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private ParticleSystem flourParticle; // 밀가루 파티클 효과

        [Header("Particle Settings")]
        [Tooltip("파티클 생성 위치 (체 아래)")]
        [SerializeField] private Transform particleSpawnPoint;

        [Header("Audio (Optional)")]
        [SerializeField] private AudioClip siftingSound;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failSound;
        [SerializeField] private AudioSource audioSource;

        private Action<bool> _onComplete;
        private float _currentProgress = 0f;
        private bool _isPlaying;
        private bool _isDragging = false;
        private Vector2 _lastMousePosition;
        private float _totalDistance = 0f;
        private float _lastShakeTime = 0f;
        private float _shakeSpeed = 0f;

        public void Initialize(Action<bool> onComplete)
        {
            _onComplete = onComplete;
            _currentProgress = 0f;
            _isPlaying = false;
            _isDragging = false;
            _totalDistance = 0f;
            _shakeSpeed = 0f;

            // UI 초기화
            UpdateUI();

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }

            if (instructionsText != null)
            {
                instructionsText.text = "마우스를 드래그하여 체를 흔드세요!";
                instructionsText.gameObject.SetActive(true);
            }

            // 파티클 초기화
            if (flourParticle != null)
            {
                flourParticle.Stop();
                var emission = flourParticle.emission;
                emission.enabled = false;
            }

            Debug.Log("[FlourSiftingMiniGame] 초기화 완료");
        }

        public void StartGame()
        {
            _isPlaying = true;
            gameObject.SetActive(true);
            Debug.Log("[FlourSiftingMiniGame] 게임 시작!");
        }

        public void EndGame(bool success)
        {
            _isPlaying = false;
            _isDragging = false;

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
                    resultText.text = "성공! 밀가루를 모두 체질했습니다!";
                    resultText.color = Color.green;
                }
                else
                {
                    resultText.text = "실패! 목표 진행도에 도달하지 못했습니다!";
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

            // 파티클 정지
            if (flourParticle != null)
            {
                flourParticle.Stop();
                var emission = flourParticle.emission;
                emission.enabled = false;
            }

            // 사운드 정지
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            Debug.Log($"[FlourSiftingMiniGame] 게임 종료 - {(success ? "성공" : "실패")}");

            if (success)
            {
                _onComplete?.Invoke(true);
            }
        }

        public void CleanUp()
        {
            _isPlaying = false;
            _isDragging = false;

            if (flourParticle != null)
            {
                flourParticle.Stop();
            }

            gameObject.SetActive(false);
            Debug.Log("[FlourSiftingMiniGame] 정리 완료");
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            // 마우스 입력 처리
            HandleMouseInput();

            // UI 업데이트
            UpdateUI();

            // 목표 달성 체크
            if (_currentProgress >= targetProgress)
            {
                EndGame(true);
            }
        }

        private void HandleMouseInput()
        {
            // 마우스 버튼을 누르면 드래그 시작
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
                _lastShakeTime = Time.time;
            }

            // 마우스 버튼을 떼면 드래그 종료
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
                if (flourParticle != null)
                {
                    flourParticle.Stop();
                    var emission = flourParticle.emission;
                    emission.enabled = false;
                }
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }

            // 드래그 중일 때
            if (_isDragging)
            {
                Vector2 currentMousePosition = Input.mousePosition;
                Vector2 mouseDelta = currentMousePosition - _lastMousePosition;
                float distance = mouseDelta.magnitude;

                // 흔들기 속도 계산
                float deltaTime = Time.time - _lastShakeTime;
                if (deltaTime > 0f)
                {
                    _shakeSpeed = distance / deltaTime;
                }

                // 최소 속도 이상일 때만 진행도 증가
                if (_shakeSpeed >= minShakeSpeed)
                {
                    _totalDistance += distance;
                    _currentProgress += distance * progressPerDistance;
                    _currentProgress = Mathf.Clamp01(_currentProgress);

                    // 체 이미지 흔들기 효과 (시각적 피드백)
                    if (sieveImage != null)
                    {
                        float shakeAmount = Mathf.Clamp01(_shakeSpeed / 200f) * 5f;
                        float shakeX = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                        float shakeY = UnityEngine.Random.Range(-shakeAmount, shakeAmount);
                        RectTransform sieveRect = sieveImage.GetComponent<RectTransform>();
                        sieveRect.anchoredPosition = new Vector2(shakeX, shakeY);
                    }

                    // 파티클 효과 활성화
                    if (flourParticle != null)
                    {
                        if (!flourParticle.isPlaying)
                        {
                            flourParticle.Play();
                        }
                        var emission = flourParticle.emission;
                        emission.enabled = true;
                        emission.rateOverTime = Mathf.Clamp(_shakeSpeed / 10f, 10f, 50f);
                    }

                    // 사운드 재생
                    if (audioSource != null && siftingSound != null)
                    {
                        if (!audioSource.isPlaying)
                        {
                            audioSource.clip = siftingSound;
                            audioSource.loop = true;
                            audioSource.Play();
                        }
                    }
                }
                else
                {
                    // 속도가 너무 느리면 파티클 정지
                    if (flourParticle != null && flourParticle.isPlaying)
                    {
                        flourParticle.Stop();
                        var emission = flourParticle.emission;
                        emission.enabled = false;
                    }
                }

                _lastMousePosition = currentMousePosition;
                _lastShakeTime = Time.time;
            }
            else
            {
                // 드래그가 끝나면 체 이미지 원위치
                if (sieveImage != null)
                {
                    RectTransform sieveRect = sieveImage.GetComponent<RectTransform>();
                    sieveRect.anchoredPosition = Vector2.zero;
                }
            }
        }

        private void UpdateUI()
        {
            // 진행도 게이지 업데이트
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = _currentProgress / targetProgress;

                // 색상 변경 (진행도에 따라)
                if (_currentProgress >= targetProgress)
                {
                    progressBarFill.color = Color.green;
                }
                else if (_currentProgress >= targetProgress * 0.7f)
                {
                    progressBarFill.color = Color.yellow;
                }
                else
                {
                    progressBarFill.color = Color.white;
                }
            }

            // 진행도 텍스트 업데이트
            if (progressText != null)
            {
                float percentage = (_currentProgress / targetProgress) * 100f;
                progressText.text = $"진행도: {percentage:F1}%";
            }
        }

        private void Awake()
        {
            // 체 이미지가 없으면 생성
            if (sieveImage == null)
            {
                GameObject sieveObj = new GameObject("SieveImage");
                sieveObj.transform.SetParent(transform, false);
                RectTransform sieveRect = sieveObj.AddComponent<RectTransform>();
                sieveRect.anchoredPosition = new Vector2(0, 50);
                sieveRect.sizeDelta = new Vector2(200, 200);
                sieveImage = sieveObj.AddComponent<Image>();
                sieveImage.color = new Color(0.8f, 0.8f, 0.7f); // 베이지색
                MiniGameUIFactory.SetDefaultSprite(sieveImage);
            }

            // 진행도 게이지가 없으면 생성
            if (progressBarFill == null)
            {
                GameObject progressObj = new GameObject("ProgressBar");
                progressObj.transform.SetParent(transform, false);
                RectTransform progressRect = progressObj.AddComponent<RectTransform>();
                progressRect.anchoredPosition = new Vector2(0, -150);
                progressRect.sizeDelta = new Vector2(400, 30);
                progressBarFill = progressObj.AddComponent<Image>();
                progressBarFill.color = Color.white;
                progressBarFill.type = Image.Type.Filled;
                progressBarFill.fillMethod = Image.FillMethod.Horizontal;
                progressBarFill.fillAmount = 0f;
                MiniGameUIFactory.SetDefaultSprite(progressBarFill);
            }

            // 진행도 텍스트가 없으면 생성
            if (progressText == null)
            {
                GameObject progressTextObj = MiniGameUIFactory.CreateTextMeshPro("ProgressText", transform);
                RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
                progressTextRect.anchoredPosition = new Vector2(0, -200);
                progressTextRect.sizeDelta = new Vector2(400, 50);
                progressText = progressTextObj.GetComponent<TextMeshProUGUI>();
                progressText.text = "진행도: 0%";
                progressText.fontSize = 32;
                progressText.alignment = TextAlignmentOptions.Center;
            }

            // 파티클 시스템이 없으면 생성
            if (flourParticle == null)
            {
                CreateFlourParticle();
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

        /// <summary>
        /// 밀가루 파티클 효과를 생성합니다.
        /// </summary>
        private void CreateFlourParticle()
        {
            GameObject particleObj = new GameObject("FlourParticle");
            if (particleSpawnPoint != null)
            {
                particleObj.transform.SetParent(particleSpawnPoint, false);
                particleObj.transform.localPosition = Vector3.zero;
            }
            else
            {
                particleObj.transform.SetParent(transform, false);
                RectTransform rect = particleObj.AddComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0, -100);
            }

            flourParticle = particleObj.AddComponent<ParticleSystem>();

            // 파티클 설정
            var main = flourParticle.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 1f, 0.95f, 1f); // 밀가루 색상
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0.5f; // 중력 적용

            var emission = flourParticle.emission;
            emission.enabled = false;
            emission.rateOverTime = 20f;

            var shape = flourParticle.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var velocityOverLifetime = flourParticle.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f, -1f); // 아래로 떨어짐

            Debug.Log("[FlourSiftingMiniGame] 밀가루 파티클 효과 생성 완료");
        }

        private void OnRetryButtonClick()
        {
            Debug.Log("[FlourSiftingMiniGame] 다시 시도");
            Initialize(_onComplete);
            StartGame();
        }

        private void OnContinueButtonClick()
        {
            Debug.Log("[FlourSiftingMiniGame] 계속하기");
            MiniGameManager.Instance.EndMiniGame(true);
        }
    }
}

