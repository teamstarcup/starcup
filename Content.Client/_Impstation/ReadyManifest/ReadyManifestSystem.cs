using Content.Shared._Impstation.ReadyManifest;

namespace Content.Client._Impstation.ReadyManifest;

public sealed class ReadyManifestSystem : EntitySystem
{
    private HashSet<string> _departments = new();

    public IReadOnlySet<string> Departments => _departments;

    public void RequestReadyManifest()
    {
        RaiseNetworkEvent(new RequestReadyManifestMessage());
    }
}
