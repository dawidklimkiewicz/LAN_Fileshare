using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class FileRemovedMessage : ValueChangedMessage<IFile>
    {
        public IFile File { get; }
        public Host Host { get; }
        public FileRemovedMessage(IFile value, Host host) : base(value)
        {
            File = value;
            Host = host;
        }
    }
}
