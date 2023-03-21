using CommunityToolkit.Maui.Views;
using Tripaui.Forms;
using Tripaui.Views.Controlls;

namespace Tripaui.Extensions
{
    public static class FormValidationExtension
    {
        public static async Task<int> ValidateForm(this BaseForm form)
        {
            int error = 0;
            foreach (var valError in form.GetErrors())
            {
                await Shell.Current
                    .ShowPopupAsync(new ValidationErrorPopup(valError.ErrorMessage));
                error++;
            }
            return error;
        }

        public static async Task<int> CheckIfPasswordsMatch(this UserRegisterForm form)
        {
            int error = 0;
            if (form.Password != form.RepeatPassword)
            {
                await Shell.Current
                    .ShowPopupAsync(new ValidationErrorPopup("Passwords do not match"));
                error++;
            }
            return error;
        }

        public static async Task<int> CheckIfStartDateIsSmallerThenEndDate(this TripForm form)
        {
            int error = 0;
            if (form.StartDate >= form.EndDate)
            {
                await Shell.Current
                    .ShowPopupAsync(new ValidationErrorPopup("Start date should precede end date"));
                error++;
            }
            return error;
        }
    }
}
