using System.Collections.Generic;

namespace Chaos.Triggers.TriggerOptions
{
    public class ChatReply
    {
        public string Name { get; set; }
        public List<string> Matches { get; set; }
        public List<string> Responses { get; set; }
    }
}
