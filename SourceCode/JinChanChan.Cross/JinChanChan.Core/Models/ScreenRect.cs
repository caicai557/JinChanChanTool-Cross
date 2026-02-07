namespace JinChanChan.Core.Models;

public readonly record struct ScreenRect(int X, int Y, int Width, int Height)
{
    public bool IsEmpty => Width <= 0 || Height <= 0;

    public (int CenterX, int CenterY) Center() => (X + Width / 2, Y + Height / 2);
}
