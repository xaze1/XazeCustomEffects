using System;
using CustomPlayerEffects;
using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using XazeAPI.API;
using XazeCustomEffects.Features;

namespace XazeCustomEffects.Patches
{
    public class StaminaPatch
    {
        [HarmonyPatchCategory(Loader.PatchGroup)]
        [HarmonyPatch(typeof(FpcStateProcessor), nameof(FpcStateProcessor.ServerUseRate), MethodType.Getter)]
        public class StaminaUsagePatch
        {
            public static bool Prefix(FpcStateProcessor __instance, ref float __result)
            {
                try
                {
                    if (__instance.Hub.roleManager.CurrentRole.ActiveTime <= __instance._respawnImmunity)
                    {
                        __result = 0f;
                        return false;
                    }

                    float num = __instance._useRate * __instance.Hub.inventory.StaminaUsageMultiplier;

                    foreach (StatusEffectBase effect in __instance.Hub.playerEffectsController.AllEffects)
                    {
                        if (effect is IStaminaModifier mod && mod.StaminaModifierActive)
                        {
                            num *= mod.StaminaUsageMultiplier;
                        }
                    }

                    if (CustomEffectsController.TryGet(__instance.Hub, out var controller))
                    {
                        foreach (CustomEffectBase effect in controller.AllEffects)
                        {
                            if (effect is IStaminaModifier mod && mod.StaminaModifierActive)
                            {
                                num *= mod.StaminaUsageMultiplier;
                            }
                        }
                    }

                    __result = num;
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.Error("[StaminaPatch] - ServerUseRate Patch failed\n" + ex);
                    return true;
                }
            }
        }

        [HarmonyPatchCategory(Loader.PatchGroup)]
        [HarmonyPatch(typeof(FpcStateProcessor), nameof(FpcStateProcessor.ServerRegenRate), MethodType.Getter)]
        public class StaminaRegenPatch
        {
            public static bool Prefix(FpcStateProcessor __instance, ref float __result)
            {
                float num = __instance._regenerationOverTime.Evaluate((float)__instance._regenStopwatch.Elapsed.TotalSeconds);

                try
                {
                    foreach (StatusEffectBase effect in __instance.Hub.playerEffectsController.AllEffects)
                    {
                        if (effect is IStaminaModifier mod && mod.StaminaModifierActive)
                        {
                            num *= mod.StaminaRegenMultiplier;
                        }
                    }

                    if (CustomEffectsController.TryGet(__instance.Hub, out var controller))
                    {
                        foreach (CustomEffectBase effect in controller.AllEffects)
                        {
                            if (effect is IStaminaModifier mod && mod.StaminaModifierActive)
                            {
                                num *= mod.StaminaRegenMultiplier;
                            }
                        }
                    }

                    __result = num;
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.Error("[StaminaPatch] - ServerRegeneRate Patch failed\n" + ex);
                    return true;
                }
            }
        }
    }
}
