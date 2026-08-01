using UnityEngine;

public class PlayerMotionVisual : MonoBehaviour
{
    [Header("References")]
    [Tooltip("코드 기반 흔들림을 적용할 중심 Transform입니다.")]
    public Transform motionPivot;

    [Tooltip("플레이어 이동 스크립트입니다.")]
    public PlayerMovement playerMovement;

    [Tooltip("플레이어 기절 상태를 확인합니다.")]
    public StunController stunController;

    [Tooltip("집게에 잡힌 상태를 확인합니다.")]
    public PlayerGrabController grabController;

    [Header("Basic Motion Feel")]
    [Tooltip("이동 중 몸이 앞으로 기울어지는 각도입니다.")]
    public float forwardLeanAngle = 9f;

    [Tooltip("이동 중 좌우로 흔들리는 각도입니다.")]
    public float sideSwayAngle = 4f;

    [Tooltip("이동 중 위아래로 움직이는 높이입니다.")]
    public float bobHeight = 0.04f;

    [Tooltip("흔들림 속도입니다.")]
    public float bobSpeed = 6f;

    [Tooltip("정지하거나 상태가 바뀔 때 원래 자세로 돌아가는 속도입니다.")]
    public float returnSpeed = 8f;

    [Header("Turn Wobble")]
    [Tooltip("방향 전환 시 몸이 휘청이는 강도입니다.")]
    public float turnWobbleAngle = 10f;

    [Tooltip("방향 전환 휘청임이 사라지는 속도입니다.")]
    public float turnWobbleReturnSpeed = 8f;

    [Tooltip("이 값보다 큰 방향 변화가 있을 때만 휘청임을 발생시킵니다.")]
    public float turnSensitivity = 0.35f;

    [Tooltip("이 속도 이상으로 움직일 때만 방향 전환 휘청임을 적용합니다.")]
    public float minTurnSpeed = 0.4f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Rigidbody rb;
    private float motionTimer;

    private Vector3 previousMoveDirection;
    private Vector3 turnWobble;

    private void Awake()
    {
        if (motionPivot != null)
        {
            originalLocalPosition = motionPivot.localPosition;
            originalLocalRotation = motionPivot.localRotation;
        }

        rb = GetComponent<Rigidbody>();

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (stunController == null)
        {
            stunController = GetComponent<StunController>();
        }

        if (grabController == null)
        {
            grabController = GetComponent<PlayerGrabController>();
        }
    }

    private void LateUpdate()
    {
        if (motionPivot == null)
            return;

        if (rb == null)
        {
            ReturnToOriginal();
            return;
        }

        if (stunController != null && stunController.isStunned)
        {
            ResetMotionState();
            ReturnToOriginal();
            return;
        }

        if (grabController != null && grabController.isGrabbed)
        {
            ResetMotionState();
            ReturnToOriginal();
            return;
        }

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        if (speed < 0.05f)
        {
            motionTimer = 0f;
            previousMoveDirection = Vector3.zero;

            turnWobble = Vector3.Lerp(
                turnWobble,
                Vector3.zero,
                turnWobbleReturnSpeed * Time.deltaTime
            );

            ReturnToOriginal();
            return;
        }

        Vector3 currentMoveDirection = horizontalVelocity.normalized;

        UpdateTurnWobble(currentMoveDirection, speed);

        motionTimer += Time.deltaTime * bobSpeed;

        float speedRatio = Mathf.Clamp01(speed / 5f);

        float bobOffset =
            Mathf.Abs(Mathf.Sin(motionTimer)) *
            bobHeight *
            speedRatio;

        float sway =
            Mathf.Sin(motionTimer) *
            sideSwayAngle *
            speedRatio;

        float forwardLean =
            forwardLeanAngle *
            speedRatio;

        Vector3 targetPosition =
            originalLocalPosition +
            new Vector3(
                0f,
                bobOffset,
                0f
            );

        Quaternion targetRotation =
            originalLocalRotation *
            Quaternion.Euler(
                forwardLean + turnWobble.x,
                0f,
                -sway + turnWobble.z
            );

        motionPivot.localPosition =
            Vector3.Lerp(
                motionPivot.localPosition,
                targetPosition,
                returnSpeed * Time.deltaTime
            );

        motionPivot.localRotation =
            Quaternion.Slerp(
                motionPivot.localRotation,
                targetRotation,
                returnSpeed * Time.deltaTime
            );

        previousMoveDirection = currentMoveDirection;
    }

    private void UpdateTurnWobble(Vector3 currentMoveDirection, float speed)
    {
        if (speed < minTurnSpeed)
        {
            turnWobble = Vector3.Lerp(
                turnWobble,
                Vector3.zero,
                turnWobbleReturnSpeed * Time.deltaTime
            );

            previousMoveDirection = currentMoveDirection;
            return;
        }

        if (previousMoveDirection == Vector3.zero)
        {
            previousMoveDirection = currentMoveDirection;
            return;
        }

        Vector3 directionChange = currentMoveDirection - previousMoveDirection;

        if (directionChange.magnitude > turnSensitivity)
        {
            Vector3 localChange =
                transform.InverseTransformDirection(directionChange);

            turnWobble = new Vector3(
                -localChange.z * turnWobbleAngle,
                0f,
                localChange.x * turnWobbleAngle
            );
        }

        turnWobble = Vector3.Lerp(
            turnWobble,
            Vector3.zero,
            turnWobbleReturnSpeed * Time.deltaTime
        );
    }

    private void ReturnToOriginal()
    {
        motionPivot.localPosition =
            Vector3.Lerp(
                motionPivot.localPosition,
                originalLocalPosition,
                returnSpeed * Time.deltaTime
            );

        motionPivot.localRotation =
            Quaternion.Slerp(
                motionPivot.localRotation,
                originalLocalRotation,
                returnSpeed * Time.deltaTime
            );
    }

    private void ResetMotionState()
    {
        motionTimer = 0f;
        previousMoveDirection = Vector3.zero;
        turnWobble = Vector3.zero;
    }
}