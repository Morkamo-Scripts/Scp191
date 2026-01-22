using Exiled.API.Features;
using Scp191.Components.Features.Components;
using UnityEngine;

namespace Scp191.Components.Features;

public sealed class Scp191Properties() : MonoBehaviour
{
    private void Awake()
    {
        Player = Player.Get(gameObject);
        PlayerProperties = new PlayerProperties(this);
    }
    
    public Player Player { get; private set; }
    public PlayerProperties PlayerProperties { get; private set; }
}