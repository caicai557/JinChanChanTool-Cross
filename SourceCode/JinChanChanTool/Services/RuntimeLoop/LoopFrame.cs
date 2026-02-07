using System.Drawing;

namespace JinChanChanTool.Services.RuntimeLoop
{
    public sealed class LoopFrame
    {
        public required Bitmap[] Bitmaps { get; init; }

        public required Rectangle[] CardRectangles { get; init; }
    }
}
