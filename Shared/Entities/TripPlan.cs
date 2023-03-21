using GoogleApi.Entities.Maps.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common
{
    public class TripPlan
    {
        public string TripId { get; set; } = "";
        public List<SinglePlacePlan> SinglePlacePlans { get; set; } = new();

        public DestinationPlacePlan Destination { get; set; } = new();

        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public TravelMode TravelModeEnum { get; set; }
        public TransitMode TransitModeEnum { get; set; }
    }
}
