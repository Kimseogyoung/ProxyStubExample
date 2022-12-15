using System.IO;
using System.Net.Sockets;
using Google.Protobuf;
using ProtobufSchema;
using ProxyStub_Socket.Server;

namespace ProxyStub_SocketTest
{
    public class ChatServerProxy : Proxy
    {
        public void SendChatAllClient(Msg msg)
        {
            byte[] sendBuff = Msg.MsgToByte(msg);

            SendNoti(new Packet
            {
                Type = (int)Protocol.PacketType.Noti,
                Action = (int)Protocol.NotiAction.ServerChat,
                Payload = ByteString.CopyFrom(sendBuff)
            });
        }
        public void SendChatClient(Socket destSocket, Msg msg)
        {
            byte[] sendBuff = Msg.MsgToByte(msg);

            SendRes(destSocket, new Packet
            {
                Type = (int)Protocol.PacketType.Res,
                Action = (int)Protocol.ResAction.Hello,
                Payload = ByteString.CopyFrom(sendBuff)
            });
        }
    }

    public class ChatServerStub : Stub
    {
        public ChatServerStub(ChatServerProxy serverProxy) : base(serverProxy)
        {
        }
        
        override protected void OnConnected(Socket destSocket)
        {
            //hello send
            Msg msg = new Msg();
            msg.Data = "hello! connected!";

            ((ChatServerProxy)serverProxy).SendChatClient(destSocket, msg);
        }
    }
}
