using System.Drawing;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public interface IFrameCaptureService
    {
        LoopFrame Capture(Rectangle[] nameRectangles, Rectangle[] cardRectangles);
    }
}
