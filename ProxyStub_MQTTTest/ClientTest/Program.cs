using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    internal class Program
    {
        static public void Main(string[] arg)
        {
            ServerTest server = new ServerTest();
            server.Start();

            ClientTest test1 = new ClientTest("user1");
            test1.Start();
        }
    }
}
