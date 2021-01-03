using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class QuitGameButton : MonoBehaviourPunCallbacks
{
    private Button _quitButton;

    private bool _roomLeft = false;

    private void Awake()
    {
        _quitButton = GetComponent<Button>();

        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }
    public override void OnLeftRoom()
    {
        _roomLeft = true;
    }

    private void OnQuitButtonPressed()
    {
        if (SettingsManager.Instance.IsLocalGame)
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            StartCoroutine(WaitUntilRoomLeftRoutine());
        }
    }

    private IEnumerator WaitUntilRoomLeftRoutine()
    {
        PhotonNetwork.LeaveRoom();

        while(!_roomLeft)
        {
            yield return null;
        }

        SceneManager.LoadScene("MainMenu");
    }
}
