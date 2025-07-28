using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class HostUsernameChangedMessage : ValueChangedMessage<Host>
    {
        public HostUsernameChangedMessage(Host value) : base(value)
        {
        }
    }
}
