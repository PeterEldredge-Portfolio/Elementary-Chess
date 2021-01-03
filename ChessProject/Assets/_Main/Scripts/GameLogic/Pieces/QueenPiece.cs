using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenPiece : GamePiece
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

    public override string Name() => "Queen";

    protected override List<Move> GetStandardMoves(GameBoard board)
    {
        List<Move> standardMoves = new List<Move>();

        foreach (Vector2Int directon in _directionsToCheck)
        {
            int count = 1;

            while (board.GetTile(new Vector2Int(Coordinates.x + directon.x * count, Coordinates.y + directon.y * count), out Tile tile))
            {
                if (!tile.CurrentPiece || tile.CurrentPiece?.Color != Color)
                {
                    standardMoves.Add(new Move(
                        Coordinates,
                        new Vector2Int(Coordinates.x + directon.x * count, Coordinates.y + directon.y * count)));
                }

                if (tile.CurrentPiece) break;

                count++;
            }
        }

        return standardMoves;
    }
}
