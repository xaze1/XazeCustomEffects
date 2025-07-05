// Copyright (c) 2025 xaze_
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
// 
// I <3 🦈s :3c

using System;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Searching;
using XazeAPI.API;
using XazeCustomEffects.Features;

namespace XazeCustomEffects.Patches
{
    [HarmonyPatchCategory(Loader.PatchGroup)]
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.SearchTimeForPlayer))]
    public class SearchTimePatch
    {
        public static bool Prefix(ItemPickupBase __instance, ref ReferenceHub hub, ref float __result)
        {
            try
            {
                float searchTime = 0.245f + 0.175f * __instance.Info.WeightKg;
                foreach(StatusEffectBase effect in hub.playerEffectsController.AllEffects)
                {
                    if (effect.IsEnabled && effect is ISearchTimeModifier mod)
                    {
                        searchTime = mod.ProcessSearchTime(searchTime);
                    }
                }

                if (CustomEffectsController.TryGet(hub, out var controller))
                {
                    foreach(CustomEffectBase customEffect in controller.AllEffects)
                    {
                        if (customEffect.IsEnabled && customEffect is ISearchTimeModifier mod)
                        {
                            searchTime = mod.ProcessSearchTime(searchTime);
                        }
                    }
                }

                if (hub.inventory.CurInstance is ISearchTimeModifier itemMod)
                {
                    searchTime = itemMod.ProcessSearchTime(searchTime);
                }

                __result = searchTime;
                return false;
            }
            catch (Exception ex)
            {
                Logging.Error("[CustomEffects - SearchTime] EXCEPTION:\n" + ex);
                return true;
            }
        }
    }
}
