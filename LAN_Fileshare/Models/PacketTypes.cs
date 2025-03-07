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
            HostInfoReply,
        }
    }
}
