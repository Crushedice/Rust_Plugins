
using Oxide.Core.Libraries.Covalence;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Configuration;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using Network;
namespace Oxide.Plugins
{
    [Info("OwnDrone", "Crushedice", 0.85)]
    [Description("Plugin Collab")]
	
  class OwnDrone : RustPlugin
    {
        private readonly int groundLayer = LayerMask.GetMask("Terrain", "World");
        const string carPrefab = "assets/prefabs/misc/marketplace/marketterminal.prefab";
	const string slotprefab = "assets/prefabs/misc/casino/slotmachine/slotmachine.prefab";
	const string carPrefab3 ="assets/prefabs/misc/marketplace/drone.delivery.prefab";
const string carPrefab2 ="assets/prefabs/misc/marketplace/marketplace.prefab";
string shopkeep = "assets/prefabs/npc/bandit/shopkeepers/bandit_shopkeeper.prefab";
				private Dictionary<ulong, float> cooldowns = new Dictionary<ulong, float>();
				float Cooldown = 300f;
	
	 
	#region Commands
	
	
		[ChatCommand("drone")]
        void boadt(BasePlayer player, string command, string[] args)
		{
			
			// if (HasCooldown(player))
            //{
            //    // The command is still on cooldown for the player, show an error message.
            //    SendMessage(player, "Cooldown", FormatCooldown(GetCooldown(player)));
			//
            //    return;
            //}
			Vector3 spawnpos = new Vector3(player.eyes.position.x,player.eyes.position.y,player.eyes.position.z);
			BaseEntity entity = GameManager.server.CreateEntity(carPrefab,spawnpos);
			
			BaseEntity entity2 = GameManager.server.CreateEntity(carPrefab2,spawnpos);
			
			BaseEntity entity3 = GameManager.server.CreateEntity(carPrefab3,spawnpos);
            entity.Spawn();            
			entity2.Spawn();   
			entity3.Spawn();   
			SetCooldown(player);
			
		}
		[ChatCommand("slot")]
        void bwoat(BasePlayer player, string command, string[] args)
		{
			
			 if (HasCooldown(player))
            {
                // The command is still on cooldown for the player, show an error message.
                SendMessage(player, "Cooldown", FormatCooldown(GetCooldown(player)));

                return;
            }
			Vector3 spawnpos = new Vector3(player.eyes.position.x,player.eyes.position.y,player.eyes.position.z - 3f);
			BaseEntity entity = GameManager.server.CreateEntity(slotprefab,spawnpos);
            entity.Spawn();            
			
			SetCooldown(player);
			
		}
		[ChatCommand("spawn")]
        void customs(BasePlayer player, string command, string[] args)
		{



            // if (HasCooldown(player))
            //{
            //    // The command is still on cooldown for the player, show an error message.
            //    SendMessage(player, "Cooldown", FormatCooldown(GetCooldown(player)));
            //
            //    return;
            //}

            RaycastHit hit;

            if (!Physics.Raycast(player.eyes.HeadRay(), out hit, 1000f, groundLayer))
            return;

                var entitty = args[0];
			Vector3 spawnpos = hit.point;
            var rota = player.transform.rotation; //* Quaternion.Euler(0, 180f, 0);
           
            BaseEntity entity = GameManager.server.CreateEntity(entitty,spawnpos, rota);
            Quaternion toRot = Quaternion.LookRotation(entity.transform.up) * Quaternion.Euler(Vector3.zero);
            entity.transform.rotation = toRot;
            entity.Spawn();            
			
			//SetCooldown(player);
			
		}
        private float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0f);
        }
        private static Vector3 GetGround(Vector3 position)
        {
            var height = TerrainMeta.HeightMap.GetHeight(position);
            position.y = Math.Max(position.y, height);
            return position;
        }
        private bool HasCooldown(BasePlayer player)
        {
            // Check if cooldown is enabled.
            if (Cooldown <= 0)
            {
                return false;
            }

            // Check if cooldown is ignored for the player.
            

            // Make sure there is an entry for the player in the dictionary.
            if (!cooldowns.ContainsKey(player.userID))
            {
                cooldowns.Add(player.userID, 0);
            }

            // Check if the command is on cooldown or not.
            return Time.realtimeSinceStartup - cooldowns[player.userID] < Cooldown;
        }
		
		private float GetCooldown(BasePlayer player)
        {
            return Time.realtimeSinceStartup - cooldowns[player.userID];
        }

        /// <summary>
        /// Sets the last use for the cooldown handling of the command for the given <see cref="BasePlayer"/>.
        /// </summary>
        /// <param name="player">The player to put the command on cooldown for. </param>
        private void SetCooldown(BasePlayer player)
        {
            // Check if cooldown is enabled.
            if (Cooldown <= 0)
            {
                return;
            }

            // Check if cooldown is ignored for the player.
           

            // Make sure there is an entry for the player in the dictionary.
            if (!cooldowns.ContainsKey(player.userID))
            {
                cooldowns.Add(player.userID, 0);
            }

            // Set the last use
            cooldowns[player.userID] = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Returns a formatted string for the given cooldown.
        /// </summary>
        /// <param name="seconds">The cooldown in seconds. </param>
        private string FormatCooldown(float seconds)
        {
            // Create a new TimeSpan from the remaining cooldown.
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            List<string> output = new List<string>();

            // Check if it is more than a single day and add it to the result.
            if (t.Days >= 1)
            {
                output.Add($"{t.Days} {(t.Days > 1 ? "days" : "day")}");
            }

            // Check if it is more than an hour and add it to the result.
            if (t.Hours >= 1)
            {
                output.Add($"{t.Hours} {(t.Hours > 1 ? "hours" : "hour")}");
            }

            // Check if it is more than a minute and add it to the result.
            if (t.Minutes >= 1)
            {
                output.Add($"{t.Minutes} {(t.Minutes > 1 ? "minutes" : "minute")}");
            }

            // Check if there is more than a second and add it to the result.
            if (t.Seconds >= 1)
            {
                output.Add($"{t.Seconds} {(t.Seconds > 1 ? "seconds" : "second")}");
            }

            // Format the result and return it.
            return output.Count >= 3 ? output.ToSentence().Replace(" and", ", and") : output.ToSentence();
        }
		private void SendMessage(BasePlayer player, string key, params object[] args)
        {
            player.ChatMessage(string.Format(GetTranslation(key, player), args));
        }
		private string GetTranslation(string key, BasePlayer player = null)
        {
            return lang.GetMessage(key, this, player?.UserIDString);
        }
		protected override void LoadDefaultMessages()
        {
            // Register all messages used by the plugin in the Lang API.
            lang.RegisterMessages(new Dictionary<string, string>
            {
                // Messages used throughout the plugin.
              
                ["Cooldown"] = "You can't use the command yet! Remaining cooldown: {0}.",
               

                // Cooldown formatting 'translations'.
                ["day"] = "day",
                ["days"] = "days",
                ["hour"] = "hour",
                ["hours"] = "hours",
                ["minute"] = "minute",
                ["minutes"] = "minutes",
                ["second"] = "second",
                ["seconds"] = "seconds",
                ["and"] = "and"
            }, this);
        }
	#endregion
	
	

}
}