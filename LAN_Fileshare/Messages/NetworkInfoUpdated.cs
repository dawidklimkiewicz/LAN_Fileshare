using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Stores;

namespace LAN_Fileshare.Messages
{
    public class NetworkInfoUpdated : ValueChangedMessage<AppStateStore>
    {
        public NetworkInfoUpdated(AppStateStore value) : base(value)
        {
        }
    }
}
