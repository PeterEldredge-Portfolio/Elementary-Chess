using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LetterLabel : MonoBehaviour
{
    private TMP_Text _tmpText;

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (!SettingsManager.Instance.IsLocalGame)
        {
            if (GameController.Instance.PlayerColor == PieceColor.Red)
            {
                _tmpText.text = $"HGFEDCBA";
            }
        }
    }
}
