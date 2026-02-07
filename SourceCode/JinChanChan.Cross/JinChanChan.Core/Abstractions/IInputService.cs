namespace JinChanChan.Core.Abstractions;

public interface IInputService
{
    Task MoveMouseAsync(int x, int y, CancellationToken cancellationToken = default);

    Task LeftClickAsync(CancellationToken cancellationToken = default);

    Task PressKeyAsync(string key, CancellationToken cancellationToken = default);
}
