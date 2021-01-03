using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPalette : MonoBehaviour
{
    public static EffectPalette Instance;

    public GameObject BlackPieceDestroyed;
    public GameObject WhitePieceDestroyed;

    private void Awake()
    {
        if(!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
