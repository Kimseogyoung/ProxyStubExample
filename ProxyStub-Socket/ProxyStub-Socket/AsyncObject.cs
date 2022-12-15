using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProxyStub_Socket
{
    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];
        }
        public AsyncObject(int bufferSize, Socket socket)
        {
            BufferSize = bufferSize;
            Buffer = new byte[BufferSize];

            WorkingSocket = socket;
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }
}
