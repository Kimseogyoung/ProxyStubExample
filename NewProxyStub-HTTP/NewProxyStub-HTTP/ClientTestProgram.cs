using System;
using System.Threading;
using PK = Packet;
namespace NewProxyStub_HTTP
{
    public class ClientTestProgram
    {
        static public ClientProxy clientProxy;
        static public ClientStub clientStub;
        static public string loginID;
        static public string sessionID;
        static public int roomNum;
        static public void Main(string[] args)
        {
            //dummy server 실행
            Thread thread = new Thread(new ThreadStart(StartServerThread));
            thread.Start();

            ClientTest();
        }
        static public void StartServerThread()
        {
            var serverProxy = new ServerProxy();
            var serverStub = new ServerStub();
            serverStub.BindProxy(serverProxy);
        }

        static public void ClientTest()
        {
            clientProxy = new ClientProxy();
            clientStub = new ClientStub();
            clientStub.OnRecvNotiChat = (noti) =>
            {
                Console.WriteLine("[Room " + noti.roomNumber + " Chat]" + noti.senderID + " : " + noti.msg);
            };

            while (true)
            {
                Console.WriteLine("nasdsadsa");
                Console.WriteLine("1.Join  2.Login  3.GET RoomList  4.Make Room  5.Join Room  6.Send Chat to CurrentRoom q.Quit");
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "1":
                        Join();
                        break;
                    case "2":
                        Login();
                        break;
                    case "3":
                        ShowRoomList();
                        break;
                    case "4":
                        MakeRoom();
                        break;
                    case "5":
                        JoinRoom();
                        break;
                    case "6":
                        SendChat();
                        break;
                    case "q":
                        Console.WriteLine("bye");
                        return;
                    case "put":
                        SessionManager.PutAndGetSessionKeyAsync(new Session());
                        break;
                    case "check":
                        SessionManager.PutSessionKeyAsync(new Session());
                        break;
                    default:
                        break;
                }
            }
        }

        static public void Join()
        {
            Console.Write("id : "); string id = Console.ReadLine();
            Console.Write("password : "); string password = Console.ReadLine();
            Console.Write("name : "); string name = Console.ReadLine();

            clientProxy.RequestJoinUser(new PK.ReqJoinUser(id, password, name),
                (PK.ResJoinUser res) =>
                {
                    Console.WriteLine(res.id + "님 가입 성공");
                },
                (PK.Error e) =>
                {
                    Console.WriteLine("[Fail] " + e.Code + " " + e.Message);

                });
        }

        static public void Login()
        {
            Console.Write("id : "); string id = Console.ReadLine();
            Console.Write("password : "); string password = Console.ReadLine();

            clientProxy.RequestLogin(new PK.ReqLoginAuth(id, password),
                (PK.ResLoginAuth res) =>
                {
                    Console.WriteLine(res.id + "님 로그인 성공" + res.token) ;
                    loginID = res.id;
                    clientProxy.sessionID = res.token;
                },
                (PK.Error e) =>
                {
                    Console.WriteLine("[Fail] "+e.Code + " " + e.Message);

                });
        }
        static public void ShowRoomList()
        {
            clientProxy.RequestGetRoomList(new PK.Packet(ProtocolType.GET, Protocol.REQ_ROOMLIST),
                (PK.ResRoomList res) =>
                {
                    for (int i=0; i<res.rooms.Count; i++)
                        Console.WriteLine(res.rooms[i].roomNumber+"번 방 " + res.rooms[i].userIdList.Count +"명");         
                },
                (PK.Error e) =>
                {
                    Console.WriteLine("[Fail] " + e.Code + " " + e.Message);

                });
        }
        static public void MakeRoom()
        {
            clientProxy.RequestMakeRoom(
                (PK.ResMakeRoom res) =>
                {
                    Console.WriteLine(res.roomNumber + "번 방 생성 완료");
                },
                (PK.Error e) =>
                {
                    Console.WriteLine("[Fail] " + e.Code + " " + e.Message);

                });
        }
        static public void JoinRoom()
        {
            Console.Write("room num : "); string num = Console.ReadLine();
            clientProxy.RequestJoinRoom(new PK.ReqJoinRoom(Convert.ToInt32(num),loginID),
                () =>
                {
                    Console.WriteLine("방 참가 성공");
                    roomNum = Convert.ToInt32(num);
                    clientStub.ChangeTopic("/chat/" + roomNum);
                },
                (PK.Error e) =>
                {
                    Console.WriteLine("[Fail] " + e.Code + " " + e.Message);

                });
        }
        static public void SendChat()
        {
            Console.Write("typing area : "); string chat = Console.ReadLine();

            clientProxy.RequestSendNotiChat(new PK.ReqSendChat(1,0, loginID,"", chat),
                () =>
                {
                    
                },
                (PK.Error e) =>
                {
                    Console.WriteLine("[Fail] " + e.Code + " " + e.Message);

                });
        }
    }
}
