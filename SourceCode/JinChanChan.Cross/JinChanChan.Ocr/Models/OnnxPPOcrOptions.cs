namespace JinChanChan.Ocr.Models;

public sealed class OnnxPPOcrOptions
{
    public required string RecognizerModelPath { get; init; }

    public string? DetectorModelPath { get; init; }

    public string? KeysFilePath { get; init; }

    public OnnxExecutionProvider PreferredProvider { get; init; } = OnnxExecutionProvider.Cpu;

    public bool AllowProviderFallback { get; init; } = true;

    public int IntraOpNumThreads { get; init; } = 1;
}
