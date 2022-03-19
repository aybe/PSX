namespace ProjectPSX.Graphics;

public interface IVRAM<T> where T : unmanaged
{
    int Width { get; }

    int Height { get; }

    T[] Pixels { get; }

    T GetPixel(int x, int y);

    void SetPixel(int x, int y, T color);
}