using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using HarmonyLib;
using Mirror;
using PlayerRoles;
using UnityEngine;

namespace XazeCustomEffects.Features
{
    public class CustomEffectBase : MonoBehaviour
    {
        public static readonly Dictionary<ReferenceHub, List<CustomEffectBase>> ActiveEffects = new();

        public static Dictionary<Type, CustomEffectBase> _effectsByType = new();

        public static event Action<CustomEffectBase> OnEnabled;

        public static event Action<CustomEffectBase> OnDisabled;

        public static event Action<CustomEffectBase, int, int> OnIntensityChanged;

        public int _intensity;

        public float _duration;

        public float _timeLeft;

        public enum EffectClassification
        {
            Technical,
            Negative,
            Positive,
            Mixed,
        }

        public virtual EffectClassification Classification { get; set; }

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

        public virtual string Name { get; set; }

        public int Intensity
        {
            get => _intensity;
            set
            {
                if (value <= _intensity || AllowEnabling)
                {
                    ForceIntensity(value);
                }
            }
        }

        public virtual int MaxIntensity => int.MaxValue;

        public ReferenceHub Hub { get; set; }

        public virtual bool IsEnabled
        {
            get => Intensity > 0;
            set
            {
                if (value != IsEnabled)
                {
                    Intensity = (value ? 1 : 0);
                }
            }
        }

        public float Duration
        {
            get => _duration;
            set => _duration = Mathf.Max(0f, value);
        }

        public float TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = Mathf.Max(0f, value);
                if (_timeLeft != 0f || Duration == 0f)
                {
                    return;
                }
                
                DisableEffect();
            }
        }

        public virtual string GetSpectatorText()
        {
            return Name?? "CustomEffect(" + GetType().Name + ")";
        }

        public void Awake()
        {
            Hub = ReferenceHub.GetHub(transform.root.gameObject);
            Name ??= GetType().Name;
            OnAwake();
        }

        public virtual void Update()
        {
            if (IsEnabled)
            {
                if (!ActiveEffects.ContainsKey(Hub))
                {
                    ActiveEffects.Add(Hub, new());
                }
                
                ActiveEffects[Hub].Add(this);

                RefreshTime();
                OnEffectUpdate();
            }
        }

        public void RefreshTime()
        {
            if (Duration == 0f)
            {
                return;
            }
            
            TimeLeft -= Time.deltaTime;
        }

        public void ForceIntensity(int value)
        {
            if (_intensity == value)
                return;

            int intensity = _intensity;
            bool flag = intensity == 0 && value > 0;

            _intensity = Mathf.Min(value, MaxIntensity);

            if (flag)
            {
                if (!ActiveEffects.ContainsKey(Hub))
                    ActiveEffects.Add(Hub, [this]);
                else
                {
                    if (!ActiveEffects[Hub].Contains(this))
                        ActiveEffects[Hub].AddItem(this);
                }
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

        [Server]
        public void ServerSetState(int intensity, float duration = 0f, bool addDuration = false)
        {
            OnIntensityChanged?.Invoke(this, Intensity, intensity);
            Intensity = intensity;
            ServerChangeDuration(duration, addDuration);
        }

        [Server]
        public void ServerDisable()
        {
            DisableEffect();
        }

        [Server]
        public void ServerChangeDuration(float duration, bool addDuration = false)
        {
            if (addDuration && duration > 0f)
            {
                Duration += duration;
                TimeLeft += duration;
            }
            else
            {
                Duration = duration;
                TimeLeft = Duration;
            }
        }

        public virtual void Start()
        {
            _intensity = 1;
            DisableEffect();
        }
        public virtual void Enabled()
        {
        }

        public virtual void Disabled()
        {
        }

        public virtual void OnAwake()
        {
        }

        public virtual void OnEffectUpdate()
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

        public virtual void DisableEffect()
        {
            Intensity = 0;

            if (!ActiveEffects.TryGetValue(Hub, out var effect))
            {
                return;
            }
            
            effect.Remove(this);
        }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}
