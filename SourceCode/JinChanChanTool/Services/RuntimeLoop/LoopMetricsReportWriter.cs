using System.Text;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public static class LoopMetricsReportWriter
    {
        public static string BuildReport(IReadOnlyList<LoopMetricsSnapshot> snapshots)
        {
            if (snapshots == null || snapshots.Count == 0)
            {
                return "暂无性能样本数据。";
            }

            StringBuilder sb = new();
            sb.AppendLine("# RuntimeLoop 性能基线报告");
            sb.AppendLine();
            sb.AppendLine($"样本数: {snapshots.Count}");
            sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            AppendMetric(sb, snapshots, "CaptureMs", s => s.CaptureMs);
            AppendMetric(sb, snapshots, "OcrMs", s => s.OcrMs);
            AppendMetric(sb, snapshots, "CorrectionMs", s => s.CorrectionMs);
            AppendMetric(sb, snapshots, "MatchMs", s => s.MatchMs);
            AppendMetric(sb, snapshots, "ActionMs", s => s.ActionMs);
            AppendMetric(sb, snapshots, "LoopTotalMs", s => s.LoopTotalMs);

            return sb.ToString();
        }

        private static void AppendMetric(StringBuilder sb, IReadOnlyList<LoopMetricsSnapshot> snapshots, string name, Func<LoopMetricsSnapshot, double> selector)
        {
            double[] values = snapshots.Select(selector).Where(v => v >= 0).OrderBy(v => v).ToArray();
            if (values.Length == 0)
            {
                return;
            }

            sb.AppendLine($"## {name}");
            sb.AppendLine($"- 平均: {values.Average():F2} ms");
            sb.AppendLine($"- P95: {Percentile(values, 95):F2} ms");
            sb.AppendLine($"- P99: {Percentile(values, 99):F2} ms");
            sb.AppendLine();
        }

        private static double Percentile(double[] sortedValues, int percentile)
        {
            if (sortedValues.Length == 0)
            {
                return 0;
            }

            double rank = (percentile / 100.0) * (sortedValues.Length - 1);
            int low = (int)Math.Floor(rank);
            int high = (int)Math.Ceiling(rank);
            if (low == high)
            {
                return sortedValues[low];
            }

            double weight = rank - low;
            return sortedValues[low] * (1 - weight) + sortedValues[high] * weight;
        }
    }
}
