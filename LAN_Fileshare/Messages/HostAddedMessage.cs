using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class HostAddedMessage : ValueChangedMessage<Host>
    {
        public HostAddedMessage(Host value) : base(value)
        {
        }
    }
}
