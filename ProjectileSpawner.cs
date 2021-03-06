using System.Collections.Generic;
using System.Linq;
using System;

namespace Oxide.Plugins
{
    [Info("Projectile Spawner", "", "1.0.0", ResourceId = 0)]
    [Description("Somethin")]
    class ProjectileSpawner : RustPlugin
    {
		void Loaded() => permission.RegisterPermission("projectilespawner.use", this);
		
        void OnPlayerInput(BasePlayer player, InputState input)
        {
			if(!permission.UserHasPermission(player.UserIDString, "projectilespawner.use"))
				return;
			
            if (input.IsDown(BUTTON.FIRE_PRIMARY) && player.GetActiveItem()?.info.displayName.english == "Targeting Computer")
            {
                SpawnProjectile(player);
            }
			if (input.IsDown(BUTTON.FIRE_SECONDARY) && player.GetActiveItem()?.info.displayName.english == "Targeting Computer")
            {
                SpawnProjectile2(player);
            }
			if (input.IsDown(BUTTON.RELOAD) && player.GetActiveItem()?.info.displayName.english == "Targeting Computer")
            {
                SpawnProjectile3(player);
            }
        }

        void SpawnProjectile(BasePlayer player)
        {
            BaseEntity entity = GameManager.server.CreateEntity("assets/prefabs/npc/sam_site_turret/rocket_sam.prefab", new UnityEngine.Vector3(player.transform.position.x, player.transform.position.y + 3.5F, player.transform.position.z), new UnityEngine.Quaternion(), true);
			
            ServerProjectile serverProjectile = entity.GetComponent<ServerProjectile>();
			
            serverProjectile.gravityModifier = 0.01F;
            serverProjectile.speed = 500F;
			
            entity.SendMessage("InitializeVelocity", (object)(player.eyes.HeadForward() * 100f));
            entity.Spawn();
        }
		 void SpawnProjectile2(BasePlayer player)
        {
            BaseEntity entity = GameManager.server.CreateEntity("assets/prefabs/npc/m2bradley/maincannonshell.prefab", new UnityEngine.Vector3(player.transform.position.x, player.transform.position.y + 3.5F, player.transform.position.z), new UnityEngine.Quaternion(), true);
			
            ServerProjectile serverProjectile = entity.GetComponent<ServerProjectile>();
			
            serverProjectile.gravityModifier = 0.01F;
            serverProjectile.speed = 500F;
			
            entity.SendMessage("InitializeVelocity", (object)(player.eyes.HeadForward() * 100f));
            entity.Spawn();
        }
		void SpawnProjectile3(BasePlayer player)
        {
            BaseEntity entity = GameManager.server.CreateEntity("assets/prefabs/ammo/rocket/rocket_basic.prefab", new UnityEngine.Vector3(player.transform.position.x, player.transform.position.y + 3.5F, player.transform.position.z), new UnityEngine.Quaternion(), true);
			
            ServerProjectile serverProjectile = entity.GetComponent<ServerProjectile>();
			
            serverProjectile.gravityModifier = 0.01F;
            serverProjectile.speed = 500F;
			
            entity.SendMessage("InitializeVelocity", (object)(player.eyes.HeadForward() * 100f));
            entity.Spawn();
        }
    }
}
