using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Move
{
    public static Dictionary<int, char> LetterConversionDictionary = new Dictionary<int, char>()
    {
        {0, 'A'},
        {1, 'B'},
        {2, 'C'},
        {3, 'D'},
        {4, 'E'},
        {5, 'F'},
        {6, 'G'},
        {7, 'H'},
    };

    public Vector2Int StartingPos { get; private set; }
    public Vector2Int EndingPos { get; private set; }

    public List<Move> NestedMoves { get; private set; }

    public Move(Move move)
    {
        StartingPos = move.StartingPos;
        EndingPos = move.EndingPos;

        NestedMoves = move.NestedMoves;
    }

    public Move(Vector2Int startingPos, Vector2Int endingPos)
    {
        StartingPos = startingPos;
        EndingPos = endingPos;

        NestedMoves = new List<Move>();
    }

    public Move(Vector2Int startingPos, Vector2Int endingPos, List<Move> nestedMoves)
    {
        StartingPos = startingPos;
        EndingPos = endingPos;

        NestedMoves = nestedMoves;
    }

    public Move(object[] content)
    {
        StartingPos = new Vector2Int((int)content[0], (int)content[1]);
        EndingPos = new Vector2Int((int)content[2], (int)content[3]);

        NestedMoves = new List<Move>();

        for (int i = 4; i < content.Length; i += 4)
        {
            NestedMoves.Add(new Move(new Vector2Int((int)content[i], (int)content[i + 1]), new Vector2Int((int)content[i + 2], (int)content[i + 3])));
        }
    }

    public string ConvertMoveToString()
    {
        return $"{ConvertCoordinatesToString(StartingPos)} to {ConvertCoordinatesToString(EndingPos)}";
    }

    private string ConvertCoordinatesToString(Vector2Int coordinates)
    {
        return $"({LetterConversionDictionary[coordinates.x]}, {coordinates.y + 1})";
    }

    public object[] ConvertToObjectArray()
    {
        object[] baseContent = new object[]
        {
            StartingPos.x,
            StartingPos.y,
            EndingPos.x,
            EndingPos.y,
        };

        object[] tempContent;
        object[] nestedContentHolder;
        object[] nestedContent = new object[0];

        foreach (Move move in NestedMoves)
        {
            tempContent = move.ConvertToObjectArray();
            nestedContentHolder = nestedContent;

            nestedContent = new object[nestedContentHolder.Length + tempContent.Length];
            nestedContentHolder.CopyTo(nestedContent, 0);
            tempContent.CopyTo(nestedContent, nestedContentHolder.Length);
        }

        tempContent = new object[baseContent.Length + nestedContent.Length];
        baseContent.CopyTo(tempContent, 0);
        nestedContent.CopyTo(tempContent, baseContent.Length);

        return tempContent;
    }
}

public enum PieceColor
{
    Blue,
    Red,
    None,
}

public abstract class GamePiece : MonoBehaviour
{
    private const float _MOVE_WAITTIME = .1f;
    private const float _MIN_MOVE_DISTANCE = .005f;
    private const float _MOVE_SPEED = 3f;

    [SerializeField] public PieceColor Color;

    public Tile CurrentTile { get; set; }
    public bool HasMoved { get; set; } = false;

    public Vector2Int Coordinates => CurrentTile.Coordinates;

    protected GameBoard _gameBoard;

    protected virtual void Awake()
    {
        _gameBoard = GetComponentInParent<GameBoard>();
    }

    public List<Move> GetValidMoves(bool validate = true)
    {
        List<Move> validMoves = new List<Move>();

        validMoves.AddRange(GetStandardMoves(_gameBoard));
        validMoves.AddRange(GetSpecialCaseMoves(_gameBoard));

        if (validate) validMoves = ValidateMoves(validMoves);

        return validMoves;
    }

    [ContextMenu("Print Valid Moves")]
    public void PrintValidMoves()
    {
        foreach(Move move in GetValidMoves())
        {
            Debug.Log(move.ConvertMoveToString());
        }
    }

    public List<Move> GetValidMoves(GameBoard board, bool validate = true)
    {
        List<Move> validMoves = new List<Move>();

        validMoves.AddRange(GetStandardMoves(board));
        validMoves.AddRange(GetSpecialCaseMoves(board));

        if (validate) validMoves = ValidateMoves(validMoves);

        return validMoves;
    }

    private List<Move> ValidateMoves(List<Move> moves)
    {
        List<Move> validatedMoves = new List<Move>();

        //This is not good, will always introduce stutter unless broken into multiple frames or maybe pooled??
        GameBoard testBoard = new GameObject("TestBoard").AddComponent<GameBoard>();

        foreach (Move move in moves)
        {
            bool checkFound = false;

            testBoard.Setup(_gameBoard.Board);

            testBoard.Move(move);

            foreach (List<Tile> columnTiles in testBoard.Board)
            {
                foreach (Tile tile in columnTiles)
                {
                    if (!tile.CurrentPiece) continue;
                    if (tile.CurrentPiece.Color == Color) continue;

                    if (tile.CurrentPiece.CheckChecking(testBoard))
                    {
                        checkFound = true;
                        break;
                    }
                }
            }

            if (!checkFound) validatedMoves.Add(move);
        }

        Destroy(testBoard.gameObject);

        return validatedMoves;
    }

    public virtual float OnMove(Move move)
    {
        _gameBoard.GetTile(move.EndingPos, out Tile currentTile);

        CurrentTile = currentTile;
        HasMoved = true;

        Vector3 endingPosition = currentTile.transform.position + GameBoard.PIECE_ADJUSTMENT;

        float moveTime = Vector3.Distance(transform.position, endingPosition) / _MOVE_SPEED;

        StartCoroutine(MovePositionRoutine(endingPosition, moveTime));

        //Values are from trial and error
        return moveTime * .2f - (moveTime / 10) + _MOVE_WAITTIME;
    }

    private IEnumerator MovePositionRoutine(Vector3 endingPosition, float moveTime)
    {
        Vector3 tempPositionHolder;

        float timer = 0f;

        yield return new WaitForSeconds(_MOVE_WAITTIME);

        while (timer < 1f)
        {
            timer += Time.deltaTime / moveTime;

            tempPositionHolder = Vector3.Lerp(transform.position, endingPosition, timer);

            if (Vector3.Distance(transform.position, tempPositionHolder) < _MIN_MOVE_DISTANCE) break;

            transform.position = tempPositionHolder;

            yield return null;
        }

        transform.position = endingPosition;
    }
    
    //Returns true if piece checks king
    public bool CheckChecking(GameBoard board)
    {
        List<Move> moves = GetValidMoves(board, false);

        board.GetKingPos(Color == PieceColor.Red ? PieceColor.Blue : PieceColor.Red, out Vector2Int kingCoordinates);

        foreach (Move move in moves)
        {
            if (move.EndingPos == kingCoordinates) return true;
        }

        return false;
    }

    //Abstract
    public abstract string Name();
    protected abstract List<Move> GetStandardMoves(GameBoard board);

    //Virtual
    public virtual void OnMoveLate(Move move) { }
    protected virtual List<Move> GetSpecialCaseMoves(GameBoard board) => new List<Move>();
}
