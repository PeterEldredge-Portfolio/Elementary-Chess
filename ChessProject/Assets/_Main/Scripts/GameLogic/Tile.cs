using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GamePiece CurrentPiece { get; set; }

    public Vector2Int Coordinates { get; private set; }

    private Sprite _standardSprite;
    private Sprite _selectableSprite;
    private Sprite _highlightedSprite;

    private GameBoard _gameBoard;

    private SpriteRenderer _renderer;

    private bool _selectable;
    private Move? _selectableMove;

    public void Initialize(Vector2Int coordinates, Vector3 position, Sprite standardSprite, Sprite selectableSprite, Sprite highlightedSprite)
    {
        _gameBoard = GetComponentInParent<GameBoard>();

        Coordinates = coordinates;

        _standardSprite = standardSprite;
        _selectableSprite = selectableSprite;
        _highlightedSprite = highlightedSprite;

        _renderer = gameObject.AddComponent<SpriteRenderer>();
        _renderer.sprite = _standardSprite;

        gameObject.AddComponent<BoxCollider2D>();

        transform.position = position;

        _selectable = false;
    }

    public void Selectable(Move move)
    {
        _selectable = true;
        _selectableMove = move;

        _renderer.sprite = _selectableSprite;
    }

    public void Unselectable()
    {
        _selectable = false;
        _selectableMove = null;

        _renderer.sprite = _standardSprite;
    }

    public void TryMove()
    {
        if (_selectable && _selectableMove != null)
        {
            _gameBoard.Move((Move) _selectableMove);
        }
    }

    private void OnMouseDown()
    {
        if (CurrentPiece &&
            !_selectable &&
            CurrentPiece.Color == GameController.Instance.CurrentTurn &&
            (CurrentPiece.Color == GameController.Instance.PlayerColor || GameController.Instance.PlayerColor == PieceColor.None) &&
            !GameCanvas.Instance.Active)
        {
            _gameBoard.HighlightValidMoves(CurrentPiece.GetValidMoves());

            StartCoroutine(HoldingPieceRoutine());
        }
    }

    private void OnMouseOver()
    {
        if(_selectable)
        {
            _renderer.sprite = _highlightedSprite;
        }
    }

    private void OnMouseExit()
    {
        if(_selectable)
        {
            _renderer.sprite = _selectableSprite;
        }
    }

    private IEnumerator HoldingPieceRoutine()
    {
        Vector3 piecePosition = CurrentPiece.transform.position;

        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 offset = new Vector3(
            mousePoint.x - CurrentPiece.transform.position.x,
            mousePoint.y - CurrentPiece.transform.position.y,
            0f);

        float distance = 0;

        SFXPalette.Instance.PlayClip(SFXPalette.Instance.PieceClicked);

        while (Input.GetMouseButton(0))
        {
            mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            CurrentPiece.transform.position = new Vector3(mousePoint.x, mousePoint.y, piecePosition.z) - offset;

            distance = Vector3.Distance(CurrentPiece.transform.position, piecePosition);

            yield return null;
        }

        if (CurrentPiece)
        {
            if(distance > GameBoard.TILE_SPACING / 2)
            {
                SFXPalette.Instance.PlayClip(SFXPalette.Instance.PieceMissed);
            }

            CurrentPiece.transform.position = piecePosition;
        }
    }
}
