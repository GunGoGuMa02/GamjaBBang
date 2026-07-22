using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [Header("Grab State")]
    [Tooltip("현재 플레이어가 집게에 잡혀 있는지 표시합니다.")]
    public bool isGrabbed = false;

    [Header("Grab Position")]
    [Tooltip("GrabPoint를 기준으로 플레이어 위치를 추가 조절합니다.")]
    public Vector3 grabPositionOffset = Vector3.zero;

    private Transform currentGrabPoint;
    private Rigidbody rb;

    private bool originalUseGravity;
    private bool originalIsKinematic;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: PlayerGrabController가 Rigidbody를 찾지 못했습니다."
            );
        }
    }

    private void LateUpdate()
    {
        if (!isGrabbed)
            return;

        if (currentGrabPoint == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: 잡힌 상태지만 GrabPoint가 없습니다."
            );

            Release();
            return;
        }

        transform.position =
            currentGrabPoint.position +
            currentGrabPoint.TransformDirection(grabPositionOffset);
    }

    public void Grab(Transform grabPoint)
    {
        if (isGrabbed)
            return;

        if (grabPoint == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: 전달받은 GrabPoint가 없습니다."
            );

            return;
        }

        currentGrabPoint = grabPoint;
        isGrabbed = true;

        if (rb != null)
        {
            originalUseGravity = rb.useGravity;
            originalIsKinematic = rb.isKinematic;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        transform.position =
            currentGrabPoint.position +
            currentGrabPoint.TransformDirection(grabPositionOffset);

        Debug.Log(
            $"{gameObject.name}이 집게에 잡혔습니다."
        );
    }

    public void Release()
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        currentGrabPoint = null;

        if (rb != null)
        {
            rb.isKinematic = originalIsKinematic;
            rb.useGravity = originalUseGravity;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log(
            $"{gameObject.name}이 집게에서 풀려났습니다."
        );
    }
}