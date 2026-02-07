using JinChanChan.Core.Services;

namespace JinChanChan.Core.Tests;

public class ConfigMigratorTests
{
    [Fact]
    public async Task ShouldMapLegacyManualSettingsToNewSchema()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"jcct_migration_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            string manual = """
            {
              "HotKey1": "F7",
              "HotKey2": "F8",
              "IsUseDynamicCoordinates": true,
              "CpuOcrConsumerCount": 2,
              "HeroNameScreenshotRectangle_1": {"X":1,"Y":2,"Width":3,"Height":4},
              "RefreshStoreButtonRectangle": {"X":10,"Y":11,"Width":12,"Height":13}
            }
            """;

            await File.WriteAllTextAsync(Path.Combine(tempDir, "ManualSettings.json"), manual);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "AutomaticSettings.json"), "{}");

            ConfigMigrator migrator = new();
            var (settings, report) = await migrator.MigrateAsync(tempDir);

            Assert.True(report.Success);
            Assert.Equal("F7", settings.Hotkeys.ToggleAutoPick);
            Assert.True(settings.Coordinates.UseDynamicCoordinates);
            Assert.Equal(2, settings.CpuOcrConsumerCount);
            Assert.Single(settings.Coordinates.CardNameRects);
            Assert.False(settings.Coordinates.RefreshButtonRect.IsEmpty);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldMapProcessAndInputKeys()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"jcct_migration_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            string manual = """
            {
              "TargetProcessName": "League of Legends",
              "TargetProcessId": 1234,
              "IsStrictMatching": true,
              "UseNewLoopEngine": false,
              "RefreshStoreKey": "D",
              "HeroPurchaseKey1": "Q",
              "HeroPurchaseKey2": "W",
              "HeroPurchaseKey3": "E",
              "HeroPurchaseKey4": "R",
              "HeroPurchaseKey5": "T"
            }
            """;

            await File.WriteAllTextAsync(Path.Combine(tempDir, "ManualSettings.json"), manual);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "AutomaticSettings.json"), "{}");

            ConfigMigrator migrator = new();
            var (settings, report) = await migrator.MigrateAsync(tempDir);

            Assert.True(report.Success);
            Assert.Equal("League of Legends", settings.TargetProcessName);
            Assert.Equal(1234, settings.TargetProcessId);
            Assert.True(settings.StrictMatching);
            Assert.False(settings.UseNewLoopEngine);
            Assert.Equal("D", settings.RefreshKey);
            Assert.Equal(["Q", "W", "E", "R", "T"], settings.PurchaseKeys);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
