using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._DeltaV.Administration.UI;

[GenerateTypedNameReferences]
public sealed partial class DepartmentWhitelistPanel : PanelContainer
{
    public Action<ProtoId<JobPrototype>, bool>? OnSetJob;

    public DepartmentWhitelistPanel(DepartmentPrototype department, IPrototypeManager proto, HashSet<ProtoId<JobPrototype>> whitelists)
    {
        RobustXamlLoader.Load(this);

        var allWhitelisted = true;
        var grey = Color.FromHex("#ccc");
        foreach (var id in department.Roles)
        {
            var thisJob = id; // closure capturing funny
            var button = new CheckBox();
            button.Text = proto.Index(id).LocalizedName;
            if (!proto.Index(id).Whitelisted)
                button.Modulate = grey; // Let admins know whitelisting this job is only for futureproofing.
            button.Pressed = whitelists.Contains(id);
            button.OnPressed += _ => OnSetJob?.Invoke(thisJob, button.Pressed);
            JobsContainer.AddChild(button);

            allWhitelisted &= button.Pressed;
        }

        Department.Text = Loc.GetString(department.Name);
        Department.Modulate = department.Color;
        Department.Pressed = allWhitelisted;
        Department.OnPressed += args =>
        {
            foreach (var id in department.Roles)
            {
                // only request to whitelist roles that aren't already whitelisted, and vice versa
                if (whitelists.Contains(id) != Department.Pressed)
                    OnSetJob?.Invoke(id, Department.Pressed);
            }
        };
    }
}
