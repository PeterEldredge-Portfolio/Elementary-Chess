using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{
    private Button _exitButton;

    private void Awake()
    {
        _exitButton = GetComponent<Button>();

        _exitButton.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        Application.Quit();
    }
}
