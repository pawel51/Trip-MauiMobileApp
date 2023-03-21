namespace Shared.Common
{
    public class VoiceHintItem
    {
        public string TextHint { get; set; } = "";
        public byte[] Voice { get; set; } = Array.Empty<byte>();
    }
}
