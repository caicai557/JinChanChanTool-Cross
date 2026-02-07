using JinChanChanTool.DataClass;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class MatchDecisionService : IMatchDecisionService
    {
        public bool[] MatchTargets(string[] correctedResults, IReadOnlyList<LineUp.LineUpUnit> units, bool strictMatching)
        {
            bool[] targetFlags = [false, false, false, false, false];
            if (correctedResults == null || correctedResults.Length == 0 || units == null || units.Count == 0)
            {
                return targetFlags;
            }

            List<string> selectedHeros = units
                .Where(u => !string.IsNullOrWhiteSpace(u.HeroName))
                .Select(u => u.HeroName)
                .Distinct()
                .ToList();

            for (int i = 0; i < correctedResults.Length && i < 5; i++)
            {
                string result = correctedResults[i] ?? string.Empty;
                if (string.IsNullOrEmpty(result))
                {
                    continue;
                }

                for (int j = 0; j < selectedHeros.Count; j++)
                {
                    string heroName = selectedHeros[j];
                    if (result == heroName)
                    {
                        targetFlags[i] = true;
                        break;
                    }

                    if (!strictMatching && result.Contains(heroName, StringComparison.Ordinal))
                    {
                        targetFlags[i] = true;
                        correctedResults[i] = heroName;
                        break;
                    }
                }
            }

            return targetFlags;
        }
    }
}
