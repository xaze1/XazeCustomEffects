// Copyright (c) 2025 xaze_
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
// 
// I <3 🦈s :3c

using System;
using HarmonyLib;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MEC;
using XazeAPI.API.AudioCore.FakePlayers;
using XazeAPI.API.Extensions;
using XazeCustomEffects.Features;

namespace XazeCustomEffects
{
    public class Loader : Plugin
    {
        public const string PatchGroup = "XAZE-CustomEffects";
        public static readonly Harmony HarmonyPatch = new Harmony("Xaze-Patches-CustomEffects");
        
        public override string Name => "Xaze-CustomEffects";
        public override string Description => "Custom Effects API made by xaze_";
        public override string Author => "xaze_";
        public override Version Version => new(1, 0, 0);
        public override Version RequiredApiVersion => new(0, 0, 0);
        public override LoadPriority Priority => LoadPriority.Lowest;

        public override void Enable()
        {
            HarmonyPatch.PatchCategory(PatchGroup);
            ReferenceHub.OnPlayerAdded += ctx => Timing.CallDelayed(0.1f, () => SetupPlayer(ctx));
        }

        public override void Disable()
        {
            HarmonyPatch.UnpatchCategory(PatchGroup);
        }

        private static void SetupPlayer(ReferenceHub hub)
        {
            if (hub.Mode == CentralAuth.ClientInstanceMode.Host || hub.Mode == CentralAuth.ClientInstanceMode.DedicatedServer || AudioManager.ActiveFakes.Contains(hub)) return;

            var customEffectController = hub.gameObject.AddComponent<CustomEffectsController>();

            LabApi.Loader.PluginLoader.Plugins.ForEach(x =>
            {
                foreach (var type in x.Value.GetTypes())
                {
                    if (type.IsAbstract || !typeof(CustomEffectBase).IsAssignableFrom(type))
                        continue;

                    customEffectController.gameObject.AddComponent(type);
                }
            });

            customEffectController.LoadEffects();
        }
    }
}