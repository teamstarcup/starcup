using System.Linq;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Commands;

/// <summary>
/// Loads every map under Resources/Maps, runs the "variantize" command and saves them.
/// </summary>
[AdminCommand(AdminFlags.Host)]
public sealed partial class VariantizeAllMapsCommand : LocalizedEntityCommands
{
    [Dependency] private IConsoleHost _console = default!;
    [Dependency] private IResourceManager _resource = default!;
    [Dependency] private MapLoaderSystem _loader = default!;

    private static readonly ResPath MapsRoot = new("/Maps");

    public override string Command => "variantizeallmaps";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length > 1)
        {
            shell.WriteLine(Help);
            return;
        }

        var filter = args.Length == 1 ? args[0] : null;

        var paths = _resource.ContentFindFiles(MapsRoot)
            .Where(p => p.Extension == "yml")
            .Where(p => filter == null || p.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.ToString())
            .ToList();

        if (paths.Count == 0)
        {
            shell.WriteError(Loc.GetString("cmd-variantizeallmaps-none", ("filter", filter ?? "")));
            return;
        }

        var loadOptions = new MapLoadOptions
        {
            DeserializationOptions = new DeserializationOptions { StoreYamlUids = true },
        };

        var saveOptions = new SerializationOptions { ExpectPreInit = true };

        var saved = 0;
        var failed = 0;

        foreach (var path in paths)
        {
            if (!_loader.TryLoadGeneric(path, out var result, loadOptions))
            {
                shell.WriteError(Loc.GetString("cmd-variantizeallmaps-load-failed", ("path", path)));
                failed++;
                continue;
            }

            try
            {
                foreach (var grid in result.Grids)
                {
                    _console.ExecuteCommand(shell.Player, $"variantize {EntityManager.GetNetEntity(grid.Owner)}");
                }

                var ok = result.Category switch
                {
                    FileCategory.Map when result.Maps.Count == 1
                        => _loader.TrySaveMap(result.Maps.First().Owner, path, saveOptions),
                    FileCategory.Grid when result.Grids.Count == 1
                        => _loader.TrySaveGrid(result.Grids.First().Owner, path, saveOptions),
                    _ => false,
                };

                if (ok)
                {
                    saved++;
                    shell.WriteLine(Loc.GetString("cmd-variantizeallmaps-saved", ("path", path), ("grids", result.Grids.Count)));
                }
                else
                {
                    failed++;
                    shell.WriteError(Loc.GetString("cmd-variantizeallmaps-save-failed", ("path", path), ("category", result.Category)));
                }
            }
            finally
            {
                foreach (var uid in result.RootNodes)
                {
                    if (EntityManager.EntityExists(uid)) {
                        EntityManager.DeleteEntity(uid);
                    }
                }
            }
        }

        shell.WriteLine(Loc.GetString("cmd-variantizeallmaps-done", ("saved", saved), ("failed", failed)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1) {
            return CompletionResult.FromHint(Loc.GetString("cmd-variantizeallmaps-hint-filter"));
        }

        return CompletionResult.Empty;
    }
}
