using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropHoleTrigger : MonoBehaviour
{
    [Header("Respawn")]
    [Tooltip("플레이어가 떨어진 뒤 다시 나타날 위치입니다.")]
    public Transform respawnPoint;

    [Tooltip("떨어진 뒤 리스폰되기까지 기다리는 시간입니다.")]
    public float respawnDelay = 1f;

    private readonly HashSet<PlayerIdentity> detectedPlayers =
        new HashSet<PlayerIdentity>();

    private void OnTriggerEnter(Collider other)
    {
        TryDetectPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDetectPlayer(other);
    }

    private void TryDetectPlayer(Collider other)
    {
        PlayerIdentity playerIdentity =
            other.GetComponentInParent<PlayerIdentity>();

        if (playerIdentity == null)
            return;

        if (detectedPlayers.Contains(playerIdentity))
            return;

        detectedPlayers.Add(playerIdentity);

        Debug.Log($"{playerIdentity.gameObject.name}이 DropHole에 떨어졌습니다.");

        StartCoroutine(RespawnRoutine(playerIdentity));
    }

    private IEnumerator RespawnRoutine(PlayerIdentity playerIdentity)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (playerIdentity == null)
            yield break;

        if (respawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: Respawn Point가 연결되지 않았습니다.");
            detectedPlayers.Remove(playerIdentity);
            yield break;
        }

        PlayerGrabController grabController =
            playerIdentity.GetComponent<PlayerGrabController>();

        if (grabController != null)
        {
            grabController.Release();
        }

        Rigidbody playerRigidbody =
            playerIdentity.GetComponent<Rigidbody>();

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        playerIdentity.transform.position = respawnPoint.position;
        playerIdentity.transform.rotation = respawnPoint.rotation;

        Debug.Log($"{playerIdentity.gameObject.name}이 리스폰되었습니다.");

        yield return new WaitForSeconds(0.5f);

        detectedPlayers.Remove(playerIdentity);
    }
}