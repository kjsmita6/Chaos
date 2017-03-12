namespace Chaos.Triggers.TriggerOptions
{
    public class TriggerOptionsBase
    {
        public TriggerType Type { get; set; }
        public string Name { get; set; }
        public ChatCommand ChatCommand { get; set; }
        public ChatReply ChatReply { get; set; }
        public DoormatOptions DoormatOptions { get; set; }
        public MainOptions MainOptions { get; set; }
    }
}
