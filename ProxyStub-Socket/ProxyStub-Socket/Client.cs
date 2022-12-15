using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using ProtobufSchema;
using System.Collections.Generic;

namespace ProxyStub_Socket.Client
{
    public class Stub
    {
        public Stub()
        {
            Connect2Server();
        }
        
        public void Disconnect2Server()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        public void RegisterRecvHandler(Protocol.ResAction command, Action<ProtobufSchema.Packet> action)
        {
            resHandler.Add(command, action);
        }
        public void RegisterRecvHandler(Protocol.NotiAction command, Action<ProtobufSchema.Packet> action)
        {
            notiHandler.Add(command, action);
        }

        private void Connect2Server()
        {
            // 소켓 객체 생성(TCP 소켓)
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //서버에 연결
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
            clientSocket.BeginConnect(ep, OnConnect, null);
        }

        private void OnConnect(IAsyncResult result)
        {
            Console.WriteLine("Connected...");

            //비동기 받기 대기
            AsyncObject obj = new AsyncObject(4096, clientSocket);
            clientSocket.BeginReceive(obj.Buffer, 0, obj.Buffer.Length, 0, OnRecv, obj);

        }
        private void OnRecv(IAsyncResult ar)
        {
            if (clientSocket.Connected == false)
                return;

            AsyncObject obj = (AsyncObject)ar.AsyncState;
            Socket socket = (Socket)obj.WorkingSocket;
            int size = socket.EndReceive(ar);
            if (obj.Buffer[0] == 0)
            {
                clientSocket.Close();
                return;
            }

            //deseralize
            byte[] buf = new byte[size];
            Array.Copy(obj.Buffer, 0, buf, 0, size);
            Packet packet = Packet.Parser.ParseFrom(buf);

            //Type 별 메서드
            if (packet.Type == (int)Protocol.PacketType.Res)
                OnRecvResponse(packet);
            else if (packet.Type == (int)Protocol.PacketType.Noti)
                OnRecvNoti(packet);



            AsyncObject obj2 = new AsyncObject(4096, clientSocket);
            //비동기 받기 대기
            socket.BeginReceive(obj2.Buffer, 0, obj2.Buffer.Length, 0, OnRecv, obj2);
        }
        private void OnRecvResponse(Packet packet)
        {
            if(resHandler.TryGetValue((Protocol.ResAction)packet.Action, out Action<Packet> action))
                action.Invoke(packet);
        }
        private void OnRecvNoti(Packet packet)
        {
            if (notiHandler.TryGetValue((Protocol.NotiAction)packet.Action, out Action<Packet> action))
                action.Invoke(packet);
        }

        public Socket GetClientSocket() { return this.clientSocket; }

        private Socket clientSocket;
        private Dictionary<Protocol.ResAction, Action<ProtobufSchema.Packet>> resHandler = new Dictionary<Protocol.ResAction, Action<ProtobufSchema.Packet>>();
        private Dictionary<Protocol.NotiAction, Action<ProtobufSchema.Packet>> notiHandler = new Dictionary<Protocol.NotiAction, Action<ProtobufSchema.Packet>>();

    }
    public class Proxy
    {
        public Proxy(Stub clientStub)
        {
            this.clientSocket = clientStub.GetClientSocket();
        }

        public void SendReq(Packet packet)
        {
            //serialize
            MemoryStream stream = new MemoryStream();
            using (var writeStream = new Google.Protobuf.CodedOutputStream(stream, true))
                packet.WriteTo(writeStream);

            //send
            byte[] sendBuff = stream.ToArray();
            clientSocket.Send(sendBuff, SocketFlags.None);

        }
        public void SendNoti(Packet packet)
        {
            //serialize
            MemoryStream stream = new MemoryStream();
            using (var writeStream = new Google.Protobuf.CodedOutputStream(stream, true))
                packet.WriteTo(writeStream);

            //send
            byte[] sendBuff = stream.ToArray();
            clientSocket.Send(sendBuff, SocketFlags.None);
        }

        private Socket clientSocket;
    }
}
