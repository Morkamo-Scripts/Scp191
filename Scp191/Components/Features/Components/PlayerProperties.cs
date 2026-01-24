using System;
using Exiled.API.Enums;
using PlayerRoles;
using Scp191.Components.Features.Components.Interfaces;
using UnityEngine;

namespace Scp191.Components.Features.Components;

public class PlayerProperties(Scp191Properties scp191Properties) : IPropertyModule
{
    public Scp191Properties Scp191Properties { get; } = scp191Properties;

    public GameObject HighlightPrefab { get; set; }
    public bool IsInEscapingProcess { get; set; } = false;
}