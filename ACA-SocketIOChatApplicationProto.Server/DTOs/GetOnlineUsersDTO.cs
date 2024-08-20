namespace ACA_SocketIOChatApplicationProto.Server.DTOs
{
    public class GetOnlineUsersDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }
    }
}
