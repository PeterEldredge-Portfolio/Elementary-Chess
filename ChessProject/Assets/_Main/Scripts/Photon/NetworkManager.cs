using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum JoinResult
{
    None,
    Waiting,
    Succeeded,
    Failed,
}

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public const string _QUICKPLAY_ROOM_PREFIX = "QUICKPLAY-";
    public const int _QUICKPLAY_MAX_ROOMS = 100;

    public static NetworkManager Instance;

    public List<RoomInfo> CachedRoomList { get; private set; } = new List<RoomInfo>();

    public bool ConnectedToMaster { get; private set; } = false;

    private JoinResult _joinResult = JoinResult.None;

    private bool _searchingForOpponent = false;
    private bool _stopSearchingForOpponent = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to the Photon master server");

        ConnectedToMaster = true;

        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room Created Successfully");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Room Joined");

        _joinResult = JoinResult.Succeeded;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        _joinResult = JoinResult.Failed;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room Creation Failed");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        bool matchFound = false;

        CachedRoomList = roomList;

        Debug.Log("Room List Updated");

        /*for (int i = 0; i < roomList.Count; i++)
        {
            matchFound = false;

            for (int j = 0; j < CachedRoomList.Count; j++)
            {
                if(roomList[i].Name == CachedRoomList[j].Name)
                {
                    matchFound = true;

                    CachedRoomList[j] = roomList[i];

                    break;
                }
            }

            if(!matchFound)
            {
                CachedRoomList.Add(roomList[i]);
            }
        }*/
    }

    public void CreateRoom(string name)
    {
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2 };

        PhotonNetwork.CreateRoom(name, roomOps);
    }

    public void JoinSelectedRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    #region Quickplay Setup

    public void OnQuickplayButtonClick()
    {
        StartCoroutine(CreateOrJoinQuickplayRoomRoutine());
    }

    private IEnumerator CreateOrJoinQuickplayRoomRoutine()
    {
        int randRoomNum = 0;

        bool creatingRoom = false;

        List<string> validRooms = CachedRoomList
            .Where(info => info.IsOpen)
            .Where(info => info.PlayerCount < info.MaxPlayers)
            .Select(info => info.Name)
            .Where(name => name.Contains(_QUICKPLAY_ROOM_PREFIX))
            .ToList();

        _joinResult = JoinResult.None;

        while (!creatingRoom && (_joinResult != JoinResult.Succeeded))
        {
            if(_joinResult != JoinResult.Waiting)
            {
                if (validRooms.Count > 0)
                {
                    if(_joinResult == JoinResult.None)
                    {
                        randRoomNum = Random.Range(0, validRooms.Count);

                        PhotonNetwork.JoinRoom(validRooms[randRoomNum]);

                        _joinResult = JoinResult.Waiting;
                    }
                    else if (_joinResult == JoinResult.Failed)
                    {
                        validRooms.RemoveAt(randRoomNum);

                        _joinResult = JoinResult.None;
                    }
                }
                else
                {
                    creatingRoom = true;

                    StartCoroutine(CreateQuickplayRoomRoutine());
                }
            }

            yield return null;
        }

        _joinResult = JoinResult.None;

        StartCoroutine(WaitForQuickplayRoomFullRoutine());
    }

    private IEnumerator CreateQuickplayRoomRoutine()
    {
        int ranRoom;

        bool roomCreated = false;
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 2 };

        while (!roomCreated)
        {
            if (ConnectedToMaster)
            {
                ranRoom = Random.Range(0, _QUICKPLAY_MAX_ROOMS);

                roomCreated = PhotonNetwork.CreateRoom(_QUICKPLAY_ROOM_PREFIX + ranRoom.ToString(), roomOps);
            }

            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator WaitForQuickplayRoomFullRoutine()
    {
        _searchingForOpponent = true;

        while (!_stopSearchingForOpponent)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    SettingsManager.Instance.SetQuickplayRules();

                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.LoadLevel("Game");
                        break;
                    }
                }
            }

            yield return null;
        }

        if (_stopSearchingForOpponent && PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }

            PhotonNetwork.LeaveRoom();

            ConnectedToMaster = false;
        }

        while (!ConnectedToMaster)
        {
            yield return null;
        }

        _searchingForOpponent = false;
        _stopSearchingForOpponent = false;
    }

    #endregion

    //Misc
    public void StopSearching()
    {
        if(_searchingForOpponent)
        {
            _stopSearchingForOpponent = true;
        }
    }

}
