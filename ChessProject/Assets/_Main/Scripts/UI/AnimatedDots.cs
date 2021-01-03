using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AnimatedDots : MonoBehaviour
{
    private const float _ANIMATION_SPEED = .5f;

    private TMP_Text _tmpText;

    private List<string> _frames = new List<string>()
    {
        "",
        ".",
        "..",
        "...",
    };

    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        StartCoroutine(DotRoutine());
    }

    private IEnumerator DotRoutine()
    {
        int currentFrame = 0;

        while(true)
        {
            _tmpText.text = _frames[currentFrame];

            yield return new WaitForSeconds(_ANIMATION_SPEED);

            currentFrame = (currentFrame + 1) % _frames.Count;
        }
    }
}
