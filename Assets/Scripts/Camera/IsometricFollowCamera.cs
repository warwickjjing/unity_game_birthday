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
        
        [Tooltip("완전히 안보이게 할지 여부 (true=invisible, false=투명)")]
        [SerializeField] private bool makeRoofsInvisible = true;
        
        [Tooltip("집 내부 영역 (플레이어가 이 영역 안에 있으면 지붕 투명화 유지)")]
        [SerializeField] private bool useInteriorBounds = true;
        
        [Tooltip("집 내부 최소 위치")]
        [SerializeField] private Vector3 interiorMin = new Vector3(-50f, 0f, -50f);
        
        [Tooltip("집 내부 최대 위치")]
        [SerializeField] private Vector3 interiorMax = new Vector3(50f, 10f, 50f);
        
        [Tooltip("집 내부 영역 자동 감지 (Floor 오브젝트의 Bounds 사용)")]
        [SerializeField] private bool autoDetectInteriorBounds = true;

        [Header("Wall Transparency")]
        [Tooltip("벽 투명화 사용 여부")]
        [SerializeField] private bool useWallTransparency = true;
        
        [Tooltip("벽 레이어 (계단 등은 제외)")]
        [SerializeField] private LayerMask wallLayer = 0;
        
        [Tooltip("완전히 안보이게 할지 여부 (true=invisible, false=투명)")]
        [SerializeField] private bool makeWallsInvisible = true;
        
        [Tooltip("벽 투명도 (0=완전 투명, 1=불투명, makeWallsInvisible=false일 때만 사용)")]
        [SerializeField] private float wallTransparency = 0.3f;

        private Vector3 _currentVelocity;
        private bool _isPaused = false;
        private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> _transparentMaterials = new Dictionary<Renderer, Material[]>();
        private List<Renderer> _allRoofRenderers = new List<Renderer>(); // Scene의 모든 지붕 Renderer
        private bool _roofRenderersInitialized = false;
        private HashSet<Renderer> _hiddenRoofRenderers = new HashSet<Renderer>(); // invisible로 숨긴 지붕들
        
        // 벽 투명화용 별도 Dictionary (지붕과 분리)
        private Dictionary<Renderer, Material[]> _originalWallMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> _transparentWallMaterials = new Dictionary<Renderer, Material[]>();
        private HashSet<Renderer> _hiddenWallRenderers = new HashSet<Renderer>(); // invisible로 숨긴 벽들

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
        }

        private void LateUpdate()
        {
            if (target == null || _isPaused)
                return;

            UpdateCameraPosition();
            UpdateCameraRotation();
            UpdateRoofTransparency();
            UpdateWallTransparency();
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
            }
            
            // 집 안에 있으면 모든 지붕을 투명하게, 밖에 있으면 레이캐스트로 감지된 것만
            HashSet<Renderer> currentRoofRenderers = new HashSet<Renderer>();
            
            if (isInsideInterior && useInteriorBounds)
            {
                // 집 안에 있으면 모든 지붕을 투명하게
                foreach (Renderer renderer in _allRoofRenderers)
                {
                    if (renderer != null)
                    {
                        // enabled 상태와 관계없이 추가 (이미 숨겨진 것도 포함)
                        currentRoofRenderers.Add(renderer);
                    }
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
                    if (renderer != null && hit.point.y > playerPos.y + 0.5f)
                    {
                        // enabled 상태와 관계없이 추가 (이미 숨겨진 것도 포함)
                        currentRoofRenderers.Add(renderer);
                    }
                }
                
            }
            
            // currentRoofRenderers에 있는 모든 Renderer를 처리
            if (makeRoofsInvisible)
            {
                // 완전히 안보이게 (Renderer.enabled = false)
                foreach (Renderer renderer in currentRoofRenderers)
                {
                    if (renderer != null)
                    {
                        // 이미 숨겨진 것은 건너뛰기 (깜빡임 방지)
                        if (!_hiddenRoofRenderers.Contains(renderer))
                        {
                            renderer.enabled = false;
                            _hiddenRoofRenderers.Add(renderer);
                        }
                    }
                }
            }
            else
            {
                // 투명하게 처리 (기존 로직)
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
                        }
                        
                        // 투명 Materials 적용 (모든 Material 배열 적용)
                        renderer.materials = _transparentMaterials[renderer];
                    }
                }
            }
            
            // 이전 프레임에서 처리했지만 이번 프레임에서는 감지되지 않은 지붕 복원
            if (makeRoofsInvisible)
            {
                // invisible로 숨긴 지붕들 복원
                List<Renderer> toRestore = new List<Renderer>();
                foreach (Renderer renderer in _hiddenRoofRenderers)
                {
                    // currentRoofRenderers에 없으면 복원 대상
                    if (!currentRoofRenderers.Contains(renderer))
                    {
                        toRestore.Add(renderer);
                    }
                }

                foreach (Renderer renderer in toRestore)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                        _hiddenRoofRenderers.Remove(renderer);
                    }
                }
            }
            else
            {
                // 투명 Material 복원
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
        }

        /// <summary>
        /// 카메라와 플레이어 사이에 벽이 있으면 투명하게 만들거나 완전히 숨깁니다.
        /// </summary>
        private void UpdateWallTransparency()
        {
            if (!useWallTransparency || target == null || wallLayer == 0)
            {
                // 비활성화 시 모든 벽 복원
                RestoreAllWalls();
                return;
            }

            Vector3 cameraPos = transform.position;
            Vector3 playerPos = target.position;
            Vector3 direction = (playerPos - cameraPos).normalized;
            float distance = Vector3.Distance(cameraPos, playerPos);

            // 카메라에서 플레이어로 레이캐스트 (모든 벽 감지)
            RaycastHit[] hits = Physics.RaycastAll(
                cameraPos,
                direction,
                distance,
                wallLayer,
                QueryTriggerInteraction.Ignore
            );

            // 현재 프레임에서 감지된 벽 Renderer들
            HashSet<Renderer> currentWallRenderers = new HashSet<Renderer>();

            foreach (RaycastHit hit in hits)
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    currentWallRenderers.Add(renderer);
                }
            }

            // currentWallRenderers에 있는 모든 Renderer를 처리
            if (makeWallsInvisible)
            {
                // 완전히 안보이게 (Renderer.enabled = false)
                foreach (Renderer renderer in currentWallRenderers)
                {
                    if (renderer != null && renderer.enabled)
                    {
                        renderer.enabled = false;
                        _hiddenWallRenderers.Add(renderer);
                    }
                }
            }
            else
            {
                // 투명하게 처리 (기존 로직)
                foreach (Renderer renderer in currentWallRenderers)
                {
                    if (renderer != null && renderer.enabled)
                    {
                        // Mesh Renderer의 모든 Material 처리
                        Material[] originalMats = renderer.sharedMaterials;

                        // 원본 Materials 저장
                        if (!_originalWallMaterials.ContainsKey(renderer))
                        {
                            _originalWallMaterials[renderer] = originalMats;
                        }

                        // 투명 Materials 생성 (없으면)
                        if (!_transparentWallMaterials.ContainsKey(renderer))
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
                                        baseColor.a = wallTransparency;
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
                                        color.a = wallTransparency;
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

                            _transparentWallMaterials[renderer] = transparentMats;
                        }

                        // 투명 Materials 적용 (모든 Material 배열 적용)
                        renderer.materials = _transparentWallMaterials[renderer];
                    }
                }
            }

            // 이전 프레임에서 처리했지만 이번 프레임에서는 감지되지 않은 벽 복원
            if (makeWallsInvisible)
            {
                // invisible로 숨긴 벽들 복원
                List<Renderer> toRestore = new List<Renderer>();
                foreach (Renderer renderer in _hiddenWallRenderers)
                {
                    if (!currentWallRenderers.Contains(renderer))
                    {
                        toRestore.Add(renderer);
                    }
                }

                foreach (Renderer renderer in toRestore)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                        _hiddenWallRenderers.Remove(renderer);
                    }
                }
            }
            else
            {
                // 투명 Material 복원
                List<Renderer> toRestore = new List<Renderer>();
                foreach (var kvp in _transparentWallMaterials)
                {
                    if (!currentWallRenderers.Contains(kvp.Key))
                    {
                        toRestore.Add(kvp.Key);
                    }
                }

                foreach (Renderer renderer in toRestore)
                {
                    if (_originalWallMaterials.ContainsKey(renderer))
                    {
                        renderer.materials = _originalWallMaterials[renderer];
                    }
                }
            }
        }

        /// <summary>
        /// 모든 벽을 복원합니다 (비활성화 시 호출).
        /// </summary>
        private void RestoreAllWalls()
        {
            // invisible로 숨긴 벽들 복원
            foreach (Renderer renderer in _hiddenWallRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
            _hiddenWallRenderers.Clear();

            // 투명 Material 복원
            foreach (var kvp in _transparentWallMaterials)
            {
                if (_originalWallMaterials.ContainsKey(kvp.Key))
                {
                    kvp.Key.materials = _originalWallMaterials[kvp.Key];
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

