using System.Text.Json;
using JinChanChanTool.DataClass;

namespace JinChanChanTool.Tests;

public class ManualSettingsCompatibilityTests
{
    [Fact]
    public void DeserializeLegacyJson_ShouldKeepNewFieldDefaults()
    {
        string legacyJson = "{\"HotKey1\":\"F7\",\"IsUseCPUForInference\":true}";

        ManualSettings settings = JsonSerializer.Deserialize<ManualSettings>(legacyJson)!;

        Assert.True(settings.UseNewLoopEngine);
        Assert.True(settings.EnablePerfMetrics);
        Assert.Equal(1, settings.CpuOcrConsumerCount);
        Assert.True(settings.OcrWarmupEnabled);
    }
}
