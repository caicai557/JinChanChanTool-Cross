namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface ILoopMetricsSink
    {
        void Track(LoopMetricsSnapshot snapshot);

        IReadOnlyList<LoopMetricsSnapshot> GetSnapshots();

        void Clear();
    }
}
