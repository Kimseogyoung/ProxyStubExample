using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PK = Packet;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters;
using System.Runtime.CompilerServices;

namespace NewProxyStub_HTTP
{
    internal class Program
    {
        static public ClientProxy clientProxy;
        static public ClientStub clientStub;
        static public string loginID;
        static public int roomNum;
        static public void Main(string[] args)
        {
            ClientTestProgram.ClientTest();
            
        }
    }
}
