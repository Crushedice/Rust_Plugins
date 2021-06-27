using Oxide.Core;
using Oxide.Core.Libraries.Covalence;

using System.Linq;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("SteamReport", "", "2.0.1", ResourceId = 0)]
    [Description("Sends in-game reports to admins via Steam.")]
    class SteamReport : CovalencePlugin
    {
        #region Config

        List<string> admins;
        string requestUrl;
        string reportCommand;

        protected override void LoadDefaultConfig()
        {
            Config["Admins"] = new List<string>
            {
                "76561198103592543"
            };
            Config["RequestUrl"] = "http://RestpiServer.net/report";
            Config["ReportCommand"] = "report";
        }

        #endregion

        #region Lang

        void InitLang()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Syntax"] = "Syntax: /report [name|id] [message]",
                ["PlayersNone"] = "No players were found.",
                ["PlayersMultiple"] = "Multiple players were found.",
                ["Fail"] = "Report failed to send.",
                ["Sent"] = "Report sent.",
            }, this);
        }

        string _(string key, string userId) => lang.GetMessage(key, this, userId);

        #endregion

        #region Hooks

        void Init()
        {
            InitLang();

            admins = Config.Get<List<string>>("Admins");
            requestUrl = Config.Get<string>("RequestUrl");
            reportCommand = Config.Get<string>("ReportCommand");

            foreach (var id in admins)
                if (!id.IsSteamId())
                    Puts($"{id} is not a valid SteamID64.");

            AddCovalenceCommand(reportCommand, "SendReport", "steamreport.use");
        }

        #endregion

        #region Command

        void SendReport(IPlayer player, string command, string[] args)
        {
            if (args == null || args.Length <= 1)
            {
                player.Reply(_("Syntax", player.Id));
                return;
            }

            var found = players.FindPlayers(args[0]).Where(p => p.IsConnected);

            if (!found.Any())
            {
                player.Reply(_("PlayersNone", player.Id));
                return;
            }

            if (found.Count() > 1)
            {
                player.Reply(_("PlayersMultiple", player.Id));
                return;
            }

            var target = found.First();
            var message = string.Empty;

            for (var i = 1; i < args.Length; i++)
                message += args[i] + (i == args.Length ? string.Empty : " ");

            var request = string.Format("{0}?adminList={1}&reporterName={2}&reporterId={3}&reporterPos={4}&reporteeName={5}&reporteeId={6}&reporteePos={7}&reportMessage={8}",
                requestUrl, string.Join("|", admins.ToArray()), player.Name, player.Id, player.Position().ToString(), target.Name, target.Id, target.Position().ToString(), message);

            


            webrequest.EnqueueGet(request, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    player.Reply(_("Fail", player.Id));
                    return;
                }

                player.Reply(_("Sent", player.Id));
            }, this);
			// -- Added for my own Backend , staff Server ticket storage
			//SendNewReport(0,"Report",ServerType,message,player.Object as BasePlayer,target.Object as BasePlayer);
        }

        #endregion
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
            "http://145.239.130.132:8853",type,server,Subject,this,msgg,pName,pId,tName,tId);
         


            webrequest.EnqueueGet(request, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    Puts($"Fail Code = {code.ToString()} -- {response}");
                    return;
                }
            }, this);
        }

    }
}
