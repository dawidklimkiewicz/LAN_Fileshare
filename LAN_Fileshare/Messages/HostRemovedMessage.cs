using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class HostRemovedMessage : ValueChangedMessage<Host>
    {
        public HostRemovedMessage(Host value) : base(value)
        {
        }
    }
}
