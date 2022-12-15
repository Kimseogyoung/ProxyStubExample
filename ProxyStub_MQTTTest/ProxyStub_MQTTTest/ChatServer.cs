using System.IO;
using System.Net.Sockets;
using Google.Protobuf;
using ProtobufSchema;
using ProxyStub_MQTT.Server;

namespace ProxyStub_MQTTTest
{
    public class ChatServerProxy : Proxy
    {
        public void SendChat2Client(Msg msg, string destTopic = "")
        {
            MemoryStream stream = new MemoryStream();
            using (var writeStream = new Google.Protobuf.CodedOutputStream(stream, true))
                msg.WriteTo(writeStream);

            byte[] sendBuff = stream.ToArray();

            string topic = "chat";
            if (destTopic.Length != 0)
                topic += destTopic;
            Send(topic, new Packet
            {
                Type = (int)ProtobufSchema.PacketType.Noti,
                Command = topic,
                Payload = ByteString.CopyFrom(sendBuff)
            });
        }
    }

    public class ChatServerStub : Stub
    {
        public ChatServerStub(Proxy serverProxy) : base(serverProxy)
        {
        }
        
        override protected void OnConnected()
        {
            //hello send
            Msg msg = new Msg();
            msg.Data = "hello! connected!";
            MemoryStream stream = new MemoryStream();
            using (var writeStream = new Google.Protobuf.CodedOutputStream(stream, true))
                msg.WriteTo(writeStream);
            byte[] sendBuff = stream.ToArray();

            m_serverProxy.Send("hello", new Packet
            {
                Type = (int)ProtobufSchema.PacketType.Noti,
                Command = "hello",
                Payload = ByteString.CopyFrom(sendBuff)
            });
        }
        
    }
}
