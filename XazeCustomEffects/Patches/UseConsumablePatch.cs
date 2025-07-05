using HarmonyLib;
using InventorySystem.Items.Usables;
using XazeCustomEffects.Features;

namespace XazeCustomEffects.Patches
{
    [HarmonyPatchCategory(Loader.PatchGroup)]
    [HarmonyPatch(typeof(Scp500), nameof(Scp500.OnEffectsActivated))]
    public class Scp500Patch
    {
        public static void Postfix(Scp500 __instance)
        {
            if (!CustomEffectsController.TryGet(__instance.Owner, out var controller))
            {
                return;
            }

            controller.UseMedicalItem(__instance);
        }
    }
    
    [HarmonyPatchCategory(Loader.PatchGroup)]
    [HarmonyPatch(typeof(Adrenaline), nameof(Adrenaline.OnEffectsActivated))]
    public class AdrenalinePatch
    {
        public static void Postfix(Adrenaline __instance)
        {
            if (!CustomEffectsController.TryGet(__instance.Owner, out var controller))
            {
                return;
            }

            controller.UseMedicalItem(__instance);
        }
    }
    
    [HarmonyPatchCategory(Loader.PatchGroup)]
    [HarmonyPatch(typeof(Medkit), nameof(Medkit.OnEffectsActivated))]
    public class MedkitPatch
    {
        public static void Postfix(Medkit __instance)
        {
            if (!CustomEffectsController.TryGet(__instance.Owner, out var controller))
            {
                return;
            }

            controller.UseMedicalItem(__instance);
        }
    }
    
    [HarmonyPatchCategory(Loader.PatchGroup)]
    [HarmonyPatch(typeof(Painkillers), nameof(Painkillers.OnEffectsActivated))]
    public class PainkillersPatch
    {
        public static void Postfix(Painkillers __instance)
        {
            if (!CustomEffectsController.TryGet(__instance.Owner, out var controller))
            {
                return;
            }

            controller.UseMedicalItem(__instance);
        }
    }
}
