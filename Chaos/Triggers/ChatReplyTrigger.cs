using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Triggers
{
    class ChatReplyTrigger : BaseTrigger
    {
        public ChatReplyTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override Task<bool> respondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            return base.respondToChatMessage(roomID, chatterId, message);
        }

        private async Task<bool> Respond(ulong toID, ulong userID, string message)
        {
            if (CheckMessage(message) != false)
            {
                string response = PickResponse();
                await SendMessageAfterDelay(toID, response);
                return true;
            }
            return false;
        }

        private bool CheckMessage(string message)
        {
            if (Options.ChatReply.Matches != null && Options.ChatReply.Matches.Count > 0)
            {
                for (int i = 0; i < Options.ChatReply.Matches.Count; i++)
                {
                    string match = Options.ChatReply.Matches[i];
                    if (message.ToLower() == match.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string PickResponse()
        {
            if (Options.ChatReply.Responses != null && Options.ChatReply.Responses.Count > 0)
            {
                Random rnd = new Random();
                int index = rnd.Next(0, Options.ChatReply.Responses.Count);
                return Options.ChatReply.Responses[index];
            }
            return "";
        }
    }
}
