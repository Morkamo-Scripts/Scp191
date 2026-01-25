using System;
using System.Collections;
using AdvancedCassie.Components.Extensions;
using AdvancedInterfaces.Components;
using AdvancedInterfaces.Events;
using AdvancedInterfaces.Events.EventArgs.Player;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp939;
using MEC;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using Scp191.Components;
using Scp191.Components.Extensions;
using Scp191.Components.Features;
using UnityEngine;
using events = Exiled.Events.Handlers;
using Object = UnityEngine.Object;

namespace Scp191.Variants;

public class Scp191Chaos : Scp191Component
{
    public override uint Id { get; set; } = 8;
    public override string Name { get; set; } = "SCP-191-CHAOS";

    public override string Description { get; set; } = "<size=35><color=#028a00>Ты всё ещё <b>SCP-191</color></b>\n" +
                                                       "<color=#ff8c00>Игра продолжается!</color></size>";
    public override RoleTypeId Role { get; set; } = RoleTypeId.ChaosConscript;
    public override int AdaptiveShieldMaxValue { get; set; } = 100;

    private const float HealBlockSeconds = 10f;
    private float _nextHealTime;
    private bool _isScp053Died;
    private bool _isAshLost;

    private Coroutine _ashCoroutine;
    private Coroutine _spCoroutine;
    private Coroutine _hCoroutine;

    protected override void SubscribeEvents()
    {
        EventManager.PlayerEvents.CustomRoleTypeDied += OnCustomRoleDied;
        events.Player.Spawned += OnSpawned;
        events.Player.Hurt += OnHurt;
        events.Player.ReceivingEffect += OnReceivingEffect;
        events.Player.Hurting += OnHurting;
        events.Player.UsingItem += OnUsingItem;
        events.Scp939.ValidatingVisibility += OnValidatingVisibilityTargetFor939;
        events.Player.Died += OnDied;
        events.Player.Escaping += OnEscaping;
        events.Player.Escaped += OnEscaped;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        EventManager.PlayerEvents.CustomRoleTypeDied -= OnCustomRoleDied;
        events.Player.Spawned -= OnSpawned;
        events.Player.Hurt -= OnHurt;
        events.Player.ReceivingEffect -= OnReceivingEffect;
        events.Player.Hurting -= OnHurting;
        events.Player.UsingItem -= OnUsingItem;
        events.Scp939.ValidatingVisibility -= OnValidatingVisibilityTargetFor939;
        events.Player.Died -= OnDied;
        events.Player.Escaping -= OnEscaping;
        events.Player.Escaped -= OnEscaped;
        base.UnsubscribeEvents();
    }
    
    protected override void RoleAdded(Player player)
    {
        base.RoleAdded(player);

        Timing.CallDelayed(0.3f, () =>
        {
            player.ResetInventory(Role.GetInventory().Items);
            player.AddAmmo(Role.GetStartingAmmo());
        });
    }
    
    protected override void RoleRemoved(Player player)
    {
        base.RoleRemoved(player);
        player.AdvancedCassie().PlayerProperties.IsCustomScp = false;
        Object.Destroy(player.Scp191().PlayerProperties.HighlightPrefab);
        player.Scp191().PlayerProperties.HighlightPrefab = null;
    }

    private void OnSpawned(SpawnedEventArgs ev)
    {
        Timing.CallDelayed(0.1f, () =>
        {
            if (ev.Player.IsNPC || !Check(ev.Player))
                return;

            var props = ev.Player.Scp191().PlayerProperties;
            if (props.HighlightPrefab != null)
            {
                Object.Destroy(props.HighlightPrefab);
                props.HighlightPrefab = null;
            }

            if (_ashCoroutine != null)
            {
                CoroutineRunner.Stop(_ashCoroutine);
                _ashCoroutine = null;
            }
            
            if (_spCoroutine != null)
            {
                CoroutineRunner.Stop(_spCoroutine);
                _spCoroutine = null;
            }
            
            if (_hCoroutine != null)
            {
                CoroutineRunner.Stop(_hCoroutine);
                _hCoroutine = null;
            }

            _ashCoroutine = CoroutineRunner.Run(AshProcessor(ev.Player));
            _spCoroutine = CoroutineRunner.Run(StaminaProcessor(ev.Player));
            _hCoroutine = CoroutineRunner.Run(HintsProcessor(ev.Player));
            
            ev.Player.EnableEffect(EffectType.NightVision, 15);
            ev.Player.AdvancedCassie().PlayerProperties.IsCustomScp = true;
            ev.Player.Scale = new Vector3(0.8f, 0.8f, 0.8f);
            InitGlow(ev.Player);
        });
    }

    private void OnReceivingEffect(ReceivingEffectEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsConnected || ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (ev.Effect.GetEffectType() == EffectType.NightVision && ev.Intensity == 0)
            ev.IsAllowed = false;

        if (ev.Effect.GetEffectType() == EffectType.Flashed && ev.Intensity != 0)
            ev.IsAllowed = false;
    }

    private void OnValidatingVisibilityTargetFor939(ValidatingVisibilityEventArgs ev)
    {
        if (Check(ev.Target))
        {
            ev.IsLateSeen = true;
            ev.IsAllowed = true;
        }
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (Plugin.Instance.Config.NotAllowedItems.Contains(ev.Usable.Type))
        {
            ev.IsAllowed = false;
                
            RueDisplay.Get(ev.Player).Show(
                new Tag(),
                new BasicElement(900, "<size=50><b><color=#ff7d00>Этот предмет запрещен для использования у\n\n SCP-191</color></b></size>"), 3);
                
            Timing.CallDelayed(3.1f, () => RueDisplay.Get(ev.Player).Update());

            foreach (var player in ev.Player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(player).Show(
                    new Tag(),
                    new BasicElement(900, "<size=50><b><color=#ff7d00>Этот предмет запрещен для использования у\n\n SCP-191</color></b></size>"), 3);
                    
                Timing.CallDelayed(3.1f, () => RueDisplay.Get(player).Update());
            }
        }
    }

    private void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (ev.DamageHandler.Type == DamageType.Tesla || ev.DamageHandler.Type == DamageType.MicroHid)
        {
            ev.IsAllowed = false;
            
            if (ev.Player.Health < ev.Player.MaxHealth)
                ev.Player.Heal(2f);
            else
                ev.Player.HumeShield = Mathf.Clamp(ev.Player.HumeShield + 2, 0, AdaptiveShieldMaxValue);
            
            return;
        }
        
        if ((int)ev.Player.HumeShield > 0 && ev.DamageHandler.Type == DamageType.Falldown)
        {
            ev.IsAllowed = false;
            ev.Player.HumeShield = 0;
            ev.Player.PlayShieldBreakSound();
            _nextHealTime = Time.time + HealBlockSeconds;

            var effector = new GameObject()
            {
                transform =
                {
                    position = ev.Player.Transform.position - new Vector3(0, 0.65f, 0)
                }
            };
            
            HighlightManager.ProceduralExplosionParticles(effector, 
                new Color32(195, 0, 255, 255), 
                40,
                new Vector3(1f, 1f, 1f), 
                0.25f, 
                10, 
                2, 4, 2);
        }
    }

    private void OnHurt(HurtEventArgs ev)
    {
        if (!ev.Player.IsNPC && Check(ev.Player))
            _nextHealTime = Time.time + HealBlockSeconds;
    }
    
    private void OnDied(DiedEventArgs ev)
    {
        if (!Check(ev.Player))
            return;
        
        EventManager.PlayerEvents.InvokeCustomRoleDied(ev.Player, CustomRoleType.Scp191);
    }
    
    private void OnCustomRoleDied(CustomRoleDiedEventArgs ev)
    {
        if (ev.Role == CustomRoleType.Scp053)
            _isScp053Died = true;

        if (ev.Role == CustomRoleType.Scp191)
            ev.Player.AdvancedCassie().PlayerProperties.IsCustomScp = false;
    }

    private void OnEscaping(EscapingEventArgs ev)
    {
        if (Check(ev.Player))
            ev.Player.Scp191().PlayerProperties.IsInEscapingProcess = true;
    }
    
    private void OnEscaped(EscapedEventArgs ev) { }

    private void InitGlow(Player player)
    {
        var properties = player.Scp191().PlayerProperties;
        properties.HighlightPrefab = new GameObject("Scp191HighlighterGO")
        {
            transform =
            {
                position = player.Transform.position - new Vector3(0, 0.65f, 0)
            }
        };
        properties.HighlightPrefab.transform.SetParent(player.Transform);
        
        HighlightManager.ProceduralParticles(properties.HighlightPrefab, 
            new Color32(255, 125, 0, 255), 0, 0.05f,
            new(1.2f, 1.2f, 1.2f), 0.125f, 12, 8, 60, 1f);
    }

    private IEnumerator AshProcessor(Player player)
    {
        player.CustomHumeShieldStat.MaxValue = AdaptiveShieldMaxValue;
        player.HumeShield = 0;

        for (player.HumeShield = 0;
             player.HumeShield < AdaptiveShieldMaxValue && player.IsConnected && Check(player);
             player.HumeShield++)
        {
            player.HumeShield = Mathf.Clamp(player.HumeShield + 1, 0, AdaptiveShieldMaxValue);
            yield return new WaitForSeconds(0.08f);
        }

        while (player.IsConnected && Check(player))
        {
            if (player.Health < player.MaxHealth)
                _isAshLost = true;
            
            if (_isAshLost)
            {
                if (player.Health >= player.MaxHealth)
                    _isAshLost = false;
                
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            
            if (Time.time > _nextHealTime)
                player.HumeShield = Mathf.Clamp(player.HumeShield + 1, 0, AdaptiveShieldMaxValue);

            if (_isScp053Died)
                yield return new WaitForSeconds(1f);
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator StaminaProcessor(Player player)
    {
        while (player.IsConnected && Check(player))
        {
            if (!_isAshLost && player.IsUsingStamina)
                player.ResetStamina();
            
            player.IsUsingStamina = _isAshLost;
            yield return new WaitForSeconds(0.25f);
        }
    }
    
    private IEnumerator HintsProcessor(Player player)
    {
        var properties = player.Scp191().PlayerProperties;
        
        RueDisplay.Get(player).Show(
            new Tag(),
            new BasicElement(250, Description), 7);

        Timing.CallDelayed(7.1f, () => RueDisplay.Get(player).Update());
            
        foreach (var spec in player.CurrentSpectatingPlayers)
        {
            RueDisplay.Get(spec).Show(
                new Tag(),
                new BasicElement(250, Description), 7);
            
            Timing.CallDelayed(7.1f, () => RueDisplay.Get(spec).Update());
        }
            
        while (player.IsConnected && player.IsAlive)
        {
            foreach (var spec in player.CurrentSpectatingPlayers)
            {
                RueDisplay.Get(spec).Show(
                    new Tag(),
                    new BasicElement(120, "<align=right><size=30><b><color=#ff7d00>Игрок играет за SCP-191</color></b></size>"), 1);
                    
                Timing.CallDelayed(1.1f, () => RueDisplay.Get(spec).Update());
            }
                
            yield return new WaitForSeconds(1f);
        }
    }
}