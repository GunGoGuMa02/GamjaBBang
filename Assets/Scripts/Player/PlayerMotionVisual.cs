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
    public float forwardLeanAngle = 10f;

    [Tooltip("이동 중 좌우로 흔들리는 각도입니다.")]
    public float sideSwayAngle = 4f;

    [Tooltip("이동 중 위아래로 움직이는 높이입니다.")]
    public float bobHeight = 0.035f;

    [Tooltip("흔들림 속도입니다.")]
    public float bobSpeed = 5.5f;

    [Tooltip("정지하거나 상태가 바뀔 때 원래 자세로 돌아가는 속도입니다.")]
    public float returnSpeed = 7f;

    [Header("Turn Wobble")]
    [Tooltip("방향 전환 시 몸이 휘청이는 강도입니다.")]
    public float turnWobbleAngle = 18f;

    [Tooltip("방향 전환 휘청임이 사라지는 속도입니다.")]
    public float turnWobbleReturnSpeed = 4f;

    [Tooltip("이 값보다 큰 방향 변화가 있을 때만 휘청임을 발생시킵니다.")]
    public float turnSensitivity = 0.18f;

    [Tooltip("이 속도 이상으로 움직일 때만 방향 전환 휘청임을 적용합니다.")]
    public float minTurnSpeed = 0.2f;

    [Header("Attack Motion")]
    [Tooltip("공격할 때 몸이 앞으로 쏠리는 각도입니다.")]
    public float attackLeanAngle = 16f;

    [Tooltip("공격할 때 몸이 앞으로 살짝 나가는 거리입니다.")]
    public float attackForwardOffset = 0.08f;

    [Tooltip("공격 연출이 유지되는 시간입니다.")]
    public float attackMotionDuration = 0.16f;

    [Tooltip("공격 후 몸이 원래 자세로 돌아오는 속도입니다.")]
    public float attackReturnSpeed = 9f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Rigidbody rb;
    private float motionTimer;

    private Vector3 previousMoveDirection;
    private Vector3 turnWobble;

    private float attackTimer;
    private float attackPower;

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

        UpdateAttackMotion();

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;
        float speedRatio = Mathf.Clamp01(speed / 5f);

        Vector3 currentMoveDirection = Vector3.zero;

        if (speed >= 0.05f)
        {
            currentMoveDirection = horizontalVelocity.normalized;
            UpdateTurnWobble(currentMoveDirection, speed);
            motionTimer += Time.deltaTime * bobSpeed;
        }
        else
        {
            motionTimer = 0f;
            previousMoveDirection = Vector3.zero;

            turnWobble = Vector3.Lerp(
                turnWobble,
                Vector3.zero,
                turnWobbleReturnSpeed * Time.deltaTime
            );
        }

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

        float attackRatio = GetAttackRatio();

        float attackLean =
            attackLeanAngle *
            attackRatio *
            attackPower;

        float attackOffset =
            attackForwardOffset *
            attackRatio *
            attackPower;

        Vector3 targetPosition =
            originalLocalPosition +
            new Vector3(
                0f,
                bobOffset,
                attackOffset
            );

        Quaternion targetRotation =
            originalLocalRotation *
            Quaternion.Euler(
                forwardLean + attackLean + turnWobble.x,
                0f,
                -sway + turnWobble.z
            );

        float finalReturnSpeed = returnSpeed;

        if (attackRatio > 0f)
        {
            finalReturnSpeed = attackReturnSpeed;
        }

        motionPivot.localPosition =
            Vector3.Lerp(
                motionPivot.localPosition,
                targetPosition,
                finalReturnSpeed * Time.deltaTime
            );

        motionPivot.localRotation =
            Quaternion.Slerp(
                motionPivot.localRotation,
                targetRotation,
                finalReturnSpeed * Time.deltaTime
            );

        if (speed >= 0.05f)
        {
            previousMoveDirection = currentMoveDirection;
        }
    }

    public void PlayAttackMotion()
    {
        if (stunController != null && stunController.isStunned)
            return;

        if (grabController != null && grabController.isGrabbed)
            return;

        attackTimer = attackMotionDuration;
        attackPower = 1f;
    }

    private void UpdateAttackMotion()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                attackTimer = 0f;
            }
        }

        attackPower = Mathf.Lerp(
            attackPower,
            0f,
            attackReturnSpeed * Time.deltaTime
        );
    }

    private float GetAttackRatio()
    {
        if (attackMotionDuration <= 0f)
            return 0f;

        float normalizedTime = attackTimer / attackMotionDuration;

        return Mathf.Clamp01(normalizedTime);
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
        attackTimer = 0f;
        attackPower = 0f;
    }
}