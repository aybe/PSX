namespace PSX.Frontend.WPF.Emulation.Messaging;

// BUG this should pass depth as well so we don't have to carry some state
public record UpdateVideoDataMessage(IntSize Size, IntRect Rect, int[] Buffer24, ushort[] Buffer16);