using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface IGameStateReader
{
    LiveGameState Read(
        IReadOnlyList<string> shopCards,
        IReadOnlyList<string> preferredTargets,
        bool autoPickEnabled,
        bool autoRefreshEnabled,
        DateTimeOffset timestamp);
}
