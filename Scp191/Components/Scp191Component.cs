using System;
using System.Linq;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;
using PlayerRoles;
using Scp191.Events;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace Scp191.Components;

public abstract class Scp191Component : CustomRole
{
    public abstract override string Name { get; set; }
    public abstract override string Description { get; set; }
    public override string CustomInfo { get; set; } = String.Empty;
    public abstract override RoleTypeId Role { get; set; }
    public override Vector3 Scale { get; set; } = new(0.8f, 0.8f, 0.8f);
    public override int MaxHealth { get; set; } = 100;
    public abstract int AdaptiveShieldMaxValue { get; set; }
}