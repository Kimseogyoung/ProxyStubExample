namespace ProtobufSchema
{//임시
    public enum PacketType
    {
        Req, Res, Noti
    }
    public enum ReqAction
    {
        Hello, GetServerName
    }
    public enum ResAction
    {
        Hello, GetServerName
    }
    public enum NotiAction
    {
        ServerChat
    }
}
