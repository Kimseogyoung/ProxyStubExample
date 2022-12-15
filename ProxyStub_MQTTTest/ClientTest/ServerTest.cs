using System;
using System.Linq;
using System.Net.Sockets;
using ProtobufSchema;
using Google.Protobuf;
using ProxyStub_MQTT;
using System.IO;
using System.Security.Policy;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using System.Text;
using ProxyStub_MQTTTest;

namespace ClientTest
{

    class ServerTest
    {
        ChatServerProxy m_sProxy;
        static private List<string> m_userList = new List<string>();
        public void Start()
        {

            //server setting
            m_sProxy = new ChatServerProxy();
            ChatServerStub sStub = new ChatServerStub(m_sProxy);
            sStub.Subscribe("", OnConnect);
            sStub.Subscribe("chat", OnRecvChat);
            sStub.Subscribe("chat/secret", OnRecvSecretChat);
            sStub.Subscribe("get/userList", GetUserList);

        }

        #region serverMethod
        public void OnConnect(Packet _packet)
        {
            Msg msg = Msg.Parser.ParseFrom(_packet.Payload.ToByteArray());
            Console.WriteLine("[Server Side] : Connect " + msg.SenderId);

            msg.Data = "welcome! " + msg.SenderId;
            m_userList.Add(msg.SenderId);

            m_sProxy.SendChat2Client(msg, "/hello/" + msg.SenderId);
        }
        public void OnRecvChat(Packet _packet)
        {
            Msg msg = Msg.Parser.ParseFrom(_packet.Payload.ToByteArray());
            Console.WriteLine("[Server Side] : Chat " + msg.Data);
            m_sProxy.SendChat2Client(msg);
        }
        public void OnRecvSecretChat(Packet packet)
        {
            Msg msg = Msg.Parser.ParseFrom(packet.Payload.ToByteArray());
            Console.WriteLine("[Server Side] : Secret Chat " + msg.Data);

            m_sProxy.SendChat2Client(msg, "/secret/" + msg.ReceiverId);
        }
        public Packet GetUserList(Packet packet)
        {
            Msg msg = Msg.Parser.ParseFrom(packet.Payload.ToByteArray());
            Console.WriteLine("[Server Side] : GET UserList ");

            StringBuilder myStringBuilder = new StringBuilder();

            for (int i = 0; i < m_userList.Count; i++)
                myStringBuilder.AppendLine(m_userList[i]);
            msg.Data = myStringBuilder.ToString();

            return new Packet
            {
                Type = (int)PacketType.Res,
                Payload = ByteString.CopyFrom(Msg.MsgToByte(msg))
            };
        }
        #endregion
    }
}

