namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class RecognizedFrame
    {
        public required string[] RawResults { get; init; }

        public required string[] CorrectedResults { get; init; }

        public required bool[] TargetFlags { get; init; }
    }
}
