// Copyright (c) 2025 xaze_
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
// 
// I <3 🦈s :3c

using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using Mirror;
using Mirror.RemoteCalls;
using PlayerRoles;
using PlayerRoles.Spectating;
using UnityEngine;
using Utils.NonAllocLINQ;
using XazeAPI.API;
using XazeAPI.API.EffectStacks;

namespace XazeCustomEffects.Features
{
    public class CustomEffectsController : NetworkBehaviour
    {
        public static readonly Dictionary<ReferenceHub, CustomEffectsController> List = new();
        public readonly Dictionary<Type, CustomEffectBase> _effectsByType = new();

        public ReferenceHub? Hub;
        public GameObject? effectsGameObject;
        public CustomEffectBase[] AllEffects { get; private set; } = [];

        public void EnableEffect<T>(EffectStack stack) where T : CustomEffectBase
        {
            if (!TryGetEffect<T>(out var effect))
                return;
            effect.ServerAddStack(stack);
        }
        
        public EffectStack? EnableEffect<T>(int intensity = 1, float duration = 0f) where T : CustomEffectBase
        {
            if (!TryGetEffect<T>(out var effect))
                return null;
            
            var stack = new  EffectStack { Intensity = intensity, Duration = duration };
            effect.ServerAddStack(stack);
            return stack;
        }
        
        public bool DisableEffect<T>() where T : CustomEffectBase
        {
            if (!TryGetEffect<T>(out var effect))
                return false;
            return effect.ServerDisable();
        }
        
        public bool DisableEffect<T>(EffectStack stack) where T : CustomEffectBase
        {
            if (!TryGetEffect<T>(out var effect))
                return false;
            return effect.ServerRemoveStack(stack);
        }

        public static EffectStack? EnableEffect<T>(ReferenceHub Hub, int intensity, float duration = 0) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
                return null;
            
            return controller.EnableEffect<T>(intensity, duration);
        }
        
        public static EffectStack? EnableEffect<T>(Player Hub, int intensity, float duration = 0) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
                return null;
            
            return controller.EnableEffect<T>(intensity, duration);
        }

        public static bool DisableEffect<T>(ReferenceHub Hub) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
                return false;
            
            return controller.DisableEffect<T>();
        }

        public static bool DisableEffect<T>(Player Hub) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
                return false;
            
            return controller.DisableEffect<T>();
        }

        public bool TryGetEffect(string effectName, out CustomEffectBase playerEffect)
        {
            foreach (CustomEffectBase statusEffectBase in AllEffects)
            {
                if (!statusEffectBase.Name.StartsWith(effectName, StringComparison.InvariantCultureIgnoreCase) &&
                    !statusEffectBase.ToString()
                        .EndsWith(effectName, StringComparison.InvariantCultureIgnoreCase)) continue;
                
                playerEffect = statusEffectBase;
                return true;
            }

            playerEffect = null;
            return false;
        }

        public bool TryGetEffect<T>(out T playerEffect) where T : CustomEffectBase
        {
            if (_effectsByType.TryGetValue(typeof(T), out var value) && value is T val)
            {
                playerEffect = val;
                return true;
            }

            playerEffect = null;
            return false;
        }

        public void UseMedicalItem(ItemBase item)
        {
            foreach (CustomEffectBase statusEffectBase in AllEffects)
            {
                if (statusEffectBase is IHealableEffect healablePlayerEffect && healablePlayerEffect.IsHealable(item.ItemTypeId))
                {
                    statusEffectBase.IsEnabled = false;
                }
            }
        }

        public CustomEffectBase ChangeState(string effectName, int intensity, float duration = 0f)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'CustomStatusEffects.CustomEffectBase PlayerCustomEffectsController::ChangeState(System.String,System.Byte,System.Single,System.Boolean)' called when server was not active");
                return null;
            }

            if (TryGetEffect(effectName, out var playerEffect))
            {
                playerEffect.ServerSetState(intensity, duration);
            }

            return playerEffect;
        }

        public T ChangeState<T>(int intensity, float duration = 0f) where T : CustomEffectBase
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'T PlayerCustomEffectsController::ChangeState(System.Byte,System.Single,System.Boolean)' called when server was not active");
                return null;
            }

            if (TryGetEffect<T>(out var playerEffect))
            {
                playerEffect.ServerSetState(intensity, duration);
            }

            return playerEffect;
        }

        public void DisableAllEffects()
        {
            var allEffects = AllEffects;
            foreach (var t in allEffects)
            {
                t.ServerDisable();
            }
        }

        public T GetEffect<T>() where T : CustomEffectBase
        {
            if (!TryGetEffect<T>(out var playerEffect))
                return null;
            return playerEffect;
        }

        public CustomEffectBase GetEffect(Type effectType)
        {
            if (!_effectsByType.TryGetValue(effectType, out var playerEffect))
                return null;
            return playerEffect;
        }

        public void Awake()
        {
            if (gameObject.TryGetComponent(out CustomEffectsController controller) && controller != this)
            {
                Destroy(this);
                Logging.Warn("A second", nameof(CustomEffectsController), "was added to", gameObject);
                return;
            }
            
            Hub = ReferenceHub.GetHub(gameObject);
            List.Add(Hub, this);
        }

        public void LoadEffects()
        {
            effectsGameObject = gameObject;
            AllEffects = effectsGameObject.GetComponentsInChildren<CustomEffectBase>();
            var allEffects = AllEffects;
            foreach (CustomEffectBase statusEffectBase in allEffects)
            {
                _effectsByType.Add(statusEffectBase.GetType(), statusEffectBase);
            }
        }

        private void Start()
        {
            effectsGameObject?.SetActive(value: true);
        }

        private void OnEnable()
        {
            PlayerRoleManager.OnRoleChanged += OnRoleChanged;
        }

        private void OnDisable()
        {
            PlayerRoleManager.OnRoleChanged -= OnRoleChanged;
        }

        public void OnRoleChanged(ReferenceHub targetHub, PlayerRoleBase oldRole, PlayerRoleBase newRole)
        {
            if (targetHub != Hub)
            {
                return;
            }

            bool flag = oldRole != null && oldRole.Team != Team.Dead && newRole.Team == Team.Dead;
            foreach (var effect in AllEffects)
            {
                if (flag)
                {
                    effect.OnDeath(oldRole);
                }
                else
                {
                    effect.OnRoleChanged(oldRole, newRole);
                }
            }
        }
        
        public static CustomEffectsController? Get(Player plr)
        {
            if (!List.TryGetValue(plr.ReferenceHub, out var controller))
                return plr.GameObject?.AddComponent<CustomEffectsController>();
            
            return controller;
        }
        
        public static CustomEffectsController? Get(ReferenceHub? hub)
        {
            if (hub == null)
                return null;
            
            if (!List.TryGetValue(hub, out var controller))
                return hub.gameObject.AddComponent<CustomEffectsController>();
            
            return controller;
        }

        public static bool TryGet(ReferenceHub? hub, out CustomEffectsController controller)
        {
            controller = null;
            if (hub == null)
                return false;

            if (!List.TryGetValue(hub, out var customController)) 
                return false;
            
            controller = customController;
            return true;
        }
        
        public static bool TryGet(Player plr, out CustomEffectsController controller)
        {
            controller = null;
            if (!List.TryGetValue(plr.ReferenceHub, out var customController)) 
                return false;
            
            controller = customController;
            return true;
        }
    }
}
