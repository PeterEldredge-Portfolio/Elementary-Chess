using UnityEngine;
using TMPro;

public class QuickplayTimeLabel : MonoBehaviour
{
    private TMP_Text _tmpText;

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        int hours = Mathf.FloorToInt(SettingsManager.QUICKPLAY_MOVE_TIME / 3600);
        int minutes = Mathf.FloorToInt((SettingsManager.QUICKPLAY_MOVE_TIME - hours * 3600) / 60);
        int seconds = Mathf.FloorToInt(SettingsManager.QUICKPLAY_MOVE_TIME - hours * 3600 - minutes * 60);

        _tmpText.text = $"Move Time - {ConvertToString(hours)} : {ConvertToString(minutes)} : {ConvertToString(seconds)}";
    }

    private string ConvertToString(int value)
    {
        string tempString = value.ToString();

        if (tempString.Length == 1)
        {
            tempString = $"0{tempString}";
        }

        return tempString;
    }
}
