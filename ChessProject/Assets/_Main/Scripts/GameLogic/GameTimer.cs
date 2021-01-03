using System.Collections;
using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    private const string ZERO = "0";

    [SerializeField] private PieceColor _pieceColor;

    private TMP_Text _textUI;

    private float _time;

    private int _hours = 0;
    private int _minutes = 0;
    private int _seconds = 0;

    private void Awake()
    {
        _textUI = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            _time = _pieceColor == PieceColor.Blue ? GameController.Instance.WhiteTimer : GameController.Instance.BlackTimer;

            _textUI.text = ConvertTimeToString();

            yield return new WaitForSeconds(.1f);
        }
    }

    private string ConvertTimeToString()
    {
        _hours = Mathf.FloorToInt(_time / 3600f);
        _minutes = Mathf.FloorToInt((_time - (_hours * 3600f)) / 60f);
        _seconds = Mathf.FloorToInt(_time - (_hours * 3600f) - (_minutes * 60f));

        return $"{ZeroOrSpace(_hours)}{_hours}:{ZeroOrSpace(_minutes)}{_minutes}:{ZeroOrSpace(_seconds)}{_seconds}";
    }

    private string ZeroOrSpace(int i)
    {
        return i < 10 ? ZERO : string.Empty;
    }
}
