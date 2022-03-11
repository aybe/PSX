namespace ProjectPSX.Core.Graphics;

public struct DrawMode
{
    public byte TextureXBase;
    public byte TextureYBase;
    public byte TransparencyMode;
    public byte TextureDepth;
    public bool IsDithered;
    public bool IsDrawingToDisplayAllowed;
    public bool IsTextureDisabled;
    public bool IsTexturedRectangleXFlipped;
    public bool IsTexturedRectangleYFlipped;
}