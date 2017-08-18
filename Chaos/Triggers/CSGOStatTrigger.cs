using Chaos.Helpers;
using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Triggers
{
    class CSGOStatTrigger : BaseTrigger
    {
        public CSGOStatTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> RespondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            bool result = await Respond(roomID, chatterId, message);
            return result;
        }

        private async Task<bool> Respond(ulong toID, ulong fromID, string message)
        {
            string[] query = StripCommand(message, Options.ChatCommandAPI.ChatCommand.Command);
            if (query != null && query.Length > 1)
            {
                CSGOStatResponse response = await SteamAPI.Request<CSGOStatResponse>("ISteamUserStats", "GetUserStatsForGame", "v0002", "GET", Options.ChatCommandAPI.APIKey, "&steamid=" + query[1] + "&appid=730");
                if(response == null)
                {
                    await SendMessageAfterDelay(toID, "There was an error retrieving stats for this user. Please check http://steamstat.us for Steam status.");
                    return true;
                }
                Stat[] stats = response.playerstats.stats;
                int totalKills = stats.Where(x => x.name == "total_kills").ElementAt(0).value;
                int totalDeaths = stats.Where(x => x.name == "total_deaths").ElementAt(0).value;
                int shotsHit = stats.Where(x => x.name == "total_shots_hit").ElementAt(0).value;
                int shotsFired = stats.Where(x => x.name == "total_shots_fired").ElementAt(0).value;
                int matchesWon = stats.Where(x => x.name == "total_matches_won").ElementAt(0).value;
                int matchesTotal = stats.Where(x => x.name == "total_matches_played").ElementAt(0).value;
                int timePlayed = stats.Where(x => x.name == "total_time_played").ElementAt(0).value;
                int headshots = stats.Where(x => x.name == "total_kills_headshot").ElementAt(0).value;
                int windows = stats.Where(x => x.name == "total_broken_windows").ElementAt(0).value;

                TimeSpan date = new TimeSpan(0, 0, timePlayed);

                double kdRatio = (totalKills * 1.0) / totalDeaths;
                kdRatio = Math.Round(kdRatio, 2);

                double accuracy = (shotsHit * 1.0) / shotsFired;
                accuracy *= 100;
                accuracy = Math.Round(accuracy, 2);

                double winRatio = (matchesWon * 1.0) / matchesTotal;
                winRatio *= 100;
                winRatio = Math.Round(winRatio, 2);

                string name = SteamAPI.Request<SteamSummaryResult>("ISteamUser", "GetPlayerSummaries", "v0002", "GET", Options.ChatCommandAPI.APIKey, "&steamids=" + query[1]).Result.response.players[0].personaname;
                await SendMessageAfterDelay(toID, string.Format("{0} ({1}) has {2} kills and {3} deaths ({4} kd-ratio), {5} shots fired ({6}% accuracy), {7} games played ({8}% win percentage), total playtime of {9}, {10} headshots, and {11} broken windows",
                    name, query[1], totalKills, totalDeaths, kdRatio, shotsFired, accuracy, matchesTotal, winRatio, date.ToString(), headshots, windows));
                return true;
            }
            return false;
        }
    }

    #region csgo response

    public class CSGOStatResponse
    {
        public Playerstats playerstats { get; set; }
    }

    public class Playerstats
    {
        public string steamID { get; set; }
        public string gameName { get; set; }
        public Stat[] stats { get; set; }
        public Achievement[] achievements { get; set; }
    }

    public class Stat
    {
        public string name { get; set; }
        public int value { get; set; }
    }

    public class Achievement
    {
        public string name { get; set; }
        public int achieved { get; set; }
    }

    #endregion

    #region steam response

    public class SteamSummaryResult
    {
        public Response response { get; set; }
    }

    public class Response
    {
        public Player[] players { get; set; }
    }

    public class Player
    {
        public string steamid { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public int lastlogoff { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public int personastate { get; set; }
        public string realname { get; set; }
        public string primaryclanid { get; set; }
        public int timecreated { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
        public string locstatecode { get; set; }
        public int loccityid { get; set; }
    }

    #endregion
}
