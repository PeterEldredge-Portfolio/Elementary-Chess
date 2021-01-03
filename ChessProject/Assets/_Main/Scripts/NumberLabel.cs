using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumberLabel : MonoBehaviour
{
    private TMP_Text _tmpText;

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if(!SettingsManager.Instance.IsLocalGame)
        {
            if(GameController.Instance.PlayerColor == PieceColor.Red)
            {
                _tmpText.text = $"1\n2\n3\n4\n5\n6\n7\n8";
            }
        }
    }
}
