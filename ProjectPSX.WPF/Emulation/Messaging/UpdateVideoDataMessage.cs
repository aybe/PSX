namespace ProjectPSX.WPF.Emulation.Messaging;

public record UpdateVideoDataMessage(IntSize Size, IntRect Rect, int[] Buffer24, ushort[] Buffer16);