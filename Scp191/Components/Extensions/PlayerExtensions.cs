using CommandSystem;
using Exiled.API.Features;
using Scp191.Components.Features;

namespace Scp191.Components.Extensions;

public static class PlayerExtensions
{
    public static Player AsPlayer(this ICommandSender sender)
        => Player.Get(sender);

    public static Scp191Properties Scp191(this Player player)
        => player.ReferenceHub.GetComponent<Scp191Properties>();
}