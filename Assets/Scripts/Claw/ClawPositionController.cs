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

    [Header("Lever Visuals")]
    [Tooltip("좌우 이동 레버의 회전 중심입니다.")]
    public Transform horizontalLeverPivot;

    [Tooltip("앞뒤 이동 레버의 회전 중심입니다.")]
    public Transform verticalLeverPivot;

    [Tooltip("레버가 최대로 기울어지는 각도입니다.")]
    public float leverTiltAngle = 25f;

    [Tooltip("레버가 목표 각도로 움직이는 속도입니다.")]
    public float leverReturnSpeed = 10f;

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

    private float horizontalInput;
    private float verticalInput;

    private void Update()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (claw == null)
            return;

        if (Keyboard.current == null)
            return;

        bool canMoveClaw =
            clawMovement == null ||
            clawMovement.IsMoving == false;

        if (canMoveClaw)
        {
            ReadKeyboardInput();
            MoveClaw();
        }

        UpdateTargetZonePosition();
        UpdateLeverVisuals();
    }

    private void ReadKeyboardInput()
    {
        if (Keyboard.current.jKey.isPressed)
        {
            horizontalInput -= 1f;
        }

        if (Keyboard.current.lKey.isPressed)
        {
            horizontalInput += 1f;
        }

        if (Keyboard.current.iKey.isPressed)
        {
            verticalInput += 1f;
        }

        if (Keyboard.current.kKey.isPressed)
        {
            verticalInput -= 1f;
        }
    }

    private void MoveClaw()
    {
        Vector3 moveDirection = new Vector3(
            horizontalInput,
            0f,
            verticalInput
        );

        if (moveDirection == Vector3.zero)
            return;

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

    private void UpdateLeverVisuals()
    {
        if (horizontalLeverPivot != null)
        {
            Quaternion targetRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -horizontalInput * leverTiltAngle
                );

            horizontalLeverPivot.localRotation =
                Quaternion.Slerp(
                    horizontalLeverPivot.localRotation,
                    targetRotation,
                    leverReturnSpeed * Time.deltaTime
                );
        }

        if (verticalLeverPivot != null)
        {
            Quaternion targetRotation =
                Quaternion.Euler(
                    verticalInput * leverTiltAngle,
                    0f,
                    0f
                );

            verticalLeverPivot.localRotation =
                Quaternion.Slerp(
                    verticalLeverPivot.localRotation,
                    targetRotation,
                    leverReturnSpeed * Time.deltaTime
                );
        }
    }
}