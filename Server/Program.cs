using Common;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        // Client Collection
        private static List<ConnectedObject> _clients;
        // Thread Signal
        private static ManualResetEvent _connected = new ManualResetEvent(false);
        // Server socket
        private static Socket _server = null;

        static void Main(string[] args)
        {
            Console.Title = "Server";
            _clients = new List<ConnectedObject>();
            StartListening();
            Console.ReadLine();
            CloseAllSockets();
        }

        /// <summary>
        /// Listen for client connections
        /// </summary>
        private static void StartListening()
        {
            try
            {
                Console.WriteLine("Starting server");
                _server = ConnectionManager.CreateListener();
                Console.WriteLine($"Server Started, Waiting for a connection ...");

                while (true)
                {
                    // Set the event to nonsignaled state
                    _connected.Reset();

                    // Start an asynchronous socket to listen for connections
                    _server.BeginAccept(new AsyncCallback(AcceptCallback), _server);

                    // Wait until a connection is made before continuing
                    _connected.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Handler for new connections
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptCallback(IAsyncResult ar)
        {
            PrintConnectionState("Connection received");

            // Signal the main thread to continue accepting new connections
            _connected.Set();

            // Accept new client socket connection
            Socket socket = _server.EndAccept(ar);

            // Create a new client connection object and store the socket
            ConnectedObject client = new ConnectedObject();
            client.Socket = socket;

            // Store all clients
            _clients.Add(client);

            // Begin receiving messages from new connection
            try
            {
                client.Socket.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Handler for received messages
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            string err;
            ConnectedObject client;
            int bytesRead;

            // Check for null values
            if (!CheckState(ar, out err, out client))
            {
                Console.WriteLine(err);
                return;
            }

            // Read message from the client socket
            try
            {
                bytesRead = client.Socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    client.PrintMessage();

                    // Reset message
                    client.ClearIncomingMessage();

                    // Acknowledge message
                    SendReply(client);
                }
            }

            // Listen for more incoming messages
            try
            {
                client.Socket.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Sends a reply to client
        /// </summary>
        /// <param name="client"></param>
        private static void SendReply(ConnectedObject client)
        {
            if (client == null)
            {
                Console.WriteLine("Unable to send reply: client null");
                return;
            }

            Console.Write("Sending Reply: ");

            // Create reply
            client.CreateOutgoingMessage("Message Received");
            var byteReply = client.OutgoingMessageToBytes();

            // Listen for more incoming messages
            try
            {
                client.Socket.BeginSend(byteReply, 0, byteReply.Length, SocketFlags.None, new AsyncCallback(SendReplyCallback), client);
            }
            catch (SocketException)
            {
                // Client was forcebly closed on the client side
                CloseClient(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Handler after a reply has been sent
        /// </summary>
        /// <param name="ar"></param>
        private static void SendReplyCallback(IAsyncResult ar)
        {
            Console.WriteLine("Reply Sent");
        }

        /// <summary>
        /// Checks IAsyncResult for null value
        /// </summary>
        /// <param name="ar"></param>
        /// <param name="err"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private static bool CheckState(IAsyncResult ar, out string err, out ConnectedObject client)
        {
            // Initialise
            client = null;
            err = "";

            // Check ar
            if (ar == null)
            {
                err = "Async result null";
                return false;
            }

            // Check client
            client = (ConnectedObject)ar.AsyncState;
            if (client == null)
            {
                err = "Client null";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Closes a client socket and removes it from the client list
        /// </summary>
        /// <param name="client"></param>
        private static void CloseClient(ConnectedObject client)
        {
            PrintConnectionState("Client disconnected");
            client.Close();
            if (_clients.Contains(client))
            {
                _clients.Remove(client);
            }
        }

        /// <summary>
        /// Closes all client and server connections
        /// </summary>
        private static void CloseAllSockets()
        {
            // Close all clients
            foreach (ConnectedObject connection in _clients)
            {
                connection.Close();
            }
            // Close server
            _server.Close();
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
