using System;
using System.Collections;
using AdvancedInterfaces.Components;
using AdvancedInterfaces.Events.EventArgs.Player;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using RueI.API;
using RueI.API.Elements;
using Scp191.Components;
using Scp191.Components.Features;
using UnityEngine;
using events = Exiled.Events.Handlers;

namespace Scp191;

public class Scp191Handler : Scp191Component
{
    public override uint Id { get; set; } = 7;
    public override int AdaptiveShieldMaxValue { get; set; } = 100;

    private const float HealBlockSeconds = 10f;
    private float _nextHealTime;
    private bool _isScp053Died = false;

    private Coroutine _ashCoroutine;
    private Coroutine _spCoroutine;
    private bool _isAshLost = false;

    protected override void SubscribeEvents()
    {
        AdvancedInterfaces.Events.EventManager.PlayerEvents.CustomRoleTypeDied += OnCustomRoleDied;
        events.Player.Spawned += OnSpawned;
        events.Player.Hurt += OnHurt;
        events.Player.ReceivingEffect += OnReceivingEffect;
        events.Player.Hurting += OnHurting;
        events.Player.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        AdvancedInterfaces.Events.EventManager.PlayerEvents.CustomRoleTypeDied -= OnCustomRoleDied;
        events.Player.Spawned -= OnSpawned;
        events.Player.Hurt -= OnHurt;
        events.Player.ReceivingEffect -= OnReceivingEffect;
        events.Player.Hurting -= OnHurting;
        events.Player.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnSpawned(SpawnedEventArgs ev)
    {
        Timing.CallDelayed(0.1f, () =>
        {
            if (ev.Player.IsNPC || !Check(ev.Player))
                return;

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

            _ashCoroutine = CoroutineRunner.Run(AshProcessor(ev.Player));
            _spCoroutine = CoroutineRunner.Run(StaminaProcessor(ev.Player));
            
            ev.Player.EnableEffect(EffectType.NightVision, 15);
        });
    }

    private void OnReceivingEffect(ReceivingEffectEventArgs ev)
    {
        if (ev.Player == null || !ev.Player.IsConnected || ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (ev.Effect.GetEffectType() == EffectType.NightVision && ev.Intensity == 0)
            ev.IsAllowed = false;
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if (ev.Usable.Type == ItemType.Adrenaline)
        {
            ev.IsAllowed = false;

            RueDisplay.Get(ev.Player).Show(new Tag(), new BasicElement(750,
                    "<color=orange><size=40>SCP-191 не может использовать инъекторы адреналина!</size></color>")
                , 5f);

            Timing.CallDelayed(5.1f, () => RueDisplay.Get(ev.Player).Update());
        }
    }

    private void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Player.IsNPC || !Check(ev.Player))
            return;

        if ((int)ev.Player.HumeShield > 0 && ev.DamageHandler.Type == DamageType.Falldown)
        {
            ev.IsAllowed = false;
            ev.Player.HumeShield = 0;
            _nextHealTime = Time.time + HealBlockSeconds;
        }
    }

    private void OnHurt(HurtEventArgs ev)
    {
        if (!ev.Player.IsNPC && Check(ev.Player))
            _nextHealTime = Time.time + HealBlockSeconds;
    }

    private void OnCustomRoleDied(CustomRoleTypeDiedEventArgs ev)
    {
        if (ev.Role == CustomRoleType.Scp053)
            _isScp053Died = true;
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
            if (_isAshLost)
            {
                yield return new WaitForSeconds(1f);
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
            _isAshLost = (int)player.HumeShield <= 0;
            player.IsUsingStamina = _isAshLost;
            yield return new WaitForSeconds(0.25f);
        }
    }
}