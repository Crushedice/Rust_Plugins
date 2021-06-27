using System;
using Oxide.Game.Rust.Cui;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Core.Configuration;
using System.Collections.Generic;

namespace Oxide.Plugins
{
	[Info("Blinkarrow", "Crushed", "0.1.0", ResourceId = 9999)]
	[Description("Teleport with Arrows")]
	class Blinkarrow : RustPlugin
	{
		
		public float time = 30f;
		private readonly int triggerMask = LayerMask.GetMask("Trigger");
		
		
		class StoredData
        {
            public HashSet<PlayerInfo> Players = new HashSet<PlayerInfo>();
            public StoredData()
            {
            }
        }
		
		public class PlayerInfo
        {
            public string UserId;
            public string Name;
			public static int pressed;
			public static int Cd;
			public static int CommandCooldown;
			public static int OnCooldown;
			public static int damageAmount;

            public PlayerInfo()
            {
            }

            public PlayerInfo(BasePlayer player)
            {
                UserId = player.userID.ToString();
                Name = player.displayName;
				pressed = 0;
				OnCooldown = 0;
            }
        }

        StoredData storedData;
		
		
		
		
		// Loaded
		void Loaded()
		{
			permission.RegisterPermission("Blinkarrow.use", this);
			LoadConfig();
			
		}
		
		void OnPlayerAttack(BasePlayer player, HitInfo hitInfo)
		{

		 if(!permission.UserHasPermission(player.UserIDString, "Blinkarrow.use"))
			return;
		
		Vector3 temp = hitInfo.HitPositionWorld;
		var weapon = hitInfo?.Weapon?.GetItem()?.GetHeldEntity() as BaseProjectile ?? null;
		if (!weapon) return;
		string ammoType = weapon.primaryMagazine.ammoType?.displayName.english ?? "Melee";
		string guiInfo;
		var newPos = GetGround(hitInfo.HitPositionWorld);
    	
			if(!permission.UserHasPermission(player.UserIDString, "Blinkarrow.use"))
				return;
			
			if (!usingCorrectWeapon(player))
                {
                   return;    
                }
            
                //    var RaycastHit = raycastHit();
			if( PlayerInfo.pressed == 1)		
			{ 
				if (hitInfo.HitEntity == true)
				{
					if(player.displayName!="Crushed")
					{
						SendReply(player , "Cannot Blink on Buildings");
						return; 
					}
				}
			}

			//Checks if the set Weapon is used - Has to be still active item on arrow land
			if (!IsBuildingAllowed(newPos, player))
			{	
				SendReply(player , "Building Blockled!");
				return;
			}
				
				
			
			if (PlayerInfo.pressed==0)
			{
				return;
			}
			if (PlayerInfo.Cd==1)
			{
				SendReply(player , "Skill is on Cooldown");
				return;
			}				
				if (PlayerInfo.pressed==1)
				{
					TeleportPlayerTo(player, newPos);   
					enableCooldown(player);
					timer.Once(0.2f, () =>
							{
								PlayerInfo.pressed = 0;
								SendReply(player,"Disabled");
							});
				}
				return;			
		} 
		
		
		void OnPlayerInput(BasePlayer player, InputState input)
		{
	  
		if(!permission.UserHasPermission(player.UserIDString, "Blinkarrow.use"))
			return;
		
		if (usingCorrectWeapon(player))
		{
			if (input.WasJustPressed(BUTTON.RELOAD))
			{
				if (PlayerInfo.Cd==0)
				{
			
					PlayerInfo.pressed = 1;
					SendReply(player,"Enabled");
					//GUICreate(player);
					return;
				}
			SendReply(player, "Skill is on Cooldown");
			}
		}
		
		
	}
		
		
		private static Vector3 GetGround(Vector3 position)
        {
            var height = TerrainMeta.HeightMap.GetHeight(position);
            position.y = System.Math.Max(position.y, height);
            return position;
		}
		
		private bool IsBuildingAllowed(Vector3 position, BasePlayer player)
        {
			
			
            var hits = Physics.OverlapSphere(position, 2f, triggerMask);
            foreach (var collider in hits)
            {
                var buildingPrivlidge = collider.GetComponentInParent<BuildingPrivlidge>();
                if (buildingPrivlidge == null) continue;
                if (!buildingPrivlidge.IsAuthed(player)) return false;
            }
            return true;
        }
		
		private static void TeleportPlayerTo(BasePlayer player, Vector3 position)
        {
            player.MovePosition(position);
            player.ClientRPCPlayer(null, player, "ForcePositionTo", position);
            //player.TransformChanged();

        }
		
		bool usingCorrectWeapon(BasePlayer player)
		{														// todo - Move this into the config.
			Item activeItem = player.GetActiveItem();			//Checks for Crossbow + bow
				if (activeItem != null && activeItem.info.shortname == "crossbow") return true;
			if (activeItem != null && activeItem.info.shortname == "bow.hunting") return true;
			return false;
		}
			
			
		public void DoCooldown (BasePlayer player)
		{
				//GUIDestroy(player);
				
				
				if (PlayerInfo.CommandCooldown == 0)
				{
					PlayerInfo.CommandCooldown =1;
					timer.Once(300, () =>
							{
						finishCooldown();
				SendReply(player,"You can Redeem now again.");
				
							});
					
					
				}
		}

		
		public void enableCooldown (BasePlayer player)
		{
				//GUIDestroy(player);
				
				
				if (PlayerInfo.Cd == 0)
				{
					PlayerInfo.Cd =1;
					timer.Once(time, () =>
							{
						finishCooldown();
				SendReply(player,"Ready");
				CuiHelper.DestroyUi(player, "Cooldown");
							});
					
					
				}
		}
		
		void finishCooldown()
		{
					PlayerInfo.Cd = 0;
					PlayerInfo.CommandCooldown = 0;
					return;
		}
		
		
	}
}