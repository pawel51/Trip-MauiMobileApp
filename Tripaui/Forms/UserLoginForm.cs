using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tripaui.Forms
{
    public sealed partial class UserLoginForm : BaseForm
    {
        [ObservableProperty]
        [EmailAddress(ErrorMessage = "Not valid email address")]
        [Required(ErrorMessage = "Required")]
        private string email = "";

        [ObservableProperty]
        [StringLength(64, MinimumLength = 6,
            ErrorMessage = "The password should contain at least 6 characters.")]
        [Required(ErrorMessage = "Required")]
        private string password = "";


    }
}
