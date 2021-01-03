using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class UIConsole : GameEventUserObject, IOnEventCallback
{
    public static UIConsole Instance { get; private set; }

    [SerializeField] private GameObject _logTemplate;

    public Color StandardMessage;
    public Color TurnMessage;
    public Color ImportantMessage;
    public Color NetworkMessage;
    public Color GameEndingMessage;

    private ScrollRect _scrollrect;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        _scrollrect = GetComponentInParent<ScrollRect>();
    }

    public override void Subscribe()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void Unsubscribe()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void Log(string message)
    {
        TMP_Text tmpText = Instantiate(_logTemplate, transform).GetComponent<TMP_Text>();

        tmpText.text = message;
    }

    public void Log(string message, Color color)
    {
        TMP_Text tmpText = Instantiate(_logTemplate, transform).GetComponent<TMP_Text>();

        tmpText.text = message;
        tmpText.color = color;

        _scrollrect.velocity = new Vector2(0, 1000);
    }

    #region Networking Code

    public void SendNetworkedMessage(string message)
    {
        Log(message, NetworkMessage);

        object[] content = new object[] { message, };

        PhotonNetwork.RaiseEvent(
            PhotonEvents.MESSAGE_EVENT_CODE,
            content,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case PhotonEvents.MESSAGE_EVENT_CODE:
                OnMessageEvent(photonEvent);
                break;
        }
    }

    private void OnMessageEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;

        Log((string)data[0], NetworkMessage);
    }

    #endregion

}
