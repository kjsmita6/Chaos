using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Net;

namespace Chaos.Triggers
{
    public class IsUpTrigger : BaseTrigger
    {
        public IsUpTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> respondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            bool result = await Respond(roomID, chatterId, message);
            return result;
        }

        private async Task<bool> Respond(ulong toID, ulong userID, string message)
        {
            string[] query = StripCommand(message, Options.ChatCommand.Command);
            if (query != null && query.Length == 1)
            {
                await SendMessageAfterDelay(toID, "Usage: " + Options.ChatCommand.Command + " <uri> - checks online status of a uri");
                return true;
            }
            else if (query != null && query.Length == 2)
            {
                SocketGuildChannel channel = Bot.client.GetChannel(toID) as SocketGuildChannel;
                HttpWebResponse response;
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query[1]);
                    response = (HttpWebResponse)(await request.GetResponseAsync());
                }
                catch(UriFormatException ufe)
                {
                    Log.Instance.Error(IfError(ufe));
                    await SendMessageAfterDelay(toID, "Uri was not in the correct format (needs http:// or https://)");
                    response = null;
                    return true;
                }
                catch(WebException we)
                {
                    Log.Instance.Error(IfError(we));
                    response = (HttpWebResponse)we.Response;
                }
                await SendMessageAfterDelay(toID, response.StatusDescription.ToString() + " (" + (int)response.StatusCode + ")");
                return true;
            }
            return false;
        }
    }
}
