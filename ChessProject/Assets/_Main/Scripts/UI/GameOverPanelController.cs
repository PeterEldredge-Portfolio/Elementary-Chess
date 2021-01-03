using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameOverPanelController : MonoBehaviour
{
    private const string YOU_WON_TEXT = "You Won!";
    private const string YOU_LOST_TEXT = "You Lost!";
    private const string WHITE_WON_TEXT = "Blue Won!";
    private const string BLACK_WON_TEXT = "Red Won!";
    private const string STALE_MATE_TEXT = "Stalemate!";

    [SerializeField] private TMP_Text _gameOverLabel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _closeButton;

    private void Start()
    {     
        if(!SettingsManager.Instance.IsLocalGame)
        {
            if(!PhotonNetwork.IsMasterClient)
            {
                _restartButton.gameObject.SetActive(false);
            }
        }

        _restartButton.onClick.AddListener(RestartGame);
        _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        if(GameController.Instance)
        {
            if (GameController.Instance.WinningColor != null)
            {
                if (GameController.Instance.WinningColor == PieceColor.None)
                {
                    SFXPalette.Instance.PlayClip(SFXPalette.Instance.Lose, .2f);

                    _gameOverLabel.text = STALE_MATE_TEXT;
                }
                else
                {
                    if (SettingsManager.Instance.IsLocalGame)
                    {
                        SFXPalette.Instance.PlayClip(SFXPalette.Instance.Win, .2f);

                        _gameOverLabel.text = GameController.Instance.WinningColor == PieceColor.Blue ? WHITE_WON_TEXT : BLACK_WON_TEXT;
                    }
                    else
                    {
                        if (GameController.Instance.WinningColor == GameController.Instance.PlayerColor)
                        {
                            SFXPalette.Instance.PlayClip(SFXPalette.Instance.Win, .2f);

                            _gameOverLabel.text = YOU_WON_TEXT;
                        }
                        else
                        {
                            SFXPalette.Instance.PlayClip(SFXPalette.Instance.Lose, .2f);

                            _gameOverLabel.text = YOU_LOST_TEXT;
                        }
                    }
                }
            }
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
