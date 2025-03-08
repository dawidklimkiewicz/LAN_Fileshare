using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class FileAddedMessage : ValueChangedMessage<IFile>
    {
        public IFile File { get; }
        public FileAddedMessage(IFile value) : base(value)
        {
            File = value;
        }
    }
}
