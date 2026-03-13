using System.Runtime.CompilerServices;

namespace Engine.Events;

public sealed class EventBus
{
    private static class Channel<T> where T : struct
    {
        public static readonly List<Action<T>> Listeners = new(8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        Channel<T>.Listeners.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        Channel<T>.Listeners.Remove(handler);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish<T>(in T evt) where T : struct
    {
        var listeners = Channel<T>.Listeners;
        for (int i = 0; i < listeners.Count; i++)
            listeners[i](evt);
    }

    public void Clear<T>() where T : struct
    {
        Channel<T>.Listeners.Clear();
    }
}
