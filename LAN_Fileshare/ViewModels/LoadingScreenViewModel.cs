using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LAN_Fileshare.ViewModels
{
    [ObservableRecipient]
    public partial class LoadingScreenViewModel : ObservableValidator
    {
        [ObservableProperty]
        [Required]
        [MinLength(1)]
        private string _usernameInputContent = "Test";
    }
}
