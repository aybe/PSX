using System.Diagnostics.CodeAnalysis;
using System.Windows;
using PSX.Frontend.Core.Services;

namespace PSX.Frontend.WPF.Frontend.Shared;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class IApplicationWpf : IApplication // TODO rename
{
    void IApplication.Shutdown()
    {
        Application.Current.Shutdown();
    }
}