using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CancelSearchButton : MonoBehaviour
{
    private Button _cancelButton;

    private void Awake()
    {
        _cancelButton = GetComponent<Button>();

        _cancelButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if(PhotonNetwork.CurrentRoom == null)
        {
            _cancelButton.interactable = false;
        }
        else
        {
            _cancelButton.interactable = true;
        }
    }

    private void OnClick()
    {
        NetworkManager.Instance.StopSearching();
    }
}
