using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Triggers
{
    class ChooseTrigger : BaseTrigger
    {

        public ChooseTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> RespondToChatMessage(ulong roomID, ulong chatterId, string message)
        {
            bool result = await Respond(roomID, chatterId, message);
            return result;
        }

        private async Task<bool> Respond(ulong toID, ulong fromID, string message)
        {
            string[] query = StripCommand(message, Options.ChatCommand.Command);
            if (query != null && query.Length > 1)
            {
                string[] choices = new string[query.Length - 1];
                for (int i = 1; i < query.Length; i++)
                {
                    choices[i - 1] = query[i];
                }

                Random rng = new Random();
                double rnd = rng.NextDouble();
                int choice = (int)Math.Floor(rnd * choices.Length);
                await SendMessageAfterDelay(toID, "I have chosen " + choices[choice]);
                return true;
            }
            return false;
        }
    }
}
