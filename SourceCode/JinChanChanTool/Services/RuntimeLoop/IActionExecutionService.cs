namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface IActionExecutionService
    {
        Task PurchaseAsync(bool[] targetFlags, CancellationToken cancellationToken = default);

        Task RefreshStoreAsync(CancellationToken cancellationToken = default);

        Task ClickOneTimeAsync(CancellationToken cancellationToken = default);
    }
}
