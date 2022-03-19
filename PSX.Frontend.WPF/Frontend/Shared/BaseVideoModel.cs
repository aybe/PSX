﻿using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.WPF.Emulation.Messaging;

namespace PSX.Frontend.WPF.Frontend.Shared;

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
        var current = App.Current;
        if (current is null)
        {
            return; // BUG application will be null on close
        }

        current.Dispatcher.BeginInvoke(UpdateVideoDataImpl, DispatcherPriority.Background, message);
    }

    private unsafe void UpdateVideoDataImpl(UpdateVideoDataMessage message)
    {
        using var context = Bitmap.GetBitmapContext(ReadWriteMode.ReadWrite);

        if (Bitmap24)
        {
            if (IsFrameBuffer)
            {
                for (var y = 0; y < context.Height; y++)
                {
                    for (var x = 0; x < context.Width; x++)
                    {
                        var i = (0 + y) * 1024 + (0 + x) * 1;
                        var r = ((message.Buffer16[i] >> 00) & 0b11111) * 255 / 31;
                        var g = ((message.Buffer16[i] >> 05) & 0b11111) * 255 / 31;
                        var b = ((message.Buffer16[i] >> 10) & 0b11111) * 255 / 31;
                        var j = context.Pixels + (y * context.Width + x);
                        *j = (255 << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
            else
            {
                var span = MemoryMarshal.Cast<ushort, byte>(message.Buffer16);

                for (var y = 0; y < context.Height; y++)
                {
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
        }
        else
        {
            if (IsFrameBuffer)
            {
                for (var y = 0; y < context.Height; y++)
                {
                    for (var x = 0; x < context.Width; x++)
                    {
                        var i = (0 + y) * 1024 + (0 + x) * 1;
                        var r = ((message.Buffer16[i] >> 00) & 0b11111) * 255 / 31;
                        var g = ((message.Buffer16[i] >> 05) & 0b11111) * 255 / 31;
                        var b = ((message.Buffer16[i] >> 10) & 0b11111) * 255 / 31;
                        var j = context.Pixels + (y * context.Width + x);
                        *j = (255 << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
            else
            {
                for (var y = 0; y < context.Height; y++)
                {
                    for (var x = 0; x < context.Width; x++)
                    {
                        var x1 = message.Size.X + x;
                        var y1 = message.Size.Y + y;
                        var i = y1 * 1024 + x1 * 1;
                        var r = ((message.Buffer16[i] >> 00) & 0b11111) * 255 / 31;
                        var g = ((message.Buffer16[i] >> 05) & 0b11111) * 255 / 31;
                        var b = ((message.Buffer16[i] >> 10) & 0b11111) * 255 / 31;
                        var j = context.Pixels + (y * context.Width + x);
                        *j = (255 << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
        }
    }

    private void UpdateVideoSize(UpdateVideoSizeMessage message)
    {
        App.Current.Dispatcher.BeginInvoke(UpdateVideoSizeImpl, message);
    }

    private void UpdateVideoSizeImpl(UpdateVideoSizeMessage message)
    {
        Bitmap = IsFrameBuffer 
            ? BitmapFactory.New(1024, 512) 
            : BitmapFactory.New(message.Size.X, message.Size.Y);

        Bitmap24 = message.Is24Bit;
    }
}