using ProtobufSchema;
using ProtobufSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ProxyStub_MQTT.Server
{
    public class Stub
    {
        public Stub(Proxy serverProxy)
        {
            // create client instance 
            m_client = new MqttClient("127.0.0.1");

            // register to message received 
            m_client.MqttMsgPublishReceived += OnRecv;

            string clientId = Guid.NewGuid().ToString();
            m_client.Connect(clientId);
            BindProxy(serverProxy);

            m_client.Subscribe(new string[] { rootTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public void OnRecv(object sender, MqttMsgPublishEventArgs e)
        {
            Packet packet = Packet.Parser.ParseFrom(e.Message);
            switch (packet.Type)
            {
                case  (int)PacketType.Req:
                    OnRecvRequest(e.Topic, packet);
                    break;
                case (int)PacketType.Noti:
                    OnRecvNoti(e.Topic, packet);
                    break;
                case (int)PacketType.Res:
                    break;
                default:
                    Console.WriteLine("Server.cs OnRecv() : wrong packet type " + packet.Type);
                    break;
            }

        }

        public void Subscribe(string topic, Func<Packet, Packet> func)
        {
            m_client.Subscribe(new string[] { rootTopic + topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            RegisterResHandler(rootTopic + topic, func);
        }

        public void Subscribe(string topic, Action<Packet> action)
        {
            m_client.Subscribe(new string[] { rootTopic + topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            RegisterVoidHandler(rootTopic + topic, action);
        }

        public void Unsubscribe(string topic)
        {
            m_client.Unsubscribe(new string[] { rootTopic + topic });
        }

        public void Disconnect()
        {
            m_client.Disconnect();
        }
       
        private void OnRecvRequest(string recvTopic, Packet packet)
        {
            Packet resPacket = null;
            if (m_reqHandler.TryGetValue(recvTopic, out Func<Packet, Packet> action))
                resPacket = action.Invoke(packet);

            m_serverProxy.Send(packet.Command, resPacket);
        }

        private void OnRecvNoti(string recvTopic, Packet packet)
        {
            if (m_notiHandler.TryGetValue(recvTopic, out Action<Packet> action) == true)
                action.Invoke(packet);
        }

        private void BindProxy(Proxy proxy)
        {
            this.m_serverProxy = proxy;
        }

        private void RegisterResHandler(string command, Func<Packet, Packet> func)
        {
            m_reqHandler.Add(command, func);
        }
        private void RegisterVoidHandler(string _command, Action<Packet> _action)
        {
            m_notiHandler.Add(_command, _action);
        }

        virtual protected void OnConnected() { }


        protected const string rootTopic = "/server/";

        protected MqttClient m_client;
        protected Proxy m_serverProxy;
        private Dictionary<string, Func<Packet, Packet>> m_reqHandler = new Dictionary<string, Func<Packet, Packet>>();
        private Dictionary<string, Action<Packet>> m_notiHandler = new Dictionary<string, Action<Packet>>();
    }
    public class Proxy
    {
        public Proxy()
        {
            // create client instance 
            m_client = new MqttClient("127.0.0.1");

            string clientId = Guid.NewGuid().ToString();
            m_client.Connect(clientId);
        }
        public void Send(string destTopic, Packet packet)
        {
            //serialize
            MemoryStream stream = new MemoryStream();
            using (var writeStream = new Google.Protobuf.CodedOutputStream(stream, true))
                packet.WriteTo(writeStream);

            //send
            byte[] sendBuff = stream.ToArray();
            m_client.Publish(m_destTopicRoot + destTopic, sendBuff, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

            //callback
            OnSend?.Invoke();
        }
        public void SetRootDestTopic(string rootDestTopic) { this.m_destTopicRoot = rootDestTopic; }

        protected string m_destTopicRoot = "/client/";
        protected MqttClient m_client;

        public Action OnSend;

    }
}
