using UnityEngine;

namespace XazeCustomEffects.Features
{
    public abstract class CustomTickingBase : CustomEffectBase
    {
        public abstract void OnTick();

        public override void Enabled()
        {
            base.Enabled();
            this._timeTillTick = this.TimeBetweenTicks;
        }

        public override void OnEffectUpdate()
        {
            base.OnEffectUpdate();
            this._timeTillTick -= Time.deltaTime;
            if (this._timeTillTick > 0f)
            {
                return;
            }
            this._timeTillTick += this.TimeBetweenTicks;
            this.OnTick();
        }

        public CustomTickingBase()
        {
        }

        public float TimeBetweenTicks = 1f;

        public float _timeTillTick;
    }
}
