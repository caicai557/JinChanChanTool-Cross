using JinChanChan.Core.Models;
using System.Text.Json;

namespace JinChanChan.Core.Utilities;

public static class LineupTemplateStore
{
    public static async Task<(IReadOnlyList<LineupTemplateDefinition> Templates, string? Error)> LoadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (Array.Empty<LineupTemplateDefinition>(), "阵容模板路径为空。");
        }

        if (!File.Exists(path))
        {
            return (Array.Empty<LineupTemplateDefinition>(), $"阵容模板文件不存在: {path}");
        }

        try
        {
            string json = await File.ReadAllTextAsync(path, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return (Array.Empty<LineupTemplateDefinition>(), "阵容模板文件为空。");
            }

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            List<LineupTemplateDefinition>? templates = JsonSerializer.Deserialize<List<LineupTemplateDefinition>>(json, options);
            if (templates == null || templates.Count == 0)
            {
                return (Array.Empty<LineupTemplateDefinition>(), "阵容模板解析后为空。");
            }

            return (templates, null);
        }
        catch (Exception ex)
        {
            return (Array.Empty<LineupTemplateDefinition>(), $"阵容模板加载失败: {ex.Message}");
        }
    }
}
