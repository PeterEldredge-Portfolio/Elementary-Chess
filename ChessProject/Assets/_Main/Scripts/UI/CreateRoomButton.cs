using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class CreateRoomButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField _roomName;
    
    private Button _createButton;

    private RoomFinderPanelController _panelController;

    private void Awake()
    {
        _createButton = GetComponent<Button>();
        _panelController = GetComponentInParent<RoomFinderPanelController>();

        _createButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if(_roomName.text.Length > 0 && !_panelController.LobbyController.RoomExists(_roomName.text))
        {
            _createButton.interactable = true;
        }
        else
        {
            _createButton.interactable = false;
        }
    }

    private void OnClick()
    {
        NetworkManager.Instance.CreateRoom(_roomName.text);
    }
}
