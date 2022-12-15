using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using ProtobufSchema;
using System.Collections.Generic;

namespace ProxyStub_Socket.Server
{
    public class Stub
    {
        public Stub(Proxy serverProxy)
        {
            BindProxy(serverProxy);
        }

        public void StartServer()
        {

            //소켓 생성
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
            serverSocket.Bind(serverEP);

            // backlog 인수 : 최대 대기수
            serverSocket.Listen(10);
            Console.WriteLine("Listening....");

            serverSocket.BeginAccept(Accect, null);
        }

        public void StopServer()
        {
            serverSocket.Close();
            serverSocket = null;
        }

        public void OnRecv(IAsyncResult ar)
        {
            //넘겨줄 소켓,byte 정보
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            Socket socket = obj.WorkingSocket;

            int size = socket.EndReceive(ar);

            //응답 종료
            if (obj.Buffer[0] == 0)
                return;

            //스트림 처리 코드 임시 주석
                //buffer = buffer.Concat(obj.Buffer.ToArray<byte>()).ToArray();    
                //if(bufferSize == buffer.Length)

            byte[] buf = new byte[size];
            Array.Copy(obj.Buffer, 0, buf, 0, size);
            Packet packet = Packet.Parser.ParseFrom(buf);

            switch ((Protocol.PacketType)packet.Type)
            {
                case Protocol.PacketType.Req:
                    OnRecvRequest(socket, packet);
                    break;
                case Protocol.PacketType.Res:
                    break;
                case Protocol.PacketType.Noti:
                    OnRecvNoti(packet);
                    break;
                default:
                    Console.WriteLine("Server.cs : OnRecv wrong packet type" + (Protocol.PacketType)packet.Type);
                    break;
            }

            //비동기 받기 대기
            AsyncObject obj2 = new AsyncObject(4096, socket);
            socket.BeginReceive(obj2.Buffer, 0, obj2.Buffer.Length, 0, OnRecv, obj2);
        }

        private void OnRecvRequest(Socket clientSocket,Packet packet)
        {
            Packet resPacket = null;
            if (reqHandler.TryGetValue((Protocol.ReqAction)packet.Action, out Func<Packet,Packet> action))
                resPacket = action.Invoke(packet);

            serverProxy.SendRes(clientSocket, resPacket);
        }

        private void OnRecvNoti(Packet packet)
        {
            if (notiHandler.TryGetValue((Protocol.NotiAction)packet.Action, out Action<Packet> action)==true)
                action.Invoke(packet);
        }

        private void Accect(IAsyncResult ar)
        {
            Socket clientSocket = serverSocket.EndAccept(ar);
            this.serverProxy.AddClientSocket(clientSocket);

            //새로운 클라이언트가 연결되었을 때 콜백
            OnConnected(clientSocket);

            // 또 다른 클라이언트의 연결을 대기
            serverSocket.BeginAccept(Accect, null);

            //비동기 받기 대기
            AsyncObject obj = new AsyncObject(4096, clientSocket);
            clientSocket.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, 0, OnRecv, obj);

        }
        private void BindProxy(Proxy proxy)
        {
            this.serverProxy = proxy;
        }

        public void RegisterRecvHandler(Protocol.ReqAction command, Func<Packet, Packet> func)
        {
            reqHandler.Add(command, func);
        }
        public void RegisterRecvHandler(Protocol.NotiAction command, Action<Packet> action)
        {
            notiHandler.Add(command, action);
        }

        virtual protected void OnConnected(Socket socket) {}

        protected Proxy serverProxy;
        private Socket serverSocket;
        private Dictionary<Protocol.ReqAction, Func<ProtobufSchema.Packet, ProtobufSchema.Packet>> reqHandler = new Dictionary<Protocol.ReqAction, Func<ProtobufSchema.Packet, ProtobufSchema.Packet>>();
        private Dictionary<Protocol.NotiAction, Action<ProtobufSchema.Packet>> notiHandler = new Dictionary<Protocol.NotiAction, Action<ProtobufSchema.Packet>>();

        //private int bufferSize = -1;
        //private byte[] buffer;
        //private bool receiveLock = false;

        public int maxWaitClient = 10;

    }
    public class Proxy
    {
        public void SendRes(Socket destSocket, Packet packet)
        {
            destSocket.Send(PacketToByte(packet));

            //callback
            OnSend?.Invoke();
        }

        public void SendNoti(Packet packet)
        {
            byte[] sendBuff = PacketToByte(packet);

            foreach (Socket socket in this.connectedSocket)
                socket.Send(sendBuff);

            OnSend?.Invoke();
        }

        public void SendNoti(Socket destSocket, Packet packet)
        {
            destSocket.Send(PacketToByte(packet));

            OnSend?.Invoke();
        }

        public void AddClientSocket(Socket clicnetSocket)
        {
            this.connectedSocket.Add(clicnetSocket);
        }
        private byte[] PacketToByte(Packet packet)
        {
            //serialize
            MemoryStream stream = new MemoryStream();
            using (var writeStream = new Google.Protobuf.CodedOutputStream(stream, true))
                packet.WriteTo(writeStream);

            //send
            return stream.ToArray();
        }
        public Action OnSend;
        private List<Socket> connectedSocket= new List<Socket>();
    }
    /*
 * 참고 메모
 * 
 * byte[] stx = { 0x02 };
  byte[] version = BitConverter.GetBytes(1);
        byte[] command = BitConverter.GetBytes(1003L);
        byte[] payload = Encoding.ASCII.GetBytes("Add 1, Please ^^");
        Array.Resize(ref payload, 100);
        byte[] etx = { 0x03 };

        Byte[] data = stx.Concat(version).Concat(command).Concat(payload).Concat(etx).ToArray();
*/
}
