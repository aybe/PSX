namespace PSX.Core.Graphics.Internal;

internal struct Primitive
{
    public bool IsShaded;

    public bool IsTextured;

    public bool IsSemiTransparent;

    /// <remarks>
    ///     If not blended.
    /// </remarks>
    public bool IsRawTextured;

    public int Depth;

    public int SemiTransparencyMode;

    public Point2D Clut;

    public Point2D TextureBase;
}