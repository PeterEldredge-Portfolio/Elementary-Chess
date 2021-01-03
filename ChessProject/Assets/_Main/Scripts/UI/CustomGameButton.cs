using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CustomGameButton : MonoBehaviour
{
    private Button _customGameButton;

    private void Awake()
    {
        _customGameButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            _customGameButton.interactable = true;
        }
        else
        {
            _customGameButton.interactable = false;
        }
    }
}
