using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Events
{
    public struct MoveMadeEventArgs : IGameEvent
    {
        public GameBoard GameBoard { get; private set; }
        public Move Move { get; private set; }
        public PieceColor PieceColor { get; private set; }
        public bool Checking { get; private set; } 
        public float WaitTime { get; private set; }

        public MoveMadeEventArgs(GameBoard gameBoard, Move move, PieceColor pieceColor, bool checking, float waitTime)
        {
            GameBoard = gameBoard;
            Move = move;
            PieceColor = pieceColor;
            Checking = checking;
            WaitTime = waitTime;
        }
    }

    public struct PieceDestroyedEventArgs : IGameEvent
    {
        public PieceColor Color { get; private set; }
        public Type Type { get; private set; }

        public PieceDestroyedEventArgs(PieceColor color, Type type)
        {
            Color = color;
            Type = type;
        }
    }
}

public enum PieceType
{
    PAWN,
    ROOK,
    BISHOP,
    KNIGHT,
    QUEEN,
    KING,
}


public class GameBoard : GameEventUserObject, IOnEventCallback
{
    //Static
    public const float TILE_SPACING = 1.405f;

    public static Vector3 PIECE_ADJUSTMENT => new Vector3(0f, 0f, -5f);
    public static Vector3 STARTING_POSITION => new Vector3(-8.25f, -4.9f, 0f) + _BLACK_ADJUSTMENT;
    private static Vector3 _BLACK_ADJUSTMENT => (GameController.Instance.PlayerColor == PieceColor.Red ? new Vector3(6.75f, 0f, 0f) : Vector3.zero);

    private static Dictionary<Type, Dictionary<PieceColor, GameObject>> _PIECE_DICTIONARY;
    private static Dictionary<Enum, Type> _ENUM_TO_TYPE_DICTIONARY;

    #region Inspector

    [SerializeField] private Transform _background;

    [Header("Tiles")]

    [SerializeField] private Sprite _whiteTile;
    [SerializeField] private Sprite _whiteSelectableTile;
    [SerializeField] private Sprite _whiteHighlightedTile;

    [SerializeField] private Sprite _blackTile;
    [SerializeField] private Sprite _blackSelectableTile;
    [SerializeField] private Sprite _blackHighlightedTile;

    [Header("Game Pieces")]

    [SerializeField] private GameObject _whiteKing;
    [SerializeField] private GameObject _whiteQueen;
    [SerializeField] private GameObject _whiteRook;
    [SerializeField] private GameObject _whiteBishop;
    [SerializeField] private GameObject _whiteKnight;
    [SerializeField] private GameObject _whitePawn;

    [SerializeField] private GameObject _blackKing;
    [SerializeField] private GameObject _blackQueen;
    [SerializeField] private GameObject _blackRook;
    [SerializeField] private GameObject _blackBishop;
    [SerializeField] private GameObject _blackKnight;
    [SerializeField] private GameObject _blackPawn;

    #endregion

    //Properties
    public List<List<Tile>> Board { get; private set; }

    public List<GamePiece> GamePieces { get; private set; }

    public bool RunningCheckMateCheck { get; private set; }
    public bool Check { get; private set; }
    public bool CheckMate { get; private set; }
    public bool WaitingForPromotion => PawnOnTopOrBottom();

    //Private
    private GameObject _pieceHolder;

    private void Awake()
    {
        SetPieceDictionary();
    }

    public override void Subscribe()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void Unsubscribe()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #region Piece Dictionary

    private void SetPieceDictionary()
    {
        if (_PIECE_DICTIONARY != null) return;

        _PIECE_DICTIONARY = new Dictionary<Type, Dictionary<PieceColor, GameObject>>()
        {
            {
                typeof(KingPiece),
                new Dictionary<PieceColor, GameObject>()
                {
                    { PieceColor.Blue, _whiteKing },
                    { PieceColor.Red, _blackKing },
                }
            },
            {
                typeof(QueenPiece),
                new Dictionary<PieceColor, GameObject>()
                {
                    { PieceColor.Blue, _whiteQueen },
                    { PieceColor.Red, _blackQueen },
                }
            },
            {
                typeof(RookPiece),
                new Dictionary<PieceColor, GameObject>()
                {
                    { PieceColor.Blue, _whiteRook },
                    { PieceColor.Red, _blackRook },
                }
            },
            {
                typeof(BishopPiece),
                new Dictionary<PieceColor, GameObject>()
                {
                    { PieceColor.Blue, _whiteBishop },
                    { PieceColor.Red, _blackBishop },
                }
            },
            {
                typeof(KnightPiece),
                new Dictionary<PieceColor, GameObject>()
                {
                    { PieceColor.Blue, _whiteKnight },
                    { PieceColor.Red, _blackKnight },
                }
            },
            {
                typeof(PawnPiece),
                new Dictionary<PieceColor, GameObject>()
                {
                    { PieceColor.Blue, _whitePawn },
                    { PieceColor.Red, _blackPawn },
                }
            },
        };

        _ENUM_TO_TYPE_DICTIONARY = new Dictionary<Enum, Type>()
        {
            { PieceType.PAWN, typeof(PawnPiece) },
            { PieceType.ROOK,typeof(RookPiece) },
            { PieceType.BISHOP,typeof(BishopPiece) },
            { PieceType.KNIGHT,typeof(KnightPiece) },
            { PieceType.QUEEN,typeof(QueenPiece) },
            { PieceType.KING,typeof(KingPiece) },
        };
    }

    #endregion

    public void Setup()
    {
        Clean();

        _pieceHolder = new GameObject("PieceHolder");
        _pieceHolder.transform.parent = transform;

        GamePieces = new List<GamePiece>();

        SetupBoard();
        SetupPieces();

        if (GameController.Instance.PlayerColor == PieceColor.Red)
        {
            transform.localEulerAngles += new Vector3(0, 0, 180);

            if(_background)
            {
                _background.position = new Vector3(-3.4f, 0f, 1f);
                _background.localEulerAngles = new Vector3(0f, 0f, 0f);
            }
        }
    }

    public void Setup(List<List<Tile>> board)
    {
        Clean();

        _pieceHolder = new GameObject("PieceHolder");
        _pieceHolder.transform.parent = transform;

        GamePieces = new List<GamePiece>();

        SetupBoard();
        SetupPieces(board);
    }

    private void Clean()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    #region Setup

    private void SetupBoard()
    {
        Transform rowParent;

        Board = new List<List<Tile>>();

        RunningCheckMateCheck = false;
        Check = false;
        CheckMate = false;

        for (int i = 0; i < 8; i++)
        {
            Board.Add(new List<Tile>());

            rowParent = new GameObject($"{i}").transform;
            rowParent.parent = transform;

            for (int j = 0; j < 8; j++)
            {
                Board[i].Add(new GameObject($"{i}x{j}").AddComponent<Tile>());

                Board[i][j].transform.parent = rowParent;

                Board[i][j].Initialize(
                    new Vector2Int(i, j),
                    new Vector3(
                        STARTING_POSITION.x + TILE_SPACING * i,
                        STARTING_POSITION.y + TILE_SPACING * j,
                        0f),
                    (i + j) % 2 == 0 ? _blackTile : _whiteTile,
                    (i + j) % 2 == 0 ? _blackSelectableTile : _whiteSelectableTile,
                    (i + j) % 2 == 0 ? _blackHighlightedTile : _whiteHighlightedTile); //Not good right now but short term, may need to make another object to hold sprite data
            }
        }
    }

    private void SetupPieces()
    {
        SetPiece(_whiteRook, new Vector2Int(0, 0));
        SetPiece(_whitePawn, new Vector2Int(0, 1));
        SetPiece(_blackPawn, new Vector2Int(0, 6));
        SetPiece(_blackRook, new Vector2Int(0, 7));

        SetPiece(_whiteKnight, new Vector2Int(1, 0));
        SetPiece(_whitePawn, new Vector2Int(1, 1));                                        
        SetPiece(_blackPawn, new Vector2Int(1, 6));
        SetPiece(_blackKnight, new Vector2Int(1, 7));

        SetPiece(_whiteBishop, new Vector2Int(2, 0));
        SetPiece(_whitePawn, new Vector2Int(2, 1));
        SetPiece(_blackPawn, new Vector2Int(2, 6));
        SetPiece(_blackBishop, new Vector2Int(2, 7));

        SetPiece(_whiteQueen, new Vector2Int(3, 0));
        SetPiece(_whitePawn, new Vector2Int(3, 1));
        SetPiece(_blackPawn, new Vector2Int(3, 6));
        SetPiece(_blackQueen, new Vector2Int(3, 7));
        
        SetPiece(_whiteKing, new Vector2Int(4, 0));
        SetPiece(_whitePawn, new Vector2Int(4, 1));
        SetPiece(_blackPawn, new Vector2Int(4, 6));
        SetPiece(_blackKing, new Vector2Int(4, 7));

        SetPiece(_whiteBishop, new Vector2Int(5, 0));
        SetPiece(_whitePawn, new Vector2Int(5, 1));
        SetPiece(_blackPawn, new Vector2Int(5, 6));
        SetPiece(_blackBishop, new Vector2Int(5, 7));

        SetPiece(_whiteKnight, new Vector2Int(6, 0));
        SetPiece(_whitePawn, new Vector2Int(6, 1));
        SetPiece(_blackPawn, new Vector2Int(6, 6));
        SetPiece(_blackKnight, new Vector2Int(6, 7));

        SetPiece(_whiteRook, new Vector2Int(7, 0));
        SetPiece(_whitePawn, new Vector2Int(7, 1));
        SetPiece(_blackPawn, new Vector2Int(7, 6));
        SetPiece(_blackRook, new Vector2Int(7, 7));
    }

    private void SetupPieces(List<List<Tile>> board)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GamePiece currentPiece = board[i][j].CurrentPiece;

                if (!currentPiece) continue;

                SetPiece(_PIECE_DICTIONARY[currentPiece.GetType()][currentPiece.Color], new Vector2Int(i, j));
            }
        }
    }

    private void SetPiece(GameObject prefab, Vector2Int coordinates)
    {
        GameObject pieceObject = Instantiate(prefab, _pieceHolder.transform);
        if(!GetTile(coordinates, out Tile tile)) return;

        //Sets the piece's transform position/Rotation
        pieceObject.transform.position = tile.transform.position + PIECE_ADJUSTMENT;
        if(GameController.Instance.PlayerColor == PieceColor.Red) pieceObject.transform.localEulerAngles += new Vector3(0, 0, 180);

        //Sets the piece's reference to the current board tile
        tile.CurrentPiece = pieceObject.GetComponent<GamePiece>();

        //Sets the tile reference to the current piece
        //A little janky but it only needs to be set like this at the start of the game
        tile.CurrentPiece.CurrentTile = tile;

        GamePieces.Add(tile.CurrentPiece);
    }

    #endregion

    //This logic should probably be somewhere else, but its not too bad
    private void Update()
    {
        CheckIfMoveMade();
    }

    private void CheckIfMoveMade()
    {
        if (Input.GetMouseButtonUp(0))
        {
            RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (hit) hit.transform.GetComponent<Tile>()?.TryMove();
        }
    }

    public void HighlightValidMoves(List<Move> moves)
    {
        ClearBoard();

        foreach (Move move in moves)
        {
            if(GetTile(move.EndingPos, out Tile endingTile))
            {
                endingTile.Selectable(move);
            }            
        }
    }

    //Bad Nested Logic in here, needs to be cleaned up
    public void Move(Move move, bool sendEvent = true)
    {
        //All Sequential actions taken before event is called
        GetTile(move.StartingPos, out Tile startingTile);
        GetTile(move.EndingPos, out Tile endingTile);

        bool inCheck = false;
        float moveTime = startingTile.CurrentPiece.OnMove(move);

        List<AudioClip> moveClips;

        if (endingTile.CurrentPiece)
        {
            RemovePiece(endingTile.CurrentPiece, moveTime);

            moveClips = endingTile.CurrentPiece.Color == PieceColor.Blue ? SFXPalette.Instance.WhitePieceDestroyed : SFXPalette.Instance.BlackPieceDestroyed;

            if(GameController.Instance.CurrentGameBoard == this)
            {
                UIConsole.Instance.Log($"{endingTile.CurrentPiece.Color} {endingTile.CurrentPiece.Name()} Captured!", UIConsole.Instance.ImportantMessage);
            }
        }
        else
        {
            moveClips = SFXPalette.Instance.PieceMoved;
        }

        if (GameController.Instance.CurrentGameBoard == this)
        {
            SFXPalette.Instance.PlayeClipOnDelay(moveClips, moveTime);
        }

        endingTile.CurrentPiece = startingTile.CurrentPiece;
        startingTile.CurrentPiece = null;

        #region Handle Nested Moves

        foreach (Move nestedMove in move.NestedMoves)
        {
            GetTile(nestedMove.StartingPos, out Tile nestedStartingTile);
            GetTile(nestedMove.EndingPos, out Tile nestedEndingTile);

            nestedStartingTile.CurrentPiece.OnMove(nestedMove);

            if (nestedEndingTile.CurrentPiece)
            {
                RemovePiece(nestedEndingTile.CurrentPiece);
            }

            nestedEndingTile.CurrentPiece = nestedStartingTile.CurrentPiece;
            nestedStartingTile.CurrentPiece = null;
        }

        #endregion

        ClearBoard();

        if (GameController.Instance.CurrentGameBoard == this) //If the current game board used by the Game Controller
        {
            inCheck = endingTile.CurrentPiece.CheckChecking(this);

            if(!inCheck)
            {
                inCheck = InCheck(endingTile.CurrentPiece.Color);
            }

            if (sendEvent && !GameController.Instance.IsLocalGame)
            {
                PhotonNetwork.RaiseEvent(
                    PhotonEvents.MOVE_EVENT_CODE,
                    move.ConvertToObjectArray(),
                    new RaiseEventOptions() { Receivers = ReceiverGroup.Others },
                    SendOptions.SendReliable);
            }

            UIConsole.Instance.Log($"{endingTile.CurrentPiece.Color} {endingTile.CurrentPiece.Name()}: {move.ConvertMoveToString()}", UIConsole.Instance.StandardMessage);

            EventManager.Instance.TriggerEvent(new Events.MoveMadeEventArgs(
                this, 
                move, 
                endingTile.CurrentPiece.Color, 
                inCheck,
                moveTime));
        }

        endingTile.CurrentPiece.OnMoveLate(move);
    }

    public void RemovePiece(GamePiece piece)
    {
        GameObject particles = piece.Color == PieceColor.Blue ? EffectPalette.Instance.WhitePieceDestroyed : EffectPalette.Instance.BlackPieceDestroyed;
        GameObject explosion = Instantiate(particles);

        //GamePieces.Remove(piece);

        explosion.transform.position = new Vector3(
            piece.transform.position.x - PIECE_ADJUSTMENT.x,
            piece.transform.position.y - PIECE_ADJUSTMENT.y,
            explosion.transform.position.z);

        EventManager.Instance.TriggerEvent(new Events.PieceDestroyedEventArgs(piece.Color, piece.GetType()));

        Destroy(piece.gameObject);
    }

    public void RemovePiece(GamePiece piece, float delay)
    {
        StartCoroutine(PerformActionOnDelay(new Action<GamePiece>(RemovePiece), piece, delay));
    }

    #region Networking Code

    public void OnEvent(EventData photonEvent)
    {
        switch(photonEvent.Code)
        {
            case PhotonEvents.MOVE_EVENT_CODE:
                OnMoveEvent(photonEvent);
                break;
        }
    }

    private void OnMoveEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;

        Move(new Move(data), false);
    }

    #endregion

    #region Helpers

    public void SetPiece(Type type, PieceColor color, Vector2Int coordinates)
    {
        SetPiece(_PIECE_DICTIONARY[type][color], coordinates);
    }

    public void SetPiece(PieceType type, PieceColor color, Vector2Int coordinates)
    {
        SetPiece(_PIECE_DICTIONARY[_ENUM_TO_TYPE_DICTIONARY[type]][color], coordinates);
    }

    public bool GetTile(Vector2Int coordinates, out Tile tile)
    {
        if (coordinates.x >= 0 &&
            coordinates.x < 8 &&
            coordinates.y >= 0 &&
            coordinates.y < 8)
        {
            tile = Board[coordinates.x][coordinates.y];
            return true;
        }

        tile = null;
        return false;
    }

    public bool GetKingPos(PieceColor color, out Vector2Int coordinates)
    {
        foreach(GamePiece gamePiece in GamePieces)
        {
            if(gamePiece)
            {
                if (gamePiece.Color != color) continue;
                if (gamePiece.GetType() == typeof(KingPiece))
                {
                    coordinates = gamePiece.Coordinates;
                    return true;
                }
            }
        }

        coordinates = new Vector2Int();
        return false;
    }

    //Takes in color of person check mating
    //Returns true if checkmate
    public IEnumerator CheckIfCheckMate(PieceColor color)
    {
        RunningCheckMateCheck = true;

        yield return null;

        foreach (GamePiece gamePiece in GamePieces)
        {
            if(gamePiece)
            {
                if (gamePiece.Color == color)
                {
                    if (gamePiece.GetValidMoves().Count > 0)
                    {
                        //Debug.Log(gamePiece.GetValidMoves()[0].ConvertMoveToString());

                        RunningCheckMateCheck = false;

                        yield break;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        CheckMate = true;

        RunningCheckMateCheck = false;
    }

    public IEnumerator PerformActionOnDelay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);

        action.Invoke();
    }

    public IEnumerator PerformActionOnDelay<T>(Action<T> action, T parameters, float delay)
    {
        yield return new WaitForSeconds(delay);

        action.Invoke(parameters);
    }

    public void ApplyToEachTile(Action<Tile> action)
    {
        for (int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                action.Invoke(Board[i][j]);
            }
        }
    }

    private bool PawnOnTopOrBottom()
    {
        for(int i = 0; i < 8; i++)
        {
            if (Board[i][0].CurrentPiece)
            {
                if (Board[i][0].CurrentPiece.GetType() == typeof(PawnPiece))
                {
                    return true;
                }
            }

            if (Board[i][7].CurrentPiece)
            {
                if (Board[i][7].CurrentPiece.GetType() == typeof(PawnPiece))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool InCheck(PieceColor color)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if(Board[i][j].CurrentPiece)
                {
                    if (Board[i][j].CurrentPiece.Color == color)
                    {
                        if (Board[i][j].CurrentPiece.CheckChecking(this)) return true;
                    }
                }
            }
        }

        return false;
    }

    public void ClearBoard() => ApplyToEachTile((t) => t.Unselectable());

    #endregion

}
