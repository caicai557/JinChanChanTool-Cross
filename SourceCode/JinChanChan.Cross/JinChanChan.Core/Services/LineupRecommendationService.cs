using JinChanChan.Core.Abstractions;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public sealed class LineupRecommendationService : ILineupRecommendationService
{
    private readonly ILineupMatcherService _matcher;
    private readonly IReadOnlyList<LineupTemplateDefinition> _templates;

    public LineupRecommendationService(ILineupMatcherService matcher)
        : this(matcher, null)
    {
    }

    public LineupRecommendationService(
        ILineupMatcherService matcher,
        IReadOnlyList<LineupTemplateDefinition>? templates)
    {
        _matcher = matcher;
        _templates = NormalizeTemplates(templates);
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

        foreach (LineupTemplateDefinition template in _templates)
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
                Tier = NormalizeTier(template.Tier),
                MatchScore = score,
                MatchedHeroes = matched,
                MissingHeroes = missing,
                Traits = template.Traits,
                SearchTokens = BuildQueryTokens(template),
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
            .ThenBy(x => TierSortRank(x.Tier))
            .ThenBy(x => x.LineupName, StringComparer.Ordinal)
            .Take(limit)
            .ToArray();
    }

    private static bool MatchesQuery(LineupTemplateDefinition template, string query)
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

        IReadOnlyList<string> queryTokens = BuildQueryTokens(template);
        if (queryTokens.Any(x => x.Contains(query, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        string initials = BuildInitials(queryTokens);
        return initials.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<LineupTemplateDefinition> NormalizeTemplates(
        IReadOnlyList<LineupTemplateDefinition>? templates)
    {
        if (templates == null || templates.Count == 0)
        {
            return BuildBuiltInTemplates();
        }

        List<LineupTemplateDefinition> normalized = new();
        foreach (LineupTemplateDefinition template in templates)
        {
            if (string.IsNullOrWhiteSpace(template.Name))
            {
                continue;
            }

            string[] heroes = template.Heroes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            string[] traits = template.Traits
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            string[] keyUnits = template.KeyUnits
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            string[] guide = template.Guide
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToArray();

            string[] carryItems = template.CarryItems
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            string[] augments = template.Augments
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            string[] carousel = template.CarouselPriorities
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            string[] searchTokens = template.SearchTokens
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            normalized.Add(new LineupTemplateDefinition
            {
                Name = template.Name.Trim(),
                Tier = NormalizeTier(template.Tier),
                Heroes = heroes,
                Traits = traits,
                KeyUnits = keyUnits,
                Guide = guide,
                CarryHero = template.CarryHero?.Trim() ?? string.Empty,
                CarryItems = carryItems,
                Augments = augments,
                CarouselPriorities = carousel,
                SearchTokens = searchTokens
            });
        }

        return normalized.Count == 0 ? BuildBuiltInTemplates() : normalized;
    }

    private static IReadOnlyList<LineupTemplateDefinition> BuildBuiltInTemplates()
    {
        return
        [
            new()
            {
                Name = "重装德莱文",
                Tier = "S",
                Heroes = ["德莱文","蕾欧娜","慎","锤石","阿狸","加里奥","洛","锐雯"],
                Traits = ["重装","裁决","法师"],
                KeyUnits = ["德莱文","蕾欧娜","锤石"],
                Guide = ["2-1前留重装前排","3-2补齐德莱文体系","4-2追两星德莱文并保经济","8人口补控制与橙卡"],
                CarryHero = "德莱文",
                CarryItems = ["无尽","轻语","巨杀"],
                Augments = ["成吨利息","潘朵拉装备","治疗法球"],
                CarouselPriorities = ["反曲弓","暴风大剑","拳套"],
                SearchTokens = ["德莱文","重装","裁决","dlyw","zzdlw"]
            },
            new()
            {
                Name = "法转九五",
                Tier = "S",
                Heroes = ["阿狸","安妮","维克托","丽桑卓","婕拉","拉克丝","辛德拉","娜美"],
                Traits = ["法师","学者","护卫"],
                KeyUnits = ["阿狸","维克托","辛德拉"],
                Guide = ["前期走连败拿装备","3-5到4-1提速找核心二星","5阶段补九五质量","橙卡优先补控制"],
                CarryHero = "阿狸",
                CarryItems = ["蓝霸符","珠光护手","科技枪"],
                Augments = ["法杖工坊","学习拼图","升级咯"],
                CarouselPriorities = ["女神泪","大棒","大剑"],
                SearchTokens = ["法转","法师","九五","fzjw"]
            },
            new()
            {
                Name = "裁决七源",
                Tier = "A",
                Heroes = ["烬","卡莎","薇古丝","瑟庄妮","慎","剑魔","艾克","猪妹"],
                Traits = ["裁决","源计划","格斗"],
                KeyUnits = ["烬","卡莎","艾克"],
                Guide = ["2阶段优先连胜保血","3阶段补强前排","4阶段拉8找双C","后期补五费控制"],
                CarryHero = "烬",
                CarryItems = ["无尽","最后的轻语","飓风"],
                Augments = ["潘朵拉备战席","锻炉","珠光莲花"],
                CarouselPriorities = ["暴风大剑","反曲弓","拳套"],
                SearchTokens = ["裁决","源计划","七源","cjqy"]
            },
            new()
            {
                Name = "术士召唤",
                Tier = "A",
                Heroes = ["玛尔扎哈","蛇女","锤石","璐璐","娜美","莫甘娜","塞拉斯","辛德拉"],
                Traits = ["术士","法师","守护者"],
                KeyUnits = ["玛尔扎哈","莫甘娜","塞拉斯"],
                Guide = ["前期靠术士过渡","中期补双前排","4阶段找玛尔扎哈三星窗口","后期根据质量上9"],
                CarryHero = "玛尔扎哈",
                CarryItems = ["蓝霸符","法爆","帽子"],
                Augments = ["治疗法球","法师之徽","开摆"],
                CarouselPriorities = ["女神泪","大棒","腰带"],
                SearchTokens = ["术士","召唤","法师","sszh"]
            }
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

    private static IReadOnlyList<string> BuildQueryTokens(LineupTemplateDefinition template)
    {
        if (template.SearchTokens.Count > 0)
        {
            return template.SearchTokens;
        }

        return template.Heroes
            .Concat(template.Traits)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string BuildInitials(IEnumerable<string> tokens)
    {
        char[] chars = tokens
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => char.ToLowerInvariant(x.Trim()[0]))
            .ToArray();
        return new string(chars);
    }

    private static string NormalizeTier(string? tier)
    {
        if (string.IsNullOrWhiteSpace(tier))
        {
            return "B";
        }

        string upper = tier.Trim().ToUpperInvariant();
        return upper is "S" or "A" or "B" or "C" ? upper : "B";
    }

    private static int TierSortRank(string tier)
    {
        return tier switch
        {
            "S" => 0,
            "A" => 1,
            "B" => 2,
            "C" => 3,
            _ => 4
        };
    }
}
