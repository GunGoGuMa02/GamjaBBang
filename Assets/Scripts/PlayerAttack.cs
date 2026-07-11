using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform visual;
    public float attackDistance = 1.2f;
    public float attackRadius = 0.8f;
    public float stunDamage = 20f;

    [Header("Attack Motion")]
    public Transform body;
    public float leanAngle = 15f;
    public float motionDuration = 0.12f;

    private Quaternion originalBodyRotation;
    private Coroutine attackMotionCoroutine;
    private StunController stunController;

    private void Start()
    {
        if (body != null)
        {
            originalBodyRotation = body.localRotation;
        }

        stunController = GetComponent<StunController>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (stunController != null && stunController.isStunned)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            PerformAttack();
            PlayAttackMotion();
        }
    }

    private void PerformAttack()
    {
        if (visual == null)
            return;

        Vector3 attackCenter =
            transform.position + visual.forward * attackDistance;

        Collider[] hitColliders =
            Physics.OverlapSphere(attackCenter, attackRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.transform.root == transform.root)
                continue;

            HitReaction hitReaction =
                hitCollider.GetComponent<HitReaction>();

            if (hitReaction != null)
            {
                hitReaction.PlayHitReaction();
            }

            StunController targetStunController =
                hitCollider.GetComponent<StunController>();

            if (targetStunController != null)
            {
                targetStunController.AddStun(stunDamage);
            }
        }
    }

    private void PlayAttackMotion()
    {
        if (body == null)
            return;

        if (attackMotionCoroutine != null)
        {
            StopCoroutine(attackMotionCoroutine);
        }

        attackMotionCoroutine =
            StartCoroutine(AttackMotionRoutine());
    }

    private IEnumerator AttackMotionRoutine()
    {
        Quaternion leanedRotation =
            originalBodyRotation * Quaternion.Euler(leanAngle, 0f, 0f);

        float halfDuration = motionDuration / 2f;
        float elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / halfDuration;

            body.localRotation = Quaternion.Lerp(
                originalBodyRotation,
                leanedRotation,
                progress
            );

            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / halfDuration;

            body.localRotation = Quaternion.Lerp(
                leanedRotation,
                originalBodyRotation,
                progress
            );

            yield return null;
        }

        body.localRotation = originalBodyRotation;
        attackMotionCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        if (visual == null)
            return;

        Vector3 attackCenter =
            transform.position + visual.forward * attackDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);
    }
}