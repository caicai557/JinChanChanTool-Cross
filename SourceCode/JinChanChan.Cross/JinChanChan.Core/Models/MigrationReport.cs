namespace JinChanChan.Core.Models;

public sealed class MigrationReport
{
    public bool Success { get; set; }

    public string SourceVersion { get; set; } = "legacy";

    public List<string> AppliedMappings { get; set; } = new();

    public List<string> Warnings { get; set; } = new();

    public Dictionary<string, string> LegacyPayload { get; set; } = new();
}
