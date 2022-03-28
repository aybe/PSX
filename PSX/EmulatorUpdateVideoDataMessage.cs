namespace PSX;

// BUG this should pass depth as well so we don't have to carry some state
public record EmulatorUpdateVideoDataMessage(IntSize Size, IntRect Rect, int[] Buffer24, ushort[] Buffer16);