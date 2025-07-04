using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Utility;

namespace Content.Client.Light;

// starcup: #38276 early merge
public sealed class AmbientOcclusionOverlay : Overlay
{
    private static readonly ProtoId<ShaderPrototype> UnshadedShader = "unshaded";
    private static readonly ProtoId<ShaderPrototype> StencilMaskShader = "StencilMask";
    private static readonly ProtoId<ShaderPrototype> StencilEqualDrawShader = "StencilEqualDraw";

    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowEntities;

    private IRenderTexture? _aoTarget;
    private IRenderTexture? _aoBlurBuffer;

    public AmbientOcclusionOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = AfterLightTargetOverlay.ContentZIndex + 1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var viewport = args.Viewport;
        var mapId = args.MapId;
        var worldBounds = args.WorldBounds;
        var worldHandle = args.WorldHandle;
        var color = Color.FromHex("#04080FAA");
        //var color = Color.Red;
        var target = viewport.RenderTarget;
        var lightScale = target.Size / (Vector2) viewport.Size;
        var scale = viewport.RenderScale / (Vector2.One / lightScale);
        var maps = _entManager.System<SharedMapSystem>();
        var lookups = _entManager.System<EntityLookupSystem>();
        var query = _entManager.System<OccluderSystem>();
        var xformSystem = _entManager.System<SharedTransformSystem>();
        var turfSystem = _entManager.System<TurfSystem>();
        var invMatrix = args.Viewport.GetWorldToLocalMatrix();

        if (_aoTarget?.Texture.Size != target.Size)
        {
            _aoTarget?.Dispose();
            _aoTarget = _clyde.CreateRenderTarget(target.Size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "ambient-occlusion-target");
        }

        if (_aoBlurBuffer?.Texture.Size != target.Size)
        {
            _aoBlurBuffer?.Dispose();
            _aoBlurBuffer = _clyde.CreateRenderTarget(target.Size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "ambient-occlusion-blur-target");
        }

        if (_aoStencilTarget?.Texture.Size != target.Size)
        {
            _aoStencilTarget?.Dispose();
            _aoStencilTarget = _clyde.CreateRenderTarget(target.Size, new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb), name: "ambient-occlusion-stencil-target");
        }

        // Draw the texture data to the texture.
        args.WorldHandle.RenderInRenderTarget(_aoTarget,
            () =>
            {
                worldHandle.UseShader(_proto.Index(UnshadedShader).Instance());
                var invMatrix = _aoTarget.GetWorldToLocalMatrix(viewport.Eye!, scale);

                var query = _entManager.System<OccluderSystem>();
                var xformSystem = _entManager.System<SharedTransformSystem>();

                foreach (var entry in query.QueryAabb(mapId, worldBounds))
                {
                    DebugTools.Assert(entry.Component.Enabled);
                    var matrix = xformSystem.GetWorldMatrix(entry.Transform);
                    var localMatrix = Matrix3x2.Multiply(matrix, invMatrix);

                    worldHandle.SetTransform(localMatrix);
                    // 4 pixels
                    worldHandle.DrawRect(Box2.UnitCentered.Enlarged(4f / EyeManager.PixelsPerMeter), Color.White);
                }
            }, Color.Transparent);

        _clyde.BlurRenderTarget(viewport, _aoTarget, _aoBlurBuffer, viewport.Eye!, 14f);

        args.WorldHandle.RenderInRenderTarget(target,
            () =>
            {
                // Don't want lighting affecting it.
                worldHandle.UseShader(_proto.Index(UnshadedShader).Instance());

                foreach (var grid in _mapManager.FindGridsIntersecting(mapId, worldBounds))
                {
                    var transform = xformSystem.GetWorldMatrix(grid.Owner);
                    var worldToTextureMatrix = Matrix3x2.Multiply(transform, invMatrix);
                    var tiles = maps.GetTilesEnumerator(grid.Owner, grid, worldBounds);
                    worldHandle.SetTransform(worldToTextureMatrix);
                    while (tiles.MoveNext(out var tileRef))
                    {
                        if (turfSystem.IsSpace(tileRef))
                            continue;

                        var bounds = lookups.GetLocalBounds(tileRef, grid.TileSize);
                        worldHandle.DrawRect(bounds, Color.White);
                    }
                }

            }, Color.Transparent);

        // Draw the stencil texture to depth buffer.
        worldHandle.UseShader(_proto.Index(StencilMaskShader).Instance());
        worldHandle.DrawTextureRect(_aoStencilTarget!.Texture, worldBounds);

        // Draw the Blurred AO texture finally.
        worldHandle.UseShader(_proto.Index(StencilEqualDrawShader).Instance());
        worldHandle.DrawTextureRect(_aoTarget!.Texture, worldBounds, color);

        args.WorldHandle.SetTransform(Matrix3x2.Identity);
    }
}
