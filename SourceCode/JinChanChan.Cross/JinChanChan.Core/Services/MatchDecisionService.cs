using JinChanChan.Core.Abstractions;

namespace JinChanChan.Core.Services;

public sealed class MatchDecisionService : IMatchDecisionService
{
    public bool[] MatchTargets(string[] correctedResults, IReadOnlyList<string> targetHeroes, bool strictMatching)
    {
        bool[] targets = [false, false, false, false, false];
        if (correctedResults == null || correctedResults.Length == 0 || targetHeroes == null || targetHeroes.Count == 0)
        {
            return targets;
        }

        List<string> selected = targetHeroes
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        for (int i = 0; i < correctedResults.Length && i < 5; i++)
        {
            string current = correctedResults[i] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(current))
            {
                continue;
            }

            foreach (string hero in selected)
            {
                if (current.Equals(hero, StringComparison.Ordinal))
                {
                    targets[i] = true;
                    break;
                }

                if (!strictMatching && current.Contains(hero, StringComparison.Ordinal))
                {
                    correctedResults[i] = hero;
                    targets[i] = true;
                    break;
                }
            }
        }

        return targets;
    }
}
