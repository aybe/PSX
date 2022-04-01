namespace PSX.Frontend.Core.Old.Messages;

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