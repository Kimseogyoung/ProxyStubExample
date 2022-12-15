using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientTest;


    public class Program
    {
        static void Main(string[] args)
        {
            ClientTest.ClientTest test2 = new ClientTest.ClientTest("user2");
            test2.Start();       
        }   
    }

