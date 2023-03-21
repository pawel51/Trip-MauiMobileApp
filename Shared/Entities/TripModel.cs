using GoogleApi.Entities.Maps.Common.Enums;

namespace Shared.Entities
{
    public sealed class TripModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public TimeSpan StartTime { get; set; } = new TimeSpan(12, 0, 0);

        public DateTime EndDate { get; set; } = DateTime.Now + TimeSpan.FromDays(1);

        public bool IsArchivized { get; set; } = false;

        public List<string> Places { get; set; } = new();

        public TravelMode TravelModeEnum { get; set; } = TravelMode.Walking;
        public TransitMode TransitModeEnum { get; set; } = TransitMode.Bus;
    }
}
