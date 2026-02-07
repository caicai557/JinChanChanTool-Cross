using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Config;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class ConfigMigrator : IConfigMigrator
{
    private const string ManualFileName = "ManualSettings.json";
    private const string AutomaticFileName = "AutomaticSettings.json";

    public async Task<(AppSettings Settings, MigrationReport Report)> MigrateAsync(string resourcesPath, CancellationToken cancellationToken = default)
    {
        AppSettings settings = new();
        MigrationReport report = new() { Success = true };

        if (string.IsNullOrWhiteSpace(resourcesPath) || !Directory.Exists(resourcesPath))
        {
            report.Success = false;
            report.Warnings.Add("资源目录不存在，使用默认配置。");
            return (settings, report);
        }

        string manualPath = Path.Combine(resourcesPath, ManualFileName);
        string automaticPath = Path.Combine(resourcesPath, AutomaticFileName);

        if (File.Exists(manualPath))
        {
            try
            {
                string manualJson = await File.ReadAllTextAsync(manualPath, cancellationToken);
                using JsonDocument doc = JsonDocument.Parse(manualJson);
                MapManualSettings(doc.RootElement, settings, report);
            }
            catch (Exception ex)
            {
                report.Success = false;
                report.Warnings.Add($"ManualSettings.json 迁移失败: {ex.Message}");
            }
        }
        else
        {
            report.Warnings.Add("未找到 ManualSettings.json。");
        }

        if (File.Exists(automaticPath))
        {
            try
            {
                string autoJson = await File.ReadAllTextAsync(automaticPath, cancellationToken);
                using JsonDocument doc = JsonDocument.Parse(autoJson);
                MapAutomaticSettings(doc.RootElement, settings, report);
            }
            catch (Exception ex)
            {
                report.Success = false;
                report.Warnings.Add($"AutomaticSettings.json 迁移失败: {ex.Message}");
            }
        }
        else
        {
            report.Warnings.Add("未找到 AutomaticSettings.json。");
        }

        return (settings, report);
    }

    private static void MapManualSettings(JsonElement root, AppSettings settings, MigrationReport report)
    {
        TryMapString(root, "HotKey1", v => settings.Hotkeys.ToggleAutoPick = v, report, "HotKey1 -> Hotkeys.ToggleAutoPick");
        TryMapString(root, "HotKey2", v => settings.Hotkeys.ToggleRefresh = v, report, "HotKey2 -> Hotkeys.ToggleRefresh");
        TryMapString(root, "HotKey3", v => settings.Hotkeys.ToggleMainWindow = v, report, "HotKey3 -> Hotkeys.ToggleMainWindow");
        TryMapString(root, "HotKey4", v => settings.Hotkeys.HoldRoll = v, report, "HotKey4 -> Hotkeys.HoldRoll");
        TryMapString(root, "HotKey5", v => settings.Hotkeys.ToggleHighlight = v, report, "HotKey5 -> Hotkeys.ToggleHighlight");

        TryMapBool(root, "IsUseDynamicCoordinates", v => settings.Coordinates.UseDynamicCoordinates = v, report, "IsUseDynamicCoordinates -> Coordinates.UseDynamicCoordinates");
        TryMapBool(root, "IsKeyboardHeroPurchase", v => settings.UseKeyboardPurchase = v, report, "IsKeyboardHeroPurchase -> UseKeyboardPurchase");
        TryMapBool(root, "IsMouseHeroPurchase", v => settings.UseMousePurchase = v, report, "IsMouseHeroPurchase -> UseMousePurchase");
        TryMapBool(root, "IsKeyboardRefreshStore", v => settings.UseKeyboardRefresh = v, report, "IsKeyboardRefreshStore -> UseKeyboardRefresh");
        TryMapBool(root, "IsMouseRefreshStore", v => settings.UseMouseRefresh = v, report, "IsMouseRefreshStore -> UseMouseRefresh");
        TryMapBool(root, "IsStrictMatching", v => settings.StrictMatching = v, report, "IsStrictMatching -> StrictMatching");
        TryMapBool(root, "EnablePerfMetrics", v => settings.EnablePerfMetrics = v, report, "EnablePerfMetrics -> EnablePerfMetrics");
        TryMapBool(root, "UseNewLoopEngine", v => settings.UseNewLoopEngine = v, report, "UseNewLoopEngine -> UseNewLoopEngine");
        TryMapInt(root, "CpuOcrConsumerCount", v => settings.CpuOcrConsumerCount = Math.Max(1, v), report, "CpuOcrConsumerCount -> CpuOcrConsumerCount");
        TryMapBool(root, "OcrWarmupEnabled", v => settings.OcrWarmupEnabled = v, report, "OcrWarmupEnabled -> OcrWarmupEnabled");
        TryMapString(root, "RefreshStoreKey", v => settings.RefreshKey = v, report, "RefreshStoreKey -> RefreshKey");
        TryMapString(root, "TargetProcessName", v => settings.TargetProcessName = v, report, "TargetProcessName -> TargetProcessName");
        TryMapInt(root, "TargetProcessId", v => settings.TargetProcessId = Math.Max(0, v), report, "TargetProcessId -> TargetProcessId");
        MapPurchaseKeys(root, settings, report);

        MapRectRange(root, "HeroNameScreenshotRectangle_", settings.Coordinates.CardNameRects, report, "CardNameRects");
        MapRectRange(root, "HighLightRectangle_", settings.Coordinates.CardClickRects, report, "CardClickRects");
        MapRect(root, "RefreshStoreButtonRectangle", rect => settings.Coordinates.RefreshButtonRect = rect, report, "RefreshStoreButtonRectangle -> Coordinates.RefreshButtonRect");

        StoreLegacy(root, settings, report, new HashSet<string>(StringComparer.Ordinal)
        {
            "HotKey1","HotKey2","HotKey3","HotKey4","HotKey5",
            "IsUseDynamicCoordinates","IsKeyboardHeroPurchase","IsMouseHeroPurchase","IsKeyboardRefreshStore","IsMouseRefreshStore",
            "IsStrictMatching","EnablePerfMetrics","UseNewLoopEngine","CpuOcrConsumerCount","OcrWarmupEnabled",
            "HeroPurchaseKey1","HeroPurchaseKey2","HeroPurchaseKey3","HeroPurchaseKey4","HeroPurchaseKey5",
            "RefreshStoreKey","TargetProcessName","TargetProcessId",
            "HeroNameScreenshotRectangle_1","HeroNameScreenshotRectangle_2","HeroNameScreenshotRectangle_3","HeroNameScreenshotRectangle_4","HeroNameScreenshotRectangle_5",
            "HighLightRectangle_1","HighLightRectangle_2","HighLightRectangle_3","HighLightRectangle_4","HighLightRectangle_5",
            "RefreshStoreButtonRectangle"
        });
    }

    private static void MapAutomaticSettings(JsonElement root, AppSettings settings, MigrationReport report)
    {
        if (settings.Coordinates.CardNameRects.Count == 0)
        {
            MapRectRange(root, "HeroNameScreenshotRectangle_", settings.Coordinates.CardNameRects, report, "Automatic.CardNameRects");
        }

        if (settings.Coordinates.CardClickRects.Count == 0)
        {
            MapRectRange(root, "HighLightRectangle_", settings.Coordinates.CardClickRects, report, "Automatic.CardClickRects");
        }

        if (settings.Coordinates.RefreshButtonRect.IsEmpty)
        {
            MapRect(root, "RefreshStoreButtonRectangle", rect => settings.Coordinates.RefreshButtonRect = rect, report, "Automatic.RefreshStoreButtonRectangle");
        }
    }

    private static void TryMapString(JsonElement root, string property, Action<string> apply, MigrationReport report, string mappingName)
    {
        if (root.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.String)
        {
            string? text = value.GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                apply(text);
                report.AppliedMappings.Add(mappingName);
            }
        }
    }

    private static void TryMapBool(JsonElement root, string property, Action<bool> apply, MigrationReport report, string mappingName)
    {
        if (root.TryGetProperty(property, out JsonElement value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            apply(value.GetBoolean());
            report.AppliedMappings.Add(mappingName);
        }
    }

    private static void TryMapInt(JsonElement root, string property, Action<int> apply, MigrationReport report, string mappingName)
    {
        if (root.TryGetProperty(property, out JsonElement value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
        {
            apply(intValue);
            report.AppliedMappings.Add(mappingName);
        }
    }

    private static void MapRectRange(JsonElement root, string prefix, List<ScreenRect> target, MigrationReport report, string mappingName)
    {
        if (target.Count > 0)
        {
            return;
        }

        List<ScreenRect> list = new();
        for (int i = 1; i <= 5; i++)
        {
            if (TryGetRect(root, $"{prefix}{i}", out ScreenRect rect))
            {
                list.Add(rect);
            }
        }

        if (list.Count > 0)
        {
            target.AddRange(list);
            report.AppliedMappings.Add(mappingName);
        }
    }

    private static void MapRect(JsonElement root, string property, Action<ScreenRect> apply, MigrationReport report, string mappingName)
    {
        if (TryGetRect(root, property, out ScreenRect rect))
        {
            apply(rect);
            report.AppliedMappings.Add(mappingName);
        }
    }

    private static bool TryGetRect(JsonElement root, string property, out ScreenRect rect)
    {
        rect = default;
        if (!root.TryGetProperty(property, out JsonElement node) || node.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!node.TryGetProperty("X", out JsonElement xNode) || !xNode.TryGetInt32(out int x))
        {
            return false;
        }

        if (!node.TryGetProperty("Y", out JsonElement yNode) || !yNode.TryGetInt32(out int y))
        {
            return false;
        }

        if (!node.TryGetProperty("Width", out JsonElement wNode) || !wNode.TryGetInt32(out int width))
        {
            return false;
        }

        if (!node.TryGetProperty("Height", out JsonElement hNode) || !hNode.TryGetInt32(out int height))
        {
            return false;
        }

        rect = new ScreenRect(x, y, width, height);
        return true;
    }

    private static void StoreLegacy(JsonElement root, AppSettings settings, MigrationReport report, HashSet<string> mappedKeys)
    {
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (mappedKeys.Contains(property.Name))
            {
                continue;
            }

            settings.Legacy[property.Name] = property.Value.Clone();
            report.LegacyPayload[property.Name] = property.Value.ToString();
        }
    }

    private static void MapPurchaseKeys(JsonElement root, AppSettings settings, MigrationReport report)
    {
        List<string> keys = new(5);
        string[] names = ["HeroPurchaseKey1", "HeroPurchaseKey2", "HeroPurchaseKey3", "HeroPurchaseKey4", "HeroPurchaseKey5"];
        foreach (string property in names)
        {
            if (!root.TryGetProperty(property, out JsonElement value) || value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            string? key = value.GetString();
            if (!string.IsNullOrWhiteSpace(key))
            {
                keys.Add(key);
            }
        }

        if (keys.Count > 0)
        {
            settings.PurchaseKeys = keys;
            report.AppliedMappings.Add("HeroPurchaseKey1..5 -> PurchaseKeys");
        }
    }
}
