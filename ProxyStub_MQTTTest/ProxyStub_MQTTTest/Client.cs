using ProtobufSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ProxyStub_MQTT.Client
{
    public class Stub
    {
        public Stub()
        {
            // create client instance 
            m_client = new MqttClient("127.0.0.1");

            // register to message received 
            m_client.MqttMsgPublishReceived += OnRecv;

            string clientId = Guid.NewGuid().ToString();
            m_client.Connect(clientId);

            m_client.Subscribe(new string[] { m_rootTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        public void Subscribe(string topic, Action<Packet> action)
        {
            m_client.Subscribe(new string[] { m_rootTopic + topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            RegisterRecvHandler(m_rootTopic + topic, action);
        }

        public void Disconnect()
        {
            m_client.Disconnect();
        }

        public void Unsubscribe(string topic)
        {
            m_client.Unsubscribe(new string[] { m_rootTopic + topic });
            UnregisterRecvHandler(m_rootTopic + topic);
        }

        private void OnRecv(object sender, MqttMsgPublishEventArgs e)
        {
            Packet packet = Packet.Parser.ParseFrom(e.Message);
            if(m_topicPacketHandlers.TryGetValue(e.Topic, out Action<Packet> action))
                action(packet); 
        }

        private void RegisterRecvHandler(string command, Action<ProtobufSchema.Packet> _action)
        {
            m_topicPacketHandlers.Add(command, _action);
        }
        private void UnregisterRecvHandler(string _command)
        {
            m_topicPacketHandlers.Remove(_command);
        }

        public void BindProxy(Proxy clientProxy) { this.m_clientProxy = clientProxy; }
        public string GetRootTopic() { return m_rootTopic; }

        protected const string m_rootTopic = "/client/";
        protected MqttClient m_client;

        private Proxy m_clientProxy;
        private Dictionary<string, Action<Packet>> m_topicPacketHandlers = new Dictionary<string, Action<Packet>>();

    }
    public class Proxy
    {
        public Proxy(Stub clientStub)
        { 
            // create client instance 
            m_client = new MqttClient("127.0.0.1");

            string clientId = Guid.NewGuid().ToString();
            m_client.Connect(clientId);

            clientStub.BindProxy(this);
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

        }
        public void SetRootDestTopic(string rootDestTopic) { this.m_destTopicRoot = rootDestTopic; }

        protected MqttClient m_client;
        protected string m_destTopicRoot = "/server/";
    }
}
