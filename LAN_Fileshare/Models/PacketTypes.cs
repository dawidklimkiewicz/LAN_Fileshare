namespace LAN_Fileshare.Models
{
    public class PacketTypes
    {
        public enum PacketType
        {
            Acknowledge,
            Ping,
            HostInfo,
            HostInfoReply,
            FileInformation,
            RemoveFile,
            FileRequest,
            FileData,
            StopFileTransmission,
            FileDataAck,
        }
    }
}
