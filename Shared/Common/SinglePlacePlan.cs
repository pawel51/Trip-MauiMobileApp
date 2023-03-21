namespace Shared.Common
{
    public partial class SinglePlacePlan : PlacePlan
    {
        public string DurationToNextPlace { get; set; } = "";
        public string DistanceToNextPlace { get; set; } = "";

        public List<VoiceHintItem> VoiceHints { get; set; } = new();
    }
}
