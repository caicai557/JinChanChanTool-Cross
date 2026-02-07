using JinChanChan.Core.Models;

namespace JinChanChan.Core.Config;

public sealed class CoordinateSettings
{
    public bool UseDynamicCoordinates { get; set; }

    public List<ScreenRect> CardNameRects { get; set; } = new();

    public List<ScreenRect> CardClickRects { get; set; } = new();

    public ScreenRect RefreshButtonRect { get; set; }
}
