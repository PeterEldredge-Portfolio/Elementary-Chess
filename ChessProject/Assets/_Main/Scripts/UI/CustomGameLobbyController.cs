using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine.UI;

public class CustomGameLobbyController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] private TMP_Text _roomName;
    [SerializeField] private TMP_Text _gameTimeGuestText;
    [SerializeField] private TMP_Text _colorGuestText;
    [SerializeField] private GameObject _labelObjects;
    [SerializeField] private GameObject _sessionInfoLabel;
    [SerializeField] private GameObject _hostObjects;
    [SerializeField] private GameObject _guestObjects;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _hostLeaveButton;
    [SerializeField] private Button _guestLeaveButton;

    private GameTimeController _gameTimeController;
    private PlayerColorDropdown _playerColorDropdown;

    private void Awake()
    {
        _gameTimeController = GetComponentInChildren<GameTimeController>();
        _playerColorDropdown = GetComponentInChildren<PlayerColorDropdown>();

        _startButton.onClick.AddListener(OnStartButtonClicked);

        _hostLeaveButton.onClick.AddListener(OnHostBackButtonClicked);
        _guestLeaveButton.onClick.AddListener(OnGuestBackButtonClicked);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        _roomName.gameObject.SetActive(false);
        _labelObjects.SetActive(false);
        _hostObjects.SetActive(false);
        _guestObjects.SetActive(false);
    }

    private void Update()
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            _hostLeaveButton.interactable = true;

            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                _sessionInfoLabel.SetActive(false);
                _startButton.interactable = true;
            }
            else
            {
                _sessionInfoLabel.SetActive(true);
                _startButton.interactable = false;
            }
        }
        else
        {
            _sessionInfoLabel.SetActive(true);
            _hostLeaveButton.interactable = false;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        SetupPanel();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        _hostLeaveButton.onClick.Invoke();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        _guestLeaveButton.onClick.Invoke();
    }

    public void OnStartButtonClicked()
    {
        StartCoroutine(StartButtonRoutine());
    }

    private IEnumerator StartButtonRoutine()
    {
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            SettingsManager.Instance.IsLocalGame = false;
            SettingsManager.Instance.IsQuickplayGame = false;
            SettingsManager.Instance.MoveTime = _gameTimeController.GetGameTime();
            SettingsManager.Instance.HostColor = _playerColorDropdown.HostWhite ? PieceColor.Blue : PieceColor.Red;

            yield return new WaitForSeconds(.5f);

            PhotonNetwork.LoadLevel("Game");
        }
    }

    public void OnHostBackButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;

        PhotonNetwork.LeaveRoom();
    }

    public void OnGuestBackButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void SetupPanel()
    {
        _labelObjects.SetActive(true);
        _roomName.gameObject.SetActive(true);

        _roomName.text = PhotonNetwork.CurrentRoom.Name;

        if (PhotonNetwork.IsMasterClient)
        {
            _hostObjects.SetActive(true);
            _guestObjects.SetActive(false);

            StartCoroutine(SendCustomSettigs());
        }
        else
        {
            _hostObjects.SetActive(false);
            _guestObjects.SetActive(true);
        }
    }

    private IEnumerator SendCustomSettigs()
    {
        _gameTimeController.SetGameTime(SettingsManager.CUSTOM_MOVE_TIME_DEFAULT);

        while (true)
        {
            object[] content = new object[] 
            { 
                _gameTimeController.GetGameTime(), 
                _playerColorDropdown.HostWhite, 
            };

            PhotonNetwork.RaiseEvent(
                PhotonEvents.UPDATED_SETTINGS_EVENT_CODE,
                content,
                new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
                SendOptions.SendReliable);

            yield return new WaitForSeconds(PhotonEvents.TICK_SPEED);
        }
    }

    #region Networking Code

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case PhotonEvents.UPDATED_SETTINGS_EVENT_CODE:
                OnSettingsUpdateEvent(photonEvent);
                break;
        }
    }

    private void OnSettingsUpdateEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;

        UpdateGameTimeText((float)data[0]);
        UpdatePieceColorText((bool)data[1]);

        SettingsManager.Instance.IsLocalGame = false;
        SettingsManager.Instance.MoveTime = (float)data[0];
        SettingsManager.Instance.HostColor = (bool)data[1] ? PieceColor.Blue : PieceColor.Red;
    }

    private void UpdateGameTimeText(float gameTime)
    {
        int hours = Mathf.FloorToInt(gameTime / 3600);
        int minutes = Mathf.FloorToInt((gameTime - hours * 3600) / 60);
        int seconds = Mathf.FloorToInt(gameTime - hours * 3600 - minutes * 60);

        _gameTimeGuestText.text = $"{ConvertToString(hours)} : {ConvertToString(minutes)} : {ConvertToString(seconds)}";
    }

    private string ConvertToString(int value)
    {
        string tempString = value.ToString();

        if (tempString.Length == 1)
        {
            tempString = $"0{tempString}";
        }

        return tempString;
    }

    private void UpdatePieceColorText(bool hostWhite)
    {
        _colorGuestText.text = hostWhite ? "Red" : "Blue";
    }

    #endregion
}
