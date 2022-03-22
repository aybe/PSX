using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace PSX.Frontend.WPF.Frontend.Shared;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal sealed class IApplicationWpf : IApplication
{
    void IApplication.Shutdown()
    {
        Application.Current.Shutdown();
    }
}