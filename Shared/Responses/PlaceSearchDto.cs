namespace Shared.Responses
{
    public sealed class PlaceSearchDto<T> : BaseResponse
    {
        public PlaceSearchDto() : base(0)
        {
            PlacesList = new();
        }

        public List<T> PlacesList { get; set; } = new();
    }
}
