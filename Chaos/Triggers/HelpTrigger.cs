using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Triggers
{
    class HelpTrigger : BaseTrigger
    {
        public HelpTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> RespondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            bool result = await Respond(roomID, chatterId, message);
            return result;
        }

        private async Task<bool> Respond(ulong toID, ulong userID, string message)
        {
            string[] query = StripCommand(message, Options.ChatCommand.Command);

            if(query != null && query.Length == 1)
            {
                Dictionary<BaseTrigger, Dictionary<string, string>> commands = new Dictionary<BaseTrigger, Dictionary<string, string>>();
                foreach(BaseTrigger trigger in Bot.Triggers.Where(x => x.Options.ChatCommand != null || x.Options.ChatCommandAPI != null))
                {
                    if(trigger.Options.ChatCommandAPI != null)
                    {
                        Dictionary<string, string> usage = new Dictionary<string, string>();
                        usage.Add(trigger.Options.ChatCommandAPI.ChatCommand.Command, trigger.Options.ChatCommandAPI.ChatCommand.Usage);
                        commands.Add(trigger, usage);
                    }
                    else
                    {
                        Dictionary<string, string> usage = new Dictionary<string, string>();
                        usage.Add(trigger.Options.ChatCommand.Command, trigger.Options.ChatCommand.Usage);
                        commands.Add(trigger, usage);
                    }
                }
                string help = "";
                foreach(Dictionary<string, string> command in commands.Values)
                {
                    help += command.Keys.ElementAt(0) + " - " + command.Values.ElementAt(0) + "\n";
                }
                await SendMessageAfterDelay(toID, help.Trim());
                return true;
            }
            return false;
        }
    }
}
