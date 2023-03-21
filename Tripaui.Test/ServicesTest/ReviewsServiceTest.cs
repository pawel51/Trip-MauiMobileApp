using Repositories;
using Services;
using Shared.Entities;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tripaui.Test.ServicesTest
{
    public class ReviewsServiceTest : FirebaseServiceTest
    {
        private string tripId = "tripofid1";
        ReviewsService _reviewsService;
        TripsService _tripsService;
        public ReviewsServiceTest()
        {
            _reviewsService = new ReviewsService(new ReviewsRepository(ct), null);
            _tripsService = new TripsService(new TripsRepository(ct), null);
        }

        [Fact]
        public async Task ShouldAddNewReview()
        {
            await _reviewsService.DeleteItemOfTrip(tripId);
            ReviewModel review1 = new ReviewModel() { Text = "That trip was very nice 11111111!" };
            ReviewModel review2 = new ReviewModel() { Text = "That trip was very nice 222222222!" };
            ReviewModel review3 = new ReviewModel() { Text = "That trip was very nice 3333333333!" };
            var response1 = await _reviewsService.SaveItemForTripAsync(tripId, review1) as OkResponse;
            var response2 = await _reviewsService.SaveItemForTripAsync(tripId, review2) as OkResponse;
            var response3 = await _reviewsService.SaveItemForTripAsync(tripId, review3) as OkResponse;
            Assert.NotNull(response1);
            Assert.NotNull(response2);
            Assert.NotNull(response3);
            var response4 = await _reviewsService.GetAllItemsOfTrip(tripId) as OkResponse;
            Assert.Equal(3, ((List<ReviewModel>)response4.Payload).Count);
            Assert.NotNull(response4);
        }

        [Fact]
        public async Task ShouldDeleteReview()
        {
            await _reviewsService.DeleteItemOfTrip(tripId);
            ReviewModel review = new ReviewModel() { Text = "That trip was very nice!" };
            var addResponse = await _reviewsService.SaveItemForTripAsync(tripId, review) as OkResponse;
            Assert.NotNull(addResponse);

            var getResponse = await _reviewsService.GetItemOfTrip(tripId) as OkResponse;
            Assert.NotNull(getResponse);
            if (getResponse == null)
            {
                throw new Exception("Cant test deletion because addition is not working");
            }
            var addedItem = getResponse.Payload as ReviewModel;
            Assert.NotNull(addedItem);

            var deleteResponse = await _reviewsService.DeleteItemOfTrip(tripId) as OkResponse;
            Assert.NotNull(deleteResponse);
        }

    }
}
