using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GoogleApi.Entities.Places.Details.Request.Enums;
using Services;
using Shared.Common;
using Shared.Entities;
using Shared.Responses;
using System.Collections.ObjectModel;
using System.Globalization;
using Tripaui.Abstractions;
using Tripaui.Extensions;
using Tripaui.Platforms;
using Tripaui.Views.Controlls;

namespace Tripaui.ViewModels.Trips
{
    [QueryProperty(nameof(Trip), nameof(Trip))]
    [QueryProperty(nameof(TripId), nameof(TripId))]
    [QueryProperty(nameof(PlacesNames), nameof(PlacesNames))]
    public sealed partial class ReviewsViewModel : BaseViewModel
    {
        public IList<OrderState> Chips { get; set; } = new List<OrderState>();
        public BottomSheet AddBottomSheet;
        public BottomSheet UpdateBottomSheet;
        public VerticalStackLayout SortingButtonsLayout;

        private readonly ReviewsService _reviewService;
        private readonly PlacesService _placesService;
        private readonly ISpeechToText _speechToText;
        private CancellationTokenSource _stopSpeaking = null;
        private CancellationTokenSource _endSpeech;
        private ReviewModel CurrentlySpokenReview = null;
        private ReviewModel selectedReview;

        [ObservableProperty]
        List<PlaceName> placesNames;

        [ObservableProperty]
        PlaceName selectedName = new();

        [ObservableProperty]
        ObservableCollection<ReviewModel> reviews = new();

        
        [ObservableProperty]
        ReviewModel currentReview = new();

        [ObservableProperty]
        TripDetailsDto trip;

        [ObservableProperty]
        string tripId;

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotSaving))]
        bool isSaving = false;

        [ObservableProperty]
        bool isSortExpanded = false;

        public bool IsNotSaving => !IsSaving;

        [ObservableProperty]
        bool isRecording = false;

        [ObservableProperty]
        OrderState selectedOrder = OrderState.Newest;



        public ReviewsViewModel(ReviewsService reviewService, 
            PlacesService placesService, 
            ISpeechToText speechToText)
        {
            Chips.Add(OrderState.Newest);
            Chips.Add(OrderState.Oldest);

            _reviewService = reviewService;
            _placesService = placesService;
            _speechToText = speechToText;
        }


        [RelayCommand]
        private void ChangeSortDirection()
        {
            OrderReviewsBeforeChange();
        }

        private void OrderReviewsBeforeChange()
        {
            List<ReviewModel> sortedReviews = null;
            if (SelectedOrder == OrderState.Oldest)
                sortedReviews = Reviews.OrderBy(review => review.CreatedAt).ToList();
            else
                sortedReviews = Reviews.OrderByDescending(review => review.CreatedAt).ToList();

            Reviews.Clear();
            sortedReviews.ForEach(Reviews.Add);
        }

        [RelayCommand]
        private async Task PageAppearing()
        {
            IsBusy = true;
            var reviewsResponse = await _reviewService.GetAllItemsOfTrip(TripId);
            if (await reviewsResponse.ValidateResponse() > 0)
            {
                IsBusy = false;
                return;
            }
            var reviewsList = ((IEnumerable<ReviewModel>)((OkResponse)reviewsResponse).Payload);
            Reviews.Clear();
            reviewsList.ToList().ForEach(Reviews.Add);
            OrderReviewsBeforeChange();
            IsBusy = false;
        }

        [RelayCommand]
        private async Task Record(Border recordButton)
        {
            if (!IsRecording)
            {
                if (!await _speechToText.RequestPermissions())
                {
                    await Shell.Current.DisplayAlert("Error", "No microphone permission", "OK");
                    return;
                }    
                IsRecording = true;
                recordButton.BackgroundColor = Color.FromArgb("#9d0208");
                _endSpeech = new();
                try
                {
                    CurrentReview.Text += await _speechToText.Listen(CultureInfo.GetCultureInfo("en-us"), new Progress<string>(partialText =>
                    {
                        CurrentReview.Text += ". " + partialText;
                    }), _endSpeech.Token);
                }
                catch (Exception ex)
                {
                    await Toast.Make(ex.Message).Show(_endSpeech.Token);
                }
                finally
                {
                    IsRecording = false;
                    recordButton.BackgroundColor = Color.FromArgb("#e85d04");
                }
            }
        }

        [RelayCommand]
        private async Task CreateNewReview()
        {
            if (! await IsPlaceSelected())
                return;
            IsSaving = true;
            ReviewModel newReview = await GetNewReview();
            if (! await TryAddReviewToDb(newReview))
                return;
            Reviews.Add(newReview);
            OrderReviewsBeforeChange();
            await DoCleanUpAfterAdd();
            return;
        }

        [RelayCommand]
        private async Task OpenBottomSheetForUpdate(ReviewModel reviewModel)
        {
            CancelSpeech();
            CurrentReview.Text = reviewModel.Text;
            SelectedName = placesNames.FirstOrDefault(e => e.Id == reviewModel.PlaceId);
            await UpdateBottomSheet.OpenBottomSheet();
            selectedReview = reviewModel;
        }

        [RelayCommand]
        private async Task UpdateReview()
        {
            if (!await IsPlaceSelected())
                return;
            IsSaving = true;
            if (!await TryAddReviewToDb(selectedReview))
            {
                Reviews.Remove(selectedReview);
                return;
            }
            await DoCleanUpAfterAdd();
            return;
        }

        [RelayCommand]
        private async Task DeleteReview(ReviewModel review)
        {
            IsRefreshing = true;
            if (CurrentlySpokenReview == review)
            {
                CancelSpeech();
            }
            var deleteResponse = await _reviewService.DeleteItemOfTripById(TripId, review.Id.ToString());
            if (await deleteResponse.ValidateResponse() > 0)
            {
                IsRefreshing = false;
                return;
            }
            Reviews.Remove(review);
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task GoBack()
        {
            CancelSpeech();
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task OpenAddBottomSheet()
        {
            CancelSpeech();
            CurrentReview.Text = "";
            SelectedName = null;
            await AddBottomSheet.OpenBottomSheet();
        }

        [RelayCommand]
        private async Task CloseBottomSheet(BottomSheet bottomSheet)
        {
            await bottomSheet.CloseBottomSheet();
        }

        [RelayCommand]
        private async Task StartVoiceListening(ReviewModel reviewModel)
        {
            _stopSpeaking = new CancellationTokenSource();
            CurrentlySpokenReview = reviewModel;
            await TextToSpeech.Default.SpeakAsync(reviewModel.Text, cancelToken: _stopSpeaking.Token);
        }

        private async Task<bool> IsPlaceSelected()
        {
            if (String.IsNullOrEmpty(SelectedName.Name))
            {
                await Shell.Current.ShowPopupAsync(new ValidationErrorPopup("Please Select some place"));
                return false;
            }
            return true;
        }

        private async Task<byte[]> GetImageForSelectedName()
        {
            BaseResponse placeDetailResponse = await _placesService
                .GetSmallPlaceDetails(SelectedName.Id, 1, 100, FieldTypes.Photo);
            if (await placeDetailResponse.ValidateResponse() > 0)
            {
                return Array.Empty<byte>();
            }
            var images = ((PlaceDetailsDto)placeDetailResponse).Images;
            return images.Count > 0 ? images[0].Buffer : Array.Empty<byte>();
        }

        private async Task<ReviewModel> GetNewReview()
        {
            ReviewModel newReview = new();
            newReview.Photo = await GetImageForSelectedName();
            newReview.Text = CurrentReview.Text;
            newReview.PlaceId = SelectedName.Id;
            return newReview;
        }

        private async Task<bool> TryAddReviewToDb(ReviewModel newReview)
        {
            var saveResponse = await _reviewService.SaveItemForTripAsync(TripId, newReview);
            if (await saveResponse.ValidateResponse() > 0)
            {
                IsSaving = false;
                return false;
            }
            return true;
        }

        private async Task DoCleanUpAfterAdd()
        {
            CurrentReview.Text = "";
            SelectedName = null;
            IsSaving = false;
            await AddBottomSheet.CloseBottomSheet();
        }
        private async Task DoCleanUpAfterUpdate()
        {
            CurrentReview.Text = "";
            SelectedName = null;
            IsSaving = false;
            await UpdateBottomSheet.CloseBottomSheet();
        }

        private void CancelSpeech()
        {
            if (_stopSpeaking?.IsCancellationRequested ?? true)
                return;

            _stopSpeaking.Cancel();
        }
    }
}
