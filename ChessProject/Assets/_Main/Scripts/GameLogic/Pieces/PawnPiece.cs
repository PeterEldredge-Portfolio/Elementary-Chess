using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnPiece : GamePiece, IOnEventCallback
{
    public bool LastMoveSpecialCase { get; private set; } = false;

    private int _topDownMultiplier = 1;

    protected override void Awake()
    {
        base.Awake();

        if (Color == PieceColor.Red) _topDownMultiplier = -1;
    }

    private void OnMoveMade(Events.MoveMadeEventArgs args)
    {
        if (args.PieceColor != Color) LastMoveSpecialCase = false;
    }

    protected void OnEnable()
    {
        EventManager.Instance.AddListener<Events.MoveMadeEventArgs>(this, OnMoveMade);

        PhotonNetwork.AddCallbackTarget(this);
    }

    protected void OnDisable()
    {
        EventManager.Instance.RemoveListener<Events.MoveMadeEventArgs>(this, OnMoveMade);

        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override string Name() => "Pawn";

    public override float OnMove(Move move)
    {
        float moveTime = base.OnMove(move);

        if ((move.EndingPos.y - move.StartingPos.y) * _topDownMultiplier > 1) //Checks if move was the special case
        {
            LastMoveSpecialCase = true;
        }
        else if ((move.EndingPos - move.StartingPos).magnitude > 1 && //This is a brutal check for En Passant
                  _gameBoard == GameController.Instance.CurrentGameBoard)
        {
            if(_gameBoard.GetTile(move.EndingPos, out Tile enPassantCheck))
            {
                if(!enPassantCheck.CurrentPiece)
                {
                    if(_gameBoard.GetTile(move.EndingPos + new Vector2Int(0, _topDownMultiplier * -1), out enPassantCheck))
                    {
                        _gameBoard.RemovePiece(enPassantCheck.CurrentPiece, moveTime);
                    }
                }
            }
        }

        return moveTime;
    }

    public override void OnMoveLate(Move move)
    {
        base.OnMoveLate(move);

        if (Coordinates.y == (_topDownMultiplier == 1 ? 7 : 0) &&
            _gameBoard == GameController.Instance.CurrentGameBoard)
        {
            if (Color == GameController.Instance.PlayerColor ||
                GameController.Instance.PlayerColor == PieceColor.None)
            {
                StartCoroutine(PromoteRoutine());
            }
        }
    }

    protected override List<Move> GetStandardMoves(GameBoard board)
    {
        List<Move> standardMoves = new List<Move>();

        if(board.GetTile(new Vector2Int(Coordinates.x, Coordinates.y + 1 * _topDownMultiplier), out Tile tile))
        {
            if (!tile.CurrentPiece)
            {
                standardMoves.Add(new Move(
                    Coordinates,
                    new Vector2Int(Coordinates.x, Coordinates.y + 1 * _topDownMultiplier)));
            }
        }

        //Handles pawn piece taking pieces diagonally
        if (board.GetTile(new Vector2Int(Coordinates.x - 1, Coordinates.y + 1 * _topDownMultiplier), out Tile diagonalLeftTile))
        {
            if (diagonalLeftTile.CurrentPiece && diagonalLeftTile.CurrentPiece?.Color != Color)
            {
                standardMoves.Add(new Move(
                    Coordinates,
                    new Vector2Int(Coordinates.x - 1, Coordinates.y + 1 * _topDownMultiplier)));
            }
        }

        if (board.GetTile(new Vector2Int(Coordinates.x + 1, Coordinates.y + 1 * _topDownMultiplier), out Tile diagonalRightTile))
        {
            if (diagonalRightTile.CurrentPiece && diagonalRightTile.CurrentPiece?.Color != Color)
            {
                standardMoves.Add(new Move(
                    Coordinates,
                    new Vector2Int(Coordinates.x + 1, Coordinates.y + 1 * _topDownMultiplier)));
            }
        }

        return standardMoves;
    }

    protected override List<Move> GetSpecialCaseMoves(GameBoard board)
    {
        List<Move> specialCaseMoves = new List<Move>();
        PawnPiece tempPiece;

        //En Passant
        if (board.GetTile(new Vector2Int(Coordinates.x - 1, Coordinates.y), out Tile leftTile))
        {
            tempPiece = leftTile.CurrentPiece as PawnPiece;

            if (tempPiece)
            {
                if (tempPiece.Color != Color &&
                   tempPiece.LastMoveSpecialCase)
                {
                    specialCaseMoves.Add(new Move(
                        Coordinates,
                        new Vector2Int(Coordinates.x - 1, Coordinates.y + 1 * _topDownMultiplier)));
                }
            }
        }

        if (board.GetTile(new Vector2Int(Coordinates.x + 1, Coordinates.y), out Tile rightTile))
        {
            tempPiece = rightTile.CurrentPiece as PawnPiece;

            if (tempPiece)
            {
                if (tempPiece.Color != Color &&
                   tempPiece.LastMoveSpecialCase)
                {
                    specialCaseMoves.Add(new Move(
                        Coordinates,
                        new Vector2Int(Coordinates.x + 1, Coordinates.y + 1 * _topDownMultiplier)));
                }
            }
        }

        if (HasMoved) return specialCaseMoves;
        
        //First Move Special Case
        if (board.GetTile(new Vector2Int(Coordinates.x, Coordinates.y + 1 * _topDownMultiplier), out Tile oneSpaceTile) &&
            board.GetTile(new Vector2Int(Coordinates.x, Coordinates.y + 2 * _topDownMultiplier), out Tile twoSpaceTile))
        {
            if (!oneSpaceTile.CurrentPiece && !twoSpaceTile.CurrentPiece)
            {
                specialCaseMoves.Add(new Move(
                    Coordinates,
                    new Vector2Int(Coordinates.x, Coordinates.y + 2 * _topDownMultiplier)));
            }
        }

        return specialCaseMoves;
    }

    private IEnumerator PromoteRoutine()
    {
        StartCoroutine(PerformActionOnDelay(() => GameCanvas.Instance.GetPawnPromotionPiece(), 1f));

        while(GameCanvas.Instance.SelectedPiece == null)
        {
            yield return null;
        }

        Promote((PieceType)GameCanvas.Instance.SelectedPiece);

        if(!SettingsManager.Instance.IsLocalGame)
        {
            object[] content = new object[]
            {
                Coordinates.x,
                Coordinates.y,
                (int) GameCanvas.Instance.SelectedPiece,
            };

            PhotonNetwork.RaiseEvent(
                PhotonEvents.PAWN_PROMOTION_EVENT_CODE,
                content,
                new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
                SendOptions.SendReliable);
        }

        GameCanvas.Instance.CleanupPawnPromotionPanel();
    }

    private void Promote(PieceType type) //Needs to be updated so pieces may be selected
    {
        _gameBoard.SetPiece(type, Color, Coordinates);
        //_gameBoard.GamePieces.Remove(this);

        if(_gameBoard == GameController.Instance.CurrentGameBoard)
        {
            UIConsole.Instance.Log($"Pawn Promoted to { CurrentTile.CurrentPiece.Name() }!", UIConsole.Instance.ImportantMessage);
        }

        if(_gameBoard.GamePieces[_gameBoard.GamePieces.Count - 1].CheckChecking(_gameBoard)) UIConsole.Instance.Log("Check!", UIConsole.Instance.ImportantMessage);

        Destroy(gameObject);
    }

    #region Networking Code

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case PhotonEvents.PAWN_PROMOTION_EVENT_CODE:
                OnPiecePromotionEvent(photonEvent);
                break;
        }
    }

    private void OnPiecePromotionEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;

        if (Coordinates.x == (int)data[0] &&
            Coordinates.y == (int)data[1])
        {
            Promote((PieceType)data[2]);
        }
    }

    #endregion

    public IEnumerator PerformActionOnDelay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);

        action.Invoke();
    }
}
