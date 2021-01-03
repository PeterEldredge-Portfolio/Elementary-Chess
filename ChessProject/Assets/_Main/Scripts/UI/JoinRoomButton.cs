using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomButton : MonoBehaviour
{
    private RoomFinderPanelController _roomFinderController;
    private Button _joinButton;

    private void Awake()
    {
        _roomFinderController = GetComponentInParent<RoomFinderPanelController>();
        _joinButton = GetComponent<Button>();

        _joinButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if(string.IsNullOrEmpty(_roomFinderController.LobbyController.CurrentRoomToJoin))
        {
            _joinButton.interactable = false;
        }
        else
        {
            _joinButton.interactable = true;
        }
    }

    private void OnClick()
    {
        _roomFinderController.LobbyController.RemoveRoom(_roomFinderController.LobbyController.CurrentRoomToJoin);

        NetworkManager.Instance.JoinSelectedRoom(_roomFinderController.LobbyController.CurrentRoomToJoin);
    }
}
