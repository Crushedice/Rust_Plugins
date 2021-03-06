using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("StartProtection", "Norn / wazzzup", "2.0.2", ResourceId = 1342)]
    [Description("Give people some leeway when they first join the game.")]
    public class StartProtection : RustPlugin
    {
        [PluginReference]
        Plugin Friends;

        class StoredData
        {
            public Dictionary<ulong, ProtectionInfo> Players = new Dictionary<ulong, ProtectionInfo>();
            public StoredData()
            {
            }
        }
        class StoredPlayersData
        {
            public List<ulong> Players = new List<ulong>();
            public StoredPlayersData()
            {
            }
        }

        class ProtectionInfo
        {
            public ulong UserId;
            public int TimeLeft;
            public bool Multiple;
            public int InitTimestamp;
            public ProtectionInfo()
            {
            }
        }

        Timer ProtectionTimer;
        StoredData storedData;
        StoredPlayersData storedPlayersData;
        StoredData storedDataEx;
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static readonly double MaxUnixSeconds = (DateTime.MaxValue - UnixEpoch).TotalSeconds;


        #region Commands

        [ChatCommand("sp")]
        private void SPCommand(BasePlayer player, string command, string[] args)
        {
            if (Convert.ToBoolean(Config["bProtectionEnabled"]) == false && player.net.connection.authLevel != Convert.ToInt32(Config["iAuthLevel"]))
            {
                PrintToChatEx(player, GetMessage("tDisabled", player.UserIDString));
                return;
            }
            if (args.Length == 0 || args.Length > 2)
            {
                PrintToChatEx(player, "USAGE: /sp <time | end>");
                if (player.net.connection.authLevel >= Convert.ToInt32(Config["iAuthLevel"]))
                {
                    PrintToChatEx(player, "<color=yellow>ADMIN: /sp <toggle | togglesleep | cleardb | me></color>");
                }
            }
            else if (args[0] == "me")
            {
                if (player.net.connection.authLevel >= Convert.ToInt32(Config["iAuthLevel"]))
                {
                    if (storedPlayersData.Players.Contains(player.userID))
                    {
                        storedPlayersData.Players.Remove(player.userID);
                    }
                    OnPlayerFirstInit(player.userID);
                    ProtectionInfo p = null;
                    if (storedData.Players.TryGetValue(player.userID, out p))
                    {
                        string minutes = Convert.ToInt32(TimeSpan.FromSeconds(p.TimeLeft).TotalMinutes).ToString();
                        Puts("Start protection enabled for " + player.displayName + " [" + player.userID.ToString() + "] - Duration: " + minutes + " minutes.");
                        string parsed_config = GetMessage("tFirstSpawn", player.UserIDString);
                        parsed_config = parsed_config.Replace("{minutes_left}", minutes.ToString());
                        PrintToChatEx(player, parsed_config);
                    }
                    else { Puts("Failed..."); }

                }
                else
                {
                    PrintToChatEx(player, GetMessage("tNoAuthLevel", player.UserIDString));
                }
            }
            else if (args[0] == "cleardb")
            {
                if (player.net.connection.authLevel >= Convert.ToInt32(Config["iAuthLevel"]))
                {
                    storedData.Players.Clear();
                    storedPlayersData.Players.Clear();
                    PrintToChatEx(player, GetMessage("tDBCleared", player.UserIDString));
                    SaveData();
                }
                else
                {
                    PrintToChatEx(player, GetMessage("tNoAuthLevel", player.UserIDString));
                }
            }
            else if (args[0] == "togglesleep")
            {
                if (player.net.connection.authLevel >= Convert.ToInt32(Config["iAuthLevel"]))
                {
                    if (Convert.ToBoolean(Config["bSleeperProtection"]) == true)
                    {
                        PrintToChatEx(player, "Sleep Protection: <color=red>disabled</color>.");
                        Puts("Start Protection sleeper protection has been disabled by " + player.displayName + " (type /sp togglesleep to enable).");
                        Config["bSleeperProtection"] = false;
                        SaveConfig();
                    }
                    else
                    {
                        PrintToChatEx(player, "Sleep Protection: <color=green>enabled</color>.");
                        Puts("Start Protection sleeper protection has been enabled by " + player.displayName + " (type /sp togglesleep to disabled).");
                        Config["bSleeperProtection"] = true;
                        SaveConfig();
                    }
                }
                else
                {
                    PrintToChatEx(player, GetMessage("tNoAuthLevel", player.UserIDString));
                }
            }
            else if (args[0] == "toggle")
            {
                if (player.net.connection.authLevel >= Convert.ToInt32(Config["iAuthLevel"]))
                {
                    if (Convert.ToBoolean(Config["bProtectionEnabled"]) == true)
                    {
                        if (ProtectionTimer != null)
                        {
                            ProtectionTimer.Destroy();
                        }
                        PrintToChatEx(player, GetMessage("tDisabled", player.UserIDString));
                        Puts("Start Protection has been disabled by " + player.displayName + " (type /sp toggle to enable).");
                        Config["bProtectionEnabled"] = false;
                        SaveConfig();
                    }
                    else
                    {
                        int seconds = Convert.ToInt32(Config["iUpdateTimerInterval"]);
                        ProtectionTimer = timer.Repeat(seconds, 0, () => UpdateProtectedList());
                        PrintToChatEx(player, GetMessage("tEnabled", player.UserIDString));
                        int minutes = Convert.ToInt32(TimeSpan.FromSeconds(Convert.ToInt32(Config["iTime"])).TotalMinutes);
                        Puts("Start Protection has been enabled by " + player.displayName + " [Minutes: " + minutes.ToString() + "] (type /sp toggle to disable).");
                        Config["bProtectionEnabled"] = true;
                        SaveConfig();
                    }
                }
                else
                {
                    PrintToChatEx(player, GetMessage("tNoAuthLevel", player.UserIDString));
                }
            }
            else if (args[0] == "end")
            {
                ProtectionInfo p = null;
                if (storedData.Players.TryGetValue(player.userID, out p))
                {
                    PunishPlayer(player, Convert.ToInt32(Config["iTime"]) + 1, false);
                }
                else
                {

                    PrintToChatEx(player, GetMessage("tNoProtection", player.UserIDString));
                }
            }
            else if (args[0] == "time")
            {
                ProtectionInfo p = null;
                if (storedData.Players.TryGetValue(player.userID, out p))
                {
                    string minutes = Convert.ToInt32(TimeSpan.FromSeconds(p.TimeLeft).TotalMinutes).ToString();

                    string parsed_config = GetMessage("tSpawn", player.UserIDString);
                    parsed_config = parsed_config.Replace("{minutes_left}", minutes.ToString());
                    PrintToChatEx(player, parsed_config);
                }
                else
                {

                    PrintToChatEx(player, GetMessage("tNoProtection", player.UserIDString));
                }
            }
        }
        #endregion

        #region Oxide Hooks

        protected override void LoadDefaultConfig()
        {
            Puts("No configuration file found, generating...");
            Config.Clear();

            // --- [ GENERAL ] ---
            Config["bProtectionEnabled"] = true;
            Config["bSleeperProtection"] = true;
            Config["canLootFriends"] = true;
            Config["preventLooting"] = true;
            Config["preventRaiding"] = true;
            Config["iTime"] = 1800;
            Config["iPunishment"] = 300;
            Config["bHelicopterProtection"] = false;
            Config["iAuthLevel"] = 2;
            Config["iInactiveDays"] = 1;
            Config["iUpdateTimerInterval"] = 10;

            // --- [ MESSAGES ] ---
            SaveConfig();
        }

        private void OnServerInitialized()
        {
            if (Config["bSleeperProtection"] == null) { Puts("Resetting configuration file (out of date)..."); LoadDefaultConfig(); }
            if (Convert.ToBoolean(Config["bProtectionEnabled"]) == true)
            {
                RemoveOldUsers();
                int seconds = Convert.ToInt32(Config["iUpdateTimerInterval"]);
                ProtectionTimer = timer.Repeat(seconds, 0, () => UpdateProtectedList());
                string minutes = Convert.ToInt32(TimeSpan.FromSeconds(Convert.ToInt32(Config["iTime"])).TotalMinutes).ToString();
                Puts("Start Protection has been enabled [Minutes: " + minutes + "] (type /sp toggle to disable).");
            }
            else
            {
                Puts("Start Protection is not enabled (type /sp toggle to enable).");
            }
        }

        private void Loaded()
        {
            storedData = Interface.GetMod().DataFileSystem.ReadObject<StoredData>(this.Title);
            storedPlayersData = Interface.GetMod().DataFileSystem.ReadObject<StoredPlayersData>(this.Title+"Players");
            LoadDefaultMessages();
            ulong u = 0;
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                if (!permission.UserExists(player.userID.ToString()) || !storedPlayersData.Players.Contains(player.userID))
                {
                    OnPlayerFirstInit(player.userID);
                }
            }
        }

        void Unload()
        {
            Puts("Saving protection database...");
            if (ProtectionTimer != null)
            {
                ProtectionTimer.Destroy();
            }
            SaveData();
        }

        void OnServerSave() => SaveData();
        void OnServerShutdown() => SaveData();

        void OnPlayerFirstInit(ulong steamid, int timeleft = -1)
        {
            ProtectionInfo p = null;
            if (storedData.Players.TryGetValue(steamid, out p))
            {
                if (p.Multiple == false && p.TimeLeft == Convert.ToInt32(Config["iTime"]))
                {
                    Puts("Removing " + steamid + " from protection list, cleaning up...");
                    storedData.Players.Remove(steamid);
                    OnPlayerFirstInit(steamid,timeleft);
                }
            }
            else
            {
                var info = new ProtectionInfo();
                if (timeleft == -1) timeleft = Convert.ToInt32(Config["iTime"]);
                info.TimeLeft = timeleft;
                info.Multiple = false;
                info.InitTimestamp = UnixTimeStampUTC();// Timestamp
                info.UserId = steamid;
                storedData.Players.Add(steamid, info);
                Interface.GetMod().DataFileSystem.WriteObject(this.Title, storedData);
            }
        }

        void OnUserApprove(Network.Connection connection)
        {
            string userid = connection.userid.ToString();
            ulong u = 0;
            if (!permission.UserExists(userid) || !storedPlayersData.Players.Contains(connection.userid))
            {
                OnPlayerFirstInit(connection.userid);
            }
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            ProtectionInfo p = null;
            if (storedData.Players.TryGetValue(player.userID, out p))
            {
                if (!p.Multiple)
                {
                    string minutes = Convert.ToInt32(TimeSpan.FromSeconds(p.TimeLeft).TotalMinutes).ToString();
                    Puts("Start protection enabled for " + player.displayName + " [" + player.userID.ToString() + "] - Duration: " + minutes + " minutes.");
                    string parsed_config = GetMessage("tFirstSpawn", player.UserIDString);
                    parsed_config = parsed_config.Replace("{minutes_left}", minutes.ToString());
                    PrintToChatEx(player, parsed_config);
                    p.Multiple = true;
                }
                else
                {
                    string minutes = Convert.ToInt32(TimeSpan.FromSeconds(p.TimeLeft).TotalMinutes).ToString();
                    string parsed_config = GetMessage("tSpawn", player.UserIDString);
                    parsed_config = parsed_config.Replace("{minutes_left}", minutes.ToString());
                    PrintToChatEx(player, parsed_config);
                }
            }
        }

        private HitInfo OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (Convert.ToBoolean(Config["bProtectionEnabled"]) == true)
            {
                if (entity is BasePlayer)
                {
                    var player = entity as BasePlayer;

                    ProtectionInfo p = null;
                    ProtectionInfo z = null;
                    if (hitInfo.Initiator is BasePlayer)
                    {
                        var attacker = hitInfo.Initiator as BasePlayer;
                        if (storedData.Players.TryGetValue(player.userID, out p))
                        {
                            if (storedData.Players.TryGetValue(attacker.userID, out z))
                            {
                                if (attacker.userID == player.userID)
                                {
                                    return null;
                                }
                                else
                                {
                                    PunishPlayer(attacker);
                                    Puts("Punishing " + attacker.displayName.ToString() + " for attempting to pvp.");
                                }
                            }
                            if (attacker.userID != player.userID)
                            {
                                if (player.IsSleeping())
                                {
                                    if (Convert.ToBoolean(Config["bSleeperProtection"]) == false)
                                    {
                                        storedData.Players.Remove(player.userID);
                                        storedPlayersData.Players.Add(player.userID);
                                        Puts("Removed " + player.displayName.ToString() + " (Sleeping) from the Start Protection list.");
                                        return null;
                                    }
                                }
                            }
                            PrintToChatEx(attacker, GetMessage("tAttackAttempt", attacker.UserIDString));
                            hitInfo.damageTypes.ScaleAll(0f);
                            return hitInfo;
                        }
                        else
                        {
                            if (storedData.Players.TryGetValue(attacker.userID, out p))
                            {
                                PunishPlayer(attacker);
                                Puts("Punishing " + attacker.displayName.ToString() + " for attempting to pvp.");
                                hitInfo.damageTypes.ScaleAll(0f);
                                return hitInfo;
                            }
                        }
                    }
                    else if (hitInfo.Initiator is BaseHelicopter)
                    {
                        if (Convert.ToBoolean(Config["bHelicopterProtection"]) == true)
                        {
                            if (player == null) { return null; }
                            if (storedData.Players.TryGetValue(player.userID, out z))
                            {
                                hitInfo.damageTypes.ScaleAll(0f);
                                return hitInfo;
                            }
                        }
                    }
                }
                else if(Convert.ToBoolean(Config["preventRaiding"]) == true && (entity is BuildingBlock || entity is Door || (entity.PrefabName?.Contains("building") ?? false) || (entity.PrefabName?.Contains("deployable") ?? false)))
                {
                    if (hitInfo.Initiator is BasePlayer && entity.OwnerID!=0 && entity.OwnerID!=(hitInfo.Initiator as BasePlayer).userID)
                    {
                        ProtectionInfo p = null;
                        var attacker = hitInfo.Initiator as BasePlayer;

                        if ((entity as BaseEntity).OwnerID!=attacker.userID) {
                            if (storedData.Players.TryGetValue(attacker.userID, out p))
                            {
                                PunishPlayer(attacker);
                                Puts("Punishing " + attacker.displayName.ToString() + " for attempting to blow.");
                                hitInfo.damageTypes.ScaleAll(0f);
                                return hitInfo;
                            }
                        }
                    }
                }
                else if(Convert.ToBoolean(Config["preventRaiding"]) == true && entity is LootableCorpse)
                {
                    if (hitInfo.Initiator is BasePlayer)
                    {
                        ProtectionInfo p = null;
                        var attacker = hitInfo.Initiator as BasePlayer;

                        if ((entity as LootableCorpse).playerSteamID!=attacker.userID) {
                            if (storedData.Players.TryGetValue(attacker.userID, out p))
                            {
                                PunishPlayer(attacker);
                                Puts("Punishing " + attacker.displayName.ToString() + " for attempting to corpse.");
                                hitInfo.damageTypes.ScaleAll(0f);
                                return hitInfo;
                            }
                        }
                    }

                }
            }
            return null;
        }

        void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (Convert.ToBoolean(Config["bProtectionEnabled"]) == true && Convert.ToBoolean(Config["preventLooting"]) == true)
            {
                ProtectionInfo p = null;
                var hasProtection = storedData.Players.TryGetValue(player.userID, out p);
                if (!hasProtection) return;

                var corpse = entity as LootableCorpse;
                var sleeper = entity as BasePlayer;
                string minutes = Convert.ToInt32(TimeSpan.FromSeconds(p.TimeLeft).TotalMinutes).ToString();
                string parsed_config = GetMessage("tSpawn", player.UserIDString);
                parsed_config = parsed_config.Replace("{minutes_left}", minutes.ToString());
                
                if (corpse != null && corpse.playerSteamID!=player.userID)
                {
                    PrintToChatEx(player, parsed_config);
                    timer.Once(0.01f, player.EndLooting);
                }
                else if (sleeper != null && Convert.ToBoolean(Config["canLootFriends"]) != true && !(bool) (Friends?.CallHook("HasFriend", sleeper.userID,player.userID) ?? false))
                {
                    PrintToChatEx(player, parsed_config);
                    timer.Once(0.01f, player.EndLooting);
                }
                else if (entity.PrefabName.Contains("heli_crate") || (entity.PrefabName.Contains("deployable") && entity.OwnerID!=0 && entity.OwnerID!=player.userID && !(bool) (Friends?.CallHook("HasFriend", entity.OwnerID,player.userID) ?? false)))
                {
                    PrintToChatEx(player, parsed_config);
                    timer.Once(0.01f, player.EndLooting);
                }
            }
        }

        #endregion

        #region Helpers

        private void PrintToChatEx(BasePlayer player, string result, string tcolour = "orange")
        {
            PrintToChat(player, "<color=\"" + tcolour + "\">[" + GetMessage("title", player.UserIDString) + "]</color> " + result);
        }

        private void UpdateProtectedListEx(BasePlayer player)
        {
            if (player != null)
            {
                ProtectionInfo p = null;
                if (storedData.Players.TryGetValue(player.userID, out p))
                {
                    if (p.TimeLeft >= 1 && p.TimeLeft <= Convert.ToInt32(Config["iTime"]))
                    {
                        p.TimeLeft = p.TimeLeft - Convert.ToInt32(Config["iUpdateTimerInterval"]);
                    }
                    else
                    {
                        storedData.Players.Remove(player.userID);
                        storedPlayersData.Players.Add(player.userID);
                        PrintToChatEx(player, GetMessage("tProtectionEnded", player.UserIDString));
                    }
                }
            }
        }

        private void UpdateProtectedList()
        {
            foreach (BasePlayer player in BasePlayer.activePlayerList)
            {
                UpdateProtectedListEx(player);
            }
        }

        public Int32 UnixTimeStampUTC()
        {
            Int32 unixTimeStamp;
            DateTime currentTime = DateTime.Now;
            DateTime zuluTime = currentTime.ToUniversalTime();
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            unixTimeStamp = (Int32)(zuluTime.Subtract(unixEpoch)).TotalSeconds;
            return unixTimeStamp;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return unixTimeStamp > MaxUnixSeconds
               ? UnixEpoch.AddMilliseconds(unixTimeStamp)
               : UnixEpoch.AddSeconds(unixTimeStamp);
        }

        private void RemoveOldUsers()
        {
            int removed = 0;
            new List<ulong>(storedData.Players.Keys).ForEach(u =>
            {
                ulong steamid = u; ProtectionInfo item = null;
                if (storedData.Players.TryGetValue(steamid, out item))
                {
                    if (item.InitTimestamp == 0)
                    {
                        storedData.Players.Remove(steamid);
                        storedPlayersData.Players.Add(steamid);
                        removed++;
                    }
                    else
                    {
                        DateTime compareDate = UnixTimeStampToDateTime(item.InitTimestamp);
                        var days = (compareDate - DateTime.Now).Days;
                        if (days >= Convert.ToDouble(Config["iInactiveDays"]))
                        {
                            storedData.Players.Remove(steamid);
                            storedPlayersData.Players.Add(steamid);
                            removed++;
                        }
                    }
                }
            });
            if (removed >= 1)
            {
                Puts("Removing " + removed.ToString() + " old entries from the protection list.");
                SaveData();
            }
            else
            {
                Puts("Entry list up to date.");
            }
        }


        private void PunishPlayer(BasePlayer player, int new_time = -1, bool message = true)
        {
            ProtectionInfo p = null;
            if (storedData.Players.TryGetValue(player.userID, out p))
            {
                int punish = 0;
                if (new_time != -1)
                {
                    punish = new_time;
                }
                else
                {
                    punish = Convert.ToInt32(Config["iPunishment"]);
                }
                p.TimeLeft = p.TimeLeft - punish;
                if (p.TimeLeft <= 0) { UpdateProtectedListEx(player); }
                if (message)
                {
                    string minutes = Convert.ToInt32(TimeSpan.FromSeconds(p.TimeLeft).TotalMinutes).ToString();
                    string punishment = Convert.ToInt32(TimeSpan.FromSeconds(punish).TotalMinutes).ToString();
                    string parsed_config = GetMessage("tPunishment", player.UserIDString);
                    parsed_config = parsed_config.Replace("{minutes_revoked}", punishment.ToString());
                    parsed_config = parsed_config.Replace("{minutes_left}", minutes.ToString());
                    PrintToChatEx(player, parsed_config);
                }
            }
        }

        void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(this.Title, storedData);
            Interface.Oxide.DataFileSystem.WriteObject(this.Title+"Players", storedPlayersData);
        }

        #endregion

        #region Localization

        void LoadDefaultMessages()
        {
            var messages = new Dictionary<string, string>
            {
                {"title", "StartProtection"},
                {"tPunishment", "<color=#FF3300>You have been punished for attempting to PVP with</color> Start Protection <color=#99FF66>Enabled!</color>\n\n{minutes_revoked} minutes revoked.\n\nYou now have <color=#FF3300>{minutes_left}</color> minutes left before your Start Protection is disabled."},
                {"tFirstSpawn", "Start protection <color=#66FF66>enabled</color> for <color=#66FF66>{minutes_left}</color> minutes, during this time you <color=#FF3300>will not be able to pvp</color> on any level.\n\nYou can check how much time you have left by typing <color=#66FF66>/sp time</color>\n\n<color=#FF3300>Do not</color> squander this time." },
                {"tSpawn", "You have <color=#FF3300>{minutes_left}</color> minutes left before your Start Protection is disabled."},
                {"tProtectionEnded", "Start protection <color=#FF3300>disabled</color>, you are now on your own."},
                {"tNoProtection", "Start protection status is currently <color=#FF3300>disabled</color>."},
                {"tAttackAttempt","The player you are trying to attack has Start Protection enabled and <color=#FF3300>cannot</color> be damaged."},
                {"tDisabled", "Start Protection is currently <color=#FF3300>disabled</color> server-wide."},
                {"tEnabled", "Start Protection has been <color=#66FF66>enabled</color>, new players will now be protected upon spawning."},
                {"tNoAuthLevel", "You <color=#FF3300>do not</color> have access to this command."},
                {"tDBCleared", "You have <color=#FF3300>cleared</color> the Start Protection database."},
            };
            lang.RegisterMessages(messages, this);
        }
        string GetMessage(string key, string steamId = null) => lang.GetMessage(key, this, steamId);

        #endregion
    }
}