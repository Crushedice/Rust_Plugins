
using System;

using Oxide.Game.Rust.Cui;
using Oxide.Core;
using Oxide.Core.Plugins;
using Facepunch.Extend;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System.Linq;
using System.Globalization;
using Facepunch;
using Rust;
using System.Collections;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

using System.Linq;
using Oxide.Core.Configuration;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Oxide.Plugins
{
    [Info("EponasSong", "Crushed", "0.1.0", ResourceId = 9999)]
    [Description("")]
    class EponasSong : RustPlugin
    {

       [PluginReference] Plugin PathFinding;
    
        private readonly int groundLayer = LayerMask.GetMask("Terrain", "World");
        private const string PREFAB_RIDABLE_HORSE = "assets/rust.ai/nextai/testridablehorse.prefab";
        public List<RidableHorse> aliveHorses = new List<RidableHorse>();
        public List<ulong> playerwithhorse = new List<ulong>();
        private readonly Vector3 Down = Vector3.down;
        class StoredData
        {
            public Dictionary<HorsePlayer, ulong> EponaData = new Dictionary<HorsePlayer, ulong>();
            public StoredData()
            {
            }
        }



        public class HorsePlayer
        {
            public ulong UserId;
            public string Name;

            public HorsePlayer()
            {
            }

            public HorsePlayer(BasePlayer player)
            {
                UserId = player.userID;
                Name = player.displayName;

            }
        }

        StoredData storedData;



        // Loaded
        void Loaded()
        {
            var ridableHorse = GameManager.server.FindPrefab(PREFAB_RIDABLE_HORSE)?.GetComponent<RidableHorse>();

            foreach (RidableHorse sc in Resources.FindObjectsOfTypeAll<RidableHorse>())
            {
                aliveHorses.Add(sc);

                if(sc.OwnerID!=0)
                {
                    playerwithhorse.Add(sc.OwnerID);

                }


            }


        }



        [ChatCommand("epona")]
        void playSong(BasePlayer player, string cmd, string[] args)
        {
        
            RidableHorse actualHorse = null;
        
            foreach (var h in aliveHorses)
            {
                
                if (h.OwnerID == player.userID)
                {
                    actualHorse = h;
                    
                    break;
                }


            }

            if (actualHorse != null)
            {
                SendReply(player, $"Your Horse is comming to you.");
                sendhorse(player,actualHorse);
            }
            else
            {
                SendReply(player, "Cannot find your Horse.");
            }



        }


        void sendhorse(BasePlayer target,RidableHorse horse)
        {
            int distance = 15;
            
          //  horse.gameObject.SetActive(false);

            float x = UnityEngine.Random.Range(-distance, distance);
            var z = (float)System.Math.Sqrt(System.Math.Pow(distance, 2) - System.Math.Pow(x, 2));
            var destination = target.transform.position;
            destination.x = destination.x - x;
            destination.z = destination.z - z;
            horse.transform.position = GetGround(destination);


            timer.Once(1f, () =>
            {
               // horse.gameObject.SetActive(true);
                PathFinding?.Call("FindAndFollowPath", horse, horse.ServerPosition, target.transform.position);
            });
          

        }
        void Unloaded()
        {

        }
        private void StartLerping(BasePlayer dest , RidableHorse ent)
        {
           
             
           
            
        }
        private static Vector3 GetGround(Vector3 position)
        {
            var height = TerrainMeta.HeightMap.GetHeight(position);
            position.y = Math.Max(position.y, height);
            return position;
        }
        object OnHorseLead(BaseMountable animal, BasePlayer player)
            {

            var hhorse = animal.GetComponent<RidableHorse>();
            if(hhorse==null)return null;
            if (animal == null)
            {
                return null;
            }

            if(playerwithhorse.Contains(player.userID))
            {
          //  SendReply(player,"You already have a Horse");
            return null ;
            }

            if (animal.OwnerID == 0)
            {
                animal.OwnerID = player.userID;
                playerwithhorse.Add(player.userID);
                animal._name="DisplayName";
                SendReply(player, "This is now your Horse.");
               var b = hhorse.gameObject.AddComponent<AnimalBrain>();
              
                var n = hhorse.gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
                if (b != null && n != null)
               {
                    Puts("Brain + agent Exists.");
                    //b.InitializeAI();
                 
               }


                return false;
            }


            return null;
        }

        
        void OnEntityKill(BaseNetworkable entity)
        {
            
            if (!entity.GetComponent<RidableHorse>())
            {
                return;
            }
            var thehorse = entity.GetComponent<RidableHorse>();
           
            
                if(thehorse.OwnerID!=0)
                {
                    playerwithhorse.Remove(thehorse.OwnerID);
                    SendReply(BasePlayer.Find(thehorse.OwnerID.ToString()),"Your Horse just died :(");
                }

            

        }

        private void OnEntitySpawned(BaseNetworkable entity)
        {
            if (!entity.GetComponent<RidableHorse>())
            {
                return;
            }
            if (!aliveHorses.Contains(entity))
                aliveHorses.Add(entity as RidableHorse);

        }
       

     
    }
}
