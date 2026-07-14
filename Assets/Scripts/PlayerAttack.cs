using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform visual;
    public float attackDistance = 1.2f;
    public float attackRadius = 0.8f;
    public float stunDamage = 20f;

    [Header("Knockback Settings")]
    [Tooltip("일반 공격으로 상대가 실제로 밀리는 세기입니다.")]
    public float knockbackStrength = 8f;

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
            transform.position +
            visual.forward * attackDistance;

        Collider[] hitColliders =
            Physics.OverlapSphere(
                attackCenter,
                attackRadius
            );

        // 대상에게 Collider가 여러 개 있어도
        // 한 번의 공격으로 한 번만 맞게 한다.
        HashSet<Transform> processedTargets =
            new HashSet<Transform>();

        foreach (Collider hitCollider in hitColliders)
        {
            Transform targetRoot =
                hitCollider.transform.root;

            // 자기 자신은 공격 대상에서 제외한다.
            if (targetRoot == transform.root)
                continue;

            if (processedTargets.Contains(targetRoot))
                continue;

            processedTargets.Add(targetRoot);

            // 공격자에서 피격자를 향하는 수평 방향
            Vector3 hitDirection =
                targetRoot.position - transform.position;

            hitDirection.y = 0f;

            if (hitDirection.sqrMagnitude <= 0.001f)
            {
                hitDirection = visual.forward;
                hitDirection.y = 0f;
            }

            hitDirection.Normalize();

            // 기존 방향별 피격 모션
            HitReaction hitReaction =
                targetRoot.GetComponent<HitReaction>();

            if (hitReaction != null)
            {
                hitReaction.PlayHitReaction(
                    hitDirection
                );
            }

            // 실제 넉백
            PlayerMovement targetMovement =
                targetRoot.GetComponent<PlayerMovement>();

            if (targetMovement != null)
            {
                targetMovement.AddKnockback(
                    hitDirection,
                    knockbackStrength
                );
            }
            else
            {
                Debug.LogWarning(
                    $"{targetRoot.name}에서 PlayerMovement를 찾지 못했습니다."
                );
            }

            // 기존 기절 수치 증가
            StunController targetStunController =
                targetRoot.GetComponent<StunController>();

            if (targetStunController != null)
            {
                targetStunController.AddStun(
                    stunDamage
                );
            }
        }
    }

    private void PlayAttackMotion()
    {
        if (body == null)
            return;

        if (attackMotionCoroutine != null)
        {
            StopCoroutine(
                attackMotionCoroutine
            );
        }

        attackMotionCoroutine =
            StartCoroutine(
                AttackMotionRoutine()
            );
    }

    private IEnumerator AttackMotionRoutine()
    {
        Quaternion leanedRotation =
            originalBodyRotation *
            Quaternion.Euler(
                leanAngle,
                0f,
                0f
            );

        float halfDuration =
            motionDuration / 2f;

        float elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsedTime / halfDuration
                );

            body.localRotation =
                Quaternion.Lerp(
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

            float progress =
                Mathf.Clamp01(
                    elapsedTime / halfDuration
                );

            body.localRotation =
                Quaternion.Lerp(
                    leanedRotation,
                    originalBodyRotation,
                    progress
                );

            yield return null;
        }

        body.localRotation =
            originalBodyRotation;

        attackMotionCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        if (visual == null)
            return;

        Vector3 attackCenter =
            transform.position +
            visual.forward * attackDistance;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRadius
        );
    }
}