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
