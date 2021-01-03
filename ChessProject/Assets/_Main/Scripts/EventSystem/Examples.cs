using System.Collections;
using UnityEngine;

public class EventExampleObj : GameEventUserObject
{
    private void DisplayFinalScore(Events.GameOverEventArgsExample eventArgs)
    {
        Debug.Log("Display " + eventArgs.Time.ToString());
    }

    public override void Subscribe()
    {
        EventManager.Instance.AddListener<Events.GameOverEventArgsExample>(this, DisplayFinalScore);
    }

    public override void Unsubscribe()
    {
        EventManager.Instance.RemoveListener<Events.GameOverEventArgsExample>(this, DisplayFinalScore);
    }
}

public class EventExampleTriggererObj : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(EndGame(5f));
    }

    private IEnumerator EndGame(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        EventManager.Instance.TriggerEvent(new Events.GameOverEventArgsExample(seconds));
    }
}