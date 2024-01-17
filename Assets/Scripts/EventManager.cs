using System;
using System.Collections.Generic;

public class GameEvent { }

public class EventManager
{
    private static readonly Dictionary<Type, List<Action<GameEvent>>> eventListeners = new Dictionary<Type, List<Action<GameEvent>>>();

    public static void AddListener<T>(Action<T> listener) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (!eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType] = new List<Action<GameEvent>>();
        }

        eventListeners[eventType].Add((e) => listener((T)e));
    }

    public static void RemoveListener<T>(Action<T> listener) where T : GameEvent
    {
        Type eventType = typeof(T);

        if (eventListeners.ContainsKey(eventType))
        {
            eventListeners[eventType].Remove((e) => listener((T)e));

            // Remove the event type entry if there are no more listeners
            if (eventListeners[eventType].Count == 0)
            {
                eventListeners.Remove(eventType);
            }
        }
    }

    public static void Broadcast(GameEvent gameEvent)
    {
        Type eventType = gameEvent.GetType();

        if (eventListeners.ContainsKey(eventType))
        {
            foreach (var listener in eventListeners[eventType])
            {
                listener.Invoke(gameEvent);
            }
        }
    }

    public static void Clear()
    {
        eventListeners.Clear();
    }

    private static string previousScene;


    public static string GetPreviousScene()
    {
        return previousScene;
    }

    public static void SetPreviousScene(string sceneName)
    {
        previousScene = sceneName;
    }
}
