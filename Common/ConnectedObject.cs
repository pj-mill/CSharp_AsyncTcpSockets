using System;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    /// <summary>
    /// Wrapper for client connections
    /// </summary>
    public class ConnectedObject
    {
        #region Properties
        // Client socket
        public Socket Socket { get; set; }
        // Size of receive buffer
        public int BufferSize { get; set; } = 1024;
        // Receive buffer
        public byte[] Buffer { get; set; }
        // Received data string
        private StringBuilder IncomingMessage { get; set; }
        // Message to be sent
        private StringBuilder OutgoingMessage { get; set; }
        // Terminator for each message
        public string MessageTerminator { get; set; } = "<END>";
        #endregion

        #region Constructors
        public ConnectedObject()
        {
            Buffer = new byte[BufferSize];
            IncomingMessage = new StringBuilder();
            OutgoingMessage = new StringBuilder();
        }
        #endregion

        #region Outgoing Message Methods
        /// <summary>
        /// Converts the outgoing message to bytes
        /// </summary>
        /// <returns></returns>
        public byte[] OutgoingMessageToBytes()
        {
            if (OutgoingMessage.ToString().IndexOf(MessageTerminator) < 0)
            {
                OutgoingMessage.Append(MessageTerminator);
            }
            return Encoding.ASCII.GetBytes(OutgoingMessage.ToString());
        }


        /// <summary>
        /// Creates a new outgoing message
        /// </summary>
        /// <param name="msg"></param>
        public void CreateOutgoingMessage(string msg)
        {
            OutgoingMessage.Clear();
            OutgoingMessage.Append(msg);
            OutgoingMessage.Append(MessageTerminator);
        }

        #endregion

        #region Incoming Message Methods
        /// <summary>
        /// Converts the buffer to a string ans stores it
        /// </summary>
        public void BuildIncomingMessage(int bytesRead)
        {
            IncomingMessage.Append(Encoding.ASCII.GetString(Buffer, 0, bytesRead));
        }

        /// <summary>
        /// Determines if the message was fully received
        /// </summary>
        /// <returns></returns>
        public bool MessageReceived()
        {
            return IncomingMessage.ToString().IndexOf(MessageTerminator) > -1;
        }

        /// <summary>
        /// Clears the current incoming message so that we can start building for the next message
        /// </summary>
        public void ClearIncomingMessage()
        {
            IncomingMessage.Clear();
        }

        /// <summary>
        /// Gets the length of the incoming message
        /// </summary>
        /// <returns></returns>
        public int IncomingMessageLength()
        {
            return IncomingMessage.Length;
        }
        #endregion

        #region Connected Object Methods
        /// <summary>
        /// Closes the connection
        /// </summary>
        public void Close()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
            catch (Exception)
            {
                System.Console.WriteLine("connection already closed");
            }
        }

        public string GetRemoteEndPoint()
        {
            return Socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// Print the details of the current incoming message
        /// </summary>
        public void PrintMessage()
        {
            string divider = new String('=', 60);
            Console.WriteLine();
            Console.WriteLine(divider);
            Console.WriteLine("Message Received");
            Console.WriteLine(divider);
            Console.WriteLine($"Read {IncomingMessageLength()} bytes from socket.");
            Console.WriteLine($"Message: {IncomingMessage.ToString()}");
        }
        #endregion
    }
}
