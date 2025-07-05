using System;
using CommandSystem;
using RemoteAdmin;
using Utils;
using XazeAPI.API;
using XazeCustomEffects.Features;

namespace XazeCustomEffects.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CustomEffectCommand : ICommand, IUsageProvider
    {
        public string Command => "customeffect";

        public string[] Aliases => new string[0];

        public string Description => "Applies a Custom Effect to a player";

        public string[] Usage { get; set; } = { "CustomEffect", "Intensity", "Duration", "%player%" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(PlayerPermissions.Effects, out response))
                return false;

            if (arguments.Count < 3)
            {
                return returnUsage(arguments, out response);
            }

            if (!int.TryParse(arguments.At(1), out int intensity) || !float.TryParse(arguments.At(2), out float duration))
            {
                response = "Intensity(Int) or Duration(Float) are not a correct value";
                return false;
            }

            if (arguments.Count < 4)
            {
                if (sender is not PlayerCommandSender cmdSender ||
                    !CustomPlayer.TryGet(cmdSender.ReferenceHub, out CustomPlayer User))
                {
                    return returnUsage(arguments, out response);
                }
                
                if (CustomEffectsController.TryGet(User.Player, out var contr) && contr.TryGetEffect(arguments.At(0), out var effect))
                {
                    effect.ServerSetState(intensity, duration);

                    response = $"Done! {arguments.At(0)} set to {intensity} Intensity with a duration of {duration}s";
                    return true;
                }

                response = "Couldn't find Effect";
                return false;

            }

            var targets = RAUtils.ProcessPlayerIdOrNamesList(arguments, 3, out _);

            if (targets == null)
                return returnUsage(arguments, out response);

            int index = 0;
            foreach (ReferenceHub target in targets)
            {
                if (!CustomPlayer.TryGet(target, out CustomPlayer User))
                {
                    continue;
                }
                    
                if (CustomEffectsController.TryGet(User.Player, out var contr) && contr.TryGetEffect(arguments.At(0), out var effect))
                {
                    effect.ServerSetState(intensity, duration);

                    index++;
                }
                else
                {
                    response = "Couldn't find Effect";
                    return false;
                }
            }

            response = $"Done! This request affected {index} {(index == 1? "Player" : "Players")}";
            return true;
        }

        private bool returnUsage(ArraySegment<string> arguments, out string response)
        {
            response = "Usage: " + arguments.Array?[0] + this.DisplayCommandUsage();
            return false;
        }
    }
}
