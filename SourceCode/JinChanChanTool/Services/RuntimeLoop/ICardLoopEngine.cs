namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface ICardLoopEngine
    {
        bool IsHighLight { get; }

        bool IsGetCard { get; }

        bool IsRefreshStore { get; }

        event Action<bool> isHighLightStatusChanged;

        event Action<bool> isGetCardStatusChanged;

        event Action<bool> isRefreshStoreStatusChanged;

        void StartHighLight();

        void StopHighLight();

        void ToggleHighLight();

        void StartLoop();

        void StopLoop();

        void ToggleLoop();

        void ToggleRefreshStore();

        void AutoRefreshOn();

        void AutoRefreshOff();

        void MouseLeftButtonDown();

        void MouseLeftButtonUp();
    }
}
