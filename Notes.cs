
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Oxide.Core.Libraries;
using Oxide.Game.Rust.Cui;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Core.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Data;
using UnityEngine;
using System.Linq;
using System.Reflection; 
using System.Text;
using Newtonsoft.Json;
using static UnityEngine.Camera;
using Newtonsoft.Json.Linq;
using System.Data;
using Network;
using Rust;
using Facepunch;
using Facepunch.Extend;

namespace Oxide.Plugins
{
    [Info("Notes", "Crushedice", 0.1)]
    [Description("Makes epic stuff happen")]
  class Notes : RustPlugin
    {
		

		protected override void LoadDefaultConfig()
		{
			if(Config["Effects", "Height"] == null) Config["Effects", "Height"] = "2.5";
            if(Config["Effects", "Height"].ToString() != "2.5") return;

			Puts("Default configuration created");
		}

		void Loaded () 
		{
		
	//permission.RegisterPermission("Blinkarrow.use", this);
	
		
		}

		 [ConsoleCommand("note")]
        private void ccmdJailss(ConsoleSystem.Arg arg)
        {
         
		 
		 if(arg.Args.Length != 2)
					{
						//SendReply(player,"Useage ; /Note <PlayerName> <Text>");
                        return;						
					}
					string NoteText;
					string PlayerName;
                    if (!TryConvert(arg.Args[0], out PlayerName))
                    {
                      //  SendReply(player,"Please enter a Playername");
                        return;
                    }
					if (!TryConvert(arg.Args[1], out NoteText))
                    {
                       // SendReply(player,"Enter a Text, use Quotes.");
                        return;
                    }
		
					var foundplayer = FindPlayer(PlayerName);
					var NoteItem = ItemManager.CreateByItemID(1414245162, 1, 0); 
					NoteItem.text = NoteText;
					foundplayer.GiveItem(NoteItem);
					//SendReply(player,"Note Sent.");
        }


		private static BasePlayer FindPlayer(string nameOrIdOrIp)
        {
            foreach (var activePlayer in BasePlayer.activePlayerList)
            {
                if (activePlayer.UserIDString == nameOrIdOrIp)
                    return activePlayer;
                if (activePlayer.displayName.Contains(nameOrIdOrIp))
                    return activePlayer;
                if (activePlayer.net?.connection != null && activePlayer.net.connection.ipaddress == nameOrIdOrIp)
                    return activePlayer;
            }
            return null;
        }
		
		 ////////////////////////////////////////
        ///     Converting
        ////////////////////////////////////////

        string ListToString<T>(List<T> list, int first, string seperator) => string.Join(seperator, (from item in list select item.ToString()).Skip(first).ToArray());

        bool TryConvert<S, C>(S source, out C converted)
        {
            try
            {
                converted = (C) Convert.ChangeType(source, typeof(C));
                return true;
            }
            catch(Exception)
            {
                converted = default(C);
                return false;
            }
        }
	
    }
}