using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalLobbyController : MonoBehaviour
{
    private GameTimeController _gameTimeController;

    private void Awake()
    {
        _gameTimeController = GetComponentInChildren<GameTimeController>();
    }

    private void OnEnable()
    {
        _gameTimeController.SetGameTime(SettingsManager.LOCAL_MOVE_TIME);
    }

    public void StartGame()
    {
        SettingsManager.Instance.IsLocalGame = true;
        SettingsManager.Instance.MoveTime = _gameTimeController.GetGameTime();

        SceneManager.LoadScene("Game");
    }
}
