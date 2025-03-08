using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.ViewModels;

namespace LAN_Fileshare.Messages
{
    public class SelectedHostChangedMessage : ValueChangedMessage<HostListingItemViewModel>
    {
        public SelectedHostChangedMessage(HostListingItemViewModel value) : base(value)
        {
        }
    }
}
