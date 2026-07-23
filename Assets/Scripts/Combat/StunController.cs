using System.Collections;
using UnityEngine;

public class StunController : MonoBehaviour
{
    [Header("Stun Settings")]
    public float maxStun = 100f;
    public float stunDuration = 3f;

    [Header("Current Stun")]
    public float currentStun = 0f;

    [Header("Stun State")]
    public bool isStunned = false;

    [Header("Stun Visual")]
    public Transform stunPivot;

    public float wobbleAngle = 20f;
    public float fallAngle = 90f;

    public float wobbleDuration = 0.08f;
    public float fallDuration = 0.25f;
    public float bounceDuration = 0.12f;
    public float recoveryDuration = 0.35f;

    private Quaternion originalRotation;
    private Coroutine stunCoroutine;

    private void Start()
    {
        if (stunPivot != null)
        {
            originalRotation = stunPivot.localRotation;
        }
    }

    public void AddStun(float amount)
    {
        if (isStunned)
            return;

        currentStun += amount;

        if (currentStun >= maxStun)
        {
            currentStun = maxStun;
            EnterStun();
        }
    }

    private void EnterStun()
    {
        if (isStunned)
            return;

        isStunned = true;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        if (stunPivot == null)
        {
            yield return new WaitForSeconds(stunDuration);

            currentStun = 0f;
            isStunned = false;
            stunCoroutine = null;

            yield break;
        }

        Quaternion wobbleRotation =
            originalRotation * Quaternion.Euler(0f, 0f, wobbleAngle);

        Quaternion fallenRotation =
            originalRotation * Quaternion.Euler(0f, 0f, fallAngle);

        Quaternion bounceRotation =
            originalRotation * Quaternion.Euler(0f, 0f, fallAngle - 8f);

        yield return RotatePivot(
            originalRotation,
            wobbleRotation,
            wobbleDuration
        );

        yield return RotatePivot(
            wobbleRotation,
            fallenRotation,
            fallDuration
        );

        yield return RotatePivot(
            fallenRotation,
            bounceRotation,
            bounceDuration / 2f
        );

        yield return RotatePivot(
            bounceRotation,
            fallenRotation,
            bounceDuration / 2f
        );

        yield return new WaitForSeconds(stunDuration);

        yield return RotatePivot(
            fallenRotation,
            originalRotation,
            recoveryDuration
        );

        stunPivot.localRotation = originalRotation;

        currentStun = 0f;
        isStunned = false;
        stunCoroutine = null;
    }

    private IEnumerator RotatePivot(
        Quaternion startRotation,
        Quaternion endRotation,
        float duration
    )
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsedTime / duration);

            stunPivot.localRotation = Quaternion.Lerp(
                startRotation,
                endRotation,
                progress
            );

            yield return null;
        }

        stunPivot.localRotation = endRotation;
    }
}