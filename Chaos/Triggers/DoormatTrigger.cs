using Chaos.Triggers.TriggerOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chaos.Triggers
{
    public class DoormatTrigger : BaseTrigger
    {
        public DoormatTrigger(TriggerType type, string name, TriggerOptionsBase options) : base(type, name, options)
        { }

        public override async Task<bool> respondToEnteredMessage(ulong roomID, ulong userID)
        {
            bool result = await Respond(roomID, userID);
            return result;
        }

        private async Task<bool> Respond(ulong toID, ulong userID)
        {
            string message = Options.DoormatOptions.Message.Replace("#", Bot.client.GetChannel(toID).GetUser(userID).Username);
            await SendMessageAfterDelay(toID, message);
            return true;
        }
    }
}
