using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingPiece : GamePiece
{
    private List<Vector2Int> _directionsToCheck = new List<Vector2Int>()
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
    };

    public override string Name() => "King";

    protected override List<Move> GetStandardMoves(GameBoard board)
    {
        List<Move> standardMoves = new List<Move>();

        foreach (Vector2Int directon in _directionsToCheck)
        {
            if (board.GetTile(new Vector2Int(Coordinates.x + directon.x, Coordinates.y + directon.y), out Tile tile))
            {
                if (!tile.CurrentPiece || tile.CurrentPiece?.Color != Color)
                {
                    standardMoves.Add(new Move(
                        Coordinates,
                        new Vector2Int(Coordinates.x + directon.x, Coordinates.y + directon.y)));
                }
            }
        }

        return standardMoves;
    }

    protected override List<Move> GetSpecialCaseMoves(GameBoard board)
    {
        bool checkFound = false;
        List<Move> specialCaseMoves = new List<Move>();

        if (HasMoved) return specialCaseMoves;

        GameBoard testBoard = new GameObject("TestBoard").AddComponent<GameBoard>();

        //Uh, this should probably be refactored at some point
        if (board.GetTile(new Vector2Int(0, Coordinates.y), out Tile tempTile))
        {
            if (tempTile.CurrentPiece)
            {
                if (!tempTile.CurrentPiece.HasMoved)
                {
                    for (int i = 1; i < Coordinates.x; i++)
                    {
                        if (board.GetTile(new Vector2Int(i, Coordinates.y), out tempTile))
                        {
                            if (tempTile.CurrentPiece != null)
                            {
                                break;
                            }
                            else if (i == (Coordinates.x - 1))
                            {
                                testBoard.Setup(_gameBoard.Board);

                                testBoard.Move(new Move(
                                    Coordinates,
                                    new Vector2Int(Coordinates.x - 1, Coordinates.y)));

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

                                if (!checkFound)
                                {
                                    specialCaseMoves.Add(new Move(
                                        Coordinates,
                                        new Vector2Int(Coordinates.x - 2, Coordinates.y),
                                        new List<Move>()
                                        {
                                            new Move(
                                            new Vector2Int(0, Coordinates.y),
                                            new Vector2Int(Coordinates.x - 1, Coordinates.y))
                                        }));
                                }
                            }
                        }
                    }
                }
            }
        }

        checkFound = false;

        if (board.GetTile(new Vector2Int(7, Coordinates.y), out tempTile))
        {
            if(tempTile.CurrentPiece)
            {
                if (!tempTile.CurrentPiece.HasMoved)
                {
                    for (int i = Coordinates.x + 1; i < 7; i++)
                    {
                        if (board.GetTile(new Vector2Int(i, Coordinates.y), out tempTile))
                        {
                            if (tempTile.CurrentPiece != null)
                            {
                                break;
                            }
                            else if (i == 6)
                            {
                                testBoard.Setup(_gameBoard.Board);

                                testBoard.Move(new Move(
                                    Coordinates,
                                    new Vector2Int(Coordinates.x + 1, Coordinates.y)));

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

                                if(!checkFound)
                                {
                                    specialCaseMoves.Add(new Move(
                                            Coordinates,
                                            new Vector2Int(Coordinates.x + 2, Coordinates.y),
                                            new List<Move>()
                                            {
                                                new Move(
                                                new Vector2Int(7, Coordinates.y),
                                                new Vector2Int(Coordinates.x + 1, Coordinates.y))
                                            }));
                                }
                            }
                        }
                    }
                }
            }
        }

        Destroy(testBoard.gameObject);

        return specialCaseMoves;
    }
}
