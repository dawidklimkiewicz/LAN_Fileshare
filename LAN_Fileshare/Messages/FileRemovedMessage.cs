using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class FileRemovedMessage : ValueChangedMessage<IFile>
    {
        public FileRemovedMessage(IFile value) : base(value)
        {
        }
    }
}
