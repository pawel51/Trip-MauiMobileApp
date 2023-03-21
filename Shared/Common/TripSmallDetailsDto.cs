using Shared.Entities;

namespace Shared.Common
{
    public class TripSmallDetaisDto
    {

        public TripModel TripModel { get; set; } = new();

        public List<string> PlaceNames { get; set; } = new();
    }
}
