using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._starcup.UserInterface.RichText;

public sealed class EmojiTag : IMarkupTagHandler
{
    private const string EmojiFont = "Emoji";

    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Name => "emoji";

    /// <inheritdoc/>
    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        var font = FontTag.CreateFont(context.Font, node, _resourceCache, _prototypeManager, EmojiFont);
        context.Font.Push(font);
    }

    /// <inheritdoc/>
    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        context.Font.Pop();
    }
}
