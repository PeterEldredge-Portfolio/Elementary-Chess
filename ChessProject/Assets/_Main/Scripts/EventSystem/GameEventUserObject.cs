using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameEventUserObject : MonoBehaviour, IUseGameEvents
{
    protected virtual void OnEnable()
    {
        Subscribe();
    }

    protected virtual void OnDisable()
    {
        Unsubscribe();
    }

    public virtual void Subscribe() {}
    public virtual void Unsubscribe() {}
}
