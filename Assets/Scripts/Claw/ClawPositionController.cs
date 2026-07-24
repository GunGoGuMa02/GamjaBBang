using UnityEngine;
using UnityEngine.InputSystem;

public class ClawPositionController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("실제로 움직일 집게 오브젝트입니다.")]
    public Transform claw;

    [Tooltip("집게의 이동/작동 상태를 관리하는 ClawMovement입니다.")]
    public ClawMovement clawMovement;

    [Tooltip("집게 아래의 감지 구역 오브젝트입니다.")]
    public Transform targetZone;

    [Header("Movement")]
    [Tooltip("집게가 좌우/앞뒤로 이동하는 속도입니다.")]
    public float moveSpeed = 3f;

    [Tooltip("집게가 움직일 수 있는 최소 X 위치입니다.")]
    public float minX = -6f;

    [Tooltip("집게가 움직일 수 있는 최대 X 위치입니다.")]
    public float maxX = 6f;

    [Tooltip("집게가 움직일 수 있는 최소 Z 위치입니다.")]
    public float minZ = -6f;

    [Tooltip("집게가 움직일 수 있는 최대 Z 위치입니다.")]
    public float maxZ = 6f;

    [Header("Target Zone")]
    [Tooltip("TargetZone의 Y 위치입니다. 바닥 근처 높이로 유지합니다.")]
    public float targetZoneY = 0.05f;

    private void Update()
    {
        if (claw == null)
            return;

        if (Keyboard.current == null)
            return;

        if (clawMovement != null && clawMovement.IsMoving)
        {
            UpdateTargetZonePosition();
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current.jKey.isPressed)
        {
            moveDirection.x -= 1f;
        }

        if (Keyboard.current.lKey.isPressed)
        {
            moveDirection.x += 1f;
        }

        if (Keyboard.current.iKey.isPressed)
        {
            moveDirection.z += 1f;
        }

        if (Keyboard.current.kKey.isPressed)
        {
            moveDirection.z -= 1f;
        }

        if (moveDirection == Vector3.zero)
        {
            UpdateTargetZonePosition();
            return;
        }

        moveDirection.Normalize();

        Vector3 nextClawPosition =
            claw.position + moveDirection * moveSpeed * Time.deltaTime;

        nextClawPosition.x = Mathf.Clamp(
            nextClawPosition.x,
            minX,
            maxX
        );

        nextClawPosition.z = Mathf.Clamp(
            nextClawPosition.z,
            minZ,
            maxZ
        );

        claw.position = nextClawPosition;

        UpdateTargetZonePosition();
    }

    private void UpdateTargetZonePosition()
    {
        if (targetZone == null || claw == null)
            return;

        targetZone.position = new Vector3(
            claw.position.x,
            targetZoneY,
            claw.position.z
        );
    }
}