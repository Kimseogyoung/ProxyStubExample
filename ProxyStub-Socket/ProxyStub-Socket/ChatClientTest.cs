using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using ProtobufSchema;
using Google.Protobuf;
using System.IO;
using ProxyStub_Socket.Client;

namespace ProxyStub_Socket
{

    class ChatClientTest
    {
        static void Main(string[] args)
        {
            Stub cStub = new Stub();
            Proxy cProxy = new Proxy(cStub);

            cStub.RegisterRecvHandler(Protocol.ResAction.Hello, Hello);
            cStub.RegisterRecvHandler(Protocol.NotiAction.ServerChat, Chat);

            string cmd = string.Empty;
            byte[] receiverBuff = new byte[Byte.MaxValue];

            try
            {
                while ((cmd = Console.ReadLine()) != "Q")
                {

                    Msg msg = new Msg();
                    msg.Data = cmd;
                    byte[] sendBuff = Msg.MsgToByte(msg);

                    // 서버에 데이타 전송
                    cProxy.SendNoti(new Packet
                    {
                        Type = (int)Protocol.PacketType.Noti,
                        Action = (int)Protocol.NotiAction.ServerChat,
                        Payload = ByteString.CopyFrom(sendBuff)

                    });

                }
                cStub.Disconnect2Server();
            }
            catch
            {
                cStub.Disconnect2Server();
            }
        }
        
        static void Hello(Packet packet)
        {
            Msg msg = Msg.Parser.ParseFrom(packet.Payload.ToByteArray());
            Console.WriteLine(msg.Data);
        }

        static void Chat(Packet packet)
        {
            Msg msg = Msg.Parser.ParseFrom(packet.Payload.ToByteArray());
            Console.WriteLine("chat : " + msg.Data);
        }
    }
}
