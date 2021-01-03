using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public struct GameStartedEventArgsExample : IGameEvent { }

    public struct GameOverEventArgsExample : IGameEvent
    {
        public float Time { get; }

        public GameOverEventArgsExample(float time)
        {
            Time = time;
        }
    }
}