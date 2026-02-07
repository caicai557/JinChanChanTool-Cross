using JinChanChanTool.Services.DataServices.Interface;
using JinChanChanTool.Tools.KeyBoardTools;
using JinChanChanTool.Tools.MouseTools;
using System.Drawing;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class ActionExecutionService : IActionExecutionService
    {
        private readonly IManualSettingsService _manualSettings;
        private readonly IAutomaticSettingsService _automaticSettings;

        public ActionExecutionService(IManualSettingsService manualSettings, IAutomaticSettingsService automaticSettings)
        {
            _manualSettings = manualSettings ?? throw new ArgumentNullException(nameof(manualSettings));
            _automaticSettings = automaticSettings ?? throw new ArgumentNullException(nameof(automaticSettings));
        }

        public async Task PurchaseAsync(bool[] targetFlags, CancellationToken cancellationToken = default)
        {
            if (targetFlags == null)
            {
                return;
            }

            Rectangle[] fixedRects =
            [
                _manualSettings.CurrentConfig.HighLightRectangle_1,
                _manualSettings.CurrentConfig.HighLightRectangle_2,
                _manualSettings.CurrentConfig.HighLightRectangle_3,
                _manualSettings.CurrentConfig.HighLightRectangle_4,
                _manualSettings.CurrentConfig.HighLightRectangle_5
            ];

            Rectangle[] autoRects =
            [
                _automaticSettings.CurrentConfig.HighLightRectangle_1,
                _automaticSettings.CurrentConfig.HighLightRectangle_2,
                _automaticSettings.CurrentConfig.HighLightRectangle_3,
                _automaticSettings.CurrentConfig.HighLightRectangle_4,
                _automaticSettings.CurrentConfig.HighLightRectangle_5
            ];

            for (int i = 0; i < targetFlags.Length && i < 5; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!targetFlags[i])
                {
                    continue;
                }

                if (_manualSettings.CurrentConfig.IsKeyboardHeroPurchase)
                {
                    switch (i)
                    {
                        case 0:
                            KeyboardControlTool.PressKey(_manualSettings.CurrentConfig.HeroPurchaseKey1);
                            break;
                        case 1:
                            KeyboardControlTool.PressKey(_manualSettings.CurrentConfig.HeroPurchaseKey2);
                            break;
                        case 2:
                            KeyboardControlTool.PressKey(_manualSettings.CurrentConfig.HeroPurchaseKey3);
                            break;
                        case 3:
                            KeyboardControlTool.PressKey(_manualSettings.CurrentConfig.HeroPurchaseKey4);
                            break;
                        case 4:
                            KeyboardControlTool.PressKey(_manualSettings.CurrentConfig.HeroPurchaseKey5);
                            break;
                    }

                    await Task.Delay(_manualSettings.CurrentConfig.DelayAfterOperation, cancellationToken);
                }
                else if (_manualSettings.CurrentConfig.IsMouseHeroPurchase)
                {
                    Rectangle[] sourceRects = _manualSettings.CurrentConfig.IsUseDynamicCoordinates ? autoRects : fixedRects;
                    int randomX = Random.Shared.Next(sourceRects[i].Left + sourceRects[i].Width / 3, sourceRects[i].Left + sourceRects[i].Width * 2 / 3);
                    int randomY = Random.Shared.Next(sourceRects[i].Top + sourceRects[i].Height / 3, sourceRects[i].Top + sourceRects[i].Height * 2 / 3);

                    MouseControlTool.SetMousePosition(randomX, randomY);
                    await Task.Delay(_manualSettings.CurrentConfig.DelayAfterOperation, cancellationToken);
                    await ClickOneTimeAsync(cancellationToken);
                    await Task.Delay(_manualSettings.CurrentConfig.DelayAfterOperation, cancellationToken);
                }
            }
        }

        public async Task RefreshStoreAsync(CancellationToken cancellationToken = default)
        {
            if (_manualSettings.CurrentConfig.IsMouseRefreshStore)
            {
                Rectangle sourceRect = _manualSettings.CurrentConfig.IsUseDynamicCoordinates
                    ? _automaticSettings.CurrentConfig.RefreshStoreButtonRectangle
                    : _manualSettings.CurrentConfig.RefreshStoreButtonRectangle;

                int x = Random.Shared.Next(sourceRect.X + sourceRect.Width / 5, sourceRect.X + sourceRect.Width * 4 / 5);
                int y = Random.Shared.Next(sourceRect.Y + sourceRect.Height / 5, sourceRect.Y + sourceRect.Height * 4 / 5);

                MouseControlTool.SetMousePosition(x, y);
                await Task.Delay(_manualSettings.CurrentConfig.DelayAfterOperation, cancellationToken);
                await ClickOneTimeAsync(cancellationToken);
            }
            else if (_manualSettings.CurrentConfig.IsKeyboardRefreshStore)
            {
                KeyboardControlTool.PressKey(_manualSettings.CurrentConfig.RefreshStoreKey);
            }
        }

        public async Task ClickOneTimeAsync(CancellationToken cancellationToken = default)
        {
            MouseHookTool.IncrementProgramClickCount();
            MouseControlTool.MakeMouseLeftButtonDown();
            MouseControlTool.MakeMouseLeftButtonUp();
            await Task.Delay(1, cancellationToken);
            MouseHookTool.DecrementProgramClickCount();
        }
    }
}
