using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class BytesTransmittedChangedMessage : ValueChangedMessage<long>
    {
        public IFile File { get; }
        public BytesTransmittedChangedMessage(long value, IFile file) : base(value)
        {
            File = file;
        }
    }
}
