using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform visual;

    [Tooltip("공격 판정이 캐릭터 앞쪽으로 얼마나 떨어져 있는지입니다.")]
    public float attackDistance = 0.8f;

    [Tooltip("공격 판정 구체의 크기입니다.")]
    public float attackRadius = 1.0f;

    [Tooltip("공격 판정 위치를 추가로 보정합니다. X=좌우, Y=높이, Z=앞뒤")]
    public Vector3 attackOffset = new Vector3(0f, 1.2f, 0f);

    [Tooltip("플레이어 또는 집게에 쌓이는 기절 수치입니다.")]
    public float stunDamage = 20f;

    [Header("Knockback Settings")]
    [Tooltip("일반 공격으로 상대가 실제로 밀리는 세기입니다.")]
    public float knockbackStrength = 2f;

    [Header("Attack Motion")]
    public Transform body;
    public float leanAngle = 20f;
    public float motionDuration = 0.3f;

    private Quaternion originalBodyRotation;
    private Coroutine attackMotionCoroutine;
    private StunController stunController;
    private PlayerGrabController grabController;
    private PlayerMotionVisual motionVisual;

    private void Start()
    {
        if (body != null)
        {
            originalBodyRotation = body.localRotation;
        }

        stunController = GetComponent<StunController>();
        grabController = GetComponent<PlayerGrabController>();
        motionVisual = GetComponent<PlayerMotionVisual>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (stunController != null && stunController.isStunned)
            return;

        if (grabController != null && grabController.isGrabbed)
            return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            PerformAttack();
            PlayAttackMotion();
            PlayMotionPivotAttackMotion();
        }
    }

    private void PerformAttack()
    {
        if (visual == null)
            return;

        Vector3 attackCenter = GetAttackCenter();

        Collider[] hitColliders =
            Physics.OverlapSphere(
                attackCenter,
                attackRadius
            );

        HashSet<Transform> processedTargets =
            new HashSet<Transform>();

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider == null)
                continue;

            if (hitCollider.transform.root == transform.root)
                continue;

            PlayerIdentity playerIdentity =
                hitCollider.GetComponentInParent<PlayerIdentity>();

            ClawStunController clawStunController =
                hitCollider.GetComponentInParent<ClawStunController>();

            Transform targetRoot = null;

            if (playerIdentity != null)
            {
                targetRoot = playerIdentity.transform.root;
            }
            else if (clawStunController != null)
            {
                targetRoot = clawStunController.transform.root;
            }
            else
            {
                continue;
            }

            if (targetRoot == transform.root)
                continue;

            if (processedTargets.Contains(targetRoot))
                continue;

            processedTargets.Add(targetRoot);

            Vector3 hitDirection =
                targetRoot.position - transform.position;

            hitDirection.y = 0f;

            if (hitDirection.sqrMagnitude <= 0.001f)
            {
                hitDirection = visual.forward;
                hitDirection.y = 0f;
            }

            hitDirection.Normalize();

            if (playerIdentity != null)
            {
                ApplyPlayerHit(
                    playerIdentity.gameObject,
                    hitDirection
                );
            }

            if (clawStunController != null)
            {
                clawStunController.AddStun(
                    stunDamage
                );
            }
        }
    }

    private void ApplyPlayerHit(
        GameObject targetObject,
        Vector3 hitDirection
    )
    {
        if (targetObject == null)
            return;

        HitReaction hitReaction =
            targetObject.GetComponent<HitReaction>();

        if (hitReaction != null)
        {
            hitReaction.PlayHitReaction(
                hitDirection
            );
        }

        PlayerMovement targetMovement =
            targetObject.GetComponent<PlayerMovement>();

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
                $"{targetObject.name}에서 PlayerMovement를 찾지 못했습니다."
            );
        }

        StunController targetStunController =
            targetObject.GetComponent<StunController>();

        if (targetStunController != null)
        {
            targetStunController.AddStun(
                stunDamage
            );
        }
    }

    private Vector3 GetAttackCenter()
    {
        Vector3 attackCenter =
            transform.position +
            visual.forward * attackDistance +
            visual.TransformDirection(attackOffset);

        return attackCenter;
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

    private void PlayMotionPivotAttackMotion()
    {
        if (motionVisual == null)
            return;

        motionVisual.PlayAttackMotion();
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

        Vector3 attackCenter = GetAttackCenter();

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRadius
        );
    }
}