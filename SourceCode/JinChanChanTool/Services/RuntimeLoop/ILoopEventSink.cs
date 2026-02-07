namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface ILoopEventSink
    {
        void Info(string message);

        void Error(string message);
    }
}
