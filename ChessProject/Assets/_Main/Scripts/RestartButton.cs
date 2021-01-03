using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RestartButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        _button.onClick.AddListener(RestartGame);
    }

    private void Start()
    {
        if ((SettingsManager.Instance.IsQuickplayGame == false && PhotonNetwork.IsMasterClient) ||
            SettingsManager.Instance.IsLocalGame)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void RestartGame()
    {
        object[] content = new object[] { };

        PhotonNetwork.RaiseEvent(
            PhotonEvents.RESTART_GAME_EVENT_CODE,
            content,
            new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable);

        UIConsole.Instance.Log("Game Restarted!", UIConsole.Instance.GameEndingMessage);

        GameController.Instance.StartGame();
    }
}
