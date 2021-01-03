using UnityEngine;
using TMPro;

public class GameTimeController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _hoursField;
    [SerializeField] private TMP_InputField _minutesField;
    [SerializeField] private TMP_InputField _secondsField;

    private void Awake()
    {
        _hoursField.onEndEdit.AddListener(ValidateHoursInput);
        _minutesField.onEndEdit.AddListener(ValidateMinutesInput);
        _secondsField.onEndEdit.AddListener(ValidateSecondsInput);
    }

    private void ValidateHoursInput(string value) => ValidateInput(_hoursField, value);
    private void ValidateMinutesInput(string value) => ValidateInput(_minutesField, value);
    private void ValidateSecondsInput(string value) => ValidateInput(_secondsField, value);

    private void ValidateInput(TMP_InputField field, string value)
    {
        if(value.Length == 0)
        {
            field.text = $"00";
        }
        if (value.Length == 1)
        {
            field.text = $"0{value}";
        }
        else if(value.Length == 2)
        {
            field.text = value;
        }
    }

    public float GetGameTime()
    {
        int hours = string.IsNullOrEmpty(_hoursField.text) ? 0 : int.Parse(_hoursField.text);
        int minutes = string.IsNullOrEmpty(_minutesField.text) ? 0 : int.Parse(_minutesField.text);
        int seconds = string.IsNullOrEmpty(_secondsField.text) ? 0 : int.Parse(_secondsField.text);

        return hours * 3600 + minutes * 60 + seconds;
    }

    public void SetGameTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time - hours * 3600) / 60);
        int seconds = Mathf.FloorToInt(time - hours * 3600 - minutes * 60);

        ValidateHoursInput(hours.ToString());
        ValidateMinutesInput(minutes.ToString());
        ValidateSecondsInput(seconds.ToString());
    }
}
