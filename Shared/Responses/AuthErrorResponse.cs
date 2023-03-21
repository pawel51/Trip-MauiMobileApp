namespace Shared.Responses
{
    public sealed class AuthErrorResponse : BaseResponse
    {
        public AuthErrorResponse() : base(1)
        {

        }
        public string Reason { get; set; } = "";
    }
}
