using System.Collections;
using UnityEngine;

public class ClawMovement : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("집게가 내려올 위치를 나타내는 ClawTargetZone입니다.")]
    public Transform targetZone;

    [Header("Drop Hole")]
    [Tooltip("플레이어를 떨어뜨릴 DropHole 위 위치입니다.")]
    public Transform dropPoint;

    [Header("Grab")]
    [Tooltip("집게가 아래에 도착했을 때 켤 포획 판정입니다.")]
    public ClawGrabTrigger grabTrigger;

    [Header("Timing Settings")]
    [Tooltip("버튼을 누른 뒤 집게가 내려오기 전까지 기다리는 시간입니다.")]
    public float waitBeforeDescending = 1f;

    [Tooltip("집게가 아래에 도착한 뒤 머무는 시간입니다.")]
    public float waitAtBottom = 2f;

    [Tooltip("DropHole 위에 도착한 뒤 플레이어를 놓기 전까지 기다리는 시간입니다.")]
    public float waitBeforeRelease = 0.5f;

    [Tooltip("플레이어를 놓은 뒤 집게가 돌아가기 전까지 기다리는 시간입니다.")]
    public float waitAfterRelease = 0.5f;

    [Header("Movement Settings")]
    [Tooltip("집게가 내려가는 속도입니다.")]
    public float descendingSpeed = 2f;

    [Tooltip("집게가 다시 올라가는 속도입니다.")]
    public float ascendingSpeed = 2f;

    [Tooltip("집게가 DropHole 방향으로 이동하는 속도입니다.")]
    public float horizontalMoveSpeed = 3f;

    [Tooltip("집게 중심이 목표 구역보다 얼마나 높은 위치에서 멈출지 결정합니다.")]
    public float bottomHeightOffset = 2.5f;

    [Header("Test Settings")]
    [Tooltip("게임 시작 시 자동으로 왕복 동작을 시작합니다.")]
    public bool moveOnStart = false;

    private Vector3 startPosition;
    private Vector3 bottomPosition;

    private Coroutine movementCoroutine;
    private bool isMoving;

    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
    }

    private void Start()
    {
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
            Debug.LogWarning($"{gameObject.name}: Target Zone이 연결되지 않았습니다.");
            return;
        }

        CalculateBottomPosition();

        movementCoroutine =
            StartCoroutine(MovementRoutine());
    }

    private IEnumerator MovementRoutine()
    {
        isMoving = true;

        if (grabTrigger != null)
        {
            grabTrigger.DisableGrab();
        }

        yield return new WaitForSeconds(waitBeforeDescending);

        yield return MoveToPosition(
            bottomPosition,
            descendingSpeed
        );

        Debug.Log("집게가 아래 위치에 도착했습니다.");

        if (grabTrigger != null)
        {
            grabTrigger.EnableGrab();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Grab Trigger가 연결되지 않았습니다.");
        }

        yield return new WaitForSeconds(waitAtBottom);

        if (grabTrigger != null)
        {
            grabTrigger.DisableGrab();
        }

        bool caughtPlayer =
            grabTrigger != null &&
            grabTrigger.HasGrabbedPlayer;

        Vector3 upperPosition = new Vector3(
            transform.position.x,
            startPosition.y,
            transform.position.z
        );

        yield return MoveToPosition(
            upperPosition,
            ascendingSpeed
        );

        if (caughtPlayer)
        {
            yield return MoveToDropHoleAndRelease();
        }
        else
        {
            Debug.Log("집게가 아무도 잡지 못했습니다.");
        }

        yield return MoveToPosition(
            startPosition,
            horizontalMoveSpeed
        );

        Debug.Log("집게가 대기 위치로 돌아왔습니다.");

        isMoving = false;
        movementCoroutine = null;
    }

    private IEnumerator MoveToDropHoleAndRelease()
    {
        if (dropPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: DropPoint가 연결되지 않았습니다. 잡은 플레이어를 현재 위치에서 놓습니다.");

            if (grabTrigger != null)
            {
                grabTrigger.ReleaseAllGrabbedPlayers();
            }

            yield break;
        }

        Vector3 dropMovePosition = new Vector3(
            dropPoint.position.x,
            startPosition.y,
            dropPoint.position.z
        );

        Debug.Log("집게가 DropHole 위로 이동합니다.");

        yield return MoveToPosition(
            dropMovePosition,
            horizontalMoveSpeed
        );

        yield return new WaitForSeconds(waitBeforeRelease);

        if (grabTrigger != null)
        {
            grabTrigger.ReleaseAllGrabbedPlayers();
        }

        yield return new WaitForSeconds(waitAfterRelease);
    }

    private IEnumerator MoveToPosition(
        Vector3 targetPosition,
        float moveSpeed
    )
    {
        if (moveSpeed <= 0f)
        {
            Debug.LogWarning($"{gameObject.name}: 이동 속도는 0보다 커야 합니다.");
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