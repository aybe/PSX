using PSX.Frontend.WPF.Emulation;

namespace PSX.Frontend.WPF.Frontend.Messages;

public record UpdateVideoSizeMessage(IntSize Size, bool Is24Bit);