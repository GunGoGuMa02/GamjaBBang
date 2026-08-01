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

    [Header("Motion Feel")]
    [Tooltip("이동 중 몸이 앞으로 기울어지는 각도입니다.")]
    public float forwardLeanAngle = 8f;

    [Tooltip("이동 중 좌우로 흔들리는 각도입니다.")]
    public float sideSwayAngle = 5f;

    [Tooltip("이동 중 위아래로 움직이는 높이입니다.")]
    public float bobHeight = 0.06f;

    [Tooltip("흔들림 속도입니다.")]
    public float bobSpeed = 8f;

    [Tooltip("정지하거나 상태가 바뀔 때 원래 자세로 돌아가는 속도입니다.")]
    public float returnSpeed = 10f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Rigidbody rb;
    private float motionTimer;

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
            ReturnToOriginal();
            return;
        }

        if (grabController != null && grabController.isGrabbed)
        {
            ReturnToOriginal();
            return;
        }

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        if (speed < 0.05f)
        {
            motionTimer = 0f;
            ReturnToOriginal();
            return;
        }

        motionTimer += Time.deltaTime * bobSpeed;

        float speedRatio = Mathf.Clamp01(
            speed / 5f
        );

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
                forwardLean,
                0f,
                -sway
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
}