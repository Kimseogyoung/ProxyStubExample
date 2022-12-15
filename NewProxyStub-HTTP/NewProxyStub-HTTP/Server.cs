using SimpleHttpServer;
using System.Threading;
using System.Text.Json;
using SimpleHttpServer.Models;
using System.Collections.Generic;
using System;
using PK = Packet;
using Packet;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using Microsoft.Win32;

namespace NewProxyStub_HTTP
{
    public class ServerProxy
    {
        public ServerProxy()
        {
            // create client instance 
            this.client = new MqttClient("127.0.0.1");

            string clientId = Guid.NewGuid().ToString();
            this.client.Connect(clientId);
        }
        public void SendNotiChat(PK.NotiChat noti)
        {
            //MQTT
            PK.Packet packet = new PK.Packet(ProtocolType.NOTI,Protocol.NOTI_CHAT,
                JsonSerializer.Serialize<PK.NotiChat>(noti));
            Send("/chat/" + noti.roomNumber, packet);
        }
        public void Send(string destTopic, PK.Packet packet)
        {
            //serialize
            byte[] sendBuff = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<PK.Packet>(packet));

            //send
            this.client.Publish(destTopic, sendBuff, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        }

        private MqttClient client;

    }
    public class ServerStub
    {
        public ServerStub()
        {
            HttpServer httpServer = new HttpServer(8282, Routes);

            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();

        }

        public void BindProxy(ServerProxy proxy)
        {
            this.proxy = proxy;
        }

        private HttpResponse OnRecvGET(HttpRequest request)
        {
            string param = request.Url.Substring(request.Url.IndexOf("?") + 1);
            string[] parameters = param.Split('&');
            for (int i=0; i<parameters.Length; i++)
            {
                string[] tmp = parameters[i].Split('=');
                switch ((Protocol)Enum.Parse(typeof(Protocol), tmp[1]))
                {
                    case Protocol.REQ_ROOMLIST:
                        return DispatchPacket(new PK.Packet(ProtocolType.GET, Protocol.REQ_ROOMLIST), request.Headers["sessionID"]);
                    default:
                        Console.WriteLine(tmp[1]);
                        break;
                }
            }
            return GetHTTPResult(ProtocolConst.RESULT_FAIL_BAD_REQUEST);
        }

        private HttpResponse OnRecvPOST(HttpRequest request)
        {
            PK.Packet packet = JsonSerializer.Deserialize<PK.Packet>(request.Content);
            return DispatchPacket(packet, request.Headers["sessionID"]);                        
        }


        public Action<Protocol> OnProtocolError;
        private HttpResponse DispatchPacket(PK.Packet packet, string sessionID)
        {
            switch (packet.protocol)
            {
                case Protocol.REQ_JOIN:
                    return RecvPacketJoin(packet);                  
                case Protocol.REQ_LOGIN:
                    return RecvPacketLogin(packet);
                case Protocol.REQ_MAKEROOM:
                    if(IsSeesionValid(sessionID))
                        return RecvPacketMakeRoom(packet);
                    break;
                case Protocol.REQ_JOINROOM:
                    if (IsSeesionValid(sessionID))
                        return RecvPacketJoinRoom(packet);
                    break;
                case Protocol.REQ_ROOMLIST:
                    if (IsSeesionValid(sessionID))
                        return RecvPacketGetRoomList(packet);
                    break;
                case Protocol.REQ_SENDCHAT:
                    if (IsSeesionValid(sessionID))
                        return RecvPacketChat(packet);
                    break;
                default:
                    Console.WriteLine("server dispatchpacket : " + packet.protocol);
                    OnProtocolError?.Invoke(packet.protocol);
                    return GetHTTPResult(ProtocolConst.RESULT_FAIL_BAD_REQUEST);
            }
            return GetHTTPResult(ProtocolConst.RESULT_FAIL_FORBIDDEN, JsonSerializer.Serialize(new PK.Error(0, "권한이 없음")));
        }

        

        #region 외부로직(?)

        private Dictionary<string, Session> sessions = new Dictionary<string, Session>();
        private bool IsSeesionValid(string sessionID)
        {
            if (!sessions.ContainsKey(sessionID))
                return false;
            return true;
        }

        //dummy user data
        private Dictionary<string, User> dummyUsers = new Dictionary<string, User> {
            { "id1", new User("id1", "password1", "name1")},
            { "id2", new User("id2", "password2", "name2")}
        };
        //dummy room data
        private List<Room> dummyRooms = new List<Room>
        {
            new Room(0),new Room(1)
        };

        HttpResponse GetHTTPResult(int statusCode, string jsonData="")
        {
            return new HttpResponse()
            {
                ContentAsUTF8 = jsonData,
                ReasonPhrase = statusCode/100 == 2 ? ProtocolConst.RESULT_SUCCESS_TEXT : ProtocolConst.RESULT_ERROR_CLIENT_TEXT,
                StatusCode = Convert.ToString(statusCode)
            };
        }
        HttpResponse RecvPacketJoin(PK.Packet packet)
        {
            PK.ReqJoinUser req = JsonSerializer.Deserialize<PK.ReqJoinUser>(packet.jsonData);

            //더미데이터에서 중복 체크
            if(dummyUsers.ContainsKey(req.id))
                return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(0, "이미 있는 아이디")));
            dummyUsers.Add(req.id, new User(req.id, req.password, req.password));

            PK.ResJoinUser resJoinUser = new PK.ResJoinUser(req.id);

            return GetHTTPResult(ProtocolConst.RESULT_OK_CREATED, JsonSerializer.Serialize(resJoinUser));
        }

        HttpResponse RecvPacketLogin(PK.Packet packet)
        {
            PK.ReqLoginAuth req = JsonSerializer.Deserialize<PK.ReqLoginAuth>(packet.jsonData);
            PK.ResLoginAuth res;

            //더미데이터에서 password 체크
            if (dummyUsers.TryGetValue(req.id, out var user))
            {
                if (user.password != req.password)
                {
                    return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(0, "password 틀림")));
                }
            }
            else
                return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(1, "id 없음")));

            if (sessions.ContainsValue(new Session { userId = req.id }))
                return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(1, "이미 로그인한 아이디")));

            string sessionId = RandomStringMaker.GenerateSessionID();
            sessions.Add(sessionId, new Session { userId = req.id });

            //성공
            res = new PK.ResLoginAuth(req.id, sessionId, "");
            return GetHTTPResult(ProtocolConst.RESULT_SUCCESS, JsonSerializer.Serialize(res));
        }

        HttpResponse RecvPacketMakeRoom(PK.Packet packet)
        {
            //더미데이터에서 
            if(dummyRooms.Count >= 10)
                return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(0, "방이 10개 넘음")));
            dummyRooms.Add(new Room(dummyRooms.Count));

            PK.ResMakeRoom resMakeRoom = new PK.ResMakeRoom(dummyRooms[dummyRooms.Count-1].roomNumber);
            //성공
            return GetHTTPResult(ProtocolConst.RESULT_SUCCESS, JsonSerializer.Serialize(resMakeRoom));
        }
        HttpResponse RecvPacketJoinRoom(PK.Packet packet)
        {
            PK.ReqJoinRoom req = JsonSerializer.Deserialize<PK.ReqJoinRoom>(packet.jsonData);

            if(req.roomNumber >= dummyRooms.Count)
                return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(0, "방이 없음")));

            dummyRooms[req.roomNumber].userIdList.Add(req.userid);

            this.proxy.SendNotiChat(new NotiChat(req.roomNumber, 0, req.userid, "", req.userid + "님이 들어왔습니다."));
            return GetHTTPResult(ProtocolConst.RESULT_SUCCESS);
        }
        HttpResponse RecvPacketGetRoomList(PK.Packet packet)
        {
            ResRoomList res = new ResRoomList(dummyRooms);
            return GetHTTPResult(ProtocolConst.RESULT_SUCCESS, JsonSerializer.Serialize<ResRoomList>(res));
        }
        HttpResponse RecvPacketChat(PK.Packet packet)
        {
            PK.ReqSendChat req = JsonSerializer.Deserialize<PK.ReqSendChat>(packet.jsonData);

            if(!dummyRooms[req.roomNumber].userIdList.Contains(req.senderID))
                return GetHTTPResult(ProtocolConst.RESULT_FAIL_CONFLICT, JsonSerializer.Serialize(new PK.Error(0, "방에 유저가 입장하지 않은 상태")));

            this.proxy.SendNotiChat(new NotiChat(
                req.roomNumber,
                req.chatType,
                req.senderID,
                req.receiverID,
                req.msg
                ));

            return GetHTTPResult(ProtocolConst.RESULT_SUCCESS);
        }
        #endregion

        private List<Route> Routes
        {
            get
            {
                return new List<Route>()
                {
                    new Route()
                    {
                        Callable = OnRecvPOST,
                        UrlRegex = "^\\/test$",
                        Method = "POST"
                    },new Route()
                    {
                        Callable = OnRecvGET,
                        UrlRegex = "^\\/test\\?req=(.*)$",
                        Method = "GET"
                    }
                };

            }
        }
        private ServerProxy proxy;
    }
}
