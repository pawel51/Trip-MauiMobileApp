using System.Collections.ObjectModel;

namespace Shared.Responses
{
    public class CollectionResponse<T> : BaseResponse
    {
        public CollectionResponse() : base(0)
        {
        }
        public ObservableCollection<T> Collection { get; set; } = new();
    }
}
