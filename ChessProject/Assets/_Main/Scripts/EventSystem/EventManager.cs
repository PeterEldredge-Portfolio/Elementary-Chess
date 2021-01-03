using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public struct ObjectActionPair<T>
{
    public object Object { get; }
    public Action<T> Action { get; }

    public ObjectActionPair(object obj, Action<T> action)
    {
        Object = obj;
        Action = action;
    }
}

public class EventManager : MonoBehaviour
{
    private const int _MAX_TRIGGERED_EVENTS_PER_FRAME = 5;

    public static EventManager Instance { get; private set; }

    public Dictionary<Type, List<ObjectActionPair<IGameEvent>>> _eventListeners = new Dictionary<Type, List<ObjectActionPair<IGameEvent>>>();

    private Queue<IGameEvent> _triggeredEventQueue = new Queue<IGameEvent>();

    private int _eventsLeftToTrigger = 0;

    private Type _eventTypeStorage;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void LateUpdate()
    {
        while (_eventsLeftToTrigger > 0 && _triggeredEventQueue.Count > 0)
        {
            TriggerEventImmediate(_triggeredEventQueue.Dequeue());
        }

        _eventsLeftToTrigger = _MAX_TRIGGERED_EVENTS_PER_FRAME;
    }

    public void AddListener<T>(object obj, Action<T> action) where T : IGameEvent
    {
        _eventTypeStorage = typeof(T);

        if (!_eventListeners.ContainsKey(_eventTypeStorage))
        {
            _eventListeners.Add(_eventTypeStorage, new List<ObjectActionPair<IGameEvent>>());
        }

        _eventListeners[_eventTypeStorage].Add(new ObjectActionPair<IGameEvent>(obj, new Action<IGameEvent>((eventArgs) => action.Invoke((T)eventArgs))));
    }

    public void RemoveListener<T>(object obj, Action<T> action) where T : IGameEvent
    {
        _eventTypeStorage = typeof(T);

        if (_eventListeners != null && _eventListeners.ContainsKey(_eventTypeStorage))
        {
            _eventListeners[_eventTypeStorage] = _eventListeners[_eventTypeStorage].OfType<ObjectActionPair<IGameEvent>>().Where(x => x.Object != obj).ToList();
        }
    }

    public void TriggerEvent(IGameEvent gameEventArgs)
    {
        _triggeredEventQueue.Enqueue(gameEventArgs);
    }

    public void TriggerEventImmediate(IGameEvent gameEventArgs)
    {
        _eventTypeStorage = gameEventArgs.GetType();

        if (_eventListeners.ContainsKey(_eventTypeStorage))
        {
            for (int i = 0; i < _eventListeners[_eventTypeStorage].Count; i++)
            {
                _eventListeners[_eventTypeStorage][i].Action.Invoke(gameEventArgs);
            }
        }

        _eventsLeftToTrigger--;
    }
}
