// Copyright (c) 2025 xaze_
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
// 
// I <3 🦈s :3c

using System;
using CustomPlayerEffects;
using HarmonyLib;
using XazeAPI.API;
using XazeCustomEffects.Features;

namespace XazeCustomEffects.Patches
{
    [HarmonyPatchCategory(Loader.PatchGroup)]
    [HarmonyPatch(typeof(UsableItemModifierEffectExtensions), nameof(UsableItemModifierEffectExtensions.TryGetSpeedMultiplier))]
    public class UsableItemPatch
    {
        public static bool Prefix(ref ItemType type, ref ReferenceHub player, ref float multiplier, ref bool __result)
        {
            try
            {
                bool result = false;
                multiplier = 1;
                foreach(StatusEffectBase effect in player.playerEffectsController.AllEffects)
                {
                    if (!effect.IsEnabled || effect is not IUsableItemModifierEffect mod ||
                        !mod.TryGetSpeed(type, out float modMult)) continue;
                    multiplier *= modMult;
                    result = true;
                }

                if (CustomEffectsController.TryGet(player, out var controller))
                {
                    foreach (CustomEffectBase customEffect in controller.AllEffects)
                    {
                        if (!customEffect.IsEnabled || customEffect is not IUsableItemModifierEffect mod ||
                            !mod.TryGetSpeed(type, out float modMult)) continue;
                        multiplier *= modMult;
                        result = true;
                    }
                }

                __result = result;
                return false;
            }
            catch (Exception ex)
            {
                Logging.Error("[CustomEffect - UsableItem] EXCEPTION:\n" + ex);
                return true;
            }
        }
    }
}
