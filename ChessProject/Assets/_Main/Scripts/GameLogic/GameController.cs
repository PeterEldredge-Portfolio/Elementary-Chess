using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

namespace Events
{
    public struct GameStartEventArgs : IGameEvent { }
}

public class GameController : GameEventUserObject, IOnEventCallback
{
    public static GameController Instance;

    public PieceColor CurrentTurn { get; private set; }
    public PieceColor PlayerColor { get; private set; }
    public PieceColor OpponentColor { get; private set; }
    public PieceColor? WinningColor { get; private set; }

    public float WhiteTimer { get; private set; }
    public float BlackTimer { get; private set; }

    public bool IsGameOver { get; private set; }
    public bool IsLocalGame { get; private set; }

    public GameBoard CurrentGameBoard { get; private set; }

    //Defaults
    private const PieceColor _CURRENT_TURN_DEFAULT = PieceColor.Blue;
    private const PieceColor _PLAYER_COLOR_DEFAULT = PieceColor.None;
    private const PieceColor _OPPONENT_COLOR_DEFAULT = PieceColor.None;

    private const float _TIMER_DEFAULT = 1800f;

    //Inspector
    [SerializeField] private GameObject _whiteTimer;
    [SerializeField] private GameObject _blackTimer;
    [SerializeField] private GameObject _chatObject;

    //Normal Members
    private bool _pauseTimer = false;

    private void Awake()
    {
        CurrentGameBoard = FindObjectOfType<GameBoard>();

        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        StartGame();

        if(!IsLocalGame)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;

            StartCoroutine(UpdateRoomStatus());
        }
    }

    public override void Subscribe()
    {
        EventManager.Instance.AddListener<Events.MoveMadeEventArgs>(this, NextTurn);

        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void Unsubscribe()
    {
        EventManager.Instance.RemoveListener<Events.MoveMadeEventArgs>(this, NextTurn);

        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void StartGame()
    {
        CancelInvoke("UpdateTimer");

        GameSetup();

        CurrentGameBoard.Setup();

        GameCanvas.Instance.OnStartGame();

        EventManager.Instance.TriggerEventImmediate(new Events.GameStartEventArgs());
    }

    public void GameOver(PieceColor winningColor, string winLog)
    {
        WinningColor = winningColor;

        if(WinningColor == PieceColor.None)
        {
            UIConsole.Instance.Log($"{winLog}", UIConsole.Instance.GameEndingMessage);
        }
        else
        {
            UIConsole.Instance.Log($"{winLog} {winningColor} Wins!", UIConsole.Instance.GameEndingMessage);
        }

        CancelInvoke("UpdateTimer");

        IsGameOver = true;
        CurrentTurn = PieceColor.None;

        CurrentGameBoard.ClearBoard();

        StartCoroutine(PerformActionOnDelay(() => GameCanvas.Instance.OnGameOver(), 1.5f));
    }

    private void GameSetup()
    {
        IsLocalGame = true;
        IsGameOver = false;

        WinningColor = null;

        CurrentTurn = _CURRENT_TURN_DEFAULT;

        PlayerColor = _PLAYER_COLOR_DEFAULT;
        OpponentColor = _OPPONENT_COLOR_DEFAULT;

        WhiteTimer = _TIMER_DEFAULT;
        BlackTimer = _TIMER_DEFAULT;
        
        if(SettingsManager.Instance)
        {
            PieceColor hostColor = SettingsManager.Instance.HostColor;

            WhiteTimer = SettingsManager.Instance.MoveTime;
            BlackTimer = WhiteTimer;

            IsLocalGame = SettingsManager.Instance.IsLocalGame;

            if (IsLocalGame)
            {
                _chatObject.SetActive(false);
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PlayerColor = hostColor;
                    OpponentColor = hostColor == PieceColor.Blue ? PieceColor.Red : PieceColor.Blue;
                }
                else
                {
                    PlayerColor = hostColor == PieceColor.Blue ? PieceColor.Red : PieceColor.Blue;
                    OpponentColor = hostColor;
                }
            }
        }

        if (WhiteTimer > 0) //If MoveTime > 0 then use move time
        {
            if (PhotonNetwork.IsMasterClient || IsLocalGame)
            {
                InvokeRepeating("UpdateTimer", 0f, 1f);
            }

            StartCoroutine(CheckTimerRoutine());
        }
        else
        {
            _whiteTimer.SetActive(false);
            _blackTimer.SetActive(false);
        }

        UIConsole.Instance.Log($"{CurrentTurn}'s Move", UIConsole.Instance.TurnMessage);
    }

    private void NextTurn(Events.MoveMadeEventArgs args)
    {
        if(!IsGameOver) StartCoroutine(WaitForCheckMateCheckRoutine(args));
    }

    private void UpdateTimer()
    {
        if(_pauseTimer)
        {
            _pauseTimer = false;
            
            return;
        }

        if (CurrentTurn == PieceColor.Blue)
        {
            WhiteTimer -= 1f;
        }
        else if (CurrentTurn == PieceColor.Red)
        {
            BlackTimer -= 1f;
        }

        if(!IsLocalGame)
        {
            object[] content = new object[] { WhiteTimer, BlackTimer, };

            PhotonNetwork.RaiseEvent(
                PhotonEvents.TIMER_UPDATE_EVENT_CODE,
                content,
                new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
                SendOptions.SendReliable);
        }
    }

    private IEnumerator UpdateRoomStatus()
    {
        while(true)
        {
            if(PhotonNetwork.CurrentRoom != null)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    UIConsole.Instance.Log("Opponent Disconnected", UIConsole.Instance.NetworkMessage);

                    break;
                }
            }

            yield return new WaitForSeconds(PhotonEvents.TICK_SPEED);
        }
    }

    private IEnumerator CheckTimerRoutine()
    {
        while (!IsGameOver)
        {
            if (WhiteTimer <= 0f)
            {
                WhiteTimer = 0f;

                GameOver(PieceColor.Red, "Time Out!");
            }
            else if (BlackTimer <= 0f)
            {
                BlackTimer = 0f;

                GameOver(PieceColor.Blue, "Time Out!");
            }

            yield return new WaitForSeconds(PhotonEvents.TICK_SPEED);
        }
    }

    private IEnumerator WaitForCheckMateCheckRoutine(Events.MoveMadeEventArgs args)
    {
        yield return new WaitForSeconds(args.WaitTime + .2f);

        PieceColor storedCurrentTurn = CurrentTurn == PieceColor.Blue ? PieceColor.Blue : PieceColor.Red;

        while (CurrentGameBoard.WaitingForPromotion)
        {
            yield return null;
        }

        CurrentTurn = PieceColor.None;

        StartCoroutine(CurrentGameBoard.CheckIfCheckMate(storedCurrentTurn == PieceColor.Blue ? PieceColor.Red : PieceColor.Blue));

        while (CurrentGameBoard.RunningCheckMateCheck)
        {
            yield return null;
        }

        if (CurrentGameBoard.CheckMate && args.Checking) GameOver(storedCurrentTurn, "Checkmate!");
        else if (CurrentGameBoard.CheckMate && !args.Checking) GameOver(PieceColor.None, "Stalemate!");
        else
        {
            if(args.Checking) UIConsole.Instance.Log("Check!", UIConsole.Instance.ImportantMessage);

            _pauseTimer = true;

            CurrentTurn = storedCurrentTurn == PieceColor.Blue ? PieceColor.Red : PieceColor.Blue;

            UIConsole.Instance.Log($"{CurrentTurn}'s Move", UIConsole.Instance.TurnMessage);
        }
    }

    #region Networking Code

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case PhotonEvents.TIMER_UPDATE_EVENT_CODE:
                OnTimerUpdateEvent(photonEvent);
                break;
            case PhotonEvents.RESTART_GAME_EVENT_CODE:
                OnRestartGameEvent();
                break;
        }
    }

    private void OnTimerUpdateEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;

        WhiteTimer = (float)data[0];
        BlackTimer = (float)data[1];
    }

    private void OnRestartGameEvent()
    {
        UIConsole.Instance.Log("Game Restarted!", UIConsole.Instance.GameEndingMessage);

        StartGame();
    }

    #endregion

    public IEnumerator PerformActionOnDelay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);

        action.Invoke();
    }
}
