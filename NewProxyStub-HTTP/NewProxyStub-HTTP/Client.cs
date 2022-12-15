using System;
using System.IO;
using System.Net;
using System.Text.Json;
using PK = Packet;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

namespace NewProxyStub_HTTP
{
    public class ClientProxy
    {
        public string serverUrl = "http://127.0.0.1:8282/test";
        public Action<ProtocolType> OnProtocolTypeError;
        public string sessionID = String.Empty;

        #region 축약 버전
        //사용 안하는 중
        //이런식으로 다 통합해버릴 수도? 있?다 
        public void Request<T1,T2>(ProtocolType type, Protocol protocol, T1 req, Action<T2> onRes, Action<PK.Error> onErr)
        {
            PK.Packet packet = new PK.Packet(type, protocol, JsonSerializer.Serialize(req));

            //GET POST 구분해서 request
            HttpWebResponse response;
            switch (type)
            {
                case ProtocolType.GET:
                    response = RequestGET(protocol);
                    break;
                case ProtocolType.POST:
                    response = RequestPOST(JsonSerializer.Serialize(packet));
                    break;
                case ProtocolType.NOTI:
                default:
                    response = null;
                    OnProtocolTypeError?.Invoke(type);
                    break;
            }

            HandleResponseContent(response, onRes, onErr);
        }
        #endregion

        #region POST
        public void RequestJoinUser(PK.ReqJoinUser req, Action<PK.ResJoinUser> onRes, Action<PK.Error> onErr)
        {
            PK.Packet packet = new PK.Packet(ProtocolType.POST, Protocol.REQ_JOIN, JsonSerializer.Serialize(req));

            //send and return result
            HttpWebResponse response = RequestPOST(JsonSerializer.Serialize(packet));

            HandleResponseContent(response, onRes, onErr);
        }
        public void RequestLogin(PK.ReqLoginAuth req, Action<PK.ResLoginAuth> onRes, Action<PK.Error> onErr)
        {
            PK.Packet packet = new PK.Packet(ProtocolType.POST, Protocol.REQ_LOGIN, JsonSerializer.Serialize(req));

            //send and return result
            HttpWebResponse response = RequestPOST(JsonSerializer.Serialize(packet));

            HandleResponseContent(response, onRes, onErr);
        }
        public void RequestMakeRoom(Action<PK.ResMakeRoom> onRes, Action<PK.Error> onErr)
        {
            PK.Packet packet = new PK.Packet(ProtocolType.POST, Protocol.REQ_MAKEROOM);

            //send and return result
            HttpWebResponse response = RequestPOST(JsonSerializer.Serialize(packet));

            HandleResponseContent(response, onRes, onErr);
        }
        public void RequestJoinRoom(PK.ReqJoinRoom req, Action onRes, Action<PK.Error> onErr)
        {
            PK.Packet packet = new PK.Packet(ProtocolType.POST, Protocol.REQ_JOINROOM, JsonSerializer.Serialize(req));

            //send and return result
            HttpWebResponse response = RequestPOST(JsonSerializer.Serialize(packet));

            HandleResponseContent(response, onRes, onErr);
        }
        public void RequestSendNotiChat(PK.ReqSendChat req, Action onRes, Action<PK.Error> onErr)
        {
            PK.Packet packet = new PK.Packet(ProtocolType.POST, Protocol.REQ_SENDCHAT, JsonSerializer.Serialize(req));

            //send and return result
            HttpWebResponse response = RequestPOST(JsonSerializer.Serialize(packet));

            HandleResponseContent(response, onRes, onErr);
        }
        #endregion

        #region GET
        public void RequestGetRoomList(PK.Packet req, Action<PK.ResRoomList> onRes, Action<PK.Error> onErr)
        {
            //send and return result
            HttpWebResponse response = RequestGET(req.protocol);

            HandleResponseContent(response, onRes, onErr);
        }
        #endregion

        private HttpWebResponse RequestPOST(string json)
        {
            var httpReq = HttpWebRequest.Create(serverUrl);
            httpReq.Method = "POST";
            httpReq.ContentType = "application/json";
            httpReq.Headers.Add("sessionID", sessionID);
            StreamWriter writer = new StreamWriter(httpReq.GetRequestStream());
            writer.Write(json);
            writer.Close();

            try
            {
                return (HttpWebResponse)httpReq.GetResponse(); //응답
            }
            catch (WebException e)
            {
                return (HttpWebResponse)e.Response;
            }
        }
        private HttpWebResponse RequestGET(Protocol protocol)
        {
            String sData = String.Format("?req={0}", protocol);
            var httpReq = HttpWebRequest.Create(serverUrl+ sData);
            httpReq.Method = "GET";
            httpReq.Headers.Add("sessionID", sessionID);

            try
            {
                return (HttpWebResponse)httpReq.GetResponse(); //응답
            }
            catch (WebException e)
            {
                return (HttpWebResponse)e.Response;
            }

        }
        public Action<int> OnResponseWrongCode;
        private void HandleResponseContent<T>(HttpWebResponse response, Action<T> onRes, Action<PK.Error> onErr)
        {
            string resultString;
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                resultString = streamReader.ReadToEnd();
            int statusCode = (int)response.StatusCode / 100 * 100;
            switch (statusCode)
            {
                case ProtocolConst.RESULT_SUCCESS:
                    T res = JsonSerializer.Deserialize<T>(resultString);
                    onRes(res);
                    break;

                case ProtocolConst.RESULT_REDIRECTION:
                    //onErr(JsonSerializer.Deserialize<PK.Error>(resultString));
                    break;

                case ProtocolConst.RESULT_ERROR_CLIENT:
                    onErr(JsonSerializer.Deserialize<PK.Error>(resultString));
                    break;

                case ProtocolConst.RESULT_ERROR_SERVER:
                    onErr(JsonSerializer.Deserialize<PK.Error>(resultString));
                    break;

                default:
                    Console.WriteLine(statusCode+" "+(int)response.StatusCode + " wrong code");
                    OnResponseWrongCode((int)response.StatusCode);
                    break;
            }
        }
        private void HandleResponseContent(HttpWebResponse response, Action onRes, Action<PK.Error> onErr)
        {
            string resultString;
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                resultString = streamReader.ReadToEnd();
            int statusCode = (int)response.StatusCode / 100 * 100;
            switch (statusCode)
            {
                case ProtocolConst.RESULT_SUCCESS:
                    onRes();
                    break;

                case ProtocolConst.RESULT_REDIRECTION:
                    //onErr(JsonSerializer.Deserialize<PK.Error>(resultString));
                    break;

                case ProtocolConst.RESULT_ERROR_CLIENT:
                    onErr(JsonSerializer.Deserialize<PK.Error>(resultString));
                    break;

                case ProtocolConst.RESULT_ERROR_SERVER:
                    onErr(JsonSerializer.Deserialize<PK.Error>(resultString));
                    break;

                default:
                    Console.WriteLine(statusCode + " " + (int)response.StatusCode + " wrong code");
                    OnResponseWrongCode((int)response.StatusCode);
                    break;
            }
        }
    }

    public class ClientStub
    {
        public Action<Protocol> OnProtocolError;
        public ClientStub()
        {
            // create client instance 
            this.client = new MqttClient("127.0.0.1");
            this.client.MqttMsgPublishReceived += OnRecv;

            SubscribeTopic("/");

            string clientId = Guid.NewGuid().ToString();
            this.client.Connect(clientId);
        }

        public void ChangeTopic(string newTopic)
        {
            this.client.Unsubscribe(new string[] { topic });
            SubscribeTopic(newTopic);
        }

        private void OnRecv(object sender, MqttMsgPublishEventArgs e)
        {
            string jsonData = Encoding.Default.GetString(e.Message);
            PK.Packet packet = JsonSerializer.Deserialize<PK.Packet>(jsonData);

            DispatchPacket(packet);
        }

        private void DispatchPacket(PK.Packet packet)
        {
            switch (packet.protocol)
            {
                case Protocol.NOTI_CHAT:
                    RecvNotiChat(packet);
                    break;
                default:
                    OnProtocolError?.Invoke(packet.protocol);
                    break;

            }
        }

        public delegate void PacketHandler(PK.NotiChat notiChat);
        public PacketHandler OnRecvNotiChat;

        private void RecvNotiChat(PK.Packet packet)
        {
            if (OnRecvNotiChat == null)
            {
                Console.WriteLine("OnRecvNotiChat    but Handler doesn't exist");
                return;
            }
            PK.NotiChat noti = JsonSerializer.Deserialize<PK.NotiChat>(packet.jsonData);
            OnRecvNotiChat?.Invoke(noti);
        }

        private void SubscribeTopic(string topic)
        {
            this.topic = topic;
            this.client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        private string topic = "/";
        private MqttClient client;
    }
}
