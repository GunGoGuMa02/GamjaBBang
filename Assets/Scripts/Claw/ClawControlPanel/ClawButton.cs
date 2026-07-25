using UnityEngine;
using UnityEngine.InputSystem;

public class ClawButton : MonoBehaviour
{
    [Header("Claw")]
    [Tooltip("작동시킬 집게 이동 스크립트입니다.")]
    public ClawMovement clawMovement;

    [Tooltip("집게 아래 감지 구역입니다.")]
    public ClawDetectionTrigger detectionTrigger;

    [Header("Input")]
    [Tooltip("버튼을 누를 수 있는 플레이어가 범위 안에 있는지 표시합니다.")]
    public bool hasPlayerInRange = false;

    private void Update()
    {
        if (!hasPlayerInRange)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        if (clawMovement == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: ClawMovement가 연결되지 않았습니다."
            );

            return;
        }

        if (detectionTrigger == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Detection Trigger가 연결되지 않았습니다."
            );

            return;
        }

        if (detectionTrigger.DetectedPlayers.Count <= 0)
        {
            Debug.Log(
                "집게 아래 감지 구역에 플레이어가 없어서 집게가 작동하지 않습니다."
            );

            return;
        }

        Debug.Log(
            $"집게 버튼을 눌렀습니다. 감지된 플레이어 수: {detectionTrigger.DetectedPlayers.Count}"
        );

        clawMovement.StartMovement();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentity playerIdentity =
            other.GetComponentInParent<PlayerIdentity>();

        if (playerIdentity == null)
            return;

        hasPlayerInRange = true;

        Debug.Log(
            $"{playerIdentity.gameObject.name}이 버튼 범위에 들어왔습니다."
        );
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentity playerIdentity =
            other.GetComponentInParent<PlayerIdentity>();

        if (playerIdentity == null)
            return;

        hasPlayerInRange = false;

        Debug.Log(
            $"{playerIdentity.gameObject.name}이 버튼 범위에서 나갔습니다."
        );
    }
}