// ReSharper disable CommentTypo

namespace PSX.Core.Graphics.Internal;

/// <summary>
///     GP0(E1h) - Draw Mode setting (aka "Texpage")
/// </summary>
public struct DrawMode
{
    public byte TexturePageXBase { get; set; }

    public byte TexturePageYBase { get; set; }

    public byte SemiTransparency { get; set; }

    public byte TexturePageColors { get; set; }

    public bool Dither24BitTo15Bit { get; set; }

    public bool DrawingToDisplayArea { get; set; }

    public bool TextureDisable { get; set; }

    public bool TexturedRectangleXFlip { get; set; }

    public bool TexturedRectangleYFlip { get; set; }
}