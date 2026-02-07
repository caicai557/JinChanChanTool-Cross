namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class LoopMetricsSink : ILoopMetricsSink
    {
        private readonly object _lock = new();
        private readonly List<LoopMetricsSnapshot> _snapshots = new();
        private const int MaxSnapshotCount = 5000;

        public void Track(LoopMetricsSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            lock (_lock)
            {
                _snapshots.Add(snapshot);
                if (_snapshots.Count > MaxSnapshotCount)
                {
                    _snapshots.RemoveRange(0, _snapshots.Count - MaxSnapshotCount);
                }
            }
        }

        public IReadOnlyList<LoopMetricsSnapshot> GetSnapshots()
        {
            lock (_lock)
            {
                return _snapshots.ToArray();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _snapshots.Clear();
            }
        }
    }
}
