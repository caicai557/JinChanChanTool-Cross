using JinChanChanTool.DataClass;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface IMatchDecisionService
    {
        bool[] MatchTargets(string[] correctedResults, IReadOnlyList<LineUp.LineUpUnit> units, bool strictMatching);
    }
}
