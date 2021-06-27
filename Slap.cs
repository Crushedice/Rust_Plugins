using System;
using UnityEngine;
using Random = System.Random;

namespace Oxide.Plugins
{
    [Info("Slap", "Wulf/lukespragg", "0.2.0", ResourceId = 1458)]
    [Description("Allows players with permission to slap other players around.")]

    class Slap : RustPlugin
    {
        // Do NOT edit this file, instead edit Slap.json in server/<identity>/oxide/config

        #region Configuration

        // Messages
        string CommandUsage => GetConfig("CommandUsage", "Usage:\n /slap name (replace 'name' with a player name)");
        string PlayerNotFound => GetConfig("PlayerNotFound", "Player '{player}' not found!");
        string PlayerSlapped => GetConfig("PlayerSlapped", "{player} got Bitchslapped!");
        string NoPermission => GetConfig("NoPermission", "Sorry, you can't use 'slap' right now");

        // Settings
        string ChatCommand => GetConfig("ChatCommand", "slap");
        int DamageAmount => GetConfig("DamageAmount", 5);
        int SlapsPerUse => GetConfig("SlapsPerUse", 3);

        protected override void LoadDefaultConfig()
        {
            // Messages
            Config["CommandUsage"] = CommandUsage;
            Config["PlayerNotFound"] = PlayerNotFound;
            Config["PlayerSlapped"] = PlayerSlapped;
            Config["NoPermission"] = NoPermission;

            // Settings
            Config["ChatCommand"] = ChatCommand;
            Config["DamageAmount"] = DamageAmount;
            Config["SlapsPerUse"] = SlapsPerUse;

            SaveConfig();
        }

        #endregion
        void Loaded()
        {
            LoadDefaultConfig();
            permission.RegisterPermission("slap.allowed", this);
            cmd.AddChatCommand(ChatCommand, this, "SlapChatCmd");
        }

        #region Chat Command

        void SlapChatCmd(BasePlayer player, string command, string[] args)
        {
            if (!HasPermission(player, "slap.allowed"))
            {
                PrintToChat(player, NoPermission);
                return;
            }

            if (args.Length != 1)
            {
                PrintToChat(player, CommandUsage);
                return;
            }

            var target = BasePlayer.Find(args[0]);
            if (target == null)
            {
                PrintToChat(player, PlayerNotFound.Replace("{player}", args[0]));
                return;
            }

            timer.Repeat(0.3f, SlapsPerUse, () =>
            {
                if (DamageAmount > 0f) target.Hurt(DamageAmount);
                SlapPlayer(target);
            });

            PrintToChat(PlayerSlapped.Replace("{player}", target.displayName));
        }

        #endregion

        #region Player Slapping

        void SlapPlayer(BasePlayer player)
        {
            var position = player.transform.position;
            var destination = new Vector3();
            var random = new Random();

            destination.x = position.x + random.Next(-3, 3);
            destination.y = position.y + random.Next(1, 3);
            destination.z = position.z + random.Next(-3, 3);

            var flinches = new[]
            {
                    BaseEntity.Signal.Flinch_Chest,
                    BaseEntity.Signal.Flinch_Head,
                    BaseEntity.Signal.Flinch_Stomach
                };
            var flinch = flinches[random.Next(flinches.Length)];
            player.SignalBroadcast(flinch, string.Empty, null);

            var effects = new[]
            {
                    "headshot",
                    "headshot_2d",
                    "impacts/slash/clothflesh/clothflesh1",
                    "impacts/stab/clothflesh/clothflesh1"
                };
            var effect = effects[random.Next(effects.Length)];
            Effect.server.Run($"assets/bundled/prefabs/fx/{effect}.prefab", player.transform.position, Vector3.zero);

            player.MovePosition(destination);
            player.ClientRPCPlayer(null, player, "ForcePositionTo", destination);
            //player.TransformChanged;
        }

        #endregion

        #region Helper Methods

        T GetConfig<T>(string name, T defaultValue)
        {
            if (Config[name] == null) return defaultValue;
            return (T)Convert.ChangeType(Config[name], typeof(T));
        }

        bool HasPermission(BasePlayer player, string perm) => permission.UserHasPermission(player.UserIDString, perm);

        #endregion
    }
}

