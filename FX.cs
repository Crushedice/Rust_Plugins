using System;
using Oxide.Game.Rust.Cui;
using Oxide.Core;
using Oxide.Core.Plugins;
using Facepunch.Extend;
using UnityEngine;
using Oxide.Core.Configuration;
using System.Collections.Generic;
using WebSocketSharp;

namespace Oxide.Plugins
{
	[Info("FX", "Crushed", "0.1.0", ResourceId = 9999)]
	[Description("Teleport with Arrows")]
	class FX : RustPlugin
	{
		
		public float time = 30f;
		private readonly int triggerMask = LayerMask.GetMask("Trigger");
		Rust.DamageType explodeDamage = Rust.DamageType.Explosion;
		Rust.DamageType flameDamage = Rust.DamageType.Heat;
		Rust.DamageType elecDamage = Rust.DamageType.ElectricShock;
		
		// Loaded
		void Loaded()
		{
			permission.RegisterPermission("Blinkarrow.use", this);
			LoadConfig();
		}
		
		void OnPlayerAttack(BasePlayer player, HitInfo hitInfo)
		{


			var fxx = "assets/bundled/prefabs/fx/gas_explosion_small.prefab";
			Vector3 temp = hitInfo.HitPositionWorld;
			Effect.server.Run((fxx), temp);
			return;
			//{-------------------------------------
			//var hotTime = 5f;
			//var healAmount = 15f;
			//	var target = hitInfo.HitEntity as BasePlayer;
			//		//var entity = temp.GetEntity();
			//		//BasePlayer target = entity;
			//			if(!target)return;
			//			//target.health = target.health - healAmount;
			//			target.metabolism.ApplyChange(MetabolismAttribute.Type.Bleeding, 50f, hotTime);
			//			target.metabolism.ApplyChange(MetabolismAttribute.Type.Poison, 50f, hotTime);
			//			target.metabolism.ApplyChange(MetabolismAttribute.Type.HealthOverTime, 50f, hotTime);
			//			var hy = target.metabolism.hydration.value;
			//			var cal = target.metabolism.calories.value;
			//			target.metabolism.hydration.value = hy - 50;
			//			target.metabolism.calories.value = cal - 50;
			//}-----------------------------------------




			float height 			= 50.0f;
				float heightspacing 	= 2.0f;
				float width 			= 3.0f;
				float widthspacing 		= 1.0f;

			//	List<Vector3> Circle = GetCircumferencePositions(player.transform.position,3f,14f,0f);
			List<Vector3> Circle = GetCircumferencePositions(temp, 5f, 25f, 0f);
			//------    First Float : Height |  Second Float : Spacing Between Effects
			foreach (var ve in Circle)
			{
				for(float i = 0F ; i <= height.ToString().ToFloat() ; i = i + heightspacing.ToString().ToFloat())
				{
                    var Exit = new Vector3(Convert.ToInt32(ve.x) , Convert.ToInt32(ve.y) +i , Convert.ToInt32(ve.z) );						
				
					for(float e = 0F ; e <= width.ToString().ToFloat() ; e = e + widthspacing.ToString().ToFloat())
					{
						var Exit2 = new Vector3(Convert.ToInt32(Exit.x) +e , Convert.ToInt32(Exit.y)  , Convert.ToInt32(Exit.z) );						
				
						for(float t = 0F ; t <= width.ToString().ToFloat() ; t = t + widthspacing.ToString().ToFloat())
						{
							var Exit3 = new Vector3(Convert.ToInt32(Exit2.x) , Convert.ToInt32(Exit2.y)  , Convert.ToInt32(Exit2.z) +t );	
							//Effect.server.Run(("assets/prefabs/locks/keypad/effects/lock.code.shock.prefab"),Exit3);		
							//Effect.server.Run(("assets/prefabs/tools/c4/effects/c4_explosion.prefab"),  Exit3);
							//Effect.server.Run(("assets/prefabs/npc/m2bradley/effects/coaxmgmuzzle.prefab"),  Exit3);
							//Effect.server.Run(("assets/prefabs/npc/patrol helicopter/effects/heli_explosion.prefab"), Exit);
							//Effect.server.Run(("assets/prefabs/npc/m2bradley/effects/maincannonshell_explosion.prefab"), Exit3);
							Effect.server.Run(("assets/bundled/prefabs/fx/fire/fire_v3.prefab"), Exit3);
							//dealDamage(Exit3,9999,250,explodeDamage );
						}
					}
				}
				
			}

	} 
		
		void OnWeaponFired(BaseProjectile projectile, BasePlayer player, ItemModProjectile mod, ProtoBuf.ProjectileShoot projectiles)
			{
					
				
			
						BaseEntity weaponEntity = player.GetHeldEntity();
						Effect.server.Run(("assets/prefabs/npc/m2bradley/effects/coaxmgmuzzle.prefab"), weaponEntity, StringPool.closest, Vector3.zero, Vector3.zero);
						Effect.server.Run(("assets/prefabs/npc/m2bradley/effects/maincannonattack.prefab"), weaponEntity, StringPool.closest, Vector3.zero, Vector3.zero);
						//Effect.server.Run(("assets/prefabs/npc/m2bradley/effects/tread_smoke.prefab"), weaponEntity, StringPool.closest, Vector3.zero, Vector3.zero);
					
				
			
			}
			

		void dealDamage(Vector3 deathPos, float damage, float radius, Rust.DamageType type)
        {
            List<BaseCombatEntity> entitiesClose = new List<BaseCombatEntity>();
            List<BaseCombatEntity> entitiesNear = new List<BaseCombatEntity>();
            List<BaseCombatEntity> entitiesFar = new List<BaseCombatEntity>();
            Vis.Entities(deathPos, radius / 3, entitiesClose);
            Vis.Entities(deathPos, radius / 2, entitiesNear);
            Vis.Entities(deathPos, radius, entitiesFar);
    
            foreach (BaseCombatEntity entity in entitiesClose)
            {
                entity.Hurt(damage, type, null, true);
               // notifyPlayer(entity);
            }
    
            foreach (BaseCombatEntity entity in entitiesNear)
            {
                if (entitiesClose.Contains(entity)) return;
                entity.Hurt(damage / 2, type, null, true);
               // notifyPlayer(entity);
            }
    
            foreach (BaseCombatEntity entity in entitiesFar)
            {
                if (entitiesClose.Contains(entity) || entitiesNear.Contains(entity)) return;
                entity.Hurt(damage / 4, type, null, true);
                //notifyPlayer(entity);
            }
        }
		
		 List<Vector3> GetCircumferencePositions(Vector3 center, float radius, float next, float y) // as the name implies
        {
            var positions = new List<Vector3>();
            float degree = 0f;

            while (degree < 360)
            {
                float angle = (float)(2 * Math.PI / 360) * degree;
                float x = center.x + radius * (float)Math.Cos(angle);
                float z = center.z + radius * (float)Math.Sin(angle);
                var position = new Vector3(x, center.y, z);

                position.y = y == 0f ? TerrainMeta.HeightMap.GetHeight(position) : y;
                positions.Add(position);

                degree += next;
            }

            return positions;
        }
		
		
		Vector3 GetGround(Vector3 position)
        {
            var height = TerrainMeta.HeightMap.GetHeight(position);
            position.y = System.Math.Max(position.y, height);
            return position;
		}
		
			
		
		
		
	}
}