using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tripaui.Forms
{
    public sealed partial class UserRegisterForm : BaseForm
    {
        [ObservableProperty]
        [EmailAddress(ErrorMessage = "Not valid email address")]
        [Required(ErrorMessage = "Required")]
        private string email = "";

        [ObservableProperty]
        [StringLength(64, MinimumLength = 4,
            ErrorMessage = "Username should contain at least 4 characters.")]
        [Required(ErrorMessage = "Required")]
        private string username = "";

        [ObservableProperty]
        [StringLength(64, MinimumLength = 6,
            ErrorMessage = "Password should contain at least 6 characters.")]
        [Required(ErrorMessage = "Required")]
        private string password = "";

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        private string repeatPassword = "";

    }
}
