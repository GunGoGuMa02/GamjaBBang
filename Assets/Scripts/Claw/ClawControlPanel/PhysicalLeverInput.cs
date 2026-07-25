using UnityEngine;

public class PhysicalLeverInput : MonoBehaviour
{
    [Header("Lever Axis")]
    [Tooltip("읽을 로컬 회전 축입니다. 좌우 레버는 Z를 사용합니다.")]
    public Vector3 localAxis = Vector3.forward;

    [Tooltip("레버가 최대로 기울어지는 각도입니다.")]
    public float maxAngle = 25f;

    [Header("Output")]
    [Tooltip("현재 레버 입력값입니다. -1 ~ 1 사이입니다.")]
    [Range(-1f, 1f)]
    public float value;

    private void Update()
    {
        Vector3 localEulerAngles =
            transform.localEulerAngles;

        float angle = 0f;

        if (Mathf.Abs(localAxis.x) > 0.5f)
        {
            angle = NormalizeAngle(localEulerAngles.x);
        }
        else if (Mathf.Abs(localAxis.y) > 0.5f)
        {
            angle = NormalizeAngle(localEulerAngles.y);
        }
        else
        {
            angle = NormalizeAngle(localEulerAngles.z);
        }

        value = Mathf.Clamp(
            angle / maxAngle,
            -1f,
            1f
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}