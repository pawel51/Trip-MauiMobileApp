using GoogleApi.Entities.Places.Search.Common.Enums;

namespace Shared.Recommendations
{
    public class CounterItem
    {
        public SearchPlaceType Name { get; init; }

        public int Counter { get; set; }
    }
}
