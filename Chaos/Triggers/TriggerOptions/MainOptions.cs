using System.Collections.Generic;

namespace Chaos.Triggers.TriggerOptions
{
    public class MainOptions
    {
        public List<ulong> Users { get; set; }
        public List<ulong> Ignores { get; set; }
        public int Timeout { get; set; }
    }
}
