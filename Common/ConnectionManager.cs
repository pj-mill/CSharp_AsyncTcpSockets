using System;
using System.Net;
using System.Net.Sockets;

namespace Common
{
    public class ConnectionManager
    {
        public static IPAddress LocalIPAddress { get { return IPAddress.Loopback; } }
        public static int Port { get { return 42000; } }
        public static IPEndPoint EndPoint { get { return new IPEndPoint(LocalIPAddress, Port); } }

        public static Socket CreateListener()
        {
            Socket socket = null;
            try
            {
                // Create a TCP/IP socket.
                socket = CreateSocket();
                socket.Bind(EndPoint);
                socket.Listen(10);
            }
            catch (Exception)
            {
                throw;
            }

            return socket;
        }

        public static Socket CreateSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
