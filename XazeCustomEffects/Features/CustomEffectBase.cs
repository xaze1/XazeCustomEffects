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
using Mirror;
using PlayerRoles;
using UnityEngine;
using XazeAPI.API.EffectStacks;

namespace XazeCustomEffects.Features
{
    public abstract class CustomEffectBase : MonoBehaviour
    {
        public static event Action<CustomEffectBase>? OnEnabled;
        public static event Action<CustomEffectBase>? OnDisabled;
        public static event Action<CustomEffectBase, int, int>? OnIntensityChanged;

        public List<EffectStack> Stacks { get; } = new();

        public EffectStack? Stack
        {
            get
            {
                if (this is not IUnstackable)
                    return null;
                return Stacks.TryGet(0, out var stack) ? stack : null;
            }
        }

        public int _intensity;

        public enum EffectClassification
        {
            Technical,
            Negative,
            Positive,
            Mixed,
        }

        public abstract EffectClassification Classification { get; }

        protected virtual bool AllowEnabling
        {
            get
            {
                if (Classification != EffectClassification.Negative)
                {
                    return true;
                }

                if (!SpawnProtected.CheckPlayer(Hub))
                {
                    return !Vitality.CheckPlayer(Hub);
                }
                return false;
            }
        }

        public abstract string Name { get; }

        public int Intensity
        {
            get => _intensity;
            private set
            {
                if (value > _intensity && !AllowEnabling)
                    return;
                
                ForceIntensity(value);
            }
        }

        public virtual int MaxIntensity => int.MaxValue;

        public ReferenceHub Hub { get; set; }

        public virtual bool IsEnabled
        {
            get => Intensity > 0;
            set
            {
                if (value == IsEnabled)
                    return;
                
                if (value)
                    ServerSetState(1);
                else
                    ServerDisable();
            }
        }

        public virtual bool GetSpectatorText(out string s)
        {
            s = Name;
            return IsEnabled;
        }

        private void Awake()
        {
            Hub = ReferenceHub.GetHub(transform.root.gameObject);
            OnAwake();
        }

        protected virtual void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            UpdateStacks();
            OnEffectUpdate();
        }

        private void UpdateIntensity()
        {
            int intensity = 0;
            Stacks.Sort((a, b) => a.MaxIntensity.CompareTo(b.MaxIntensity));
            foreach (var stack in Stacks)
            {
                if (stack.IsActive)
                    intensity = Mathf.Min(intensity + stack.MaxIntensity, Mathf.Min(stack.MaxIntensity, MaxIntensity));
            }
            
            intensity = Mathf.Max(intensity, 0);
            if (Intensity == intensity)
                return;
            
            Intensity = (byte) intensity;
        }

        private void UpdateStacks()
        {
            for (int i = Stacks.Count; i >= 0; i--)
            {
                var stack = Stacks[i];
                stack.RefreshTime(Time.deltaTime);
                
                if (stack.Duration == 0 || stack.TimeLeft > 0 || !stack.CanBeRemoved)
                    continue;
                Stacks.RemoveAt(i);
            }
            
            UpdateIntensity();
        }

        private void ForceIntensity(int value)
        {
            if (_intensity == value)
                return;

            int intensity = _intensity;
            bool flag = intensity == 0 && value > 0;

            _intensity = Mathf.Min(value, MaxIntensity);

            if (flag)
            {
                OnEnabled?.Invoke(this);
                Enabled();
            }
            else if (intensity > 0 && value == 0)
            {
                OnDisabled?.Invoke(this);
                Disabled();
            }


            IntensityChanged(intensity, value);
        }

        public void ServerAddStack(EffectStack stack)
        {
            if (Stacks.Contains(stack))
                return;
            
            if (this is IUnstackable)
                Stacks.Clear();
            
            Stacks.Add(stack);
            UpdateIntensity();
        }

        public bool ServerRemoveStack(EffectStack stack)
        {
            var outcome = Stacks.Remove(stack);
            UpdateIntensity();
            return outcome;
        }

        public void ServerSetState(int intensity, float duration = 0f)
        {
            DisableEffect();
            ServerAddStack(new EffectStack { Intensity = intensity, Duration = duration });
        }

        public bool ServerDisable() => DisableEffect();

        protected virtual void Start()
        {
            _intensity = 1;
            DisableEffect();
        }
        
        protected virtual void Enabled()
        {
        }

        protected virtual void Disabled()
        {
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnEffectUpdate()
        {
        }


        public virtual void OnDeath(PlayerRoleBase prevRole)
        {
            DisableEffect();
        }
        
        public virtual void OnRoleChanged(PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
        }

        public virtual void IntensityChanged(int prevState, int newState)
        {
        }

        public virtual void OnBeginSpectating()
        {
        }

        public virtual void OnStopSpectating()
        {
        }

        protected virtual bool DisableEffect()
        {
            if (Stacks.Count == 0)
                return false;

            var hasLockedStacks = false;
            for (int i = Stacks.Count -1; i >= 0; i--)
            {
                if (Stacks[i].CanBeRemoved)
                    Stacks.RemoveAt(i);
                else
                    hasLockedStacks = true;
            }

            if (hasLockedStacks)
            {
                UpdateIntensity();
                return false;
            }
            
            Intensity = 0;
            return true;
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}
