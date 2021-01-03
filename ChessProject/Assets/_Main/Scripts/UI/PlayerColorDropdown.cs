using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerColorDropdown : MonoBehaviour
{
    public bool HostWhite { get; private set; } = true;

    private TMP_Dropdown _dropdown;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();

        _dropdown.onValueChanged.AddListener(UpdateMembers);
        _dropdown.onValueChanged.AddListener((int i) => SFXPalette.Instance.PlayClip(SFXPalette.Instance.ButtonClicked));
    }

    private void UpdateMembers(int value)
    {
        HostWhite = value == 0;
    }

}
