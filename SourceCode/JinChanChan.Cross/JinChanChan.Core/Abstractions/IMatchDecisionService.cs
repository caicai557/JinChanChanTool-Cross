namespace JinChanChan.Core.Abstractions;

public interface IMatchDecisionService
{
    bool[] MatchTargets(string[] correctedResults, IReadOnlyList<string> targetHeroes, bool strictMatching);
}
