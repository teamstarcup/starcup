using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._starcup.UserInterface.RichText;

public sealed class SizeTag : IMarkupTagHandler
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Name => "size";

    /// <inheritdoc/>
    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        if (!node.Value.TryGetLong(out var levelParam))
            return;

        var level = Math.Min(Math.Max((int)levelParam, 1), 30);
        node.Attributes["size"] = new MarkupParameter(
            (int)Math.Ceiling(FontTag.DefaultSize * 2 / Math.Sqrt(level))
        );

        var font = FontTag.CreateFont(context.Font, node, _resourceCache, _prototypeManager, FontTag.DefaultFont);
        context.Font.Push(font);
    }

    /// <inheritdoc/>
    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        context.Font.Pop();
    }
}
