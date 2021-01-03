using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class RoomButton : MonoBehaviour
{
    public Button Button { get; private set; }
    public TMP_Text TMP_Text { get; private set; }
    public RoomInfo RoomInfo { get; private set; }

    private RoomFinderPanelController _roomFinderController;

    private Sprite _defaultSprite;

    private void Awake()
    {
        _roomFinderController = GetComponentInParent<RoomFinderPanelController>();

        Button = GetComponent<Button>();
        TMP_Text = GetComponentInChildren<TMP_Text>();

        _defaultSprite = Button.image.sprite;
    }

    private void Update()
    {
        if(_roomFinderController.LobbyController.CurrentRoomToJoin == TMP_Text.text)
        {
            Button.image.sprite = Button.spriteState.selectedSprite;
        }
        else
        {
            Button.image.sprite = _defaultSprite;
        }
    }

    public void SetRoomInfo(RoomInfo info)
    {
        RoomInfo = info;
        TMP_Text.text = RoomInfo.Name;
    }
}
