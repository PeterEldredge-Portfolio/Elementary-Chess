using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections;

public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject _roomTemplatePrefab;

    public string CurrentRoomToJoin { get; private set; } = string.Empty;

    private Dictionary<string, RoomInfo> _cachedCustomMatchRooms = new Dictionary<string, RoomInfo>();
    private Dictionary<string, RoomInfo> _currentCustomMatchRooms = new Dictionary<string, RoomInfo>();

    private Dictionary<string, RoomButton> _currentCustomMatchButtons = new Dictionary<string, RoomButton>();

    public override void OnEnable()
    {
        base.OnEnable();

        CurrentRoomToJoin = string.Empty;

        OnRoomListUpdate(NetworkManager.Instance.CachedRoomList);

        StartCoroutine(UpdateMatchListUIRoutine());
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        _cachedCustomMatchRooms.Clear();

        List<RoomInfo> validList = roomList
            .Where(r => !r.Name.Contains(NetworkManager._QUICKPLAY_ROOM_PREFIX))
            .ToList();

        foreach(RoomInfo info in validList)
        {
            _cachedCustomMatchRooms.Add(info.Name, info);
        }
    }

    private void OnClicked(RoomButton roomButton)
    {
        CurrentRoomToJoin = roomButton.TMP_Text.text;

        Debug.Log(CurrentRoomToJoin);
    }

    private void AddRoom(RoomInfo room)
    {
        if(!_currentCustomMatchButtons.ContainsKey(room.Name))
        {
            RoomButton roomButton = Instantiate(_roomTemplatePrefab, transform).GetComponent<RoomButton>();

            roomButton.Button.onClick.AddListener(() => OnClicked(roomButton));
            roomButton.SetRoomInfo(room);

            _currentCustomMatchRooms.Add(room.Name, room);
            _currentCustomMatchButtons.Add(room.Name, roomButton);
        }
    }

    public void RemoveRoom(string name)
    {
        Destroy(_currentCustomMatchButtons[name].gameObject);

        _currentCustomMatchRooms.Remove(name);
        _currentCustomMatchButtons.Remove(name);
    }

    private void RemoveRoom(RoomInfo room)
    {
        List<string> matchingRoomNames = _currentCustomMatchButtons
            .Select(b => b.Key)
            .Where(k => k == room.Name)
            .ToList();

        for (int i = 0; i < matchingRoomNames.Count; i++)
        {
            Destroy(_currentCustomMatchButtons[matchingRoomNames[i]].gameObject);

            _currentCustomMatchRooms.Remove(matchingRoomNames[i]);
            _currentCustomMatchButtons.Remove(matchingRoomNames[i]);
        }
    }

    private IEnumerator UpdateMatchListUIRoutine()
    {
        List<RoomInfo> roomsToRemove = new List<RoomInfo>();

        while(true)
        {
            roomsToRemove.Clear();
            
            foreach(RoomInfo cachedRoomInfo in _cachedCustomMatchRooms.Values)
            {
                if (cachedRoomInfo.PlayerCount == 1 && cachedRoomInfo.IsOpen)
                {
                    if (!_currentCustomMatchRooms.ContainsKey(cachedRoomInfo.Name))
                    {
                        AddRoom(cachedRoomInfo);
                    }
                }
                else
                {
                    if (_currentCustomMatchRooms.ContainsKey(cachedRoomInfo.Name))
                    {
                        roomsToRemove.Add(cachedRoomInfo);
                    }
                }
            }

            foreach(RoomInfo roomToRemove in roomsToRemove)
            {
                RemoveRoom(roomToRemove);
            }

            //Validate CurrentRoomToJoin
            if (!_currentCustomMatchRooms.ContainsKey(CurrentRoomToJoin))
            {
                CurrentRoomToJoin = string.Empty;
            }

            yield return null;
        }
    }

    public bool RoomExists(string roomName)
    {
        return _currentCustomMatchRooms.ContainsKey(roomName);
    }
}
