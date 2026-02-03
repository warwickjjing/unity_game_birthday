using UnityEngine;
using System.Collections.Generic;

namespace BirthdayCakeQuest.Camera
{
    /// <summary>
    /// 플레이어를 따라가는 3/4 뷰(Isometric 느낌) 카메라입니다.
    /// 고정된 각도에서 플레이어를 추적합니다.
    /// </summary>
    public sealed class IsometricFollowCamera : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("따라갈 대상 (플레이어)")]
        [SerializeField] private Transform target;

        [Header("Camera Settings")]
        [Tooltip("카메라와 플레이어 사이의 오프셋")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -8f);
        
        [Tooltip("카메라 회전 각도 (X축) - 3/4뷰는 30-35도 권장")]
        [SerializeField] private float angleX = 30f;

        [Header("Follow Settings")]
        [Tooltip("카메라 이동 속도 (높을수록 즉시 반응)")]
        [SerializeField] private float followSpeed = 5f;
        
        [Tooltip("부드러운 이동 사용 여부")]
        [SerializeField] private bool useSmoothFollow = true;

        [Header("Constraints")]
        [Tooltip("카메라 이동 범위 제한 (선택)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector3 boundsMin = new Vector3(-50f, 0f, -50f);
        [SerializeField] private Vector3 boundsMax = new Vector3(50f, 20f, 50f);

        [Header("Obstacle Avoidance")]
        [Tooltip("카메라가 통과할 수 없는 레이어 (지붕 등)")]
        [SerializeField] private LayerMask obstacleLayer = -1;
        
        [Tooltip("장애물 회피 사용 여부")]
        [SerializeField] private bool useObstacleAvoidance = true;
        
        [Tooltip("장애물과의 최소 거리")]
        [SerializeField] private float obstacleMinDistance = 0.5f;

        [Header("Roof Transparency")]
        [Tooltip("지붕 투명화 사용 여부")]
        [SerializeField] private bool useRoofTransparency = true;
        
        [Tooltip("지붕 레이어")]
        [SerializeField] private LayerMask roofLayer = -1;
        
        [Tooltip("지붕 투명도 (0=완전 투명, 1=불투명)")]
        [SerializeField] private float roofTransparency = 0.3f;
        
        [Tooltip("집 내부 영역 (플레이어가 이 영역 안에 있으면 지붕 투명화 유지)")]
        [SerializeField] private bool useInteriorBounds = true;
        
        [Tooltip("집 내부 최소 위치")]
        [SerializeField] private Vector3 interiorMin = new Vector3(-50f, 0f, -50f);
        
        [Tooltip("집 내부 최대 위치")]
        [SerializeField] private Vector3 interiorMax = new Vector3(50f, 10f, 50f);
        
        [Tooltip("집 내부 영역 자동 감지 (Floor 오브젝트의 Bounds 사용)")]
        [SerializeField] private bool autoDetectInteriorBounds = true;

        private Vector3 _currentVelocity;
        private bool _isPaused = false;
        private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> _transparentMaterials = new Dictionary<Renderer, Material[]>();
        private List<Renderer> _allRoofRenderers = new List<Renderer>(); // Scene의 모든 지붕 Renderer
        private bool _roofRenderersInitialized = false;

        private void Start()
        {
            // 타겟이 설정되지 않았다면 자동으로 플레이어 찾기
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("[IsometricFollowCamera] Player target 자동 설정 완료");
                }
                else
                {
                    // Tag로 못 찾으면 PlayerController로 찾기
                    var playerController = FindObjectOfType<BirthdayCakeQuest.Player.PlayerController>();
                    if (playerController != null)
                    {
                        target = playerController.transform;
                        Debug.Log("[IsometricFollowCamera] PlayerController로 타겟 설정 완료");
                    }
                    else
                    {
                        Debug.LogWarning("[IsometricFollowCamera] Player를 찾을 수 없습니다!");
                    }
                }
            }
            
            // 모든 지붕 Renderer 미리 찾기
            InitializeRoofRenderers();
            
            // 집 내부 영역 자동 감지
            if (autoDetectInteriorBounds && useInteriorBounds)
            {
                AutoDetectInteriorBounds();
            }
        }
        
        /// <summary>
        /// Floor 오브젝트를 찾아서 집 내부 영역을 자동으로 설정합니다.
        /// </summary>
        private void AutoDetectInteriorBounds()
        {
            GameObject floor = GameObject.Find("Floor");
            if (floor == null)
            {
                // Floor를 못 찾으면 "Room1", "Room2", "Hallway" 등의 오브젝트 찾기
                GameObject room1 = GameObject.Find("Room1");
                GameObject room2 = GameObject.Find("Room2");
                GameObject hallway = GameObject.Find("Hallway");
                
                Bounds combinedBounds = new Bounds();
                bool hasBounds = false;
                
                if (room1 != null)
                {
                    Renderer renderer = room1.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        combinedBounds = renderer.bounds;
                        hasBounds = true;
                    }
                }
                
                if (room2 != null)
                {
                    Renderer renderer = room2.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (hasBounds)
                            combinedBounds.Encapsulate(renderer.bounds);
                        else
                        {
                            combinedBounds = renderer.bounds;
                            hasBounds = true;
                        }
                    }
                }
                
                if (hallway != null)
                {
                    Renderer renderer = hallway.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (hasBounds)
                            combinedBounds.Encapsulate(renderer.bounds);
                        else
                        {
                            combinedBounds = renderer.bounds;
                            hasBounds = true;
                        }
                    }
                }
                
                if (hasBounds)
                {
                    interiorMin = combinedBounds.min;
                    interiorMax = combinedBounds.max;
                    // Y축은 약간 여유있게
                    interiorMin.y = Mathf.Min(interiorMin.y, 0f);
                    interiorMax.y = Mathf.Max(interiorMax.y, 10f);
                    Debug.Log($"[IsometricFollowCamera] 집 내부 영역 자동 감지 완료: Min={interiorMin}, Max={interiorMax}");
                }
            }
            else
            {
                Renderer floorRenderer = floor.GetComponent<Renderer>();
                if (floorRenderer != null)
                {
                    Bounds floorBounds = floorRenderer.bounds;
                    interiorMin = floorBounds.min;
                    interiorMax = floorBounds.max;
                    // Y축은 약간 여유있게
                    interiorMin.y = Mathf.Min(interiorMin.y, 0f);
                    interiorMax.y = Mathf.Max(interiorMax.y, 10f);
                    Debug.Log($"[IsometricFollowCamera] Floor 기반 집 내부 영역 자동 감지 완료: Min={interiorMin}, Max={interiorMax}");
                }
            }
        }
        
        /// <summary>
        /// Scene의 모든 지붕 Renderer를 미리 찾아서 저장합니다.
        /// </summary>
        private void InitializeRoofRenderers()
        {
            if (_roofRenderersInitialized || roofLayer == 0)
                return;
            
            _allRoofRenderers.Clear();
            
            // Scene의 모든 GameObject에서 Roof Layer인 것 찾기
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if ((roofLayer & (1 << obj.layer)) != 0) // Roof Layer인지 확인
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null && renderer.enabled)
                    {
                        _allRoofRenderers.Add(renderer);
                    }
                }
            }
            
            _roofRenderersInitialized = true;
            Debug.Log($"[IsometricFollowCamera] 지붕 Renderer {_allRoofRenderers.Count}개 초기화 완료");
        }

        private void LateUpdate()
        {
            if (target == null || _isPaused)
                return;

            UpdateCameraPosition();
            UpdateCameraRotation();
            UpdateRoofTransparency();
        }

        /// <summary>
        /// 카메라를 일시정지/재개합니다.
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        private void UpdateCameraPosition()
        {
            Vector3 targetPosition = target.position + offset;

            // 장애물 회피 (지붕 등 통과 방지)
            if (useObstacleAvoidance && obstacleLayer != 0)
            {
                Vector3 direction = (targetPosition - target.position).normalized;
                float distance = Vector3.Distance(target.position, targetPosition);
                
                RaycastHit hit;
                if (Physics.Raycast(target.position, direction, out hit, distance, obstacleLayer))
                {
                    // 장애물이 있으면 카메라를 장애물 앞으로 이동
                    targetPosition = hit.point - direction * obstacleMinDistance;
                    // Y축은 원래 높이 유지 (지붕을 통과하되, 너무 가까이 가지 않음)
                    targetPosition.y = target.position.y + offset.y;
                }
            }

            // 범위 제한 적용
            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, boundsMin.x, boundsMax.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, boundsMin.y, boundsMax.y);
                targetPosition.z = Mathf.Clamp(targetPosition.z, boundsMin.z, boundsMax.z);
            }

            if (useSmoothFollow)
            {
                // 부드러운 이동
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref _currentVelocity,
                    1f / followSpeed
                );
            }
            else
            {
                // 즉시 이동
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    followSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateCameraRotation()
        {
            // 3/4 쿼터뷰를 위한 고정 각도
            // X축 45도 = 위에서 내려다보는 각도
            // Y축은 항상 0도 (정면)
            Quaternion targetRotation = Quaternion.Euler(angleX, 0f, 0f);
            transform.rotation = targetRotation;
            
            // 회전 잠금 보장 (미니게임 등에서도 유지)
            transform.eulerAngles = new Vector3(angleX, 0f, 0f);
        }

        /// <summary>
        /// 타겟을 변경합니다.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// 오프셋을 변경합니다.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// 카메라를 즉시 타겟 위치로 이동합니다.
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null)
                return;

            transform.position = target.position + offset;
            _currentVelocity = Vector3.zero;
        }

        /// <summary>
        /// 카메라와 플레이어 사이에 지붕이 있으면 투명하게 만듭니다.
        /// </summary>
        private void UpdateRoofTransparency()
        {
            if (!useRoofTransparency || target == null || roofLayer == 0)
            {
                if (Time.frameCount % 300 == 0)
                {
                    Debug.Log($"[IsometricFollowCamera] 지붕 투명화 비활성화 - useRoofTransparency: {useRoofTransparency}, target: {target != null}, roofLayer: {roofLayer}");
                }
                return;
            }

            // 지붕 Renderer 초기화 (아직 안됐으면)
            if (!_roofRenderersInitialized)
            {
                InitializeRoofRenderers();
            }

            Vector3 playerPos = target.position;
            
            // 플레이어가 집 안에 있는지 확인
            bool isInsideInterior = false;
            if (useInteriorBounds)
            {
                isInsideInterior = playerPos.x >= interiorMin.x && playerPos.x <= interiorMax.x &&
                                  playerPos.y >= interiorMin.y && playerPos.y <= interiorMax.y &&
                                  playerPos.z >= interiorMin.z && playerPos.z <= interiorMax.z;
                
                // 디버그 로그 (주기적으로)
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[IsometricFollowCamera] 집 내부 판단 - 위치: {playerPos}, 범위: [{interiorMin}, {interiorMax}], 결과: {(isInsideInterior ? "안" : "밖")}");
                }
            }
            else
            {
                // useInteriorBounds가 false면 항상 레이캐스트 방식만 사용
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[IsometricFollowCamera] 집 내부 영역 감지 비활성화 - 레이캐스트 방식만 사용");
                }
            }
            
            // 집 안에 있으면 모든 지붕을 투명하게, 밖에 있으면 레이캐스트로 감지된 것만
            HashSet<Renderer> currentRoofRenderers = new HashSet<Renderer>();
            
            if (isInsideInterior && useInteriorBounds)
            {
                // 집 안에 있으면 모든 지붕을 투명하게
                foreach (Renderer renderer in _allRoofRenderers)
                {
                    if (renderer != null && renderer.enabled)
                    {
                        currentRoofRenderers.Add(renderer);
                    }
                }
                
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[IsometricFollowCamera] 집 안에 있음 - 모든 지붕 {currentRoofRenderers.Count}개 투명화 (플레이어 위치: {playerPos})");
                }
            }
            else
            {
                // 집 밖에 있으면 레이캐스트로 감지된 것만
                float raycastHeight = 20f;
                RaycastHit[] hits = Physics.RaycastAll(
                    playerPos,
                    Vector3.up,
                    raycastHeight,
                    roofLayer,
                    QueryTriggerInteraction.Ignore
                );
                
                foreach (RaycastHit hit in hits)
                {
                    Renderer renderer = hit.collider.GetComponent<Renderer>();
                    if (renderer != null && renderer.enabled && hit.point.y > playerPos.y + 0.5f)
                    {
                        currentRoofRenderers.Add(renderer);
                    }
                }
                
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[IsometricFollowCamera] 집 밖에 있음 - 레이캐스트로 {currentRoofRenderers.Count}개 감지 (플레이어 위치: {playerPos})");
                }
            }
            
            // currentRoofRenderers에 있는 모든 Renderer를 투명하게 처리
            foreach (Renderer renderer in currentRoofRenderers)
            {
                if (renderer != null && renderer.enabled)
                {
                    // Mesh Renderer의 모든 Material 처리
                    Material[] originalMats = renderer.sharedMaterials;
                    
                    // 원본 Materials 저장
                    if (!_originalMaterials.ContainsKey(renderer))
                    {
                        _originalMaterials[renderer] = originalMats;
                    }
                    
                    // 투명 Materials 생성 (없으면)
                    if (!_transparentMaterials.ContainsKey(renderer))
                    {
                        Material[] transparentMats = new Material[originalMats.Length];
                        
                        for (int i = 0; i < originalMats.Length; i++)
                        {
                            Material originalMat = originalMats[i];
                            Material transparentMat = new Material(originalMat);
                            
                            // URP Lit Shader 투명화
                            bool isURP = transparentMat.shader.name.Contains("Universal Render Pipeline") || 
                                        transparentMat.shader.name.Contains("URP") ||
                                        transparentMat.shader.name.Contains("Universal");
                            
                            if (isURP)
                            {
                                // URP Lit Shader의 경우 - 모든 투명화 설정
                                transparentMat.SetFloat("_Surface", 1); // 1 = Transparent
                                transparentMat.SetFloat("_Blend", 0); // 0 = Alpha
                                
                                // Base Color Alpha 설정
                                if (transparentMat.HasProperty("_BaseColor"))
                                {
                                    Color baseColor = transparentMat.GetColor("_BaseColor");
                                    baseColor.a = roofTransparency;
                                    transparentMat.SetColor("_BaseColor", baseColor);
                                }
                                
                                // Blend Mode 설정 (Alpha Blend)
                                transparentMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                                transparentMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                
                                // ZWrite 비활성화
                                transparentMat.SetFloat("_ZWrite", 0);
                                
                                // RenderType 태그
                                transparentMat.SetOverrideTag("RenderType", "Transparent");
                                
                                // Render Queue를 Transparent로
                                transparentMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                                
                                // 모든 키워드 비활성화 후 필요한 것만 활성화
                                transparentMat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                                transparentMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                                transparentMat.DisableKeyword("_BLEND_PREMULTIPLY");
                                transparentMat.DisableKeyword("_BLEND_ADD");
                                transparentMat.DisableKeyword("_BLEND_MULTIPLY");
                                transparentMat.EnableKeyword("_BLEND_ALPHA");
                                transparentMat.DisableKeyword("_ALPHATEST_ON");
                                transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                
                                // Material 속성 강제 업데이트
                                transparentMat.SetFloat("_Surface", 1);
                                transparentMat.SetFloat("_Blend", 0);
                            }
                            else
                            {
                                // Built-in Standard Shader
                                transparentMat.SetFloat("_Mode", 3);
                                if (transparentMat.HasProperty("_Color"))
                                {
                                    Color color = transparentMat.color;
                                    color.a = roofTransparency;
                                    transparentMat.color = color;
                                }
                                transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                                transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                                transparentMat.SetInt("_ZWrite", 0);
                                transparentMat.renderQueue = 3000;
                                transparentMat.DisableKeyword("_ALPHATEST_ON");
                                transparentMat.EnableKeyword("_ALPHABLEND_ON");
                                transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            }
                            
                            transparentMats[i] = transparentMat;
                        }
                        
                        _transparentMaterials[renderer] = transparentMats;
                        
                        if (Time.frameCount % 60 == 0)
                        {
                            Debug.Log($"[IsometricFollowCamera] {renderer.name} 투명 Material {transparentMats.Length}개 생성");
                            for (int i = 0; i < transparentMats.Length; i++)
                            {
                                Material mat = transparentMats[i];
                                Color color = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
                                float surface = mat.HasProperty("_Surface") ? mat.GetFloat("_Surface") : -1;
                                Debug.Log($"  Material[{i}]: Shader={mat.shader.name}, Surface={surface}, Alpha={color.a:F2}, Queue={mat.renderQueue}");
                            }
                        }
                    }
                    
                    // 투명 Materials 적용 (모든 Material 배열 적용)
                    renderer.materials = _transparentMaterials[renderer];
                    
                    // Material이 제대로 적용되었는지 확인
                    if (Time.frameCount % 60 == 0 && renderer.materials.Length > 0)
                    {
                        Material firstMat = renderer.materials[0];
                        Color currentColor = firstMat.HasProperty("_BaseColor") 
                            ? firstMat.GetColor("_BaseColor") 
                            : firstMat.color;
                        float surface = firstMat.HasProperty("_Surface") ? firstMat.GetFloat("_Surface") : -1;
                        Debug.Log($"[IsometricFollowCamera] {renderer.name} Material {renderer.materials.Length}개 적용됨 - Surface: {surface}, Alpha: {currentColor.a:F2}, Queue: {firstMat.renderQueue}");
                    }
                }
            }
            
            // 이전 프레임에서 투명했지만 이번 프레임에서는 감지되지 않은 지붕 복원
            List<Renderer> toRestore = new List<Renderer>();
            foreach (var kvp in _transparentMaterials)
            {
                if (!currentRoofRenderers.Contains(kvp.Key))
                {
                    toRestore.Add(kvp.Key);
                }
            }
            
            foreach (Renderer renderer in toRestore)
            {
                if (_originalMaterials.ContainsKey(renderer))
                {
                    renderer.materials = _originalMaterials[renderer];
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 타겟 연결선 표시
            if (target != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, target.position);
            }

            // 범위 제한 박스 표시
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = (boundsMin + boundsMax) * 0.5f;
                Vector3 size = boundsMax - boundsMin;
                Gizmos.DrawWireCube(center, size);
            }

            // 오프셋 시각화
            if (target != null)
            {
                Gizmos.color = Color.green;
                Vector3 targetPos = target.position + offset;
                Gizmos.DrawWireSphere(targetPos, 0.5f);
            }
        }
    }
}

