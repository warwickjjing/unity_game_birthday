using UnityEngine;
using UnityEngine.Playables;

namespace BirthdayCakeQuest.Cutscene
{
    /// <summary>
    /// Timeline Signal을 수신하여 이벤트를 발생시킵니다.
    /// (현재는 EndingCutsceneController가 직접 처리하므로 선택적 사용)
    /// </summary>
    public sealed class EndingSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        [Header("References")]
        [Tooltip("엔딩 컷씬 컨트롤러")]
        [SerializeField] private EndingCutsceneController cutsceneController;

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            // Timeline의 Marker나 SignalEmitter를 사용할 때 작동
            // 현재는 EndingCutsceneController.OnDirectorStopped가 모든 것을 처리
            Debug.Log($"[EndingSignalReceiver] Received notification: {notification?.GetType().Name ?? "null"}");
            
            // 필요시 여기에 추가 로직 구현 가능
            if (cutsceneController != null)
            {
                // 예: cutsceneController.OnSignalReceived();
            }
        }
    }
}

