using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Mvvm.Messaging;
using PSX.Frontend.Services.Emulation;

namespace PSX.Frontend.WPF.Controls;

public partial class VideoControl
{
    public VideoControl()
    {
        InitializeComponent();

        IsVisibleChanged += OnIsVisibleChanged;
    }

    private WriteableBitmap? Bitmap { get; set; }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // NOTE: the messages can't be struct as messenger only support class messages

        // NOTE: the strong reference messenger makes animation choppy for some reason

        switch (e.NewValue)
        {
            case true:
                WeakReferenceMessenger.Default.Register<UpdateVideoSize>(this, OnCreateBitmap);
                WeakReferenceMessenger.Default.Register<UpdateVideoData>(this, OnUpdateBitmap);
                break;
            case false:
                WeakReferenceMessenger.Default.Unregister<UpdateVideoSize>(this);
                WeakReferenceMessenger.Default.Unregister<UpdateVideoData>(this);
                break;
        }
    }

    #region VideoControlType

    public static readonly DependencyProperty VideoControlTypeProperty =
        DependencyProperty.Register(
            nameof(VideoControlType),
            typeof(VideoControlType),
            typeof(VideoControl),
            new PropertyMetadata(default(VideoControlType))
        );

    public VideoControlType VideoControlType
    {
        get => (VideoControlType)GetValue(VideoControlTypeProperty);
        set => SetValue(VideoControlTypeProperty, value);
    }

    #endregion

    #region Create Bitmap

    private void OnCreateBitmap(object recipient, UpdateVideoSize size)
    {
        Dispatcher.BeginInvoke(OnCreateBitmapImpl, size);
    }

    private void OnCreateBitmapImpl(UpdateVideoSize size)
    {
        if (size is null)
            throw new ArgumentNullException(nameof(size));

        var pixelWidth = VideoControlType switch
        {
            VideoControlType.Screen => size.Width,
            VideoControlType.Memory => 1024,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        var pixelHeight = VideoControlType switch
        {
            VideoControlType.Screen => size.Height,
            VideoControlType.Memory => 512,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        var pixelFormat = VideoControlType switch
        {
            VideoControlType.Screen => size.Format switch
            {
                UpdateVideoSizeFormat.Direct15 => PixelFormats.Bgr555,
                UpdateVideoSizeFormat.Direct24 => PixelFormats.Bgr24,
                _                              => throw new NotSupportedException(size.Format.ToString())
            },
            VideoControlType.Memory => PixelFormats.Bgr555,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        if (Bitmap is not null && Bitmap.PixelWidth == pixelWidth && Bitmap.PixelHeight == pixelHeight && Bitmap.Format == pixelFormat)
        {
            return; // event is constantly sent
        }

        var bitmap = new WriteableBitmap(pixelWidth, pixelHeight, 96.0d, 96.0d, pixelFormat, null);

        Image1.Source = Bitmap = bitmap;
    }

    #endregion

    #region Update Bitmap

    private void OnUpdateBitmap(object recipient, UpdateVideoData data)
    {
        Dispatcher.BeginInvoke(OnUpdateBitmapImpl, data);
    }

    private void OnUpdateBitmapImpl(UpdateVideoData data)
    {
        if (Bitmap is null)
        {
            return; // at start, DisplayAreaStart, DisplayHorizontalRange and DisplayVerticalRange are sent before DisplayMode
        }

        var format = Bitmap.Format;

        switch (true)
        {
            case true when format == PixelFormats.Bgr555:
                OnUpdateBitmap15(data);
                break;
            case true when format == PixelFormats.Bgr24:
                OnUpdateBitmap24(data);
                break;
            default: throw new NotSupportedException(format.ToString());
        }
    }

    private unsafe void OnUpdateBitmap15(UpdateVideoData data)
    {
        if (Bitmap is null)
            throw new NullReferenceException(nameof(Bitmap));

        var pixelWidth  = GetPixelWidth();
        var pixelHeight = GetPixelHeight();

        var src = data.Buffer16 as ushort[] ?? throw new InvalidOperationException();
        var dst = (ushort*)Bitmap.BackBuffer;

        Bitmap.Lock();

        var startX = VideoControlType switch
        {
            VideoControlType.Screen => data.StartX,
            VideoControlType.Memory => 0,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        var startY = VideoControlType switch
        {
            VideoControlType.Screen => data.StartY,
            VideoControlType.Memory => 0,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        for (var y = 0; y < pixelHeight; y++)
        {
            for (var x = 0; x < pixelWidth; x++)
            {
                var i = (y + startY) * 1024 + x + startX;
                var p = src[i];
                var r = (p >> 00) & 0b11111;
                var g = (p >> 05) & 0b11111;
                var b = (p >> 10) & 0b11111;
                *dst++ = (ushort)((r << 10) | (g << 5) | (b << 0));
            }
        }

        Bitmap.AddDirtyRect(new Int32Rect(0, 0, pixelWidth, pixelHeight));

        Bitmap.Unlock();
    }

    private unsafe void OnUpdateBitmap24(UpdateVideoData data)
    {
        if (Bitmap is null)
            throw new NullReferenceException(nameof(Bitmap));

        var pixelWidth  = GetPixelWidth();
        var pixelHeight = GetPixelHeight();

        Bitmap.Lock();

        var src = MemoryMarshal.Cast<ushort, byte>(data.Buffer16 as ushort[] ?? throw new InvalidOperationException());
        var dst = (byte*)Bitmap.BackBuffer;

        for (var y = 0; y < pixelHeight; y++)
        {
            for (var x = 0; x < pixelWidth; x++)
            {
                var i = (y + data.StartY) * 2048 + x * 3 + data.StartX * 2;
                var r = src[i + 0];
                var g = src[i + 1];
                var b = src[i + 2];

                *dst++ = b;
                *dst++ = g;
                *dst++ = r;
            }
        }

        Bitmap.AddDirtyRect(new Int32Rect(0, 0, pixelWidth, pixelHeight));

        Bitmap.Unlock();
    }

    #endregion

    #region Getters

    private int GetPixelWidth()
    {
        if (Bitmap is null)
            throw new NullReferenceException(nameof(Bitmap));

        var pixelWidth = VideoControlType switch
        {
            VideoControlType.Screen => Bitmap.PixelWidth,
            VideoControlType.Memory => 1024,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        return pixelWidth;
    }

    private int GetPixelHeight()
    {
        if (Bitmap is null)
            throw new NullReferenceException(nameof(Bitmap));

        var pixelHeight = VideoControlType switch
        {
            VideoControlType.Screen => Bitmap.PixelHeight,
            VideoControlType.Memory => 512,
            _                       => throw new NotSupportedException(VideoControlType.ToString())
        };

        return pixelHeight;
    }

    #endregion
}