using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Toolkit.Mvvm.Messaging;
using ProjectPSX.WPF.Emulation;
using ProjectPSX.WPF.Emulation.Messaging;

namespace ProjectPSX.WPF.Frontend.Shared;

internal class BaseVideoModel : BaseModel<BaseVideoModelCommands>
    // BUG these values should be cached to be fed when window opens after emu started
    // BUG implement IsFrameBuffer
{
    private WriteableBitmap? _bitmap;

    private bool _isFrameBuffer;

    public WriteableBitmap? Bitmap
    {
        get => _bitmap;
        private set => SetProperty(ref _bitmap, value);
    }

    public bool IsFrameBuffer
    {
        get => _isFrameBuffer;
        set => SetProperty(ref _isFrameBuffer, value);
    }

    private bool Bitmap24 { get; set; }

    protected override void OnActivated()
    {
        base.OnActivated();

        Messenger.Send(new BaseUpdateMessage<UpdateVideoDataMessageHandler>(UpdateVideoData, BaseUpdateMessageType.Add));

        Messenger.Send(new BaseUpdateMessage<UpdateVideoSizeMessageHandler>(UpdateVideoSize, BaseUpdateMessageType.Add));
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        Messenger.Send(new BaseUpdateMessage<UpdateVideoDataMessageHandler>(UpdateVideoData, BaseUpdateMessageType.Remove));

        Messenger.Send(new BaseUpdateMessage<UpdateVideoSizeMessageHandler>(UpdateVideoSize, BaseUpdateMessageType.Remove));
    }

    private void UpdateVideoData(UpdateVideoDataMessage message)
    {
        App.Current.Dispatcher.BeginInvoke(UpdateVideoDataImpl, DispatcherPriority.Render, message);
    }

    private unsafe void UpdateVideoDataImpl(UpdateVideoDataMessage message)
    {
        using var context = Bitmap.GetBitmapContext(ReadWriteMode.ReadWrite);

        if (Bitmap24)
        {
            if (IsFrameBuffer)
            {
                context.Clear();
            }
            else
            {
                var span = MemoryMarshal.Cast<ushort, byte>(message.Buffer16);

                for (var y = 0; y < context.Height; y++)
                for (var x = 0; x < context.Width; x++)
                {
                    var i = y * 2048 + x * 3;
                    var r = span[i + 0];
                    var g = span[i + 1];
                    var b = span[i + 2];
                    var j = context.Pixels + (y * context.Width + x);
                    *j = (255 << 24) | (r << 16) | (g << 8) | b;
                }
            }
        }
        else
        {
            if (IsFrameBuffer)
                context.Clear();
            else
                for (var y = 0; y < context.Height; y++)
                for (var x = 0; x < context.Width; x++)
                {
                    var i = (message.Size.Y + y) * 1024 + (message.Size.X + x) * 1;
                    var r = ((message.Buffer16[i] >> 00) & 0b11111) * 255 / 31;
                    var g = ((message.Buffer16[i] >> 05) & 0b11111) * 255 / 31;
                    var b = ((message.Buffer16[i] >> 10) & 0b11111) * 255 / 31;
                    var j = context.Pixels + (y * context.Width + x);
                    *j = (255 << 24) | (r << 16) | (g << 8) | b;
                }
        }
    }

    private void UpdateVideoSize(UpdateVideoSizeMessage message)
    {
        App.Current.Dispatcher.BeginInvoke(UpdateVideoSizeImpl, message);
    }

    private void UpdateVideoSizeImpl(UpdateVideoSizeMessage message)
    {
        Bitmap24 = message.Is24Bit;

        if (IsFrameBuffer)
            Bitmap = BitmapFactory.New(1024, 512);
        else
            Bitmap = BitmapFactory.New(message.Size.X, message.Size.Y);
    }
}