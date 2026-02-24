using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BirthdayCakeQuest.MiniGames
{
    /// <summary>
    /// 밀가루 체질 미니게임 Scene 컨트롤러입니다.
    /// 별도의 Scene에서 실행됩니다.
    /// </summary>
    public class FlourMiniGameScene : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button quitButton;
        [SerializeField] private Image sieveImage;

        [Header("Particle Settings")]
        [SerializeField] private Transform particleSpawnPoint;
        [SerializeField] private int maxParticles = 100;

        [Header("Game Settings")]
        [SerializeField] private float targetProgress = 100f;
        [SerializeField] private float shakeSensitivity = 0.5f;
        [SerializeField] private float progressDecayRate = 5f;
        [SerializeField] private float shakeDetectionThreshold = 10f;

        private float _currentProgress = 0f;
        private Vector3 _lastMousePosition;
        private bool _isGameActive = false;
        private GameObject _particleContainer;

        private void Start()
        {
            Debug.Log("[FlourMiniGameScene] 밀가루 미니게임 Scene 시작");

            // UI 초기화
            if (titleText != null)
                titleText.text = "밀가루 체질하기";

            if (instructionText != null)
                instructionText.text = "마우스를 흔들어 밀가루를 체로 거르세요!";

            if (progressBar != null)
            {
                progressBar.minValue = 0f;
                progressBar.maxValue = targetProgress;
                progressBar.value = 0f;
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }

            // 파티클 컨테이너 생성
            _particleContainer = new GameObject("ParticleContainer");
            _particleContainer.transform.SetParent(transform);

            // 게임 시작
            StartGame();
        }

        private void StartGame()
        {
            _isGameActive = true;
            _currentProgress = 0f;
            _lastMousePosition = Input.mousePosition;

            Debug.Log("[FlourMiniGameScene] 게임 시작");
        }

        private void Update()
        {
            if (!_isGameActive)
                return;

            // 마우스 흔들기 감지
            Vector3 currentMousePosition = Input.mousePosition;
            float mouseDelta = Vector3.Distance(currentMousePosition, _lastMousePosition);

            if (mouseDelta > shakeDetectionThreshold)
            {
                // 진행도 증가
                float progressIncrease = mouseDelta * shakeSensitivity * Time.deltaTime;
                _currentProgress += progressIncrease;

                // 파티클 생성
                CreateFlourParticle();

                // 체 흔들림 애니메이션 (간단한 회전)
                if (sieveImage != null)
                {
                    float rotation = Mathf.Sin(Time.time * 20f) * 10f;
                    sieveImage.transform.rotation = Quaternion.Euler(0, 0, rotation);
                }
            }
            else
            {
                // 진행도 감소
                _currentProgress -= progressDecayRate * Time.deltaTime;
                _currentProgress = Mathf.Max(0f, _currentProgress);

                // 체 원래 위치로
                if (sieveImage != null)
                {
                    sieveImage.transform.rotation = Quaternion.Lerp(
                        sieveImage.transform.rotation,
                        Quaternion.identity,
                        Time.deltaTime * 5f
                    );
                }
            }

            _lastMousePosition = currentMousePosition;

            // UI 업데이트
            UpdateUI();

            // 목표 달성 체크
            if (_currentProgress >= targetProgress)
            {
                CompleteGame(true);
            }
        }

        private void UpdateUI()
        {
            if (progressBar != null)
            {
                progressBar.value = _currentProgress;
            }

            if (progressText != null)
            {
                int percentage = Mathf.RoundToInt((_currentProgress / targetProgress) * 100f);
                progressText.text = $"{percentage}%";
            }
        }

        private void CreateFlourParticle()
        {
            if (_particleContainer.transform.childCount >= maxParticles)
                return;

            Vector3 spawnPos = particleSpawnPoint != null 
                ? particleSpawnPoint.position 
                : new Vector3(Screen.width / 2f, Screen.height * 0.7f, 0);

            // 간단한 UI 이미지로 파티클 생성
            GameObject particle = new GameObject("FlourParticle");
            particle.transform.SetParent(_particleContainer.transform);

            Image particleImage = particle.AddComponent<Image>();
            particleImage.color = new Color(1f, 1f, 1f, 0.8f);

            RectTransform rect = particle.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(10f, 10f);
            rect.position = spawnPos + new Vector3(
                Random.Range(-50f, 50f),
                Random.Range(-20f, 20f),
                0
            );

            // 떨어지는 애니메이션
            StartCoroutine(AnimateParticle(particle));
        }

        private IEnumerator AnimateParticle(GameObject particle)
        {
            RectTransform rect = particle.GetComponent<RectTransform>();
            Image image = particle.GetComponent<Image>();
            
            float lifetime = 2f;
            float elapsed = 0f;
            Vector3 startPos = rect.position;
            Vector3 velocity = new Vector3(Random.Range(-20f, 20f), -100f, 0);

            while (elapsed < lifetime && particle != null)
            {
                elapsed += Time.deltaTime;

                // 위치 업데이트
                rect.position += velocity * Time.deltaTime;

                // 페이드 아웃
                if (image != null)
                {
                    Color color = image.color;
                    color.a = 1f - (elapsed / lifetime);
                    image.color = color;
                }

                yield return null;
            }

            if (particle != null)
                Destroy(particle);
        }

        private void CompleteGame(bool success)
        {
            if (!_isGameActive)
                return;

            _isGameActive = false;

            Debug.Log($"[FlourMiniGameScene] 게임 {(success ? "성공" : "실패")}!");

            // 결과 전달 및 Scene 복귀
            var resultManager = MiniGameResult.Instance;
            if (resultManager != null)
            {
                resultManager.SetResultAndReturn(success);
            }
            else
            {
                Debug.LogError("[FlourMiniGameScene] MiniGameResult를 찾을 수 없습니다!");
            }
        }

        private void OnQuitButtonClicked()
        {
            Debug.Log("[FlourMiniGameScene] 종료 버튼 클릭");
            CompleteGame(false);
        }

        private void OnDestroy()
        {
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            }
        }
    }
}

