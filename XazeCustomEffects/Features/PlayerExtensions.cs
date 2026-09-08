// // Copyright (c) 2025 xaze_
// //
// // This source code is licensed under the MIT license found in the
// // LICENSE file in the root directory of this source tree.
// //
// // I <3 🦈s :3c

using LabApi.Features.Wrappers;
using XazeAPI.API.EffectStacks;

namespace XazeCustomEffects.Features;

public static class PlayerExtensions
{
    extension(Player plr)
    {
        public void AddEffect<T>(EffectStack stack) where T : CustomEffectBase
        {
            if (!CustomEffectsController.TryGet(plr, out var controller))
                return;
            controller.EnableEffect<T>(stack);
        }
        
        public EffectStack? AddEffect<T>(int intensity, float duration = 0) where T : CustomEffectBase
        {
            if (!CustomEffectsController.TryGet(plr, out var controller))
                return null;
            return controller.EnableEffect<T>(intensity, duration);
        }
        
        public bool RemoveEffect<T>() where T : CustomEffectBase
        {
            if (!CustomEffectsController.TryGet(plr, out var controller))
                return false;
            return controller.DisableEffect<T>();
        }
        
        public bool RemoveEffect<T>(EffectStack stack) where T : CustomEffectBase
        {
            if (!CustomEffectsController.TryGet(plr, out var controller))
                return false;
            return controller.DisableEffect<T>(stack);
        }
    }
}