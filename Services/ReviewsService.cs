using Microsoft.Extensions.Configuration;
using Repositories;
using Shared.Entities;
using Shared.Responses;

namespace Services
{
    public sealed class ReviewsService : BaseService<ReviewModel>
    {
        public ReviewsService(ReviewsRepository repository, IConfiguration configuration) 
            : base( repository, configuration)
        {

        }

        public override async Task<BaseResponse> SaveItemForTripAsync(string tripId, ReviewModel model)
        {
            try
            {
                await _repository.PostAtIdAsync(model, tripId, model.Id.ToString());
                return new OkResponse() { Message = $"Succesfully added item for trip: {tripId}" };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }
    }
}
