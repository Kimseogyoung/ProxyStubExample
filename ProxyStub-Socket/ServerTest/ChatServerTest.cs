using System;
using System.IO;
using Google.Protobuf;
using ProtobufSchema;
using ProxyStub_SocketTest;

namespace ServerTest
{
    internal class ChatServerTest
    {
        static private ChatServerProxy proxy = new ChatServerProxy();
        static private ChatServerStub stub;
        static void Main(string[] args)
        {
            stub = new ChatServerStub(proxy);
            stub.RegisterRecvHandler(Protocol.NotiAction.ServerChat, SendNewChat);
            stub.RegisterRecvHandler(Protocol.ReqAction.GetServerName, OnRecvGetServerName);

            stub.StartServer();

            // q키를 누르면 서버 종료
            Console.WriteLine("Press the q key to exit.");

            while (true)
            {
                string k = Console.ReadLine();
                if ("q".Equals(k, StringComparison.OrdinalIgnoreCase))
                    break;
            }
            stub.StopServer();

        }
        static void SendNewChat(Packet packet)
        {
            Msg msg = Msg.Parser.ParseFrom(packet.Payload.ToByteArray());
            Console.WriteLine(msg.Data);
            proxy.SendChatAllClient(msg);
        }
        static Packet OnRecvGetServerName(Packet packet)
        {
            //hello send
            Msg msg = new Msg();
            msg.Data = "SeogyoungServer";

            return new Packet
            {
                Type = (int)Protocol.PacketType.Res,
                Action = (int)Protocol.ResAction.GetServerName,
                Payload = ByteString.CopyFrom(Msg.MsgToByte(msg))
            };

        }
    }
}
