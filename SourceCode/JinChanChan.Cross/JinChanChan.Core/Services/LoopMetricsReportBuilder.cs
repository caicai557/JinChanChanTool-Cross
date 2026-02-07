using System.Text;
using JinChanChan.Core.Models;

namespace JinChanChan.Core.Services;

public static class LoopMetricsReportBuilder
{
    public static string Build(IReadOnlyList<LoopMetricsSnapshot> snapshots)
    {
        if (snapshots == null || snapshots.Count == 0)
        {
            return "暂无性能样本数据。";
        }

        StringBuilder builder = new();
        builder.AppendLine("# JinChanChan Cross 性能基线报告");
        builder.AppendLine();
        builder.AppendLine($"样本数: {snapshots.Count}");
        builder.AppendLine($"生成时间: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine();

        AppendMetrics(builder, snapshots, "CaptureMs", s => s.CaptureMs);
        AppendMetrics(builder, snapshots, "OcrMs", s => s.OcrMs);
        AppendMetrics(builder, snapshots, "MatchMs", s => s.MatchMs);
        AppendMetrics(builder, snapshots, "ActionMs", s => s.ActionMs);
        AppendMetrics(builder, snapshots, "LoopTotalMs", s => s.LoopTotalMs);

        return builder.ToString();
    }

    private static void AppendMetrics(StringBuilder builder, IReadOnlyList<LoopMetricsSnapshot> snapshots, string name, Func<LoopMetricsSnapshot, double> selector)
    {
        double[] values = snapshots.Select(selector).Where(v => v >= 0).OrderBy(v => v).ToArray();
        if (values.Length == 0)
        {
            return;
        }

        builder.AppendLine($"## {name}");
        builder.AppendLine($"- 平均: {values.Average():F2} ms");
        builder.AppendLine($"- P95: {Percentile(values, 95):F2} ms");
        builder.AppendLine($"- P99: {Percentile(values, 99):F2} ms");
        builder.AppendLine();
    }

    private static double Percentile(double[] sortedValues, int percentile)
    {
        if (sortedValues.Length == 0)
        {
            return 0;
        }

        double rank = percentile / 100d * (sortedValues.Length - 1);
        int low = (int)Math.Floor(rank);
        int high = (int)Math.Ceiling(rank);

        if (low == high)
        {
            return sortedValues[low];
        }

        double factor = rank - low;
        return sortedValues[low] * (1 - factor) + sortedValues[high] * factor;
    }
}
