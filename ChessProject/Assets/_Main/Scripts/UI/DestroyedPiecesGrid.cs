using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DestroyedPiecesGrid : GameEventUserObject
{
    [SerializeField] private PieceColor _pieceColor;

    [SerializeField] private GameObject _imagePrefab;

    [SerializeField] private Sprite _pawnSprite;
    [SerializeField] private Sprite _rookSprite;
    [SerializeField] private Sprite _bishopSprite;
    [SerializeField] private Sprite _knightSprite;
    [SerializeField] private Sprite _queenSprite;

    private Dictionary<Type, Sprite> _spriteDictionary;

    private void Awake()
    {
        _spriteDictionary = new Dictionary<Type, Sprite>()
        {
            { typeof(PawnPiece), _pawnSprite },
            { typeof(RookPiece), _rookSprite },
            { typeof(BishopPiece), _bishopSprite },
            { typeof(KnightPiece), _knightSprite },
            { typeof(QueenPiece), _queenSprite },
        };
    }

    public override void Subscribe()
    {
        EventManager.Instance.AddListener<Events.GameStartEventArgs>(this, OnGameStart);
        EventManager.Instance.AddListener<Events.PieceDestroyedEventArgs>(this, OnPieceDestroyed);
    }

    public override void Unsubscribe()
    {
        EventManager.Instance.RemoveListener<Events.GameStartEventArgs>(this, OnGameStart);
        EventManager.Instance.RemoveListener<Events.PieceDestroyedEventArgs>(this, OnPieceDestroyed);
    }

    private void OnGameStart(Events.GameStartEventArgs args)
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnPieceDestroyed(Events.PieceDestroyedEventArgs args)
    {
        if (args.Color == _pieceColor)
        {
            AddPieceToGrid(_spriteDictionary[args.Type]);
        }
    }

    private void AddPieceToGrid(Sprite sprite)
    {
        Image image = Instantiate(_imagePrefab, transform).GetComponent<Image>();

        image.sprite = sprite;
    }
}
