using Google.Protobuf;
using ProtobufSchema;
using ProxyStub_MQTT.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyStub_MQTTTest
{
    public  class ChatClientProxy : ProxyStub_MQTT.Client.Proxy
    {
        public ChatClientProxy(Stub clientStub, string userId) : base(clientStub)
        {
            byte[] sendBuff = Msg.MsgToByte(new Msg { Data = "hi!", SenderId = userId });
            Send("", new Packet
            {
                Type = (int)ProtobufSchema.PacketType.Noti,
                Command = "",
                Payload = ByteString.CopyFrom(sendBuff)
            });
        }
    }
    public class ChatClientStub : ProxyStub_MQTT.Client.Stub
    {

    }
}
