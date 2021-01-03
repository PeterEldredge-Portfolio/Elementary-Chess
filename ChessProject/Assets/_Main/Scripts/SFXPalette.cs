using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPalette : MonoBehaviour
{
    public static SFXPalette Instance;

    public List<AudioClip> ButtonClicked;

    public List<AudioClip> PieceClicked;
    public List<AudioClip> PieceMoved;
    public List<AudioClip> PieceMissed;

    public List<AudioClip> WhitePieceDestroyed;
    public List<AudioClip> BlackPieceDestroyed;

    public List<AudioClip> Win;
    public List<AudioClip> Lose;

    private AudioSource _audioSource;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayClip(List<AudioClip> clipChoices)
    {
        _audioSource.PlayOneShot(clipChoices[UnityEngine.Random.Range(0, clipChoices.Count)], .35f);
    }

    public void PlayClip(List<AudioClip> clipChoices, float volume)
    {
        _audioSource.PlayOneShot(clipChoices[UnityEngine.Random.Range(0, clipChoices.Count)], volume);
    }

    public void PlayeClipOnDelay(List<AudioClip> clipChoices, float time)
    {
        StartCoroutine(ActionOnDelay(() => PlayClip(clipChoices), time));
    }

    private IEnumerator ActionOnDelay(Action action, float time)
    {
        yield return new WaitForSeconds(time);

        action.Invoke();
    }
}
