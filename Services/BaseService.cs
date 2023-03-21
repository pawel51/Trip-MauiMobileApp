using Microsoft.Extensions.Configuration;
using Repositories;
using Shared.Common;
using Shared.Responses;

namespace Services
{
    public abstract class BaseService<T>
    {
        private string? _key;
        protected readonly FirebaseRepository<T> _repository;
        public BaseService(FirebaseRepository<T> repository, IConfiguration configuration) 
        {
            _key = configuration != null ?
               configuration
                   .GetRequiredSection($"{nameof(GoogleApiSettings)}")
                   .Get<GoogleApiSettings>().ApiKey
               : "";
            _repository = repository;
        }

        public virtual async Task<BaseResponse> SaveItemForTripAsync(string tripId, T model)
        {
            try
            {
                await _repository.PutAsync(model, tripId);
                return new OkResponse() { Message = $"Succesfully added item for trip: {tripId}" };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }
        public async Task<BaseResponse> GetItemOfTrip(string tripId)
        {
            try
            {
                var result = await _repository.GetByIdAsync(tripId);
                if (result is null)
                {
                    return new DefaultErrorResponse() { Message = "Item does not exist" };
                }
                return new OkResponse() { Message = $"Succesfully fetched item for trip: {tripId}", Payload = result };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public async Task<BaseResponse> DeleteItemOfTrip(string tripId)
        {
            try
            {
                await _repository.DeleteByIdAsync(tripId);
                return new OkResponse() { Message = $"Succesfully deleted item for trip: {tripId}" };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public async Task<BaseResponse> DeleteItemOfTripById(string tripId, string id)
        {
            try
            {
                await _repository.DeleteOfIdByIdAsync(tripId, id);
                return new OkResponse() { Message = $"Succesfully deleted item for trip: {tripId}, by id: {id}" };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }

        public async Task<BaseResponse> GetAllItemsOfTrip(string tripId)
        {
            try
            {
                var result = await _repository.GetAllOfTripItemsAsync(tripId);

                return new OkResponse() 
                { 
                    Message = $"Succesfully fetched items for trip: {tripId}", 
                    Payload = result.Select(e => e.Object).ToList() 
                };
            }
            catch (Exception ex)
            {
                return new DefaultErrorResponse() { Message = ex.Message };
            }
        }
    }
}
