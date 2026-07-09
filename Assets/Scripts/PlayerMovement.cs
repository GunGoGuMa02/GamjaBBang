using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    public Transform visual;

    private Rigidbody rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current == null)
        {
            moveInput = Vector2.zero;
            return;
        }

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

    void FixedUpdate()
    {
        Vector3 moveVelocity = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        ) * moveSpeed;

        rb.linearVelocity = new Vector3(
            moveVelocity.x,
            rb.linearVelocity.y,
            moveVelocity.z
        );
    }

    void RotateVisual()
    {
        if (moveInput == Vector2.zero)
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
}