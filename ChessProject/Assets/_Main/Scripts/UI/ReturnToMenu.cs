using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ReturnToMenu : MonoBehaviourPunCallbacks
{
    private Button _returnToMenuButton;

    private void Awake()
    {
        _returnToMenuButton = GetComponent<Button>();

        _returnToMenuButton.onClick.AddListener(ToMenu);
    }

    private void ToMenu()
    {
        //PhotonNetwork.LoadLevel("Main Menu");
    }
}
