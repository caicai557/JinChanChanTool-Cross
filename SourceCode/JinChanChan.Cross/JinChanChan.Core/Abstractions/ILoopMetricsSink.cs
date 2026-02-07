using JinChanChan.Core.Models;

namespace JinChanChan.Core.Abstractions;

public interface ILoopMetricsSink
{
    void Track(LoopMetricsSnapshot snapshot);

    IReadOnlyList<LoopMetricsSnapshot> GetSnapshots();

    void Clear();
}
