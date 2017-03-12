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
    public class KickTrigger : BaseTrigger
    {
        public KickTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> respondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            bool result = await Respond(roomID, chatterId, message);
            return result;
        }
        
        private async Task<bool> Respond(ulong toID, ulong userID, string message)
        {
            string[] query = StripCommand(message, Options.ChatCommand.Command);
            if(query != null && query.Length == 1)
            {
                await SendMessageAfterDelay(toID, "Usage: " + Options.ChatCommand.Command + " <@user> - kicks a user from chat");
                return true;
            }
            else if(query != null && query.Length == 2)
            {
                SocketGuildChannel channel = Bot.client.GetChannel(toID) as SocketGuildChannel;

                int from = query[1].IndexOf("<@") + "<@".Length;
                int to = query[1].LastIndexOf(">");
                string result = query[1].Substring(from, to - from);

                SocketGuildUser user = channel.GetUser(Convert.ToUInt64(result));
                if(user == null)
                {
                    await SendMessageAfterDelay(toID, "User not found!");
                    return true;
                }
                Log.Instance.Info("Kicking {0} from {1}", user.Username, channel.Name);
                await user.KickAsync();
                await SendMessageAfterDelay(toID, string.Format("Kicked {0} from {1}!", user.Username, channel.Name));
                return true;
            }
            return false;
        }
    }
}
