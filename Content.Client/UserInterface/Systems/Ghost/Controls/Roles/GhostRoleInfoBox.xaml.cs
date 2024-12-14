using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Systems.Ghost.Controls.Roles
{
    [GenerateTypedNameReferences]
    public sealed partial class GhostRoleInfoBox : BoxContainer
    {
        public GhostRoleInfoBox(string name, string description)
        {
            RobustXamlLoader.Load(this);

            Title.Text = name;
            Description.SetMessage(description);
        }
    }
}