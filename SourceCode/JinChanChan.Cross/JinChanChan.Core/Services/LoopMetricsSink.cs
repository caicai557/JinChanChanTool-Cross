using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class LoopMetricsSink : ILoopMetricsSink
{
    private readonly object _sync = new();
    private readonly List<LoopMetricsSnapshot> _snapshots = new();
    private const int MaxSize = 10000;

    public void Track(LoopMetricsSnapshot snapshot)
    {
        lock (_sync)
        {
            _snapshots.Add(snapshot);
            if (_snapshots.Count > MaxSize)
            {
                _snapshots.RemoveRange(0, _snapshots.Count - MaxSize);
            }
        }
    }

    public IReadOnlyList<LoopMetricsSnapshot> GetSnapshots()
    {
        lock (_sync)
        {
            return _snapshots.ToArray();
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _snapshots.Clear();
        }
    }
}
