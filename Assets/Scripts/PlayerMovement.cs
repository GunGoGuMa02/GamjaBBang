using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Visual")]
    public Transform visual;

    [Header("Knockback Settings")]
    [Tooltip("값이 클수록 넉백이 빠르게 멈춥니다.")]
    public float knockbackDamping = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;

    // 공격으로 받은 추가 이동 속도
    private Vector3 knockbackVelocity;

    private StunController stunController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stunController = GetComponent<StunController>();
    }

    private void Update()
    {
        if (stunController != null && stunController.isStunned)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical += 1f;

        moveInput = new Vector2(horizontal, vertical).normalized;

        RotateVisual();
    }

    private void FixedUpdate()
    {
        Vector3 moveVelocity = Vector3.zero;

        // 기절 중에는 입력 이동만 막는다.
        // 넉백은 기절 여부와 관계없이 계속 적용된다.
        if (stunController == null || !stunController.isStunned)
        {
            moveVelocity = new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            ) * moveSpeed;
        }

        Vector3 finalVelocity =
            moveVelocity + knockbackVelocity;

        rb.linearVelocity = new Vector3(
            finalVelocity.x,
            rb.linearVelocity.y,
            finalVelocity.z
        );

        // 넉백 속도를 매 물리 프레임 조금씩 줄인다.
        knockbackVelocity = Vector3.MoveTowards(
            knockbackVelocity,
            Vector3.zero,
            knockbackDamping * Time.fixedDeltaTime
        );
    }

    private void RotateVisual()
    {
        if (moveInput == Vector2.zero)
            return;

        if (visual == null)
            return;

        Vector3 lookDirection = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );

        Quaternion targetRotation =
            Quaternion.LookRotation(lookDirection);

        visual.rotation = Quaternion.Slerp(
            visual.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void AddKnockback(Vector3 direction, float strength)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        knockbackVelocity += direction * strength;

        // 기능 확인용 로그다.
        Debug.Log(
            $"{gameObject.name} 넉백 적용 / 세기: {strength}"
        );
    }
}