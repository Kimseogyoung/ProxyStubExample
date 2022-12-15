using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol
{//임시
    public enum PacketType
    {
        Req, Res, Noti
    }
    public enum ReqAction
    {
        Hello, GetServerName
    }
    public enum ResAction
    {
        Hello, GetServerName
    }
    public enum NotiAction
    {
        ServerChat
    }
}
