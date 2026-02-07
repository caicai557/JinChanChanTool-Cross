using JinChanChan.Core.Utilities;

namespace JinChanChan.Core.Tests;

public class LineupTemplateStoreTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnTemplates_WhenJsonIsValid()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"jcct_lineup_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            string path = Path.Combine(tempDir, "lineups.json");
            string json = """
            [
              {
                "name": "测试阵容",
                "tier": "S",
                "heroes": ["英雄A","英雄B"],
                "traits": ["羁绊A"],
                "keyUnits": ["英雄A"],
                "guide": ["步骤1"],
                "carryHero": "英雄A",
                "carryItems": ["道具A"],
                "augments": ["符文A"],
                "carouselPriorities": ["道具A"],
                "searchTokens": ["cszr"]
              }
            ]
            """;

            await File.WriteAllTextAsync(path, json);
            var (templates, error) = await LineupTemplateStore.LoadAsync(path);

            Assert.Null(error);
            Assert.Single(templates);
            Assert.Equal("测试阵容", templates[0].Name);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
