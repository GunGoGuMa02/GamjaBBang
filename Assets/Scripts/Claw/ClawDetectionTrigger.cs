using System.Collections.Generic;
using UnityEngine;

public class ClawDetectionTrigger : MonoBehaviour
{
    [Header("Detected Players")]
    [SerializeField]
    private List<PlayerIdentity> detectedPlayers =
        new List<PlayerIdentity>();

    public IReadOnlyList<PlayerIdentity> DetectedPlayers
    {
        get
        {
            return detectedPlayers;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentity playerIdentity =
            FindPlayerIdentity(other);

        if (playerIdentity == null)
            return;

        if (detectedPlayers.Contains(playerIdentity))
            return;

        detectedPlayers.Add(playerIdentity);

        Debug.Log(
            $"{playerIdentity.gameObject.name}이 집게 감지 구역에 들어왔습니다. " +
            $"현재 감지 인원: {detectedPlayers.Count}"
        );
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerIdentity playerIdentity =
            FindPlayerIdentity(other);

        if (playerIdentity == null)
            return;

        if (!detectedPlayers.Contains(playerIdentity))
            return;

        detectedPlayers.Remove(playerIdentity);

        Debug.Log(
            $"{playerIdentity.gameObject.name}이 집게 감지 구역에서 나갔습니다. " +
            $"현재 감지 인원: {detectedPlayers.Count}"
        );
    }

    private PlayerIdentity FindPlayerIdentity(Collider other)
    {
        PlayerIdentity playerIdentity =
            other.GetComponentInParent<PlayerIdentity>();

        return playerIdentity;
    }

    private void OnDisable()
    {
        detectedPlayers.Clear();
    }
}