using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Repositories;
using Shared.Common;
using Shared.Entities;
using Shared.Responses;
using System.Text;
using static GoogleApi.GoogleMaps;
using RequestWaypoint = GoogleApi.Entities.Maps.Directions.Request.WayPoint;
using ResponseWaypoint = GoogleApi.Entities.Maps.Directions.Response.WayPoint;

namespace Services
{
    public sealed class DirectionsService : BaseService<TripPlan>
    {
        private readonly DirectionsApi _directionsApi;
        private string? _key;

        public DirectionsService(DirectionsApi directionsApi, PlanRepository planRepository, IConfiguration configuration)
            : base(planRepository, configuration)
        {

            _key = configuration != null ?
                configuration
                    .GetRequiredSection($"{nameof(GoogleApiSettings)}")
                    .Get<GoogleApiSettings>().ApiKey
                : "";
            _directionsApi = directionsApi;
        }

        public async Task<BaseResponse> GetTripDirectionReponse(TripModel tripModel, string? key = null, bool optimizeWaypoints = false)
        {
            SetKey(key);

            if (tripModel.Places == null || tripModel.Places.Count == 0)
            {
                return new DefaultErrorResponse() { Message = "No place added to a trip" };
            }
            if (tripModel.Places.Count == 1)
            {
                return new DefaultErrorResponse() { Message = "At least two places are needed to plan a trip" };
            }

            var wayPoints = GetRequestWaypoints(tripModel.Places);
            
            var request = new DirectionsRequest()
            {
                Key = _key,
                Origin = GetLocationExFromPlaceId(tripModel.Places.First()),
                Destination = GetLocationExFromPlaceId(tripModel.Places.Last()),
                TravelMode = tripModel.TravelModeEnum,
                WayPoints = wayPoints,
                DepartureTime = new DateTime(
                    tripModel.StartDate.Year,
                    tripModel.StartDate.Month,
                    tripModel.StartDate.Day,
                    tripModel.StartTime.Hours,
                    tripModel.StartTime.Minutes,
                    tripModel.StartTime.Seconds
                    ),
                TransitMode = tripModel.TransitModeEnum,
                OptimizeWaypoints = optimizeWaypoints
            };
            try
            {
                var response = await _directionsApi.QueryAsync(request);
                return new OkResponse()
                {
                    Message = "Successfuly generated a trip plan!",
                    Payload = response
                };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public Dictionary<string, List<Coordinate>> GetDictionaryOfPolylineCoordinates(Route route, List<ResponseWaypoint> wayPoints) 
        {
            Dictionary<string, List<Coordinate>> polylineDictionary = new();
            int legId = 0;
            List<Coordinate> lastPolyline = new();
           foreach (var leg in route.Legs)
            {
                if (legId >= wayPoints.Count) break;
                string id = wayPoints[legId++].PlaceId;
                List<Coordinate> polyline = GetPolylineFromSteps(leg);
                polylineDictionary.TryAdd(id, polyline);
                lastPolyline = polyline;
            }
            string lastId = wayPoints[legId].PlaceId;
            polylineDictionary.TryAdd(lastId, lastPolyline.TakeLast(1).ToList());

            return polylineDictionary;
        }


        private static List<string> GetPlacesIdsFromTrip(TripPlan plan)
        {
            List<string> placesIds = plan.SinglePlacePlans.Select(plan => plan.PlaceId).ToList();
            placesIds.Add(plan.Destination.PlaceId);
            return placesIds;
        }

        public TripPlan GenerateBasicTripPlan(Route route, List<ResponseWaypoint> wayPoints, TripDetailsDto tripDetailsDto, TripPlan tripPlan = null)
        {
            if (tripPlan == null)
                tripPlan = new();
            InitializeTripPlan(tripDetailsDto, tripPlan);

            var duration = TimeSpan.FromSeconds(0);
            int legId = 0;
            tripPlan.SinglePlacePlans.Clear();
            foreach (var leg in route.Legs)
            {
                if (legId >= wayPoints.Count) break;
                SinglePlacePlan placePlan = new();
                placePlan.DurationToNextPlace = leg.Duration.Text;

                SetPlacePlanData(wayPoints, tripDetailsDto, tripPlan, duration, legId, placePlan);
                placePlan.DistanceToNextPlace = leg?.Distance.Text ?? "0";
                duration = AddTimeToSpendAtPlace(duration, placePlan);
                duration = AddTimeToNextPlace(duration, leg);
                SetHtmlInstructions(placePlan, leg);
                tripPlan.SinglePlacePlans.Add(placePlan);
                legId++;
            }

            DestinationPlacePlan destination = new();
            SetPlacePlanData(wayPoints, tripDetailsDto, tripPlan, duration, legId, destination);
            tripPlan.Destination = destination;

            duration = AddTimeToSpendAtPlace(duration, destination);
            SetTripEndTime(tripPlan, duration);
            return tripPlan;
        }


        

        private static void InitializeTripPlan(TripDetailsDto tripDetailsDto, TripPlan tripPlan)
        {
            tripPlan.TripId = tripDetailsDto.TripModel.Id.ToString();
            tripPlan.StartDate = tripDetailsDto.TripModel.StartDate;
            tripPlan.StartTime = tripDetailsDto.TripModel.StartTime;
        }
        private static void SetPlacePlanData(List<ResponseWaypoint> wayPoints, TripDetailsDto tripDetailsDto, TripPlan tripPlan, TimeSpan duration, int legId, PlacePlan placePlan)
        {
            placePlan.EntryDate = tripPlan.StartDate.Add(duration);
            placePlan.EntryTime = tripPlan.StartTime.Add(duration);
            placePlan.Name = tripDetailsDto.PlaceDetailsList[legId].PlacesDetailsResponse.Name;
            placePlan.Address = tripDetailsDto.PlaceDetailsList[legId].PlacesDetailsResponse.FormattedAddress;
            placePlan.PlaceId = wayPoints[legId].PlaceId;
        }

        private static TimeSpan AddTimeToNextPlace(TimeSpan duration, Leg? leg)
        {
            duration = duration.Add(TimeSpan.FromSeconds(leg?.Duration.Value ?? 0));
            return duration;
        }

        private static TimeSpan AddTimeToSpendAtPlace(TimeSpan duration, PlacePlan placePlan)
        {
            duration = duration.Add(placePlan.TimeToSpendHere);
            return duration;
        }

        private void SetHtmlInstructions(SinglePlacePlan singlePlacePlan, Leg leg)
        {
            foreach (Step step in leg.Steps)
            {
                VoiceHintItem voiceHint = GetVoiceHintFromHtmlInstruction(step.HtmlInstructions);
                singlePlacePlan.VoiceHints.Add(voiceHint);
            }
        }

        private VoiceHintItem GetVoiceHintFromHtmlInstruction(string htmlInstructions)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(htmlInstructions);
            var root = doc.DocumentNode;
            var sb = new StringBuilder();
            foreach (var node in root.DescendantsAndSelf())
            {
                if (!node.HasChildNodes)
                {
                    string text = node.InnerText;
                    if (!string.IsNullOrEmpty(text))
                        sb.AppendLine(text.Trim());
                }
            }
            VoiceHintItem voiceHintItem = new();
            voiceHintItem.TextHint = sb.ToString();
            return voiceHintItem;
        }

        private static void SetTripEndTime(TripPlan tripPlan, TimeSpan duration)
        {
            tripPlan.EndDate = tripPlan.StartDate.Add(duration);
            tripPlan.EndTime = tripPlan.StartTime.Add(duration);
        }

        private static List<Coordinate> GetPolylineFromSteps(Leg? leg)
        {
            List<Coordinate> polyline = new();
            if (leg is null)
                return polyline;
            foreach (Step step in leg.Steps)
            {
                polyline.AddRange(step.PolyLine.Line);
            }
            return polyline;
        }

        private static Location TransformToEmbededMapLocation(Coordinate coordinate)
        {
            return new Location(coordinate);
        }

        private static List<RequestWaypoint> GetRequestWaypoints(List<string> placesIds)
        {
            var requestWaypoints = new List<RequestWaypoint>();
            requestWaypoints
                .AddRange(
                    placesIds
                    .Where((place, index) =>
                    {
                        return index != 0 && index != placesIds.Count - 1;
                    })
                    .Select(GetRequestWaypointFromId)
                );
            return requestWaypoints;
        }

        private static RequestWaypoint GetRequestWaypointFromId(string place)
        {
            return new RequestWaypoint(GetLocationExFromPlaceId(place));
        }

        private static LocationEx GetLocationExFromPlaceId(string place)
        {
            return new LocationEx(new Place(place));
        }


        private void SetKey(string? key)
        {
            _key = !String.IsNullOrEmpty(key) ? key : _key;
        }
    }
}
