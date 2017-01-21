using Common;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                StartMultipleProcessess();
                Environment.Exit(0);
            }
            else
            {
                Console.Title = $"Client {args[0]}";
                Connect();
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Starts multiple instances of this app
        /// </summary>
        private static void StartMultipleProcessess()
        {
            for (int i = 1; i <= 2; i++)
            {
                ProcessStartInfo info = new ProcessStartInfo("Client.exe");
                info.Arguments = $"{i}";
                Process.Start(info);
            }
        }

        /// <summary>
        /// Attempts to connect to a server
        /// </summary>
        private static void Connect()
        {
            ConnectedObject client = new ConnectedObject();
            // Create a new socket
            client.Socket = ConnectionManager.CreateSocket();
            int attempts = 0;

            // Loop until we connect (server could be down)
            while (!client.Socket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);

                    // Attempt to connect
                    client.Socket.Connect(ConnectionManager.EndPoint);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            // Display connected status
            Console.Clear();
            PrintConnectionState($"Socket connected to {client.Socket.RemoteEndPoint.ToString()}");

            // Start sending & receiving
            Thread sendThread = new Thread(() => Send(client));
            Thread receiveThread = new Thread(() => Receive(client));

            sendThread.Start();
            receiveThread.Start();

            // Listen for threads to be aborted (occurs when socket looses it's connection with the server)
            while (sendThread.IsAlive && receiveThread.IsAlive) { }

            // Attempt to reconnect
            Connect();
        }

        /// <summary>
        /// Sends a message to the server
        /// </summary>
        /// <param name="client"></param>
        private static void Send(ConnectedObject client)
        {
            // Build message
            client.CreateOutgoingMessage($"Message from {Console.Title}");
            byte[] data = client.OutgoingMessageToBytes();

            // Send it on a 1 second interval
            while (true)
            {
                Thread.Sleep(3000);
                try
                {
                    client.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Server Closed");
                    client.Close();
                    Thread.CurrentThread.Abort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Thread.CurrentThread.Abort();
                }
            }
        }

        /// <summary>
        /// Message sent handler
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            Console.WriteLine("Message Sent");
        }

        private static void Receive(ConnectedObject client)
        {
            int bytesRead = 0;

            while (true)
            {
                // Read message from the server
                try
                {
                    bytesRead = client.Socket.Receive(client.Buffer, SocketFlags.None);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Server Closed");
                    client.Close();
                    Thread.CurrentThread.Abort();
                }
                catch (Exception)
                {
                    Thread.CurrentThread.Abort();
                    return;
                }


                // Check message
                if (bytesRead > 0)
                {
                    // Build message as it comes in
                    client.BuildIncomingMessage(bytesRead);

                    // Check if we received the full message
                    if (client.MessageReceived())
                    {
                        // Print message to the console
                        Console.WriteLine("Message Received");

                        // Reset message
                        client.ClearIncomingMessage();
                    }
                }
            }
        }

        /// <summary>
        /// Prints connection 'connected' or 'disconnected' states
        /// </summary>
        /// <param name="msg"></param>
        public static void PrintConnectionState(string msg)
        {
            string divider = new String('*', 60);
            Console.WriteLine();
            Console.WriteLine(divider);
            Console.WriteLine(msg);
            Console.WriteLine(divider);
        }
    }
}
