using System.Collections.Generic;
using UnityEngine;

public class ClawGrabTrigger : MonoBehaviour
{
    [Header("Grab Point")]
    [Tooltip("잡힌 플레이어가 고정될 위치입니다.")]
    public Transform grabPoint;

    [Header("Grab State")]
    [Tooltip("현재 포획 판정이 켜져 있는지 표시합니다.")]
    public bool canGrab = false;

    private readonly HashSet<PlayerGrabController> grabbedPlayers =
        new HashSet<PlayerGrabController>();

    public bool HasGrabbedPlayer
    {
        get
        {
            return grabbedPlayers.Count > 0;
        }
    }

    public void EnableGrab()
    {
        canGrab = true;
        grabbedPlayers.Clear();

        Debug.Log("집게 포획 판정이 켜졌습니다.");
    }

    public void DisableGrab()
    {
        canGrab = false;

        Debug.Log("집게 포획 판정이 꺼졌습니다.");
    }

    public void ReleaseAllGrabbedPlayers()
    {
        foreach (PlayerGrabController grabbedPlayer in grabbedPlayers)
        {
            if (grabbedPlayer == null)
                continue;

            grabbedPlayer.Release();
        }

        Debug.Log(
            $"집게가 잡고 있던 플레이어를 놓았습니다. 인원: {grabbedPlayers.Count}"
        );

        grabbedPlayers.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canGrab)
            return;

        TryGrab(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!canGrab)
            return;

        TryGrab(other);
    }

    private void TryGrab(Collider other)
    {
        PlayerGrabController grabController =
            other.GetComponentInParent<PlayerGrabController>();

        if (grabController == null)
            return;

        if (grabbedPlayers.Contains(grabController))
            return;

        if (grabPoint == null)
        {
            Debug.LogWarning(
                $"{gameObject.name}: GrabPoint가 연결되지 않았습니다."
            );

            return;
        }

        grabbedPlayers.Add(grabController);

        grabController.Grab(grabPoint);

        Debug.Log(
            $"{grabController.gameObject.name}을 집게가 잡았습니다."
        );
    }
}