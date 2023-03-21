using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Common.Enums;
using GoogleApi.Entities.Common.Extensions;
using GoogleApi.Entities.Interfaces;
using GoogleApi.Entities.Places.Common.Enums;
using GoogleApi.Entities.Places.Search.Common.Enums;
using GoogleApi.Entities.Places.Search.Find.Response;
using GoogleApi.Entities.Places.Search.NearBy.Request.Enums;
using GoogleApi.Entities.Search.Common.Enums;
using GoogleApi.Entities.Translate.Common.Enums.Extensions;
using Services;
using Shared.Common;
using Shared.Entities;
using Shared.GoogleApiModels;
using Shared.Responses;
using Shared.Utils;
using Shared.Utils.Sorting;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Tripaui.Abstractions;
using Tripaui.Extensions;
using Tripaui.Messages;
using Tripaui.Platforms;
using Tripaui.Services;
using Tripaui.Views;
using Tripaui.Views.Controlls;
using Tripaui.Views.Places;
using Tripaui.Views.Trips;
using static Android.Provider.MediaStore.Audio;

namespace Tripaui.ViewModels.Places
{
    [QueryProperty(nameof(Coordinate), nameof(Coordinate))]
    [QueryProperty(nameof(Radius), nameof(Radius))]
    [QueryProperty(nameof(Category), nameof(Category))]
    [QueryProperty(nameof(ShowRecommended), nameof(ShowRecommended))]
    public sealed partial class PlaceListViewModel : BaseViewModel
    {
        public BottomSheet FiltersContent;
        public BottomSheet SortingContent;
        public CollectionView FiltersCollection;
        public IList<SearchPlaceType> Filters { get; set; } = Enum.GetValues(typeof(SearchPlaceType)).Cast<SearchPlaceType>().ToList();

        public ObservableCollection<SearchPlaceType> SelectedFilters { get; set; } = new ObservableCollection<SearchPlaceType>();

        public List<SortingGroup> SortingGroups { get; set; } = new();

        public ObservableCollection<SearchPlaceType> RecommendedCategories { get; set; } = new();

        public ObservableCollection<SearchPlaceType> SelectedRecommended { get; set; } = new();

        #region Properties

        [ObservableProperty]
        private ObservableCollection<BasicSearchItem> searchedPlaces = new();

        [ObservableProperty]
        private ObservableCollection<BasicSearchItem> filteredPlaces = new();

        [ObservableProperty]
        private string query = "";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SearchNearByIsNotToggled))]
        private bool searchNearByIsToggled = true;

        public Coordinate Coordinate { get; set; } 

        public double Radius { get; set; } = 50000;

        public PlaceLocationType Category { get; set; }

        public bool ShowRecommended { get; set; } = false;

        public bool SearchNearByIsNotToggled => !SearchNearByIsToggled;

        public bool IsRecording { get; private set; }
        #endregion


        private readonly AuthService _authService;
        private readonly PlacesService _placesService;
        private readonly GeolocationService _geolocationService;
        private readonly RecommendationService _recommendationService;
        private readonly ISpeechToText _speechToText;
        private CancellationTokenSource _endSpeech;

        public PlaceListViewModel(
            AuthService authService, 
            PlacesService placesService,  
            GeolocationService geolocationService,
            RecommendationService recommendationService,
            ISpeechToText speechToText)
        {
            _authService = authService;
            _placesService = placesService;
            _geolocationService = geolocationService;
            _recommendationService = recommendationService;
            _speechToText = speechToText;
            SortingGroupFactory sortingGroupFactory = new(FilteredPlaces);

            SortingGroups.Add(sortingGroupFactory.CreateNameGroup());
            SortingGroups.Add(sortingGroupFactory.CreateRatingGroup());
            SortingGroups.Add(sortingGroupFactory.CreateUserAmountGroup());
            SortingGroups.Add(sortingGroupFactory.CreateDistanceGroup());
        }


        
        public void RestoreSearchedPlaces()
        {
            FilteredPlaces.Clear();
            FilteredPlaces.AddRange(SearchedPlaces);
        }

        #region Commands

        

        [RelayCommand]
        async Task PageAppearing()
        {
            IsBusy = true;
            Coordinate = await GetCurrentCoordinate();
            if (!_authService.IsAuthenticated())
            {
                var user = _authService.GetCurrentUser();
                var email = "";
                if (user != null)
                    email = user.Email;
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}", true, new Dictionary<string, object>()
                {
                    { "email",  email }
                });
                return;
            }
            await LoadPlacesByLocalisation();
            var recoModel = await _recommendationService.GetRecommendationForCurrentUser();
            if (recoModel.Categories.Count == 0)
            {
                IsBusy = false; 
                return;
            }
            List<SearchPlaceType> topCategories = recoModel.Categories
                .OrderByDescending(cat => cat.Counter)
                .Take(3)
                .Select(cat => cat.Name)
                .ToList();
            RecommendedCategories.Clear();
            RecommendedCategories.AddRange(topCategories);
            IsBusy = false;
        }

        private async Task LoadPlacesByLocalisation()
        {
            string newQuery = ShowRecommended == false ? "" : Category.ToString().Replace('_', ' ');
            if (String.Compare(Query, newQuery, true) == 0)
                return;
            Query = newQuery;
            if (String.IsNullOrWhiteSpace(Query)) 
                return;
            var response = await _placesService.SearchNearBy(
                Query,
                Coordinate,
                key: null);
            if (await response.ValidateResponse() > 0)
                return;
            var searchItems = response as PlaceSearchDto<BasicSearchItem>;
            if (searchItems is null)
                return;
            await _placesService.SetDistances(searchItems.PlacesList, Coordinate);
            SetPlacesListToDisplayCollections(searchItems.PlacesList);
            ShowRecommended = false;
        }

        [RelayCommand]
        async Task SearchPlace()
        {
            IsBusy = true;
            PlaceSearchDto<BasicSearchItem> response = null;
            if (SearchNearByIsToggled)
            {
                response = await _placesService.SearchNearBy(Query, Coordinate) as PlaceSearchDto<BasicSearchItem>;
            }
            else
            {
                response = await _placesService.SearchText(Query) as PlaceSearchDto<BasicSearchItem>;
            }
            if (response is null ||
                await response.ValidateResponse() > 0 ||
                response.PlacesList.Count() == 0)
            {
                IsBusy = false;
                return;
            }
            await _placesService.SetDistances(response.PlacesList, Coordinate);
            SetPlacesListToDisplayCollections(response.PlacesList);
            IsBusy = false;
        }

        private async Task<Coordinate> GetCurrentCoordinate()
        {
            Location location = await _geolocationService.GetCurrentLocation();
            return new Coordinate(location.Latitude, location.Longitude);
        }

        private void SetPlacesListToDisplayCollections(List<BasicSearchItem> placeList)
        {
            SearchedPlaces.Clear();
            SearchedPlaces.AddRange(placeList);
            FilteredPlaces.Clear();
            FilteredPlaces.AddRange(SearchedPlaces);
        }

        [RelayCommand]
        async Task GoToDetails(BasicSearchItem placeOnList)
        {
            var placeid = placeOnList.Candidate.PlaceId ?? "";
            try
            {
                await Shell.Current.GoToAsync($"{nameof(PlaceDetailsPage)}", true, new Dictionary<string, object>()
                {
                    { "PlaceId", placeid },
                });
            } 
            catch(Exception ex)
            {
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup(ex.Message));
            }
        }

        [RelayCommand]
        private async Task Record(Border microphoneButton)
        {
            if (!IsRecording)
            {
                if (!await _speechToText.RequestPermissions())
                {
                    await Shell.Current.DisplayAlert("Error", "No microphone permission", "OK");
                    return;
                }
                IsRecording = true;

                microphoneButton.BackgroundColor = Color.FromArgb("#9d0208");
                _endSpeech = new();
                try
                {
                    Query = "";
                    Query = await _speechToText.Listen(CultureInfo.GetCultureInfo("en-us"), new Progress<string>(partialText =>
                    {
                        Query = partialText;
                    }), _endSpeech.Token);
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show(_endSpeech.Token);
                }
                finally
                {
                    IsRecording = false;
                    microphoneButton.BackgroundColor = Color.FromArgb("#e85d04");                    
                    if (Query.Length > 1)
                    {
                        await SearchPlace();
                    }
                }
            }
        }

        [RelayCommand]
        private async Task OpenFiltersTopSheet()
        {
            IsBusy = true;
            await FiltersContent.OpenBottomSheet();
        }

        [RelayCommand]
        private async Task CloseFiltersTopSheet()
        {
            await FiltersContent.CloseBottomSheet();

            await Task.Run(() => TryFilter(SelectedFilters));

            IsBusy = false;
        }



        private void TryFilter(ObservableCollection<SearchPlaceType> Selected)
        {
            if (Selected.Count == 0)
            {
                RestoreSearchedPlaces();
                IsBusy = false;
                return;
            }

            var filteredItems = SearchedPlaces
                .Where(place => place.Candidate.Types
                .Where(type => SelectedFiltersContains(Selected, type))
                .Any());

            FilteredPlaces.Clear();
            FilteredPlaces.AddRange(filteredItems);
        }


        private bool SelectedFiltersContains(ObservableCollection<SearchPlaceType> selected, PlaceLocationType? type)
        {
            return selected
                        .Where(selectedFilter =>
                            selectedFilter.GetEnumMemberValue() == type.Value.GetEnumMemberValue())
                        .Any();
        }

        [RelayCommand]
        private void DeSelectAll() => SelectedFilters.Clear();

        [RelayCommand]
        private void SelectFilter(SearchPlaceType type)
        {
            TrySelectFilter(SelectedFilters, type);
        }

        [RelayCommand]
        private async Task SelectRecommended(SearchPlaceType type)
        {
            IsBusy = true;
            await Task.Run(() => TrySelectFilter(SelectedRecommended, type));
            await Task.Run(() => TryFilter(SelectedRecommended));
            IsBusy = false;
        }

        [RelayCommand]
        private void DeselectRecommended() => SelectedRecommended.Clear();

        private void TrySelectFilter(ObservableCollection<SearchPlaceType> selected, SearchPlaceType type)
        {
            bool deleted = false;
            if (selected.Contains(type))
                deleted = selected.Remove(type);
            else
                selected.Add(type);
        }

        [RelayCommand]
        private async Task OpenSortingTopSheet()
        {
            IsBusy = true;
            await SortingContent.OpenBottomSheet();
        }

        [RelayCommand]
        private async Task CloseSortingTopSheet()
        {
            await SortingContent.CloseBottomSheet();
            IsBusy = false;
        }

        private SortingEnum prevSort = SortingEnum.None;
        [RelayCommand]
        private async Task Sort(PlaceSortItem value)
        {
            await Task.Run(() => TrySort(value));
        }

        private void TrySort(PlaceSortItem value)
        {
            if (prevSort != value.Type)
            {
                value.SortFunc(FilteredPlaces);
                prevSort = value.Type;
            }
            else
            {
                prevSort = SortingEnum.None;
                RestoreSearchedPlaces();
                return;
            }
        }
        #endregion


    }
}
