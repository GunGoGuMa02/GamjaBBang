using System.Collections;
using UnityEngine;

public class HitReaction : MonoBehaviour
{
    [Header("Visual")]
    public Transform visual;

    [Header("Hit Reaction Settings")]
    [Tooltip("피격 시 몸이 기울어지는 최대 각도입니다.")]
    public float tiltAngle = 35f;

    [Tooltip("전체 피격 모션이 끝나는 시간입니다.")]
    public float reactionDuration = 0.18f;

    [Range(0.1f, 0.8f)]
    [Tooltip("전체 시간 중 몸이 빠르게 기울어지는 구간의 비율입니다.")]
    public float tiltTimeRatio = 0.3f;

    private Quaternion originalRotation;
    private Coroutine reactionCoroutine;

    private void Start()
    {
        if (visual != null)
        {
            originalRotation = visual.localRotation;
        }
    }

    public void PlayHitReaction(Vector3 hitDirection)
    {
        if (visual == null)
            return;

        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
        }

        reactionCoroutine =
            StartCoroutine(HitReactionRoutine(hitDirection));
    }

    private IEnumerator HitReactionRoutine(Vector3 hitDirection)
    {
        hitDirection.y = 0f;

        if (hitDirection.sqrMagnitude <= 0.001f)
        {
            hitDirection = transform.forward;
        }

        hitDirection.Normalize();

        Vector3 localHitDirection;

        if (visual.parent != null)
        {
            localHitDirection =
                visual.parent.InverseTransformDirection(hitDirection);
        }
        else
        {
            localHitDirection = hitDirection;
        }

        localHitDirection.y = 0f;
        localHitDirection.Normalize();

        Vector3 tiltAxis =
            Vector3.Cross(Vector3.up, localHitDirection);

        if (tiltAxis.sqrMagnitude <= 0.001f)
        {
            tiltAxis = Vector3.right;
        }

        tiltAxis.Normalize();

        Quaternion currentRotation = visual.localRotation;

        Quaternion tiltedRotation =
            originalRotation *
            Quaternion.AngleAxis(tiltAngle, tiltAxis);

        float tiltDuration =
            reactionDuration * tiltTimeRatio;

        float recoveryDuration =
            reactionDuration - tiltDuration;

        float elapsedTime = 0f;

        while (elapsedTime < tiltDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / tiltDuration);

            progress = Mathf.SmoothStep(0f, 1f, progress);

            visual.localRotation = Quaternion.Lerp(
                currentRotation,
                tiltedRotation,
                progress
            );

            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < recoveryDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / recoveryDuration);

            progress = Mathf.SmoothStep(0f, 1f, progress);

            visual.localRotation = Quaternion.Lerp(
                tiltedRotation,
                originalRotation,
                progress
            );

            yield return null;
        }

        visual.localRotation = originalRotation;
        reactionCoroutine = null;
    }
}