using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.DistanceMatrix.Request;
using GoogleApi.Entities.Places.Details.Request;
using GoogleApi.Entities.Places.Details.Request.Enums;
using GoogleApi.Entities.Places.Details.Response;
using GoogleApi.Entities.Places.Photos.Request;
using GoogleApi.Entities.Places.Photos.Response;
using GoogleApi.Entities.Places.Search.Common;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Request;
using GoogleApi.Entities.Places.Search.NearBy.Request.Enums;
using GoogleApi.Entities.Places.Search.NearBy.Response;
using GoogleApi.Entities.Places.Search.Text.Request;
using GoogleApi.Entities.Places.Search.Text.Response;
using Microsoft.Extensions.Configuration;
using Shared.Common;
using Shared.GoogleApiModels;
using Shared.Responses;
using static GoogleApi.GoogleMaps;
using static GoogleApi.GooglePlaces;
using static GoogleApi.GooglePlaces.Search;

namespace Services
{
    public class PlacesService
    {
        private readonly FindSearchApi _findSearchApi;
        private readonly NearBySearchApi _nearNearBySearchApi;
        private readonly TextSearchApi _textSearchApi;
        private readonly PhotosApi _photosapi;
        private readonly DetailsApi _detailsApi;
        private readonly DistanceMatrixApi _distanceApi;
        private string? _key;

        public PlacesService(
            FindSearchApi findSearchApi,
            NearBySearchApi nearNearBySearchApi,
            TextSearchApi textSearchApi,
            PhotosApi photosapi,
            DetailsApi detailsApi,
            DistanceMatrixApi distanceApi,
            IConfiguration configuration
            )
        {
            _findSearchApi = findSearchApi;
            _nearNearBySearchApi = nearNearBySearchApi;
            _textSearchApi = textSearchApi;
            _photosapi = photosapi;
            _detailsApi = detailsApi;
            _distanceApi = distanceApi;
            _key = configuration != null? 
                configuration
                    .GetRequiredSection($"{nameof(GoogleApiSettings)}")
                    .Get<GoogleApiSettings>().ApiKey 
                : "";
        }

        public async Task SetDistances(IEnumerable<BasicSearchItem> items, Coordinate deviceLocation)
        {
            List<DistanceMatrixRequest> requests = new();

            List<Task> dmTasks = new();
            foreach (var item in items)
            {
                dmTasks.Add(SetDistanceTask(deviceLocation, item));
            }
            await Task.WhenAll(dmTasks);
        }

        private async Task SetDistanceTask(Coordinate deviceLocation, BasicSearchItem item)
        {
            var request = GetRequestForPlace(item, deviceLocation);
            var response = await _distanceApi.QueryAsync(request);
            Distance? distance = response.Rows.ToList()
                .FirstOrDefault()?
                .Elements.FirstOrDefault()?
                .Distance;
            if (distance == null)
                return;
            item.TextDistanceInKm = distance.Text ?? "";
            item.DistanceInMeters = distance.Value;
        }

        private DistanceMatrixRequest GetRequestForPlace(BasicSearchItem item, Coordinate deviceLocation)
        {
            DistanceMatrixRequest request = new()
            {
                Origins = new List<LocationEx>()
                {
                    new LocationEx(new CoordinateEx(deviceLocation.Latitude, deviceLocation.Longitude))
                },
                Destinations = new List<LocationEx>()
                {
                    new LocationEx(new Place(item.Candidate.PlaceId))
                },
                Key = _key
            };
            return request;
        }

        public async Task<BaseResponse> SearchNearBy(string query, Coordinate location, string? key = null, double radius = 50000)
        {
            SetKey(key);
            var request = new PlacesNearBySearchRequest()
            {
                Key = _key,
                Keyword = query,
                Location = location,
                Radius = radius
                
            };
            try
            {
                PlacesNearbySearchResponse response = await _nearNearBySearchApi.QueryAsync(request);
                List<BasicSearchItem> modelsList = await GetThumbnailsUrls(response.Results);
                

                return new PlaceSearchDto<BasicSearchItem>() { PlacesList = modelsList };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse()
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<Shared.Responses.BaseResponse> SearchText(string query, string? key = null)
        {
            if (String.IsNullOrEmpty(query))
            {
                return new DefaultErrorResponse()
                {
                    Message = "Search Text Is Empty"
                };
            }
            SetKey(key);
            var request = new PlacesTextSearchRequest()
            {
                Key = _key,
                Query = query
            };
            try
            {
                var response = await _textSearchApi.QueryAsync(request);
                List<BasicSearchItem> modelsList = await GetThumbnailsUrls(response.Results);
                return new PlaceSearchDto<BasicSearchItem>() { PlacesList = modelsList };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse()      
                {
                    Message = ex.Message
                };
            }
        }

        private void SetKey(string? key)
        {
            _key = !String.IsNullOrEmpty(key) ? key : _key;
        }


        private async Task<List<BasicSearchItem>> GetThumbnailsUrls(IEnumerable<BaseResult> results)
        {
            List<BasicSearchItem> placeOnListModels = new();
            foreach (var result in results)
            {
                if (result.Photos == null || result.Photos.Count() < 1) continue;

                var placeOnListModel = await GetImageFromReference(result);
               
                if (placeOnListModel == null || placeOnListModel.Candidate == null) continue;

                if (result is TextResult textResult)
                    placeOnListModel.FormatedAddress = textResult.FormattedAddress;

                placeOnListModels.Add(placeOnListModel);
            }
            return placeOnListModels;
        }


        private async Task<BasicSearchItem?> GetImageFromReference(BaseResult baseResult)
        {
            PlacesPhotosRequest request = new()
            {
                Key = _key,
                PhotoReference = baseResult.Photos.First().PhotoReference,
                MaxHeight = 300
            };
            var response = await _photosapi.QueryAsync(request);
            if (response is null)
                return new BasicSearchItem() { Candidate = baseResult, Image = new() };
            return new BasicSearchItem()
            {
                Candidate = baseResult,
                Image = response
            };
        }

        public async Task<Shared.Responses.BaseResponse> GetPlaceDetails(string id, 
            FieldTypes fields = FieldTypes.Basic | FieldTypes.Rating | FieldTypes.Price_Level | FieldTypes.User_Ratings_Total | FieldTypes.Review)
        {
            var request = new PlacesDetailsRequest()
            {
                Key = _key,
                PlaceId = id,
                Fields = fields
            };
            try
            {
                var response = await _detailsApi.QueryAsync(request);
                var photosList = await GetImagesFromReference(response.Result);
                response.Result.Review ??= new List<Review>();
                return new PlaceDetailsDto() 
                { 
                    PlacesDetailsResponse = response.Result,
                    Images = photosList
                };

            } catch (Exception ex)
            {
                return new DefaultErrorResponse()
                {
                    Message = ex.Message
                };
            }
        }


        public async Task<Shared.Responses.BaseResponse> GetSmallPlaceDetails(string id, int imageCount, int imageRes, FieldTypes fields)
        {
            var request = new PlacesDetailsRequest()
            {
                Key = _key,
                PlaceId = id,
                Fields = fields
            };
            try
            {
                var response = await _detailsApi.QueryAsync(request);
                var photosList = await GetImagesFromReference(response.Result, imageCount, 100);
                response.Result.Review ??= new List<Review>();
                return new PlaceDetailsDto()
                {
                    PlacesDetailsResponse = response.Result,
                    Images = photosList
                };

            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse()
                {
                    Message = ex.Message
                };
            }
        }

        public async Task<Shared.Responses.BaseResponse> GetPlaceDataForMap(List<string> ids)
        {
            Dictionary<string, DetailsResult> placesDetailsForMap = new Dictionary<string, DetailsResult>();

            foreach (string id in ids)
            {
                var request = new PlacesDetailsRequest()
                {
                    Key = _key,
                    PlaceId = id,
                    Fields = FieldTypes.Name | FieldTypes.Place_Id | FieldTypes.Formatted_Address
                };
                try
                {
                    var response = await _detailsApi.QueryAsync(request);
                    placesDetailsForMap.TryAdd(id, response.Result);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return new OkResponse() { Payload = placesDetailsForMap, Message = "Successfuly downloaded place details for map." };
        }


        private async Task<List<PlacesPhotosResponse>> GetImagesFromReference(DetailsResult detailsResult, int imageCount = -1, int maxHeight = 300)
        {
            List<PlacesPhotosResponse> photosList = new();
            if (detailsResult.Photos is null) return photosList;

            if (imageCount == -1)
                imageCount = detailsResult.Photos.Count();

            int downloadCount = 0;
            foreach (var detailPhoto in detailsResult.Photos)
            {
                if (downloadCount >= imageCount) 
                    return photosList;
                PlacesPhotosRequest request = new()
                {
                    Key = _key,
                    PhotoReference = detailPhoto.PhotoReference,
                    MaxHeight = maxHeight
                };
                try
                {
                    var response = await _photosapi.QueryAsync(request);
                    if (response is not null)
                    {
                        photosList.Add(response);
                        downloadCount++;
                    }
                }
                catch 
                {
                    continue;
                }
            }
            return photosList;
        }

    }
}
