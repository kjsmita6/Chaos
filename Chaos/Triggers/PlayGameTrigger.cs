using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chaos.Triggers
{
    public class PlayGameTrigger : BaseTrigger
    {
        public PlayGameTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> RespondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            bool result = await Respond(roomID, chatterId, message);
            return result;
        }

        private async Task<bool> Respond(ulong toID, ulong userID, string message)
        {
            string[] query = StripCommand(message, Options.ChatCommand.Command);
            if (query != null && query.Length == 1)
            {
                await SendMessageAfterDelay(toID, "Usage: " + Options.ChatCommand.Command + " <game OR clear> - sets bot game OR clears the bot's game");
                return true;
            }
            else if (query != null && query.Length > 1)
            {
                SocketGuildChannel channel = Bot.Client.GetChannel(toID) as SocketGuildChannel;
                if(query[1] == "clear")
                {
                    await Bot.Client.SetGameAsync("");
                    return true;
                }
                string game = "";
                for (int i = 1; i < query.Length; i++)
                {
                    game += query[i];
                }
                Bot.Game = game;
                await Bot.WriteData();
                await Bot.Client.SetGameAsync(Bot.Game);
                return true;
            }
            return false;
        }
    }
}
