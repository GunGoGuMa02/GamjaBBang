using System.Collections;
using UnityEngine;

public class ClawStunController : MonoBehaviour
{
    [Header("Stun Settings")]
    [Tooltip("집게가 기절하기까지 필요한 최대 수치입니다.")]
    public float maxStun = 100f;

    [Tooltip("현재 누적된 집게 기절 수치입니다.")]
    public float currentStun = 0f;

    [Tooltip("집게가 기절한 뒤 바구니가 열려 있는 시간입니다.")]
    public float stunDuration = 3f;

    [Header("Stun State")]
    [Tooltip("현재 집게가 기절 상태인지 표시합니다.")]
    public bool isStunned = false;

    [Header("Basket Escape")]
    [Tooltip("집게가 기절했을 때 열릴 바구니 앞벽입니다.")]
    public GameObject basketFront;

    private Coroutine stunCoroutine;

    public void AddStun(float amount)
    {
        if (isStunned)
            return;

        if (amount <= 0f)
            return;

        currentStun += amount;

        currentStun = Mathf.Clamp(
            currentStun,
            0f,
            maxStun
        );

        Debug.Log(
            $"집게 기절 수치: {currentStun} / {maxStun}"
        );

        if (currentStun >= maxStun)
        {
            EnterStun();
        }
    }

    private void EnterStun()
    {
        if (isStunned)
            return;

        isStunned = true;

        Debug.Log("집게가 기절했습니다.");

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(
            StunRoutine()
        );
    }

    private IEnumerator StunRoutine()
    {
        OpenBasket();

        yield return new WaitForSeconds(
            stunDuration
        );

        CloseBasket();

        currentStun = 0f;
        isStunned = false;
        stunCoroutine = null;

        Debug.Log(
            "집게 기절 상태가 초기화되었습니다."
        );
    }

    private void OpenBasket()
    {
        if (basketFront == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Basket Front가 연결되지 않았습니다."
            );

            return;
        }

        basketFront.SetActive(false);

        Debug.Log(
            "바구니 앞벽이 열렸습니다."
        );
    }

    private void CloseBasket()
    {
        if (basketFront == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Basket Front가 연결되지 않았습니다."
            );

            return;
        }

        basketFront.SetActive(true);

        Debug.Log(
            "바구니 앞벽이 닫혔습니다."
        );
    }

    public void ResetStun()
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        currentStun = 0f;
        isStunned = false;

        CloseBasket();

        Debug.Log(
            "집게 기절 상태가 강제로 초기화되었습니다."
        );
    }
}