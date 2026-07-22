using System.Collections;
using UnityEngine;

public class ClawMovement : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("집게가 내려올 위치를 나타내는 ClawTargetZone입니다.")]
    public Transform targetZone;

    [Header("Timing Settings")]
    [Tooltip("게임 시작 후 집게가 내려오기 전까지 기다리는 시간입니다.")]
    public float waitBeforeDescending = 1f;

    [Tooltip("집게가 아래에 도착한 뒤 머무는 시간입니다.")]
    public float waitAtBottom = 1f;

    [Header("Movement Settings")]
    [Tooltip("집게가 내려가는 속도입니다.")]
    public float descendingSpeed = 2f;

    [Tooltip("집게가 다시 올라가는 속도입니다.")]
    public float ascendingSpeed = 2f;

    [Tooltip("집게 중심이 목표 구역보다 얼마나 높은 위치에서 멈출지 결정합니다.")]
    public float bottomHeightOffset = 1.5f;

    [Header("Test Settings")]
    [Tooltip("게임 시작 시 자동으로 왕복 동작을 시작합니다.")]
    public bool moveOnStart = true;

    private Vector3 startPosition;
    private Vector3 bottomPosition;

    private Coroutine movementCoroutine;
    private bool isMoving;

    private void Start()
    {
        // Play를 누른 순간의 Claw 위치를 공중 대기 위치로 저장합니다.
        startPosition = transform.position;

        CalculateBottomPosition();

        if (moveOnStart)
        {
            StartMovement();
        }
    }

    private void CalculateBottomPosition()
    {
        if (targetZone == null)
        {
            bottomPosition = transform.position;
            return;
        }

        bottomPosition = new Vector3(
            targetZone.position.x,
            targetZone.position.y + bottomHeightOffset,
            targetZone.position.z
        );
    }

    public void StartMovement()
    {
        if (isMoving)
            return;

        if (targetZone == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: ClawMovement의 Target Zone이 연결되지 않았습니다."
            );

            return;
        }

        CalculateBottomPosition();

        movementCoroutine =
            StartCoroutine(MovementRoutine());
    }

    private IEnumerator MovementRoutine()
    {
        isMoving = true;

        // 공중에서 잠시 기다립니다.
        yield return new WaitForSeconds(waitBeforeDescending);

        // 아래로 내려갑니다.
        yield return MoveToPosition(
            bottomPosition,
            descendingSpeed
        );

        Debug.Log("집게가 아래 위치에 도착했습니다.");

        // 아래에서 잠시 멈춥니다.
        yield return new WaitForSeconds(waitAtBottom);

        // 처음 공중 위치로 다시 올라갑니다.
        yield return MoveToPosition(
            startPosition,
            ascendingSpeed
        );

        Debug.Log("집게가 대기 위치로 돌아왔습니다.");

        isMoving = false;
        movementCoroutine = null;
    }

    private IEnumerator MoveToPosition(
        Vector3 targetPosition,
        float moveSpeed
    )
    {
        if (moveSpeed <= 0f)
        {
            Debug.LogWarning(
                $"{gameObject.name}: 집게 이동 속도는 0보다 커야 합니다."
            );

            yield break;
        }

        while (
            Vector3.Distance(
                transform.position,
                targetPosition
            ) > 0.01f
        )
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
    }
}