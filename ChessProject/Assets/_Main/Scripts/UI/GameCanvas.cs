using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    public static GameCanvas Instance;

    [SerializeField] private GameObject _pawnPromotionPanel;
    [SerializeField] private GameObject _gameOverPanel;

    public bool Active { get; private set; } = false;

    public PieceType? SelectedPiece = null;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        OnStartGame();
    }

    public void GetPawnPromotionPiece()
    {
        Active = true;
        SelectedPiece = null;

        _pawnPromotionPanel.SetActive(true);
    }

    public void CleanupPawnPromotionPanel()
    {
        Active = false;
        SelectedPiece = null;

        _pawnPromotionPanel.SetActive(false);
    }

    public void OnStartGame()
    {
        Active = false;

        _pawnPromotionPanel.SetActive(false);
        _gameOverPanel.SetActive(false);
    }

    public void OnGameOver()
    {
        Active = true;

        _gameOverPanel.SetActive(true);
    }
}
