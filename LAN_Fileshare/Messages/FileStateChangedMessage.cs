using CommunityToolkit.Mvvm.Messaging.Messages;
using LAN_Fileshare.Models;

namespace LAN_Fileshare.Messages
{
    public class FileStateChangedMessage : ValueChangedMessage<FileState>
    {
        public IFile File { get; set; }
        public FileStateChangedMessage(FileState value, IFile file) : base(value)
        {
            File = file;
        }
    }
}
