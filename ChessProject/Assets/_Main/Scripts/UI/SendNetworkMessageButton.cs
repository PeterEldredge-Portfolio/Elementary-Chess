using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SendNetworkMessageButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (string.IsNullOrEmpty(_inputField.text)) return;

        UIConsole.Instance.SendNetworkedMessage(_inputField.text);

        _inputField.text = "";
    }
}
