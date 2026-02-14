using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Shared.Maps;
using Content.Shared.Storage;
using Robust.Shared.EntitySerialization;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using static Content.IntegrationTests.Tests.PostMapInitTest;

namespace Content.IntegrationTests.Tests._starcup.Storage;

/// <summary>
/// Checks for container entities that contain pre-filled entities from before the grid-based storage update.
/// See teamstarcup/starcup#471
/// </summary>
[TestFixture]
public sealed class GridlessStorageTest
{
    [Test, TestCaseSource(typeof(PostMapInitTest), nameof(GameMaps))]
    public async Task GridlessContainersTest(string mapProto)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true // Stations spawn a bunch of nullspace entities and maps like centcomm.
        });
        var server = pair.Server;

        var entManager = server.ResolveDependency<IEntityManager>();
        var mapSystem = entManager.System<SharedMapSystem>();
        var protoManager = server.ResolveDependency<IPrototypeManager>();
        var ticker = entManager.EntitySysManager.GetEntitySystem<GameTicker>();

        await server.WaitPost(() =>
        {
            MapId mapId;
            try
            {
                var opts = DeserializationOptions.Default with {InitializeMaps = true};
                ticker.LoadGameMap(protoManager.Index<GameMapPrototype>(mapProto), out mapId, opts);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load map {mapProto}", ex);
            }

            var query = entManager.EntityQuery<StorageComponent, MetaDataComponent>();
            var oddballs = query.Where(ent =>
            {
                var (storage, _) = ent;
                return storage.Container.Count != storage.StoredItems.Count;
            });

            Assert.Multiple(() =>
            {
                foreach (var oddball in oddballs)
                {
                    var (storage, metadata) = oddball;
                    Assert.That(storage.StoredItems.Keys, Is.EqualTo(storage.Container.ContainedEntities),
                        () => $"Encountered pre-filled storage container without StorageComponent " +
                              $"info. It is likely corrupt, please recreate it. ProtoId: {metadata.EntityPrototype?.ID}");
                }
            });

            try
            {
                mapSystem.DeleteMap(mapId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete map {mapProto}", ex);
            }
        });
        await server.WaitRunTicks(1);

        await pair.CleanReturnAsync();
    }
}
