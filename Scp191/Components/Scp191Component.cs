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
    public override string Name { get; set; } = "SCP-191";
    public override string Description { get; set; } = String.Empty;
    public override string CustomInfo { get; set; } = String.Empty;
    public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
    public override Vector3 Scale { get; set; } = new Vector3(0.8f, 0.8f, 0.8f);
    public override int MaxHealth { get; set; } = 100;
    public abstract int AdaptiveShieldMaxValue { get; set; }
}