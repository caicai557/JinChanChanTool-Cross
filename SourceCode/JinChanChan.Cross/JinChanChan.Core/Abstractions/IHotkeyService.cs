namespace JinChanChan.Core.Abstractions;

public interface IHotkeyService : IDisposable
{
    void Register(string key, Action onPressed, Action? onReleased = null);

    void Unregister(string key);

    void UnregisterAll();
}
