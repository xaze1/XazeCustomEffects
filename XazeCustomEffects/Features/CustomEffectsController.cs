// Copyright (c) 2025 xaze_
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
// 
// I <3 🦈s :3c

using System;
using System.Collections.Generic;
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

namespace XazeCustomEffects.Features
{
    public class CustomEffectsController : NetworkBehaviour
    {
        public static Dictionary<ReferenceHub, CustomEffectsController> activeCustomControllers = new();

        public readonly Dictionary<Type, CustomEffectBase> _effectsByType = new();

        public readonly SyncList<byte> _syncEffectsIntensity = new SyncList<byte>();

        public ReferenceHub Hub;

        public GameObject effectsGameObject;

        public CustomEffectBase[] AllEffects { get; private set; }

        public int EffectsLength { get; set; }

        public bool _wasSpectated;

        public static CustomEffectsController Get(ReferenceHub hub)
        {
            return activeCustomControllers.GetValueSafe(hub);
        }
        
        public static CustomEffectsController Get(Player plr)
        {
            return activeCustomControllers.GetValueSafe(plr.ReferenceHub);
        }

        public static bool TryGet(ReferenceHub hub, out CustomEffectsController controller)
        {
            controller = null;

            activeCustomControllers.TryGetValue(hub, out controller);

            return controller is not null;
        }
        
        public static bool TryGet(Player plr, out CustomEffectsController controller)
        {
            return TryGet(plr.ReferenceHub, out controller);
        }

        public static void EnableEffect<T>(ReferenceHub Hub, int intensity, float duration = 0, bool addDuration = false) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
            {
                return;
            }
            
            controller.ChangeState<T>(intensity, duration, addDuration);
        }

        public static void EnableEffect<T>(Player Hub, int intensity, float duration = 0, bool addDuration = false) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
            {
                return;
            }
            
            controller.ChangeState<T>(intensity, duration, addDuration);
        }

        public static void DisableEffect<T>(ReferenceHub Hub) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
            {
                return;
            }
            
            controller.DisableEffect<T>();
        }

        public static void DisableEffect<T>(Player Hub) where T : CustomEffectBase
        {
            if (!TryGet(Hub, out var controller))
            {
                return;
            }
            
            controller.DisableEffect<T>();
        }

        public bool TryGetEffect(string effectName, out CustomEffectBase playerEffect)
        {
            CustomEffectBase[] allEffects = AllEffects;
            foreach (CustomEffectBase statusEffectBase in allEffects)
            {
                if (statusEffectBase.Name.StartsWith(effectName, StringComparison.InvariantCultureIgnoreCase))
                {
                    playerEffect = statusEffectBase;
                    return true;
                }
                else if(statusEffectBase.ToString().EndsWith(effectName, StringComparison.InvariantCultureIgnoreCase))
                {
                    playerEffect = statusEffectBase;
                    return true;
                }

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

        [Server]
        public void UseMedicalItem(ItemBase item)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void PlayerCustomEffectsController::UseMedicalItem(InventorySystem.Items.ItemBase)' called when server was not active");
                return;
            }

            CustomEffectBase[] allEffects = AllEffects;
            foreach (CustomEffectBase statusEffectBase in allEffects)
            {
                if (statusEffectBase is IHealableEffect healablePlayerEffect && healablePlayerEffect.IsHealable(item.ItemTypeId))
                {
                    statusEffectBase.IsEnabled = false;
                }
            }
        }

        [Server]
        public CustomEffectBase ChangeState(string effectName, byte intensity, float duration = 0f, bool addDuration = false)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'CustomStatusEffects.CustomEffectBase PlayerCustomEffectsController::ChangeState(System.String,System.Byte,System.Single,System.Boolean)' called when server was not active");
                return null;
            }

            if (TryGetEffect(effectName, out var playerEffect))
            {
                playerEffect.ServerSetState(intensity, duration, addDuration);
            }

            return playerEffect;
        }

        [Server]
        public T ChangeState<T>(int intensity, float duration = 0f, bool addDuration = false) where T : CustomEffectBase
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'T PlayerCustomEffectsController::ChangeState(System.Byte,System.Single,System.Boolean)' called when server was not active");
                return null;
            }

            if (TryGetEffect<T>(out var playerEffect))
            {
                playerEffect.ServerSetState(intensity, duration, addDuration);
            }

            return playerEffect;
        }

        [Server]
        public T EnableEffect<T>(float duration = 0f, bool addDuration = false) where T : CustomEffectBase
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'T PlayerCustomEffectsController::EnableEffect(System.Single,System.Boolean)' called when server was not active");
                return null;
            }

            return ChangeState<T>(1, duration, addDuration);
        }

        [Server]
        public T DisableEffect<T>() where T : CustomEffectBase
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'T PlayerCustomEffectsController::DisableEffect()' called when server was not active");
                return null;
            }

            return ChangeState<T>(0);
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
            {
                return null;
            }

            return playerEffect;
        }

        public void ServerSendPulse<T>() where T : IPulseEffect
        {
            for (int i = 0; i < EffectsLength; i++)
            {
                if (AllEffects[i] is not T)
                {
                    continue;
                }
                
                byte index = (byte)Mathf.Min(i, 255);
                TargetRpcReceivePulse(Hub.connectionToClient, index);
                SpectatorNetworking.ForeachSpectatorOf(Hub, delegate (ReferenceHub x)
                {
                    TargetRpcReceivePulse(x.connectionToClient, index);
                });
                break;
            }
        }

        [TargetRpc]
        public void TargetRpcReceivePulse(NetworkConnection _, byte effectIndex)
        {
            NetworkWriterPooled writer = NetworkWriterPool.Get();
            NetworkWriterExtensions.WriteByte(writer, effectIndex);
            SendTargetRPCInternal(_, "System.Void PlayerCustomEffectsController::TargetRpcReceivePulse(Mirror.NetworkConnection,System.Byte)", 483637978, writer, 0);
            NetworkWriterPool.Return(writer);
        }

        public void Awake()
        {
            Hub = ReferenceHub.GetHub(base.gameObject);
            activeCustomControllers.Add(Hub, this);
        }

        public void LoadEffects()
        {

            effectsGameObject = this.gameObject;
            AllEffects = effectsGameObject.GetComponentsInChildren<CustomEffectBase>();
            EffectsLength = AllEffects.Length;
            var allEffects = AllEffects;
            foreach (CustomEffectBase statusEffectBase in allEffects)
            {
                _effectsByType.Add(statusEffectBase.GetType(), statusEffectBase);
                _syncEffectsIntensity.Add(0);
            }
        }

        public void Update()
        {
        }

        public void Start()
        {
            effectsGameObject.SetActive(value: true);
        }

        public void OnEnable()
        {
            PlayerRoleManager.OnRoleChanged += OnRoleChanged;
        }

        public void OnDisable()
        {
            PlayerRoleManager.OnRoleChanged -= OnRoleChanged;
        }

        public void OnRoleChanged(ReferenceHub targetHub, PlayerRoleBase oldRole, PlayerRoleBase newRole)
        {
            if (targetHub != Hub)
            {
                return;
            }

            bool flag = oldRole.Team != Team.Dead && newRole.Team == Team.Dead;
            CustomEffectBase[] allEffects = AllEffects;
            foreach (CustomEffectBase statusEffectBase in allEffects)
            {
                if (flag)
                {
                    statusEffectBase.OnDeath(oldRole);
                }
                else
                {
                    statusEffectBase.OnRoleChanged(oldRole, newRole);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            SpectatorTargetTracker.OnTargetChanged += delegate
            {
                if (ReferenceHub.AllHubs.TryGetFirst((ReferenceHub x) => x.playerEffectsController._wasSpectated, out var first))
                {
                    var allEffects = activeCustomControllers.GetValueSafe(first).AllEffects;
                    foreach (var t in allEffects)
                    {
                        t.OnStopSpectating();
                    }

                    activeCustomControllers.GetValueSafe(first)._wasSpectated = false;
                }

                if (!SpectatorTargetTracker.TryGetTrackedPlayer(out var hub)) return;
                {
                    CustomEffectsController playerEffectsController = activeCustomControllers.GetValueSafe(hub);
                    foreach (var t in playerEffectsController.AllEffects)
                    {
                        t.OnBeginSpectating();
                    }

                    playerEffectsController._wasSpectated = true;
                }
            };
        }

        public CustomEffectsController()
        {
            InitSyncObject(_syncEffectsIntensity);
        }

        public void UserCode_TargetRpcReceivePulse__NetworkConnection__Byte(NetworkConnection _, byte effectIndex)
        {
            int num = Mathf.Min(effectIndex, EffectsLength - 1);
            if (AllEffects[num] is IPulseEffect pulseEffect)
            {
                pulseEffect.ExecutePulse();
            }
        }

        public static void InvokeUserCode_TargetRpcReceivePulse__NetworkConnection__Byte(NetworkBehaviour obj, NetworkReader reader, NetworkConnectionToClient senderConnection)
        {
            if (!NetworkClient.active)
            {
                Debug.LogError("TargetRPC TargetRpcReceivePulse called on server.");
            }
            else
            {
                ((PlayerEffectsController)obj).UserCode_TargetRpcReceivePulse__NetworkConnection__Byte(null, NetworkReaderExtensions.ReadByte(reader));
            }
        }

        static CustomEffectsController()
        {
            RemoteProcedureCalls.RegisterRpc(typeof(CustomEffectsController), "System.Void PlayerCustomEffectsController::TargetRpcReceivePulse(Mirror.NetworkConnection,System.Byte)", InvokeUserCode_TargetRpcReceivePulse__NetworkConnection__Byte);
        }
    }
}
