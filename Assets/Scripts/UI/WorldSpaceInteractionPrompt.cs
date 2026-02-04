using UnityEngine;
using TMPro;
using BirthdayCakeQuest.Interaction;

namespace BirthdayCakeQuest.UI
{
    /// <summary>
    /// 상호작용 가능한 오브젝트 위에 Floating UI 프롬프트를 표시합니다.
    /// "F키로 열기" 같은 텍스트가 3D 공간에 떠다닙니다.
    /// </summary>
    [RequireComponent(typeof(IInteractable))]
    public class WorldSpaceInteractionPrompt : MonoBehaviour
    {
        [Header("UI 설정")]
        [Tooltip("프롬프트 UI가 표시될 높이 (오브젝트 기준)")]
        [SerializeField] private float promptHeight = 2f;

        [Tooltip("카메라로부터의 최대 표시 거리")]
        [SerializeField] private float maxVisibleDistance = 10f;

        [Header("텍스트 설정")]
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
        [SerializeField] private float fontSize = 0.3f;

        private GameObject _promptObject;
        private TextMeshPro _textMesh;
        private IInteractable _interactable;
        private UnityEngine.Camera _mainCamera;
        private Transform _playerTransform;

        private void Awake()
        {
            _interactable = GetComponent<IInteractable>();
            _mainCamera = UnityEngine.Camera.main;

            CreatePromptUI();
        }

        private void Start()
        {
            // 플레이어 찾기
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void CreatePromptUI()
        {
            // 프롬프트 오브젝트 생성
            _promptObject = new GameObject("InteractionPrompt");
            _promptObject.transform.SetParent(transform);
            _promptObject.transform.localPosition = new Vector3(0, promptHeight, 0);

            // 배경 (Quad)
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "Background";
            background.transform.SetParent(_promptObject.transform);
            background.transform.localPosition = Vector3.zero;
            background.transform.localScale = new Vector3(1.5f, 0.5f, 1f);

            Material bgMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bgMat.color = backgroundColor;
            bgMat.SetFloat("_Surface", 1); // Transparent
            background.GetComponent<Renderer>().material = bgMat;

            // 배경 Collider 제거
            Destroy(background.GetComponent<Collider>());

            // 텍스트 (TextMeshPro)
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(_promptObject.transform);
            textObject.transform.localPosition = new Vector3(0, 0, -0.01f);

            _textMesh = textObject.AddComponent<TextMeshPro>();
            _textMesh.text = "상호작용 [F]";
            _textMesh.fontSize = fontSize;
            _textMesh.color = textColor;
            _textMesh.alignment = TextAlignmentOptions.Center;
            _textMesh.rectTransform.sizeDelta = new Vector2(3, 1);

            // 초기에는 숨김
            _promptObject.SetActive(false);
        }

        private void Update()
        {
            if (_playerTransform == null || _mainCamera == null)
                return;

            // 플레이어와의 거리 확인
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            bool shouldShow = distance <= maxVisibleDistance && _interactable.CanInteract;

            if (_promptObject.activeSelf != shouldShow)
            {
                _promptObject.SetActive(shouldShow);
            }

            if (shouldShow)
            {
                // 프롬프트 텍스트 업데이트
                _textMesh.text = _interactable.GetInteractPrompt();

                // 카메라를 향하도록 회전 (빌보드 효과)
                _promptObject.transform.LookAt(_mainCamera.transform);
                _promptObject.transform.Rotate(0, 180, 0); // 텍스트가 뒤집히지 않도록
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 프롬프트 위치 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * promptHeight, 0.2f);
        }
    }
}

