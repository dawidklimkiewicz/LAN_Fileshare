namespace LAN_Fileshare.Models
{
    public class PacketTypes
    {
        public enum PacketType
        {
            Acknowledge,
            Ping,
            KeepAlive,
            HostInfo,
            HostAndFileInfo,
            HostInfoReply,
            FileInformation,
            RemoveFile,
            FileRequest,
            FileData,
            StopFileTransmission,
            FileDataAck,
            InitialFileInformation,
            InitialFileInformationReply,
        }
    }
}
