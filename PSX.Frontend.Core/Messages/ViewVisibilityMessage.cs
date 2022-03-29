namespace PSX.Frontend.Core.Messages;

/// <summary>
///     A message indicating the visibility of a view.
/// </summary>
public sealed class ViewVisibilityMessage
{
    public ViewVisibilityMessage(ViewVisibility visibility)
    {
        Visibility = visibility;
    }

    public ViewVisibility Visibility { get; }
}