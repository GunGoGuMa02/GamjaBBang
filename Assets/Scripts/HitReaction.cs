using System.Collections;
using UnityEngine;

public class HitReaction : MonoBehaviour
{
    public Transform visual;

    public float tiltAngle = 20f;
    public float reactionDuration = 0.15f;

    private Quaternion originalRotation;
    private Coroutine reactionCoroutine;

    private void Start()
    {
        originalRotation = visual.localRotation;
    }

    public void PlayHitReaction()
    {
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
        }

        reactionCoroutine = StartCoroutine(HitReactionRoutine());
    }

    private IEnumerator HitReactionRoutine()
    {
        Quaternion tiltedRotation =
            originalRotation * Quaternion.Euler(0f, 0f, tiltAngle);

        float elapsedTime = 0f;

        while (elapsedTime < reactionDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                elapsedTime / reactionDuration;

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