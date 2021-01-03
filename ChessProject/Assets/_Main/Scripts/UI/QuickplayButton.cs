using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickplayButton : MonoBehaviour
{
    private Button _quickplayButton;

    private void Awake()
    {
        _quickplayButton = GetComponent<Button>();

        _quickplayButton.onClick.AddListener(OnQuickplayClicked);
    }

    private void Update()
    {
        if(PhotonNetwork.IsConnectedAndReady)
        {
            _quickplayButton.interactable = true;
        }
        else
        {
            _quickplayButton.interactable = false;
        }
    }

    private void OnQuickplayClicked()
    {
        NetworkManager.Instance.OnQuickplayButtonClick();
    }
}
