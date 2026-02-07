namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class LoopMetricsSnapshot
    {
        public required DateTime Timestamp { get; init; }

        public required string Mode { get; init; }

        public double CaptureMs { get; init; }

        public double OcrMs { get; init; }

        public double CorrectionMs { get; init; }

        public double MatchMs { get; init; }

        public double ActionMs { get; init; }

        public double LoopTotalMs { get; init; }
    }
}
