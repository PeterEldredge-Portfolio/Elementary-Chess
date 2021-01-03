using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFinderPanelController : MonoBehaviour
{
    public LobbyController LobbyController { get; private set; }

    private void Awake()
    {
        LobbyController = GetComponentInChildren<LobbyController>();
    }
}
