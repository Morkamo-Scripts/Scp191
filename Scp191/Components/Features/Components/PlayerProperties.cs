using System;
using Exiled.API.Enums;
using PlayerRoles;
using Scp191.Components.Features.Components.Interfaces;

namespace Scp191.Components.Features.Components;

public class PlayerProperties(Scp191Properties scp191Properties) : IPropertyModule
{
    public Scp191Properties Scp191Properties { get; } = scp191Properties;
    
    public bool HasBeenSpawned { get; set; } = false;
    public bool IsOwnerModeEnabled { get; set; } = false;
    public bool IsUnlimitedAmmo { get; set; } = false;
    public RoleTypeId? ReservedRole { get; set; } = null;
    
    public bool IsInfinityJailbird { get; set; } = false;
    
    private bool _isInfinityMagazines = false;
    public bool IsInfinityMagazines
    {
        get => _isInfinityMagazines;
        set
        {
            if (_isInfinityMagazines == value)
                return;

            _isInfinityMagazines = value;

            if (!value)
            {
                Scp191Properties.Player.ClearAmmo();
                return;
            }

            Scp191Properties.Player.ClearAmmo();

            foreach (AmmoType type in Enum.GetValues(typeof(AmmoType)))
                Scp191Properties.Player.AddAmmo(type, 1000);
        }
    }

    public DateTime? LastCall { get; set; }
    public bool ImSleeping { get; set; } = false;
}