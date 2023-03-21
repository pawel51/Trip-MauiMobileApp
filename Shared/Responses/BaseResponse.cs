namespace Shared.Responses
{
    public abstract class BaseResponse
    {
        public BaseResponse(int status)
        {
            Status = status;
        }
        public int Status { get; set; }

        public string Message { get; set; } = "";
    }
}
