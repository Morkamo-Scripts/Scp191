using System;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using Scp191.Components.Features;
using Scp191.Events;

namespace Scp191
{
    public class Plugin : Plugin<Config>
    {
        public override string Author => "Morkamo";
        public override string Name => "SCP-191";
        public override string Prefix => Name;
        public override Version Version => new(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new(9, 12, 6);

        public static Plugin Instance;
        public static Harmony Harmony;
        
        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Config.Scp191ClassD.Register();
            Config.Scp191Chaos.Register();
            Config.Scp191Ntf.Register();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Config.Scp191ClassD.Unregister();
            Config.Scp191Chaos.Unregister();
            Config.Scp191Ntf.Unregister();
            Instance = null;
            base.OnDisabled();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.IsNPC)
                return;
            
            if (ev.Player.ReferenceHub.gameObject.GetComponent<Scp191Properties>() != null)
                return;

            ev.Player.ReferenceHub.gameObject.AddComponent<Scp191Properties>();
            
            EventManager.PlayerEvents.InvokePlayerFullConnected(ev.Player);
        }
    }
}