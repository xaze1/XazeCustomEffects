// Copyright (c) 2025 xaze_
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
// 
// I <3 🦈s :3c

using UnityEngine;

namespace XazeCustomEffects.Features
{
    public abstract class CustomTickingBase : CustomEffectBase
    {
        public abstract void OnTick();

        public override void Enabled()
        {
            base.Enabled();
            _timeTillTick = TimeBetweenTicks;
        }

        public override void OnEffectUpdate()
        {
            base.OnEffectUpdate();
            _timeTillTick -= Time.deltaTime;
            if (_timeTillTick > 0f)
            {
                return;
            }
            _timeTillTick += TimeBetweenTicks;
            OnTick();
        }

        public virtual float TimeBetweenTicks => 1f;
        public float _timeTillTick;
    }
}
