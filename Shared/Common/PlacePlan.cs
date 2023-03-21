using CommunityToolkit.Mvvm.ComponentModel;

namespace Shared.Common
{
    public abstract partial class PlacePlan : ObservableObject
    {
        public string PlaceId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";

        [ObservableProperty]
        public TimeSpan timeToSpendHere = TimeSpan.FromMinutes(10);

        public TimeSpan EntryTime { get; set; }

        public DateTime EntryDate { get; set; }
    }
}
