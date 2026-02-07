using JinChanChanTool.Forms;
using JinChanChanTool.Tools;
using System.Diagnostics;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class LoopEventSink : ILoopEventSink
    {
        public void Info(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            LogTool.Log(message);
            Debug.WriteLine(message);
            try
            {
                OutputForm.Instance.WriteLineOutputMessage(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoopEventSink 输出消息写入失败: {ex.Message}");
            }
        }

        public void Error(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            LogTool.Log(message);
            Debug.WriteLine(message);
            try
            {
                OutputForm.Instance.WriteLineErrorMessage(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoopEventSink 错误消息写入失败: {ex.Message}");
            }
        }
    }
}
