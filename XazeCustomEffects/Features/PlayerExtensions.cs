// // Copyright (c) 2025 xaze_
// //
// // This source code is licensed under the MIT license found in the
// // LICENSE file in the root directory of this source tree.
// //
// // I <3 🦈s :3c

using LabApi.Features.Wrappers;

namespace XazeCustomEffects.Features;

public static class PlayerExtensions
{
    extension(Player plr)
    {
        public void EnableCustomEffect<T>(int intensity, float duration = 0, bool addDuration = false) where T : CustomEffectBase
        {
            CustomEffectsController.EnableEffect<T>(plr, intensity, duration, addDuration);
        }
        
        public void DisableCustomEffect<T>() where T : CustomEffectBase
        {
            CustomEffectsController.DisableEffect<T>(plr);
        }
        
        public void AddIntensity<T>(int intensity, int maxIntensity = 0, float duration = 0) where T : CustomEffectBase
        {
            if (!CustomEffectsController.TryGet(plr, out var controller))
            {
                return;
            }
            
            controller.AddIntensity<T>(intensity, maxIntensity, duration);
        }
        
        public void RemoveIntensity<T>(int intensity, int minIntensity = 0, float duration = 0) where T : CustomEffectBase
        {
            if (!CustomEffectsController.TryGet(plr, out var controller))
            {
                return;
            }
            
            controller.RemoveIntensity<T>(intensity, minIntensity, duration);
        }
    }
}