using NewProxyStub_HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public class User
{
    public User(string id, string password, string name, string token = "")
    {
        this.id = id;
        this.password = password;
        this.name = name;
        this.token = token;
    }

    public string id { get; set; }
    public string password { get; set; }
    public string name { get; set; }
    public string token { get; set; }
}
public class Room
{
    public Room(int roomNumber)
    {
        this.roomNumber = roomNumber;
    }
    public int roomNumber { get; set; }
    public List<string> userIdList  { get; set; } = new List<string>();
}

namespace Packet
{
    public class Error
    {
        public Error(int Code, string Message)
        {
            this.Code = Code;
            this.Message = Message;
        }

        public int Code { get; private set; }
        public string Message { get; private set; }
    }

    public class Packet
    {
        public Packet(ProtocolType type, Protocol protocol, string jsonData="")
        {
            this.type = type;
            this.protocol = protocol;
            this.jsonData = jsonData;
        }
        public ProtocolType type { get; private set; }
        public Protocol protocol { get; private set; }
        public string jsonData { get; private set; }
    }
    public class UserPacket
    {
        public UserPacket(string id, int token, string jsonData)
        {
            this.id = id;
            this.token = token;
            this.jsonData = jsonData;
        }
        public string id { get; private set; }
        public int token { get; private set; }
        public string jsonData { get; private set; }
    }

    public class ReqJoinUser
    {
        public ReqJoinUser(string id, string password, string name)
        {
            this.id = id;
            this.password = password;
            this.name = name;
        }
        public string id { get; private set; }
        public string password { get; private set; }
        public string name { get; private set; }
    }

    public class ResJoinUser
    {
        public ResJoinUser(string id)
        {
            this.id = id;
        }
        public string id { get; private set; }
    }

    public class ReqLoginAuth
    {
        public ReqLoginAuth(string id, string password)
        {
            this.id = id;
            this.password = password;
        }
        public string id { get; private set; }
        public string password { get; private set; }
    }

    public class ResLoginAuth
    {
        public ResLoginAuth(string id, string token, string msg)
        {
            this.id = id;
            this.token = token;
            this.msg = msg;
        }
        public string id { get; private set; }
        public string token { get; private set; }
        public string msg { get; private set; }
    }

    public class ResMakeRoom
    {
        public ResMakeRoom(int roomNumber)
        {
            this.roomNumber = roomNumber;
        }
        public int roomNumber { get; private set; }
    }
    public class ResRoomList
    {
        public ResRoomList(List<Room> rooms)
        {
            this.rooms = rooms;
        }
        public List<Room> rooms { get; private set; }
    }
    public class ReqJoinRoom
    {
        public ReqJoinRoom(int roomNumber, string userid)
        {
            this.roomNumber = roomNumber;
            this.userid = userid;
        }
        public int roomNumber { get; private set; }
        public string userid { get; private set; }
    }
    public class ReqSendChat
    {
        public ReqSendChat(int roomNumber, int chatType, string senderID, string receiverID, string msg)
        {
            this.roomNumber = roomNumber;
            this.chatType = chatType;
            this.senderID = senderID;
            this.receiverID = receiverID;
            this.msg = msg;
        }
        public int roomNumber { get; private set; }
        public int chatType { get; private set; }
        public string senderID { get; private set; }
        public string receiverID { get; private set; }
        public string msg { get; private set; }

    }
    public class NotiChat
    {
        public NotiChat(int roomNumber, int chatType, string senderID, string receiverID, string msg)
        {
            this.roomNumber = roomNumber;
            this.chatType = chatType;
            this.senderID = senderID;
            this.receiverID = receiverID;
            this.msg = msg;
        }
        public int roomNumber { get; private set; }
        public int chatType { get; private set;  }
        public string senderID { get; private set; }
        public string receiverID { get; private set; }
        public string msg { get; private set; }
    }


}
