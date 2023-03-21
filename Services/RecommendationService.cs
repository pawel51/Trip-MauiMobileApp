using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Common.Extensions;
using GoogleApi.Entities.Places.Details.Response;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Translate.Common.Enums.Extensions;
using Microsoft.Extensions.Configuration;
using Repositories;
using Shared.Recommendations;
using Shared.Responses;
using System.Linq;
using System.Security.Cryptography;

namespace Services
{
    public class RecommendationService : BaseService<RecoModel>
    {
        public RecommendationService(RecoRepository repository, IConfiguration configuration)
            : base(repository, configuration)
        {

        }

        public Coordinate GetMeanCoordinateForTrip(List<PlaceDetailsDto> placesDtos)
        {
            double latitude = 0;
            double longitude = 0;
            int counter = 0;
            placesDtos.ForEach(placeDto =>
            {
                latitude += placeDto.PlacesDetailsResponse.Geometry.Location.Latitude;
                longitude += placeDto.PlacesDetailsResponse.Geometry.Location.Longitude;
                counter++;
            });
            return new Coordinate(latitude/counter, longitude/counter);
        }

        public List<PlaceLocationType> GetTopCategoryForTrip(List<PlaceDetailsDto> placesDtos)
        {
            Dictionary<PlaceLocationType, int> topCategoriesDict = new();

            foreach (var placeDto in placesDtos)
            {
                CountCategoriesForPlaceDto(topCategoriesDict, placeDto);
            }
            return SortAndTake3(topCategoriesDict);            
        }

        private void CountCategoriesForPlaceDto(Dictionary<PlaceLocationType, int> topCategoriesDict, PlaceDetailsDto placeDto)
        {
            foreach (PlaceLocationType type in placeDto.PlacesDetailsResponse.Types)
            {
                if (topCategoriesDict.ContainsKey(type))
                {
                    topCategoriesDict[type]++;
                }
                else
                {
                    topCategoriesDict.Add(type, 1);
                }
            }
        }
        private List<PlaceLocationType> SortAndTake3(Dictionary<PlaceLocationType, int> topCategoriesDict)
        {
            return topCategoriesDict
                .OrderByDescending(keyValue => keyValue.Value)
                .Take(3)
                .Select(e => e.Key)
                .ToList();
        }
       

        
        public async Task<RecoModel> GetRecommendationForCurrentUser()
        {
            var response = await _repository.GetItemOfCurrentUser();
            if (response == null)
                return new RecoModel();
            return response;
        }

        public async Task PlaceAddedToTrip(DetailsResult newPlace)
        {
            RecoModel recoModel = await _repository.GetItemOfCurrentUser();
            recoModel ??= new();

            foreach (PlaceLocationType locType in newPlace.Types)
            {
                IncrementAllTypesOfNewPlace(recoModel, locType);
            }
            await _repository.PostForCurrentUser(recoModel);
        }

        private static void IncrementAllTypesOfNewPlace(RecoModel recoModel, PlaceLocationType locType)
        {
            CounterItem? item = recoModel.Categories
                .FirstOrDefault(e => e.Name.ToEnumMemberString() == locType.ToEnumMemberString());
            if (item == null)
            {
                AddNewLocToCategories(recoModel, locType);
            }
            else
                item.Counter++;
        }

        private static void AddNewLocToCategories(RecoModel recoModel, PlaceLocationType locType)
        {
            SearchPlaceType searchPlaceType;
            if (!TryConvertPlaceLocationToSearchPlace(out searchPlaceType, locType))
                return;

            CounterItem newItem = new CounterItem()
            {
                Name = searchPlaceType,
                Counter = 1
            };
            recoModel.Categories.Add(newItem);
        }

        private static bool TryConvertPlaceLocationToSearchPlace(out SearchPlaceType searchPlaceType, PlaceLocationType locType)
        {
            bool success = Enum.TryParse(locType.ToEnumMemberString(), true, out searchPlaceType);
            if (!success)
                return false;
            return true;
        }

    }
}
