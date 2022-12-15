using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyStub_MQTTTest;
using ProtobufSchema;
using Google.Protobuf;

namespace ClientTest
{
    public class ClientTest
    {
        private string m_myId = "user3";
        private ChatServerProxy m_sProxy;

        public ClientTest(string _myId)
        {
            this.m_myId = _myId;
        }

        public void Start()
        {
            //client
            ChatClientStub cStub = new ChatClientStub();
            ChatClientProxy cProxy = new ChatClientProxy(cStub, m_myId);
            cStub.Subscribe("chat/secret/" + m_myId, SecretChat);
            cStub.Subscribe("chat", Chat);
            cStub.Subscribe("chat/hello/" + m_myId, Hello);
            cStub.Subscribe("get/userList/" + m_myId, ShowUserList);


            //client console
            string cmd = string.Empty;
            byte[] receiverBuff = new byte[Byte.MaxValue];
            try
            {
                while ((cmd = Console.ReadLine()) != "Q")
                {

                    byte[] sendBuff = null;

                    if (cmd.Length <= 0)
                        continue;

                    //1. 귓속말
                    if (cmd[0] == 's' && cmd.Length >= 3)
                    {
                        string userName = cmd.Substring(2);
                        Console.WriteLine(userName + "님께 보낼 메시지를 입력하세요");
                        cmd = Console.ReadLine();

                        sendBuff = Msg.MsgToByte(new Msg
                        {
                            Data = cmd,
                            SenderId = m_myId,
                            ReceiverId = userName
                        });

                        cProxy.Send("chat/secret", new Packet
                        {
                            Type = (int)ProtobufSchema.PacketType.Noti,
                            Command = "chat/secret",
                            Payload = ByteString.CopyFrom(sendBuff)
                        });
                        continue;
                    }

                    //2.Get UserList
                    if (cmd.Equals("get"))
                    {
                        cProxy.Send("get/userList", new Packet
                        {
                            Type = (int)ProtobufSchema.PacketType.Req,
                            Command = "get/userList/" + m_myId
                        });
                        continue;
                    }

                    //3. 일반 채팅
                    sendBuff = Msg.MsgToByte(new Msg
                    {
                        Data = cmd,
                        SenderId = m_myId

                    });
                    cProxy.Send("chat", new Packet
                    {
                        Type = (int)ProtobufSchema.PacketType.Noti,
                        Command = "chat",
                        Payload = ByteString.CopyFrom(sendBuff)

                    });
                }
                cStub.Disconnect();
            }
            catch
            {
                cStub.Disconnect();
            }
        }

        #region clientStubMethod
        public void ShowUserList(Packet _packet)
        {
            Msg msg = Msg.Parser.ParseFrom(_packet.Payload.ToByteArray());
            Console.WriteLine("[Client Side] GET userList\n" + msg.Data);
        }

        public void Hello(Packet _packet)
        {
            Msg msg = Msg.Parser.ParseFrom(_packet.Payload.ToByteArray());
            Console.WriteLine("[Client Side] " + msg.Data);
        }

        public void Chat(Packet _packet)
        {
            Msg msg = Msg.Parser.ParseFrom(_packet.Payload.ToByteArray());
            Console.WriteLine("[Client Side] chat " + msg.SenderId + " : " + msg.Data);
        }
        public void SecretChat(Packet _packet)
        {
            Msg msg = Msg.Parser.ParseFrom(_packet.Payload.ToByteArray());
            Console.WriteLine("[Client Side] " + msg.SenderId + "의 귓속말 : " + msg.Data);
        }
        #endregion
    }
}
