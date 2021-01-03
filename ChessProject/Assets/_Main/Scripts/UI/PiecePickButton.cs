using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PiecePickButton : MonoBehaviour
{
    [SerializeField] private PieceType _pieceType;

    [SerializeField] private Sprite _whiteSprite;
    [SerializeField] private Sprite _blackSprite;

    private Button _button;
    private Image _image;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();

        _button.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        if(GameController.Instance)
        {
            _image.sprite = GameController.Instance.CurrentTurn == PieceColor.Blue ? _whiteSprite : _blackSprite;
        }
    }

    private void OnClick()
    {
        GameCanvas.Instance.SelectedPiece = _pieceType;
    }
}
