using Microsoft.ML.OnnxRuntime;
using JinChanChan.Ocr.Models;
using System.Diagnostics;

namespace JinChanChan.Ocr.Utilities;

internal static class OnnxProviderSelector
{
    public static string ConfigureProvider(SessionOptions options, OnnxExecutionProvider preferredProvider, bool allowFallback)
    {
        if (preferredProvider == OnnxExecutionProvider.DirectMl && OperatingSystem.IsWindows())
        {
            bool ok = TryAppendProvider(options, "AppendExecutionProvider_DML");
            if (ok)
            {
                return "DirectML";
            }

            if (!allowFallback)
            {
                throw new InvalidOperationException("DirectML provider 不可用且不允许回退。");
            }
        }

        return "CPU";
    }

    private static bool TryAppendProvider(SessionOptions options, string methodName)
    {
        var candidates = typeof(SessionOptions).GetMethods()
            .Where(m => m.Name == methodName)
            .ToArray();

        if (candidates.Length == 0)
        {
            return false;
        }

        foreach (var method in candidates)
        {
            var parameters = method.GetParameters();
            object?[] args;
            if (parameters.Length == 0)
            {
                args = [];
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
            {
                args = [0];
            }
            else
            {
                continue;
            }

            try
            {
                method.Invoke(options, args);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Append provider {methodName} 失败: {ex.Message}");
            }
        }

        return false;
    }
}
