namespace Shared.Responses
{
    public sealed class OkResponse : BaseResponse
    {
        public OkResponse() : base(0)
        {

        }

        public object Payload { get; set; }
    }
}
