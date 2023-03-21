using CommunityToolkit.Maui.Views;
using Shared.Responses;
using Tripaui.Views.Controlls;

namespace Tripaui.Extensions
{
    public static class ResponseValidationExtension
    {
        public static async Task<int> ValidateResponse(this BaseResponse response)
        {
            int error = 0;
            if (response.Status == 1)
            {
                await Shell.Current
                    .ShowPopupAsync(new ValidationErrorPopup(response.Message));
                error++;
            }
            return error;
        }
    }
}
