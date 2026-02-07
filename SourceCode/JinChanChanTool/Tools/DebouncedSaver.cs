namespace JinChanChanTool.Tools
{
    public sealed class DebouncedSaver : IDisposable
    {
        private readonly TimeSpan _delay;
        private readonly object _lock = new();
        private CancellationTokenSource _cts = new();

        public DebouncedSaver(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Invoke(Action action)
        {
            if (action == null)
            {
                return;
            }

            CancellationTokenSource localCts;
            lock (_lock)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = new CancellationTokenSource();
                localCts = _cts;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_delay, localCts.Token);
                    if (!localCts.Token.IsCancellationRequested)
                    {
                        action();
                    }
                }
                catch (OperationCanceledException)
                {
                }
            });
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }
    }
}
