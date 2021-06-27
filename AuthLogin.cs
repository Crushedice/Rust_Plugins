using System.Collections.Generic;
using Oxide.Core.Libraries;
using Oxide.Core;
using Oxide.Core.Configuration;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using System.Data;
using Network;
using UnityEngine;
using System.Linq;
using Oxide.Core.Database;
using System.Runtime.InteropServices;
using Oxide.Core.MySql.Libraries;
using System.Text;
using Oxide.Core.SQLite.Libraries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using Rust;

namespace Oxide.Plugins
{
	[Info("AuthLogin", "Crushed", "0.1.0", ResourceId = 0)]
	[Description("Safety First")]

	class AuthLogin : RustPlugin
	{


		Dictionary<ulong, string> _playerData = new Dictionary<ulong, string>();
		List<ulong> CurrIds = new List<ulong>();
		List<string> LauncherAuth = new List<string>();

	
		private uint appId = 252490;
        private string apiKey = "00000";
	

		#region Classes

		private class MsgEntry
		{
			//[JsonProperty("Top Status")]
			public string TopString;
			//[JsonProperty("Bottom Status")]
			public string BottomString;
		}

		public Dictionary<ulong, AuthPlayer> APlayers = new Dictionary<ulong, AuthPlayer>();

		public class AuthPlayer
		{

			//public uint authlevel;
			//public string ipadres;
			public ulong UserId;
			//public string Name;
			public bool Owned;
			public bool HasProfile;
			public bool Banned;

			public AuthPlayer()
			{
			}

			public AuthPlayer(Network.Connection connection)
			{
				//authlevel = connection.authLevel;
				//ipadres = connection.ipaddress;
				UserId = connection.userid;
				//Name = connection.username;
				Owned = false;
				HasProfile = false;
				Banned = false;
			}
		}


		#endregion

		// Execute query
        void Loaded()
		{
            
			webrequest.Enqueue("--.json File with all ADmins+ their curr IP --", null, (code, response) =>
			{

				//Interface.Oxide.DataFileSystem.WriteObject($"{Name}Data", response);
				_playerData = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(response);

			}, this);

		}

		void OnPlayerInit(BasePlayer player)
		{
			DisplayMessage(player.Connection, new MsgEntry { TopString = "<color=#4D66FF>Almost Done..</color>", BottomString = "<color=#F0FF00>Running Last Checks...</color>" });
			//ulong id = player.userID;
			//var storedauthplayer = APlayers[id];		
			//CheckBannedHW(id);
			//for the Bancheck
			
		}

		private static void DisplayMessage(Network.Connection con, MsgEntry msgEntry)
		{
			if (!Net.sv.write.Start())
				return;
			Net.sv.write.PacketID(Message.Type.Message);
			Net.sv.write.String(msgEntry.TopString);
			Net.sv.write.String(msgEntry.BottomString);
			Net.sv.write.Send(new SendInfo(con));
		}

		object CanClientLogin(Network.Connection conn)
		{
			//printblue("Object CanClientLogin");
			//PlayerData
			uint auth = conn.authLevel;
			string ip = conn.ipaddress;
			ulong id = conn.userid;
			string name = conn.username;
			ulong PlayerID = id;
			var Newauthplayer = new AuthPlayer(conn);
			var reporttext = "[AuthLogin] " + name + "(" + id + ") " + ip;
			int byteLength = Buffer.ByteLength(conn.token);
			int appID = 0;
			var texxt = "Kicked Emulated Player, Name: " + name + " , from IP: " + ip + " , trying to Emulate : " + PlayerID;
			Tokendebug(conn);

			if (!APlayers.ContainsKey(id))
			{

				APlayers.Add(id, Newauthplayer);

			}
			


			// AuthLevel 2 Factor Authenticator Check
			if (auth == 1 || auth == 2 || auth == 3)
			{
				DisplayMessage(conn, new MsgEntry { TopString = "<color=#4D66FF>Attempting to aquire Authlevel..</color>", BottomString = "<color=#F0FF00>Just a second.. Checking...</color>" });



				webrequest.Enqueue("--.json File with all ADmins+ their curr IP --", null, (code, response) =>
				{

					//Interface.Oxide.DataFileSystem.WriteObject($"{Name}Data", response);
					_playerData = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(response);

				}, this);

				foreach (var playerid in _playerData.Keys)
				{
					CurrIds.Add(playerid);
				}

				if (CurrIds.Contains(id))
				{
					string CurrIP = _playerData[id];

					if (ip.Substring(0, ip.LastIndexOf(":")) == CurrIP)
					{

						APlayers[PlayerID].HasProfile = true;
						APlayers[PlayerID].Owned = true;
						DisplayMessage(conn, new MsgEntry { TopString = "<color=#4D66FF>AuthLevel Granted</color>", BottomString = "<color=#F0FF00>Skipping all other Checks...</color>" });
						CurrIds.Clear();
						return null;
					}

					Puts("WARNING------" + auth.ToString() + "-----------" + ip);

					rust.RunServerCommand($"SendingReport {reporttext}");
					Net.sv.Kick(conn, "No");
					return "no.";
				}

				Puts("Warning------------" + ip);
				//var reporttext = "Warning-AuthLogin- on "+name + "--" + ip ;
				rust.RunServerCommand($"SendingReport {reporttext}");
				SendNewReport(3,"Auth Login",ServerType,texxt);
				//Net.sv.Kick(conn, "No");
				return "no";
			}
			
			if (appID != 480)
			{

				if (appID != 252490)
				{
					//	Puts(texxt);
					//rust.RunServerCommand($"SendingReport {texxt}");
					//	return "no! ";
					return null;
				}

			}

			if (byteLength != 240)
			{
				if (byteLength != 234)
				{
					Puts(texxt);
					rust.RunServerCommand($"SendingReport {texxt}");
					SendNewReport(3,"Emulation",ServerType,texxt);
					Net.sv.Kick(conn, "No");
					return "no! ";

				}

			}

			CurrIds.Clear();
           
			return null;
		}


		void Tokendebug(Network.Connection conn)
		{

			uint auth = conn.authLevel;
			string ip = conn.ipaddress;
			ulong id = conn.userid;
			string name = conn.username;
			ulong PlayerID = id;
			var guuid = conn.guid;
			string Os = conn.os;
			var ownid = conn.ownerid;
			bool encInc = conn.decryptIncoming;
			bool encOut = conn.encryptOutgoing;
			var tkn = conn.token;
			int appID = 0; 
			var enclvl = conn.encryptionLevel;
			int byteLength = Buffer.ByteLength(conn.token);
			var proto = conn.protocol;
			string status = conn.authStatus;

			var logtext =

			"Time: " + DateTime.Now + "\n" +
			"Name: " + name + "\n" +
			"IP: " + ip + "\n" +
			"ID: " + id + "\n" +
			"AppID :" + appID + "\n" +
			"Authlevel: " + auth + "\n" +
			"GUUID: " + guuid + "\n" +
			"OS: " + Os + "\n" +
			"OwnerID: " + ownid + "\n" +
			"Protocol: " + proto + "\n" +
			"AuthStatus: " + status + "\n" +
			"Decr.Incoming: " + encInc + "\n" +
			"Encr.Outgoing: " + encOut + "\n" +
			"EncryptionLevel: " + enclvl + "\n" +
			"RawToken: " + tkn + "\n" +
			"Length: " + byteLength + "\n" +
			"TokenString: " + BitConverter.ToString(tkn, 0) + "\n \n";

			LogToFile("authlogins.txt", logtext, this);

		}


        private int ServerType = 1; // Server Enum - 0=Modded 1=Vanilla 2=Training
		private void SendNewReport(int typ , string subj,int srv , string msg, BasePlayer sender =null , BasePlayer target = null)
        {
			int type = typ ; 		//TicketType --   Report = 0, Bug = 1, Support = 2, Violation = 3, Generic = 4
			string Subject = subj; 	// ex. Norecoil... flyhack...etc	
			int server= srv; 		//ServerEnum 0-mod , 1 - vanilla , 2-training	string pName ="";
			string msgg = msg; 		//the actual message	string tName = "";
		
		
			string pName ="None";
			string tName = "None";
			string pId = "0";
			string tId = "0";
			if(sender!=null)
			{
				pName = sender.displayName;
				pId=sender.UserIDString;
				
			}
			if(target!=null)
			{
				tName = target.displayName;
				tId=target.UserIDString;
			
			}

            
            var request = string.Format("{0}?Type={1}&Server={2}&Subject={3}&Plugin={4}&message={5}&sendername={6}&senderid={7}&targetmame={8}&targetid={9}",
            "http://127.0.0.1:8853",type,0,Subject,this,msgg,pName,pId,tName,tId);
         


            webrequest.EnqueueGet(request, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    Puts($"Fail Code = {code.ToString()} -- {response}");
                    return;
                }
            }, this);
        }

		private T Deserialise<T>(string json) => JsonConvert.DeserializeObject<T>(json);

		private bool NoKey() => string.IsNullOrEmpty(apiKey);

    }
}