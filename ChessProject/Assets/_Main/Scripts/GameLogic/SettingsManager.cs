using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public const float LOCAL_MOVE_TIME = 1200f;

    public const float QUICKPLAY_MOVE_TIME = 1800f;
    public const PieceColor QUICKPLAY_HOST_COLOR = PieceColor.Blue;

    public const float CUSTOM_MOVE_TIME_DEFAULT = 1800f;
    public const PieceColor CUSTOM_HOST_COLOR_DEFAULT = PieceColor.Blue;

    public static SettingsManager Instance { get; private set; }

    [HideInInspector] public float MoveTime = QUICKPLAY_MOVE_TIME;

    [HideInInspector] public bool IsLocalGame = true;
    [HideInInspector] public bool IsQuickplayGame = false;

    [HideInInspector] public PieceColor HostColor = PieceColor.Blue;
    
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
    }

    public void SetLocalRules()
    {
        MoveTime = LOCAL_MOVE_TIME;
        
        IsLocalGame = true;
        IsQuickplayGame = false;
    }

    public void SetQuickplayRules()
    {
        MoveTime = QUICKPLAY_MOVE_TIME;
        HostColor = QUICKPLAY_HOST_COLOR;

        IsLocalGame = false;
        IsQuickplayGame = true;
    }

    public void SetCustomDefaultRules()
    {
        MoveTime = CUSTOM_MOVE_TIME_DEFAULT;
        HostColor = CUSTOM_HOST_COLOR_DEFAULT;

        IsLocalGame = false;
        IsQuickplayGame = false;
    }
}
