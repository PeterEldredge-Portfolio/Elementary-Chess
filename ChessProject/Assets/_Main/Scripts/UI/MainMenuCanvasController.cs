using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCanvasController : MonoBehaviour
{
    private const float _SCROLL_END_X = -2.91f;
    private const float _SCROLL_TIME = 1f;

    [SerializeField] private Transform _gridHolder;

    private float _scrollStartX;

    private Coroutine _scroll;
    private Coroutine _unscroll;

    private void Awake()
    {
        _scrollStartX = _gridHolder.position.x;
    }

    public void Scroll()
    {
        _scroll = StartCoroutine(ScrollRoutine());
    }

    public void Unscroll()
    {
        _unscroll = StartCoroutine(UnscrollRoutine());
    }

    private IEnumerator ScrollRoutine()
    {
        float timer = 0f;

        Vector3 endPos = new Vector3(_SCROLL_END_X, _gridHolder.position.y, _gridHolder.position.z);

        if(_unscroll != null) StopCoroutine(_unscroll); 

        while (timer < _SCROLL_TIME)
        {
            _gridHolder.position = Vector3.Lerp(_gridHolder.position, endPos, timer / _SCROLL_TIME);

            yield return null;

            timer += Time.deltaTime;
        }

        _gridHolder.position = endPos;
    }

    private IEnumerator UnscrollRoutine()
    {
        float timer = 0f;

        Vector3 endPos = new Vector3(_scrollStartX, _gridHolder.position.y, _gridHolder.position.z);

        if (_scroll != null) StopCoroutine(_scroll);

        while (timer < _SCROLL_TIME)
        {
            _gridHolder.position = Vector3.Lerp(_gridHolder.position, endPos, timer / _SCROLL_TIME);

            yield return null;

            timer += Time.deltaTime;
        }

        _gridHolder.position = endPos;
    }
}
