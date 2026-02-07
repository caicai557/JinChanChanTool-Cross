using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class LineupRecommendationService : ILineupRecommendationService
{
    private readonly ILineupMatcherService _matcher;
    private readonly IReadOnlyList<LineupTemplate> _templates;

    public LineupRecommendationService(ILineupMatcherService matcher)
    {
        _matcher = matcher;
        _templates = BuildTemplates();
    }

    public LineupRecommendation Recommend(
        LiveGameState gameState,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LineupRecommendation> candidates = Search(gameState, query ?? string.Empty, 1_000, cancellationToken);
        return candidates.Count > 0
            ? candidates[0]
            : CreateEmptyRecommendation();
    }

    public IReadOnlyList<LineupRecommendation> Search(
        LiveGameState gameState,
        string query,
        int take = 5,
        CancellationToken cancellationToken = default)
    {
        string normalized = (query ?? string.Empty).Trim();
        List<LineupRecommendation> results = new();

        foreach (LineupTemplate template in _templates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!MatchesQuery(template, normalized))
            {
                continue;
            }

            LineupMatchScore score = _matcher.CalculateScore(gameState, template.Heroes, template.Traits, template.KeyUnits);
            HashSet<string> source = new(
                (gameState.ShopCards ?? Array.Empty<string>())
                    .Concat(gameState.BenchCards ?? Array.Empty<string>())
                    .Concat(gameState.PreferredTargets ?? Array.Empty<string>()),
                StringComparer.Ordinal);

            string[] matched = template.Heroes.Where(source.Contains).ToArray();
            string[] missing = template.Heroes.Where(x => !source.Contains(x)).ToArray();

            results.Add(new LineupRecommendation
            {
                LineupName = template.Name,
                Tier = template.Tier,
                MatchScore = score,
                MatchedHeroes = matched,
                MissingHeroes = missing,
                Traits = template.Traits,
                SearchTokens = template.SearchTokens,
                OnePageGuide = template.Guide,
                CarryHero = template.CarryHero,
                RecommendedItems = template.CarryItems,
                RecommendedAugments = template.Augments,
                CarouselPriorities = template.CarouselPriorities
            });
        }

        int limit = Math.Max(1, take);
        return results
            .OrderByDescending(x => x.MatchScore.TotalScore)
            .ThenBy(x => x.Tier, StringComparer.Ordinal)
            .Take(limit)
            .ToArray();
    }

    private static bool MatchesQuery(LineupTemplate template, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        if (template.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (template.Traits.Any(x => x.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (template.Heroes.Any(x => x.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (template.SearchTokens.Any(x => x.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return template.Initials.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<LineupTemplate> BuildTemplates()
    {
        return
        [
            new LineupTemplate(
                "重装德莱文",
                "S",
                ["德莱文","蕾欧娜","慎","锤石","阿狸","加里奥","洛","锐雯"],
                ["重装","裁决","法师"],
                ["德莱文","蕾欧娜","锤石"],
                ["2-1前留重装前排","3-2补齐德莱文体系","4-2追两星德莱文并保经济","8人口补控制与橙卡"],
                "德莱文",
                ["无尽","轻语","巨杀"],
                ["成吨利息","潘朵拉装备","治疗法球"],
                ["反曲弓","暴风大剑","拳套"]),
            new LineupTemplate(
                "法转九五",
                "S",
                ["阿狸","安妮","维克托","丽桑卓","婕拉","拉克丝","辛德拉","娜美"],
                ["法师","学者","护卫"],
                ["阿狸","维克托","辛德拉"],
                ["前期走连败拿装备","3-5到4-1提速找核心二星","5阶段补九五质量","橙卡优先补控制"],
                "阿狸",
                ["蓝霸符","珠光护手","科技枪"],
                ["法杖工坊","学习拼图","升级咯"],
                ["女神泪","大棒","大剑"]),
            new LineupTemplate(
                "裁决七源",
                "A",
                ["烬","卡莎","薇古丝","瑟庄妮","慎","剑魔","艾克","猪妹"],
                ["裁决","源计划","格斗"],
                ["烬","卡莎","艾克"],
                ["2阶段优先连胜保血","3阶段补强前排","4阶段拉8找双C","后期补五费控制"],
                "烬",
                ["无尽","最后的轻语","飓风"],
                ["潘朵拉备战席","锻炉","珠光莲花"],
                ["暴风大剑","反曲弓","拳套"]),
            new LineupTemplate(
                "术士召唤",
                "A",
                ["玛尔扎哈","蛇女","锤石","璐璐","娜美","莫甘娜","塞拉斯","辛德拉"],
                ["术士","法师","守护者"],
                ["玛尔扎哈","莫甘娜","塞拉斯"],
                ["前期靠术士过渡","中期补双前排","4阶段找玛尔扎哈三星窗口","后期根据质量上9"],
                "玛尔扎哈",
                ["蓝霸符","法爆","帽子"],
                ["治疗法球","法师之徽","开摆"],
                ["女神泪","大棒","腰带"])
        ];
    }

    private static LineupRecommendation CreateEmptyRecommendation()
    {
        return new LineupRecommendation
        {
            LineupName = "暂无推荐",
            Tier = "B",
            MatchScore = new LineupMatchScore(),
            OnePageGuide = ["当前没有可匹配阵容，请先设置目标英雄或完成OCR识别。"]
        };
    }

    private sealed record LineupTemplate(
        string Name,
        string Tier,
        IReadOnlyList<string> Heroes,
        IReadOnlyList<string> Traits,
        IReadOnlyList<string> KeyUnits,
        IReadOnlyList<string> Guide,
        string CarryHero,
        IReadOnlyList<string> CarryItems,
        IReadOnlyList<string> Augments,
        IReadOnlyList<string> CarouselPriorities)
    {
        public IReadOnlyList<string> SearchTokens { get; } =
            Heroes.Concat(Traits).Distinct(StringComparer.Ordinal).ToArray();

        public string Initials { get; } = BuildInitials(Heroes.Concat(Traits));

        private static string BuildInitials(IEnumerable<string> tokens)
        {
            char[] chars = tokens
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => char.ToLowerInvariant(x.Trim()[0]))
                .ToArray();
            return new string(chars);
        }
    }
}
